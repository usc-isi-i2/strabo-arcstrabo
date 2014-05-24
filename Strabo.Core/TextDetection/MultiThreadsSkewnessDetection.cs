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
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using Strabo.Core.ImageProcessing;

namespace Strabo.Core.TextDetection
{
    public class MultiThreadsSkewnessDetection
    {
        public string dir;
        public bool debug = false;

        public Bitmap srcimg;
        public List<Bitmap> srcimg_list = new List<Bitmap>();
        public int width, height;
        public int tnum;
        public int start;
        public int end;
        public int inc;
        public Hashtable rotatedimg_table = new Hashtable();
        int max_width=Int16.MinValue;
        int max_width_idx;

        int alpha;
        int beta;

        double max=Double.MinValue;
        int minc = Int16.MinValue, minc_idx, max_idx;
        public MultiThreadsSkewnessDetection() { }


        public void RotateImage(Bitmap srcimg, int angle)
        {
            if (srcimg == null)
                throw new ArgumentNullException("image");
            if (!rotatedimg_table.ContainsKey(angle))
            {

                const double pi2 = Math.PI / 2.0;

                // Why can't C# allow these to be const, or at least readonly
                // *sigh*  I'm starting to talk like Christian Graus :omg:
                double oldWidth = width;
                double oldHeight = height;

                // Convert degrees to radians
                double theta = ((double)angle) * Math.PI / 180.0;
                double locked_theta = theta;

                // Ensure theta is now [0, 2pi)
                while (locked_theta < 0.0)
                    locked_theta += 2 * Math.PI;

                double newWidth, newHeight;
                int nWidth, nHeight; // The newWidth/newHeight expressed as ints

                #region Explaination of the calculations
                #endregion

                double adjacentTop, oppositeTop;
                double adjacentBottom, oppositeBottom;

                // We need to calculate the sides of the triangles based
                // on how much rotation is being done to the bitmap.
                //   Refer to the first paragraph in the explaination above for 
                //   reasons why.
                if ((locked_theta >= 0.0 && locked_theta < pi2) ||
                    (locked_theta >= Math.PI && locked_theta < (Math.PI + pi2)))
                {
                    adjacentTop = Math.Abs(Math.Cos(locked_theta)) * oldWidth;
                    oppositeTop = Math.Abs(Math.Sin(locked_theta)) * oldWidth;

                    adjacentBottom = Math.Abs(Math.Cos(locked_theta)) * oldHeight;
                    oppositeBottom = Math.Abs(Math.Sin(locked_theta)) * oldHeight;
                }
                else
                {
                    adjacentTop = Math.Abs(Math.Sin(locked_theta)) * oldHeight;
                    oppositeTop = Math.Abs(Math.Cos(locked_theta)) * oldHeight;

                    adjacentBottom = Math.Abs(Math.Sin(locked_theta)) * oldWidth;
                    oppositeBottom = Math.Abs(Math.Cos(locked_theta)) * oldWidth;
                }

                newWidth = adjacentTop + oppositeBottom;
                newHeight = adjacentBottom + oppositeTop;

                nWidth = (int)Math.Ceiling(newWidth);
                nHeight = (int)Math.Ceiling(newHeight);

                Bitmap rotatedBmp = new Bitmap(nWidth, nHeight);

                using (Graphics g = Graphics.FromImage(rotatedBmp))
                {
                    g.Clear(Color.White);
                    // This array will be used to pass in the three points that 
                    // make up the rotated srcimg
                    Point[] points;
                    if (locked_theta >= 0.0 && locked_theta < pi2)
                    {
                        points = new Point[] { 
											 new Point( (int) oppositeBottom, 0 ), 
											 new Point( nWidth, (int) oppositeTop ),
											 new Point( 0, (int) adjacentBottom )
										 };

                    }
                    else if (locked_theta >= pi2 && locked_theta < Math.PI)
                    {
                        points = new Point[] { 
											 new Point( nWidth, (int) oppositeTop ),
											 new Point( (int) adjacentTop, nHeight ),
											 new Point( (int) oppositeBottom, 0 )						 
										 };
                    }
                    else if (locked_theta >= Math.PI && locked_theta < (Math.PI + pi2))
                    {
                        points = new Point[] { 
											 new Point( (int) adjacentTop, nHeight ), 
											 new Point( 0, (int) adjacentBottom ),
											 new Point( nWidth, (int) oppositeTop )
										 };
                    }
                    else
                    {
                        points = new Point[] { 
											 new Point( 0, (int) adjacentBottom ), 
											 new Point( (int) oppositeBottom, 0 ),
											 new Point( (int) adjacentTop, nHeight )		
										 };
                    }

                    g.DrawImage(srcimg, points);
                    g.Dispose();                
                }
                //Console.WriteLine((int)angle);
                rotatedimg_table.Add(angle, rotatedBmp);
                
                Rectangle bbx = BBX(rotatedBmp);
                if (max_width < bbx.Width)
                {
                    max_width = bbx.Width;
                    max_width_idx = angle;
                }
            }
        }
        public void kernel(object step)
        {
            int delta = (end - start + 1) / tnum;
            int start_ = delta * (int)step;
            int stop_ = start_ + delta;
            if ((int)step == tnum - 1) stop_ = end + 1;
            for (int i = start_; i < stop_; i += inc)
            {
                //Console.WriteLine("T: " + step + " i " + i + " angle " + (360 - i));
                RotateImage(srcimg_list[(int)step], i);
            }
        }
        public void RLS(int angle)
        {
            Bitmap rotatedimg = (Bitmap)(rotatedimg_table[angle]);
            //Log.WriteBitmap2Debug(rotatedimg, "angel_" + angle + "0");
            bool[,] image = ImageUtils.BitmapToBoolArray2D(rotatedimg,0,250);
            int initial_fg_count = CountFG(image);
            //Log.WriteBitmap2Debug(ImageUtils.ArrayBool2DToBitmap(image), "angel_" + angle+"1");
            image = D(image, alpha);
            //Log.WriteBitmap2Debug(ImageUtils.ArrayBool2DToBitmap(image), "angel_" + angle+"2");
            image = E(image, alpha);
            //Log.WriteBitmap2Debug(ImageUtils.ArrayBool2DToBitmap(image), "angel_" + angle+"3");
            image = E(image, beta);
            //Log.WriteBitmap2Debug(ImageUtils.ArrayBool2DToBitmap(image), "angel_" + angle+"4");
            int finish_fg_count = CountFG(image);
            double r = (double)(finish_fg_count) / (double)(initial_fg_count);
            int c = finish_fg_count - initial_fg_count;
            if (r > max) { max = r; max_idx = angle; }
           // if (c > minc) { minc = c; minc_idx = angle; }
        }
        public void kernel2(object step)
        {
            int delta = (end - start + 1) / tnum;
            int start_ = delta * (int)step;
            int stop_ = start_ + delta;
            if ((int)step == tnum - 1) stop_ = end + 1;
            for (int i = start_; i < stop_; i += inc)
                RLS(i);
        }
        public int[] Apply(int tnum, Bitmap srcimg, int size, int start, int end, int inc)
        {
            this.tnum = tnum;
            this.srcimg = srcimg;
            this.start = start;
            this.end = end;
            this.inc = inc;
            this.width = srcimg.Width;
            this.height = srcimg.Height;
            //Log.WriteBitmap2Debug(srcimg, "skew_src");
            Thread[] thread_array = new Thread[tnum];
            for (int i = 0; i < tnum; i++)
            {
                srcimg_list.Add((Bitmap)srcimg.Clone());
                thread_array[i] = new Thread(new ParameterizedThreadStart(kernel));
                thread_array[i].Start(i);
            }
            for (int i = 0; i < tnum; i++)
                thread_array[i].Join();

            
            alpha = size;// 9;
            int alpha_size = alpha >> 1;
            beta = (int)((double)(max_width) / (Math.Sqrt(2))) - alpha - 1;
            int beta_size = beta >> 1;
            beta = beta_size * 2 + 1;
            if (beta < size * 2) beta = (size * 2) - 1;
            //Console.WriteLine(size + " max_width " + max_width + " alpha " + alpha + " beta " + beta);
            Thread[] thread_array2 = new Thread[tnum];
            for (int i = 0; i < tnum; i++)
            {
                thread_array2[i] = new Thread(new ParameterizedThreadStart(kernel2));
                thread_array2[i].Start(i);
            }
            for (int i = 0; i < tnum; i++)
                thread_array2[i].Join();

         
            int[] rt = new int[2];
            rt[0] = max_idx;
            rt[1] = 180 + max_idx;

            RotateImage(srcimg,max_idx);
            RotateImage(srcimg,180+max_idx);

            return rt;
        }
        public int CountFG(bool[,] image)
        {
            int mh = image.GetLength(0);
            int mw = image.GetLength(1);
            int count =0;

            for (int i = 0; i < mw; i++)
                for (int j = 0; j < mh; j++)
                    if (image[j, i]) count++;
                    return count;
        }
        public bool[,] D(bool[,] image, int se_size) // 1: hit 0: non-hit
        {
            int mh = image.GetLength(0);
            int mw = image.GetLength(1);
            int r = se_size >> 1;
            bool[,] tmp = new bool[mh, mw];

            for (int i = 0; i < mw; i++)
                for (int j = 0; j < mh; j++)
                {
                    if (!image[j, i])
                    {
                        bool hit = false;
                        for (int w = -r; w < r; w++)
                        {
                            if ((i + w) >= 0 && (i + w) < mw)
                            {
                                if (image[j, i + w])
                                {
                                    hit = true;
                                    break;
                                }
                            }
                        }
                        tmp[j, i] = hit;
                    }
                    else
                        tmp[j, i] = true;
                }
            return tmp;
        }
        public bool[,] E(bool[,] image, int se_size)
        {
            int mh = image.GetLength(0);
            int mw = image.GetLength(1);
            int r = se_size >> 1;
            bool[,] tmp = new bool[mh, mw];

            for (int i = 0; i < mw; i++)
                for (int j = 0; j < mh; j++)
                {
                    if (image[j, i])
                    {
                        bool hit = true;
                        for (int w = -r; w < r; w++)
                        {
                            if ((i + w) >= 0 && (i + w) < mw)
                            {
                                if (!image[j, i + w])
                                {
                                    hit = false;
                                    break;
                                }
                            }
                            else
                            {
                                hit = false;
                                break;
                            }
                        }
                        tmp[j, i] = hit;
                    }
                    else
                        tmp[j, i] = false;
                }
            return tmp;
        }
        public Rectangle BBX(Bitmap srcImg)
        {
            int minx = srcImg.Width, miny = srcImg.Height, maxx = 0, maxy = 0;

            // get source image size
            int width = srcImg.Width;
            int height = srcImg.Height;

            PixelFormat srcFmt = (srcImg.PixelFormat == PixelFormat.Format8bppIndexed) ?
                PixelFormat.Format8bppIndexed : PixelFormat.Format24bppRgb;

            // lock source bitmap data
            BitmapData srcData = srcImg.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, srcFmt);

            int srcOffset = srcData.Stride - ((srcFmt == PixelFormat.Format8bppIndexed) ? width : width * 3);

            // do the job
            unsafe
            {
                byte* src = (byte*)srcData.Scan0.ToPointer();

                if (srcFmt == PixelFormat.Format8bppIndexed)
                {
                    // graysclae binarization
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++, src++)
                        {
                            if (*src < 128)
                            {
                                if (miny > y) miny = y;
                                if (maxy < y) maxy = y;
                                if (minx > x) minx = x;
                                if (maxx < x) maxx = x;
                            }
                        }
                        src += srcOffset;
                    }
                }
                else
                {
                    byte v;
                    // RGB binarization
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++, src += 3)
                        {
                            v = (byte)(0.2125f * src[RGB.R] + 0.7154f * src[RGB.G] + 0.0721f * src[RGB.B]);
                            if (v < 128)
                            {
                                if (miny > y) miny = y;
                                if (maxy < y) maxy = y;
                                if (minx > x) minx = x;
                                if (maxx < x) maxx = x;
                            }
                        }
                        src += srcOffset;
                    }
                }
            }
            srcImg.UnlockBits(srcData);
            return new Rectangle(minx, miny, maxx - minx + 1, maxy - miny + 1);
        }
    }
}
