using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Strabo.Core.Utility;
using Strabo.Core.ImageProcessing;

namespace Strabo.Core.TextLayerExtraction
{
    public class RunLengthSmoothing
    {
        public string dir;
        public bool debug = false;
        public int num = 0;
        public int remaining_fp = 0;
        public int fg_count = 0;
        public RunLengthSmoothing() { }

        public Bitmap Apply(Bitmap srcimg, int char_count_in_string)
        {
            int width = srcimg.Width;
            if (char_count_in_string == 0) return null;
            int size = srcimg.Width / char_count_in_string;

            Bitmap scaledimg = new Bitmap(srcimg.Width + size * 3, srcimg.Height + size * 3);
            Graphics g = Graphics.FromImage(scaledimg);
            g.Clear(Color.White);
            g.DrawImage(srcimg, new Point((int)(size * 1.5), (int)(size * 1.5)));
            g.Dispose();

            int alpha = size;// 9;
            int alpha_size = alpha >> 1;
            int beta = (int)((double)(width) / (Math.Sqrt(2))) - alpha - 1;
            int beta_size = beta >> 1;
            beta = beta_size * 2 + 1;
            if (beta < size * 2) beta = (size * 2) - 1;
            //Console.WriteLine(" alpha: " + alpha + " beta: " + beta);

            ImageStatistics istat = new ImageStatistics();
            istat.Calculate(scaledimg,4);

            if (debug) Console.WriteLine(num+" 0: " + (istat.PixelsCount - istat.PixelsCountWithoutBlack));
            if (debug) if (debug) Log.WriteBitmap2Debug(scaledimg, num + "_0.png");
            bool[,] image = ImageUtils.BitmapToArray2D(scaledimg, 250, 1);
            image = D(image, alpha);
            scaledimg = ImageUtils.Array2DToBitmap(image, 1);
            istat.Calculate(scaledimg,4);
            if (debug) Console.WriteLine(num + " 1: " + (istat.PixelsCount - istat.PixelsCountWithoutBlack));
            if (debug) Log.WriteBitmap2Debug(scaledimg, num+"_1.png");
            scaledimg = ImageUtils.Array2DToBitmap(image, 1);
            image = ImageUtils.BitmapToArray2D(scaledimg, 250, 1);
            image = E(image, alpha);
            scaledimg = ImageUtils.Array2DToBitmap(image, 1);
            scaledimg = ImageUtils.Array2DToBitmap(image);
            istat.Calculate(scaledimg, 4);

            if (debug) Console.WriteLine(num + " 2: " + (istat.PixelsCount - istat.PixelsCountWithoutBlack));

            if (debug) Log.WriteBitmap2Debug(scaledimg, num + "_2.png");
            scaledimg = ImageUtils.Array2DToBitmap(image, 1);
            image = ImageUtils.BitmapToArray2D(scaledimg, 250, 1);
            image = E(image, alpha);
            scaledimg = ImageUtils.Array2DToBitmap(image);
            if (debug) Log.WriteBitmap2Debug(scaledimg, num + "_3.png");
            istat.Calculate(scaledimg, 4);
            if (debug) Console.WriteLine(num + " 3: " + (istat.PixelsCount - istat.PixelsCountWithoutBlack));
            remaining_fp = istat.PixelsCount - istat.PixelsCountWithoutBlack;
            return scaledimg;
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
                    if (image[j, i] == false)
                    {
                        bool hit = false;
                        for (int w = -r; w < r; w++)
                        {
                            if ((i + w) >= 0 && (i + w) < mw)
                            {
                                if (image[j, i + w] == true)
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
                    if (image[j, i] == true)
                    {
                        bool hit = true;
                        for (int w = -r; w < r; w++)
                        {
                            if ((i + w) >= 0 && (i + w) < mw)
                            {
                                if (image[j, i + w] == false)
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
    }
}
