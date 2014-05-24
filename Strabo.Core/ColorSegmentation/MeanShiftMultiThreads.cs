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
using System.Collections;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Strabo.Core.ImageProcessing;

namespace Strabo.Core.ColorSegmentation
{
    public class MeanShiftMultiThreads
    {
        int width;
        int height;
        unsafe byte* src;
        int srcOffset;
        int srcStride;
        int rad;
        int rad2;
        float radCol;
        float radCol2;

        byte[] rgbpixel;
        int tnum;
        Hashtable color_table = new Hashtable();
        object hashlock = new object();
        //Hashtable<ushort,int> color_pa
        int ccnum;
        List<MyConnectedComponentsAnalysisFast.MyBlob> char_blobs;
        double size_ratio;
        double spatial_ratio;
        public class RGB
        {
            public const short B = 0;
            public const short G = 1;
            public const short R = 2;
        }
        public MeanShiftMultiThreads() { }
        ~MeanShiftMultiThreads() { Console.WriteLine("MeanShift Disposed"); }
        public float[] RGB2YIQ(int pos)
        {
            float Rc = rgbpixel[pos + RGB.R];
            float Gc = rgbpixel[pos + RGB.G];
            float Bc = rgbpixel[pos + RGB.B];
            float[] yiq = new float[3];
            yiq[0] = 0.299f * Rc + 0.587f * Gc + 0.114f * Bc; // Y
            yiq[1] = 0.5957f * Rc - 0.2744f * Gc - 0.3212f * Bc; // I
            yiq[2] = 0.2114f * Rc - 0.5226f * Gc + 0.3111f * Bc; // Q
            return yiq;
        }
        /* Parin
        public void kernelcctest(object step)
        {
            int start, stop;
            int length = ccnum;
            int one_step = length / tnum;
            start = (int)step * one_step;
            stop = ((int)step + 1) * one_step;
            if ((int)step == tnum - 1) stop = length;
            double shift = 0;
            int iters = 0;

            int counter = 0;
            for (int i = start; i < stop; i++)
            {
                double xc = char_blobs[i].mass_center.X;
                double yc = char_blobs[i].mass_center.Y;
                double sizec = Math.Max(char_blobs[i].bbx.Width, char_blobs[i].bbx.Height);
                double sizec2 = Math.Min(char_blobs[i].bbx.Width, char_blobs[i].bbx.Height);
                char_blobs[i].mean_shift_results[0] = xc;
                char_blobs[i].mean_shift_results[1] = yc;
                char_blobs[i].mean_shift_results[2] = sizec;
                char_blobs[i].mean_shift_results[3] = sizec2;
            }
        }
        public void kernelcc(object step)
        {
            int start, stop;
            int length = ccnum;
            int one_step = length / tnum;
            start = (int)step * one_step;
            stop = ((int)step + 1) * one_step;
            if ((int)step == tnum - 1) stop = length;
            double shift = 0;
            int iters = 0;

            int counter = 0;
            for (int i = start; i < stop; i++)
            {
                double xc = char_blobs[i].mass_center.X;
                double yc = char_blobs[i].mass_center.Y;
                double sizec = Math.Max(char_blobs[i].bbx.Width, char_blobs[i].bbx.Height);
                double sizec2 = Math.Min(char_blobs[i].bbx.Width, char_blobs[i].bbx.Height);
                double xcOld, ycOld;
                double sizeOld = sizec;
                double sizeOld2 = sizec2;
                iters = 0;
                int num = 0;

                do
                {
                    //Console.WriteLine(xc + " " + yc + " " + sizec);
                    xcOld = xc;
                    ycOld = yc;
                    sizeOld = sizec;
                    sizeOld2 = sizec2;
                    //sizec = sizeOld;
                    double mx = xc;
                    double my = yc;
                    double msize = sizec;
                    double msize2 = sizec2;
                    num = 1;
                    for (int j = 0; j < char_blobs.Count; j++)
                    {
                        if (i == j) continue;
                        int size2 = Math.Max(char_blobs[j].bbx.Width, char_blobs[j].bbx.Height);
                        double r = 0;
                        if (sizec > size2)
                            r = (double)sizec / (double)size2;
                        else
                            r = (double)size2 / (double)sizec;
                        if (r < size_ratio)
                        {
                            double distance =
                                Math.Sqrt((char_blobs[j].mass_center.X - xc) * (char_blobs[j].mass_center.X - xc) +
                                (char_blobs[j].mass_center.Y - yc) * (char_blobs[j].mass_center.Y - yc));
                            if (distance < (sizec + size2) * spatial_ratio)
                            {
                                //Console.Write("and "+j + " ");
                                mx += char_blobs[j].mass_center.X;
                                my += char_blobs[j].mass_center.Y;
                                msize += size2;
                                msize2 += Math.Min(char_blobs[j].bbx.Width, char_blobs[j].bbx.Height);
                                num++;
                            }
                        }
                    }
                    if (num != 1)
                    {
                        double num_ = (double)1 / num;
                        xc = mx * num_;
                        yc = my * num_;
                        sizec = msize * num_;
                        sizec2 = msize2 * num_;
                        double dx = xc - xcOld;
                        double dy = yc - ycOld;
                        double dsize = sizec - sizeOld;
                        double dsize2 = sizec2 - sizeOld2;
                        shift = dx * dx + dy * dy + dsize * dsize + dsize2 * dsize2;
                        iters++;
                    }
                    else
                    {
                        counter++;

                    }
                } while (shift > 3 && num > 1);
                if (i == 0)
                    Console.WriteLine(iters);
                char_blobs[i].mean_shift_results[0] = xc;
                char_blobs[i].mean_shift_results[1] = yc;
                char_blobs[i].mean_shift_results[2] = sizec;
                char_blobs[i].mean_shift_results[3] = sizec2;
            }
        }
        public void kernelccNearest(object step)
        {
            int start, stop;
            int length = ccnum;
            int one_step = length / tnum;
            start = (int)step * one_step;
            stop = ((int)step + 1) * one_step;
            if ((int)step == tnum - 1) stop = length;
            double shift = 0;
            int iters = 0;
            double spatial_shift = 0;
            int counter = 0;
            for (int i = start; i < stop; i++)
            {
                double xc = char_blobs[i].mean_shift_results[0];
                double yc = char_blobs[i].mean_shift_results[1];
                double sizec = char_blobs[i].mean_shift_results[2];

                double xcOld, ycOld;
                double sizeOld = sizec;

                iters = 0;
                int num = 0;

                do
                {
                    xcOld = xc;
                    ycOld = yc;
                    sizeOld = sizec;
                    //sizec = sizeOld;
                    double mx = xc;
                    double my = yc;
                    double msize = sizec;

                    num = 1;
                    for (int j = 0; j < char_blobs.Count; j++)
                    {
                        if (i == j) continue;
                        double size2 = char_blobs[j].mean_shift_results[2];
                        double r = 0;
                        if (sizec > size2)
                            r = (double)sizec / (double)size2;
                        else
                            r = (double)size2 / (double)sizec;
                        if (r < size_ratio)
                        {
                            double distance =
                                Math.Sqrt((char_blobs[j].mean_shift_results[0] - xc) * (char_blobs[j].mean_shift_results[0] - xc) +
                                (char_blobs[j].mean_shift_results[1] - yc) * (char_blobs[j].mean_shift_results[1] - yc));
                            if (distance < (sizec + size2) * spatial_ratio)
                            {
                                mx += char_blobs[j].mean_shift_results[0];
                                my += char_blobs[j].mean_shift_results[1];
                                msize += size2;
                                num++;
                            }
                        }
                    }
                    if (num != 1)
                    {
                        double num_ = (double)1 / num;
                        xc = mx * num_;
                        yc = my * num_;
                        sizec = msize * num_;

                        double dx = xc - xcOld;
                        double dy = yc - ycOld;
                        double dsize = sizec - sizeOld;

                        shift = dx * dx + dy * dy + dsize * dsize;
                        spatial_shift = dx * dx + dy * dy;
                        iters++;
                    }
                    else
                    {
                        counter++;

                    }
                } while (shift > 0.5 && num > 1);//iters < 100 && num > 1);
                char_blobs[i].mean_shift_results2[0] = xc;
                char_blobs[i].mean_shift_results2[1] = yc;
                char_blobs[i].mean_shift_results2[2] = sizec;

            }
            Console.WriteLine(counter++);
        }*/
        public void kernel(object step)
        {
            int start_xy, stop_xy;
            int length = width * height;
            int one_step = length / tnum;
            start_xy = (int)step * one_step;
            stop_xy = ((int)step + 1) * one_step;
            if ((int)step == tnum - 1) stop_xy = length;
            int row_counter = 0;
            for (int y = (int)step; y < height; y += tnum)
                for (int x = 0; x < width; x++)
                {
                    row_counter++;
                    float shift = 0;
                    int iters = 0;
                    int xc = x;
                    int yc = y;
                    int xcOld, ycOld;
                    float YcOld, IcOld, QcOld;
                    int pos = (y * width + x) * 3 + y * srcOffset;
                    float[] yiq = RGB2YIQ(pos);
                    float Yc = yiq[0];
                    float Ic = yiq[1];
                    float Qc = yiq[2];
                    do
                    {
                        xcOld = xc;
                        ycOld = yc;
                        YcOld = Yc;
                        IcOld = Ic;
                        QcOld = Qc;

                        float mx = 0;
                        float my = 0;
                        float mY = 0;
                        float mI = 0;
                        float mQ = 0;
                        int num = 0;

                        for (int ry = -rad; ry <= rad; ry++)
                        {
                            int y2 = yc + ry;
                            if (y2 >= 0 && y2 < height)
                            {
                                for (int rx = -rad; rx <= rad; rx++)
                                {
                                    int x2 = xc + rx;
                                    if (x2 >= 0 && x2 < width)
                                    {
                                        if (ry * ry + rx * rx <= rad2)
                                        {
                                            yiq = RGB2YIQ(y2 * srcStride + x2 * 3);
                                            float Y2 = yiq[0];
                                            float I2 = yiq[1];
                                            float Q2 = yiq[2];

                                            float dY = Yc - Y2;
                                            float dI = Ic - I2;
                                            float dQ = Qc - Q2;

                                            if (dY * dY + dI * dI + dQ * dQ <= radCol2)
                                            {
                                                mx += x2;
                                                my += y2;
                                                mY += Y2;
                                                mI += I2;
                                                mQ += Q2;
                                                num++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        float num_ = 1f / num;
                        Yc = mY * num_;
                        Ic = mI * num_;
                        Qc = mQ * num_;
                        xc = (int)(mx * num_ + 0.5);
                        yc = (int)(my * num_ + 0.5);
                        int dx = xc - xcOld;
                        int dy = yc - ycOld;
                        float dY2 = Yc - YcOld;
                        float dI2 = Ic - IcOld;
                        float dQ2 = Qc - QcOld;

                        shift = dx * dx + dy * dy + dY2 * dY2 + dI2 * dI2 + dQ2 * dQ2;
                        iters++;
                    }
                    while (shift > 3 && iters < 10);

                    int pos2 = pos;
                    unsafe
                    {
                        src[pos2 + RGB.R] = (byte)(Yc + 0.9563f * Ic + 0.6210f * Qc);
                        src[pos2 + RGB.G] = (byte)(Yc - 0.2721f * Ic - 0.6473f * Qc);
                        src[pos2 + RGB.B] = (byte)(Yc - 1.1070f * Ic + 1.7046f * Qc);
                    }
                }
        }
        public string ApplyYIQMT(string fn, int tnum, int spatial_distance, int color_distance, string outImagePath)
        {
            return ApplyYIQMT(new Bitmap(fn), tnum, spatial_distance, color_distance, outImagePath);
        }
        public string ApplyYIQMT(Bitmap srcimg, int tnum, int spatial_distance, int color_distance, string outImagePath)
        {
            try
            {
                this.tnum = tnum;
                width = srcimg.Width;
                height = srcimg.Height;
                BitmapToArray1DRGB(srcimg);
                BitmapData srcData = srcimg.LockBits(
                                new Rectangle(0, 0, width, height),
                                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                unsafe
                {
                    src = (byte*)srcData.Scan0.ToPointer();
                }
                rad = spatial_distance;
                rad2 = rad * rad;
                radCol = (float)(color_distance + 1);
                radCol2 = radCol * radCol;
                Thread[] thread_array = new Thread[tnum];
                for (int i = 0; i < tnum; i++)
                {
                    thread_array[i] = new Thread(new ParameterizedThreadStart(kernel));
                    thread_array[i].Start(i);
                }
                for (int i = 0; i < tnum; i++)
                    thread_array[i].Join();
                srcimg.UnlockBits(srcData);
                srcimg.Save(outImagePath, ImageFormat.Png);
            }
            catch (Exception e)
            { }
            return outImagePath;
        }
        public void BitmapToArray1DRGB(Bitmap srcimg)
        {
            BitmapData srcData = srcimg.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            // Get the address of the first line.
            IntPtr ptr = srcData.Scan0;
            srcStride = srcData.Stride;
            srcOffset = srcData.Stride - width * 3;
            // Declare an array to hold the bytes of the bitmap.
            int bytes = srcStride * height;
            rgbpixel = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbpixel, 0, bytes);
            srcimg.UnlockBits(srcData);
        }
    }
}