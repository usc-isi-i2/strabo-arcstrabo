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
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Strabo.Core.Utility;
using System.Threading;
using Strabo.Core.ImageProcessing;

namespace Strabo.Core.ImageProcessing
{
    public class ImageStatistics
    {
        public int PixelsCount=0;
        public int PixelsCountWithoutBlack = 0;
        
        int Height, Width;
        int tnum;
        BitmapData srcData;

        public ImageStatistics()
        { }
        public void Calculate(Bitmap srcimg, int tnum)
        {
           
            if (srcimg.PixelFormat != PixelFormat.Format24bppRgb)
            {
                using (Bitmap tmp = new Bitmap(srcimg.Width, srcimg.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
                using (Graphics gr = Graphics.FromImage(tmp))
                {
                    gr.DrawImage(srcimg, new Rectangle(0, 0, srcimg.Width, srcimg.Height));
                    srcimg = new Bitmap(tmp);
                }
            }
            Height = srcimg.Height;
            Width = srcimg.Width;
            PixelsCount = Height * Width;
            this.tnum = tnum;

            srcData = srcimg.LockBits(
                new Rectangle(0, 0, srcimg.Width, srcimg.Height),
                ImageLockMode.ReadOnly, srcimg.PixelFormat);

            Thread[] thread_array = new Thread[tnum];
            for (int i = 0; i < tnum; i++)
            {
                thread_array[i] = new Thread(new ParameterizedThreadStart(Run));
                thread_array[i].Start(i);
            }
            for (int i = 0; i < tnum; i++)
                thread_array[i].Join();

            srcimg.UnlockBits(srcData);
        }
        public void Run(object step)
        {

            int t = (int)step;
            int offset1 = (srcData.Stride - Width * 3);
            unsafe
            {
                byte* src = (byte*)srcData.Scan0.ToPointer() + t * srcData.Stride;

                // for each row
                for (int y = t; y < Height; y += tnum)
                {
                    // for each pixel
                    for (int x = 0; x < Width; x++, src += 3)
                    {
                        if(src[RGB.R] !=0 || src[RGB.G] !=0 || src[RGB.B]!=0)
                            PixelsCountWithoutBlack++;
                    }
                    src += offset1 + (tnum - 1) * srcData.Stride;
                }
            }

        }
    }
}
