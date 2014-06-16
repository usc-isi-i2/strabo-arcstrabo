/*******************************************************************************
 * Copyright 2010 University of Southern California
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * 	http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * This code was developed as part of the Strabo map processing project 
 * by the Spatial Sciences Institute and by the Information Integration Group 
 * at the Information Sciences Institute of the University of Southern 
 * California. For more information, publications, and related projects, 
 * please see: http://yoyoi.info and http://www.isi.edu/integration
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using Strabo.Core.ImageProcessing;
using Strabo.Core.Utility;

namespace Strabo.Core.TextLayerExtraction
{
    public class TextCCAnalysis
    {
        public double area_ratio = 0.7;
        public double mw=40, mh=10;
        public int cc_count = 0;
        public TextCCAnalysis() { }
        public List<Bitmap> Apply(Bitmap srcimg)
        {
            return RemoveCCUsingSize(srcimg,1,1);
        }
        public List<Bitmap> RemoveCCUsingSize(Bitmap srcimg, double upper_ratio, double lower_ratio) // for removing small line segments
        {
            srcimg= ImageUtils.ConvertGrayScaleToBinary(srcimg, 254);
            srcimg = ImageUtils.InvertColors(srcimg);
            MyConnectedComponentsAnalysisFast.MyBlobCounter bc = new MyConnectedComponentsAnalysisFast.MyBlobCounter();
            List<MyConnectedComponentsAnalysisFast.MyBlob> blobs = bc.GetBlobs(srcimg);
            List<int> remove = new List<int>();

            //for (int i = 0; i < blobs.Count; i++)
            //{
            //    double r = (double)blobs[i].bbx.Width / (double)blobs[i].bbx.Height;
            //    if ( (r>0.5 && r<2)&&
            //          blobs[i].pixel_count > (double)(blobs[i].bbx.Width * blobs[i].bbx.Height) * area_ratio)
                
            //    {
            //        if (!remove.Contains(i + 1))
            //            remove.Add(i + 1); // the index+1 of the blobs is the actual index of the bc label
            //    }
              
            //}

            for (int i = 0; i < blobs.Count; i++)
            {
                if ((Math.Max(blobs[i].bbx.Width, blobs[i].bbx.Height) < (double)Math.Max(mh, mw) / lower_ratio) ||
                    (Math.Max(blobs[i].bbx.Width, blobs[i].bbx.Height) > upper_ratio * (double)Math.Max(mh, mw)))
                {
                    if (!remove.Contains(i + 1))
                        remove.Add(i + 1); // the index+1 of the blobs is the actual index of the bc label
                }
            }
            //List<int> nei_list = new List<int>();
            //for (int i = 0; i < blobs.Count; i++)
            //{
            //    if (!remove.Contains(i + 1))
            //    {
            //        bool find = false;
            //        if (nei_list.Contains(i + 1))
            //            find = true;
            //        else
            //        {
            //            for (int j = 0; j < blobs.Count; j++)
            //            {
            //                if (i == j) continue;
            //                double r = (double)Math.Max(blobs[i].bbx.Width, blobs[i].bbx.Height) / (double)Math.Max(blobs[j].bbx.Width, blobs[j].bbx.Height);
            //                if (r > 1.5 || r < 0.667) continue;
            //                if (Distance(blobs[j].mass_center, blobs[i].mass_center) >
            //                    ((double)Math.Max(blobs[i].bbx.Width, blobs[i].bbx.Height) +
            //                    (double)Math.Max(blobs[j].bbx.Width, blobs[j].bbx.Height)) / 2 * 2.5) continue;
            //                nei_list.Add(j + 1);
            //                find = true;
            //                break;
            //            }
            //        }
            //        if (!find) remove.Add(i + 1);
            //    }
            //}

            int width = srcimg.Width; int height = srcimg.Height;
            int label = 0;
            // create new grayscale srcimg
            Bitmap dstimg = ImageUtils.CreateBlankGrayScaleBmp(width, height);
            Bitmap removedimg = ImageUtils.CreateBlankGrayScaleBmp(width, height);
            BitmapData srcData = srcimg.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            // lock destination bitmap data
            BitmapData dstData = dstimg.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
            BitmapData removedData = removedimg.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
            int offset = srcData.Stride - width;

            // do the job
            unsafe
            {
                byte* src = (byte*)srcData.Scan0.ToPointer();
                byte* dst = (byte*)dstData.Scan0.ToPointer();
                byte* removed = (byte*)removedData.Scan0.ToPointer();
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++, label++, src++, dst++, removed++)
                    {
                        int g = (int)(*src) == 0 ? 255 : 0; // src is ivt
                        if (g == 255) // src is background
                        {
                            *dst = (byte)255;
                            *removed = (byte)255;
                        }
                        else
                        {
                            bool r = remove.Contains(bc.objectLabels[label]);
                            *dst = (byte)(r ? 255 : g);
                            *removed = (byte)(r ? g : 255);
                        }
                    }
                    src += offset;
                    dst += offset;
                    removed += offset;
                }
            }
            srcimg.UnlockBits(srcData);
            dstimg.UnlockBits(dstData);
            removedimg.UnlockBits(removedData);
            //List<Bitmap> retimgList = new List<Bitmap>();
            //retimgList.Add(dstimg);
            //retimgList.Add(removedimg);
            //return retimgList;
            Log.WriteBitmap2Debug(dstimg, "dstimg");
            Log.WriteBitmap2Debug(removedimg, "removedimg");
            List<Bitmap> img_list = new List<Bitmap>();
            img_list.Add(dstimg); //img_list.Add(removedimg);
            return img_list;// removedimg;
        }
        public double Distance(Point a, Point b)
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }
        //public static Bitmap RemoveCC(int width, int height, int s, Bitmap srcimg)
        //{
        //    Invert ivt = new Invert();
        //    srcimg = ivt.Apply(srcimg);
        //    MyConnectedComponentsAnalysis.MyBlobCounter bc = new MyConnectedComponentsAnalysis.MyBlobCounter();
        //    List<MyConnectedComponentsAnalysis.MyBlob> blobs = bc.GetBlobs(srcimg);
        //    List<int> remove = new List<int>();

        //    for (int i = 0; i < blobs.Count; i++)
        //    {
        //        if (width == 0 && height == 0)
        //        {
        //            if (blobs[i].pixel_count > (Math.Sqrt(blobs[i].bbx.Width * blobs[i].bbx.Width + blobs[i].bbx.Height * blobs[i].bbx.Height) + s))
        //                if (!remove.Contains(i + 1)) remove.Add(i + 1); // the index+1 of the blobs is the actual index of the bc label
        //        }
        //        else if (s == 0)
        //        {
        //            if (blobs[i].bbx.Width < width && blobs[i].bbx.Height < height)
        //                if (!remove.Contains(i + 1)) remove.Add(i + 1);

        //        }
        //        else
        //        {

        //            if (blobs[i].bbx.Width < width && blobs[i].bbx.Height < height)
        //            {
        //                //if (blobs[i].pixel_count > (Math.Sqrt(blobs[i].bbx.Width * blobs[i].bbx.Width + blobs[i].bbx.Height * blobs[i].bbx.Height) + 1))
        //                if (!remove.Contains(i + 1)) remove.Add(i + 1);
        //            }
        //            if (blobs[i].pixel_count < s)
        //                if (!remove.Contains(i + 1)) remove.Add(i + 1);
        //        }
        //    }

        //    int width = srcimg.Width; int height = srcimg.Height;
        //    int label = 0;
        //    // create new grayscale srcimg
        //    Bitmap dstimg = AForge.Imaging.Image.CreateGrayscaleImage(width, height);
        //    Bitmap removedimg = AForge.Imaging.Image.CreateGrayscaleImage(width, height);
        //    BitmapData srcData = srcimg.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
        //    // lock destination bitmap data
        //    BitmapData dstData = dstimg.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
        //    BitmapData removedData = removedimg.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
        //    int offset = srcData.Stride - width;

        //    // do the job
        //    unsafe
        //    {
        //        byte* src = (byte*)srcData.Scan0.ToPointer();
        //        byte* dst = (byte*)dstData.Scan0.ToPointer();
        //        byte* removed = (byte*)removedData.Scan0.ToPointer();
        //        for (int y = 0; y < height; y++)
        //        {
        //            for (int x = 0; x < width; x++, label++, src++, dst++, removed++)
        //            {
        //                int g = (int)(*src) == 0 ? 255 : 0; // src is ivt
        //                if (g == 255) // src is background
        //                {
        //                    *dst = (byte)255;
        //                    *removed = (byte)255;
        //                }
        //                else
        //                {
        //                    bool r = remove.Contains(bc.objectLabels[label]);
        //                    *dst = (byte)(r ? 255 : g);
        //                    *removed = (byte)(r ? g : 255);
        //                }
        //            }
        //            src += offset;
        //            dst += offset;
        //            removed += offset;
        //        }
        //    }
        //    srcimg.UnlockBits(srcData);
        //    dstimg.UnlockBits(dstData);
        //    removedimg.UnlockBits(removedData);
        //    //List<Bitmap> retimgList = new List<Bitmap>();
        //    //retimgList.Add(dstimg);
        //    //retimgList.Add(removedimg);
        //    //return retimgList;
        //    return dstimg;
        //}
        //public static Bitmap RemoveCC(int width, int height, Bitmap srcimg, Bitmap ccimg)
        //{
        //    MyConnectedComponentsAnalysis.MyBlobCounter bc = new MyConnectedComponentsAnalysis.MyBlobCounter();
        //    List<MyConnectedComponentsAnalysis.MyBlob> blobs = bc.GetBlobs(ccimg);
        //    List<int> remove = new List<int>();

        //    for (int i = 0; i < blobs.Count; i++)
        //    {
        //        if (blobs[i].bbx.Width > width && blobs[i].bbx.Height > height)
        //            remove.Add(i + 1); // the index+1 of the blobs is the actual index of the bc label
        //    }

        //    int width = srcimg.Width; int height = srcimg.Height;
        //    int label = 0;
        //    // create new grayscale srcimg
        //    Bitmap dstimg = AForge.Imaging.Image.CreateGrayscaleImage(width, height);

        //    BitmapData srcData = srcimg.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
        //    // lock destination bitmap data
        //    BitmapData dstData = dstimg.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

        //    int offset = srcData.Stride - width;

        //    // do the job
        //    unsafe
        //    {
        //        byte* src = (byte*)srcData.Scan0.ToPointer();
        //        byte* dst = (byte*)dstData.Scan0.ToPointer();

        //        for (int y = 0; y < height; y++)
        //        {
        //            for (int x = 0; x < width; x++, label++, src++, dst++)
        //            {
        //                *dst = (byte)(remove.Contains(bc.objectLabels[label]) ? 255 : (int)(*src));
        //            }
        //            src += offset;
        //            dst += offset;
        //        }
        //    }
        //    srcimg.UnlockBits(srcData);
        //    dstimg.UnlockBits(dstData);
        //    return dstimg;
        //}
    }
}
