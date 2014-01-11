using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using KdKeys.DataMining.Clustering.KMeans;
using Strabo.Core.ImageProcessing;

namespace Strabo.Core.ColorSegmentation
{
    public class MyKMeans
    {
        int[] pixels;

        public class RGB
        {
            public const short B = 0;
            public const short G = 1;
            public const short R = 2;
        }

        Hashtable color_table = new Hashtable();
        public MyKMeans() { }
        public void RGBData(ColorHistogram colorHist, double[][] data)
        {
            int size = colorHist.getNumberOfColors();

            for (int x = 0; x < size; x++)
            {
                data[x] = new double[3];
                int rgb = colorHist.getColor(x);
                data[x][0] = ((rgb & 0xFF0000) >> 16);
                data[x][1] = ((rgb & 0xFF00) >> 8);
                data[x][2] = (rgb & 0xFF);
            }
        }
        public void YIQData(ColorHistogram colorHist, double[][] data)
        {
            int size = colorHist.getNumberOfColors();

            for (int x = 0; x < size; x++)
            {
                data[x] = new double[3];
                int rgb = colorHist.getColor(x);
                double[] yiq = RGB2YIQ(rgb);

                for (int y = 0; y < 3; y++)
                {
                    data[x][y] = yiq[y];
                }
            }
        }
        public string Apply(int k, string fn, string outImagePath)
        {
            Bitmap srcimg = new Bitmap(fn);
            pixels = ImageUtils.BitmapToArray1DIntRGB(srcimg);
            ColorHistogram colorHist = new ColorHistogram(pixels, false);
            int size = colorHist.getNumberOfColors();
            if (size <= k)
                return fn;

            double[][] data = new double[size][];

            RGBData(colorHist, data);
            ClusterCollection clusters;
            KMeansParallel kMeans = new KMeansParallel();
            //clusters = kMeans.ClusterDataSet(k, data);
            clusters = kMeans.ClusterDataSetRandomSeeding(k, data);
            PaintRGB(srcimg, kMeans, clusters).Save(outImagePath, ImageFormat.Png);
            return outImagePath;
        }

        public Bitmap PaintRGB(Bitmap srcimg, KMeansParallel kMeans, ClusterCollection clusters)
        {
            int width = srcimg.Width;
            int height = srcimg.Height;
            BitmapData srcData = srcimg.LockBits(
               new Rectangle(0, 0, width, height),
               ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int srcOffset = srcData.Stride - width * 3;
            unsafe
            {
                byte* src = (byte*)srcData.Scan0.ToPointer();
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++, src += 3)
                    {
                        int rgb = src[RGB.R] * 256 * 256 + src[RGB.G] * 256 + src[RGB.B];
                        int rgb_mean;

                        if (color_table.ContainsKey(rgb))
                            rgb_mean = (int)color_table[rgb];
                        else
                        {
                            double[] yiq = new double[3];
                            yiq[0] = src[RGB.R]; yiq[1] = src[RGB.G]; yiq[2] = src[RGB.B];

                            double min_dist = Double.MaxValue;
                            int idx = 0;

                            for (int i = 0; i < clusters.Count; i++)
                            {
                                double dist = kMeans.EuclideanDistance(yiq, clusters[i].ClusterMean);
                                if (dist < min_dist)
                                {
                                    min_dist = dist;
                                    idx = i;
                                }
                            }
                            rgb_mean = (int)(clusters[idx].ClusterMean[0]) * 256 * 256 +
                                (int)(clusters[idx].ClusterMean[1]) * 256 +
                                (int)(clusters[idx].ClusterMean[2]);

                            color_table.Add(rgb, rgb_mean);
                        }
                        src[RGB.R] = (byte)((rgb_mean & 0xFF0000) >> 16);
                        src[RGB.G] = (byte)((rgb_mean & 0xFF00) >> 8);
                        src[RGB.B] = (byte)(rgb_mean & 0xFF);
                    }
                    src += srcOffset;
                }
            }
            srcimg.UnlockBits(srcData);
            return srcimg;
        }
        public Bitmap PaintYIQ(Bitmap srcimg, KMeansParallel kMeans, ClusterCollection clusters)
        {
            int width = srcimg.Width;
            int height = srcimg.Height;
            BitmapData srcData = srcimg.LockBits(
               new Rectangle(0, 0, width, height),
               ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int srcOffset = srcData.Stride - width * 3;
            unsafe
            {
                byte* src = (byte*)srcData.Scan0.ToPointer();
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++, src += 3)
                    {
                        int rgb = src[RGB.R] * 256 * 256 + src[RGB.G] * 256 + src[RGB.B];
                        int rgb_mean;

                        if (color_table.ContainsKey(rgb))
                            rgb_mean = (int)color_table[rgb];
                        else
                        {
                            double[] yiq = RGB2YIQ(rgb);
                            double min_dist = Double.MaxValue;
                            int idx = 0;

                            for (int i = 0; i < clusters.Count; i++)
                            {
                                double dist = kMeans.EuclideanDistance(yiq, clusters[i].ClusterMean);
                                if (dist < min_dist)
                                {
                                    min_dist = dist;
                                    idx = i;
                                }
                            }
                            rgb_mean = YIQ2RGB(clusters[idx].ClusterMean[0], clusters[idx].ClusterMean[1], clusters[idx].ClusterMean[2]);
                            color_table.Add(rgb, rgb_mean);
                        }
                        src[RGB.R] = (byte)((rgb_mean & 0xFF0000) >> 16);
                        src[RGB.G] = (byte)((rgb_mean & 0xFF00) >> 8);
                        src[RGB.B] = (byte)(rgb_mean & 0xFF);
                    }
                    src += srcOffset;
                }
            }
            srcimg.UnlockBits(srcData);
            return srcimg;
        }
        public double[] RGB2YIQ(int rgb)
        {
            double Rc = ((rgb & 0xFF0000) >> 16);
            double Gc = ((rgb & 0xFF00) >> 8);
            double Bc = (rgb & 0xFF);
            double[] yiq = new double[3];
            yiq[0] = 0.299f * Rc + 0.587f * Gc + 0.114f * Bc; // Y
            yiq[1] = 0.5957f * Rc - 0.2744f * Gc - 0.3212f * Bc; // I
            yiq[2] = 0.2114f * Rc - 0.5226f * Gc + 0.3111f * Bc; // Q
            return yiq;
        }
        public int YIQ2RGB(double Yc, double Ic, double Qc)
        {
            return Convert.ToInt32((Yc + 0.9563f * Ic + 0.6210f * Qc) * 256 * 256 +
            (Yc - 0.2721f * Ic - 0.6473f * Qc) * 256 +
            (Yc - 1.1070f * Ic + 1.7046f * Qc));
        }
    }
}
