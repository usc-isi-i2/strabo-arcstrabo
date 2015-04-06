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
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using Strabo.Core.Utility;

namespace Strabo.Core.ImageProcessing
{
    public class MultiThreadBinarization
    {
        Bitmap srcimg;
        Bitmap dstimg;
        int Height;
        int Width;
        BitmapData srcData;
        BitmapData dstData;
        int tnum;
        Hashtable positive_table;
        bool use_src_color = false;
        public Bitmap ApplyWithSrcColor(Bitmap srcimg, Hashtable positive_table,int tnum)
        {
            use_src_color = true;
            return Apply(srcimg, positive_table, tnum);
        }
        public Bitmap Apply(Bitmap srcimg, Hashtable positive_table,int tnum)
        {
            this.srcimg = ImageUtils.AnyToFormat24bppRgb(srcimg);

            Height = srcimg.Height;
            Width = srcimg.Width;
            this.tnum = tnum;
            this.positive_table = positive_table;

            dstimg = new Bitmap(srcimg.Width, srcimg.Height, PixelFormat.Format24bppRgb);

            srcData = srcimg.LockBits(
                new Rectangle(0, 0, srcimg.Width, srcimg.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            dstData = dstimg.LockBits(
                new Rectangle(0, 0, srcimg.Width, srcimg.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            Thread[] thread_array = new Thread[tnum];
            for (int i = 0; i < tnum; i++)
            {
                thread_array[i] = new Thread(new ParameterizedThreadStart(Run));
                thread_array[i].Start(i);
            }
            for (int i = 0; i < tnum; i++)
                thread_array[i].Join();

            srcimg.UnlockBits(srcData);
            dstimg.UnlockBits(dstData);
            return dstimg;
        }
        public void Run(object step)
        {
            int t = (int)step;
            int offset1 =(srcData.Stride - Width * 3);
            int offset2 = (dstData.Stride - Width * 3);
            // do the job
            unsafe
            {
                byte* src = (byte*)srcData.Scan0.ToPointer() + t * srcData.Stride;
                byte* dst = (byte*)dstData.Scan0.ToPointer() + t * dstData.Stride;

                // for each row
                for (int y = t; y < Height; y+=tnum)
                {
                    // for each pixel
                    for (int x = 0; x < Width; x++, src += 3, dst += 3)
                    {
                        int color_index = src[RGB.R] * 256 * 256 + src[RGB.G] * 256 + src[RGB.B];
                        if (positive_table.ContainsKey(color_index))
                        {
                            if (use_src_color)
                            {
                                dst[RGB.R] = src[RGB.R];
                                dst[RGB.G] = src[RGB.G];
                                dst[RGB.B] = src[RGB.B];
                            }
                            else
                            {
                                dst[RGB.R] = 0;
                                dst[RGB.G] = 0;
                                dst[RGB.B] = 0;
                            }
                        }
                        else
                        {
                            dst[RGB.R] = 255; //for display only, needed to be changed latter
                            dst[RGB.G] = 255;
                            dst[RGB.B] = 255;
                        }
                    }
                    src += offset1 + (tnum - 1) * srcData.Stride;
                    dst += offset2 + (tnum - 1) * dstData.Stride;
                }
            }
           
        }
    }
}
