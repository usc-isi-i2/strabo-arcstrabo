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
