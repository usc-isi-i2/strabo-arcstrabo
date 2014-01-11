using System;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Text;
using Strabo.Core.Utility;
using Strabo.Core.ImageProcessing;

namespace Strabo.Core.TextLayerExtraction
{
    public class TextLayerExtractionTrainer
    {
        public Hashtable global_road_color_hash = new Hashtable();
        public List<Hashtable> label_text_color_hash_list = new List<Hashtable>();
        public Hashtable global_all_non_road_color_hash = new Hashtable();
        public Hashtable global_used_non_road_color_hash = new Hashtable();

        HashSet<int> negtive_color = new HashSet<int>();
        public int map_type = 0;
        public int label_count = 0;
        public int char_spacing = 0;
        public List<Bitmap> n_img_list = new List<Bitmap>();
        public TextLayerExtractionTrainer() { }

        public Bitmap Apply(Bitmap srcimg, List<Bitmap> img_list, List<Bitmap> n_img_list)
        {
            return srcimg;
        }
        public List<Bitmap> GUIProcessOneLayerOnly(Bitmap srcimg, List<Bitmap> img_list, List<Bitmap> n_img_list, int tnum)
        {
            Log.DeleteAllDebug();
            for (int i = 0; i < n_img_list.Count; i++)
                GenerateColorFromNegativeExamples(n_img_list[i]);

            List<Bitmap> bitmap_list = new List<Bitmap>();
            for (int i = 0; i < img_list.Count; i++)
            {
                label_text_color_hash_list.Add(new Hashtable());
                int label_type = GenerateColorFromPositiveExamples(img_list[i], i);
                MultiThreadBinarization mtb = new MultiThreadBinarization();
                img_list[i] = mtb.ApplyWithSrcColor(img_list[i], label_text_color_hash_list[i], tnum);
            }

            MultiThreadBinarization mtb2 = new MultiThreadBinarization();
            bitmap_list.Add(mtb2.Apply(srcimg, global_road_color_hash, tnum));
            return bitmap_list;
        }
        public void GenerateColorFromNegativeExamples(Bitmap srcimg)
        {
            ColorHistogram color_histogram = new ColorHistogram();
            HashSet<int> color = color_histogram.GetColorHashSet(srcimg);
            foreach (int c in color)
                negtive_color.Add(c);
        }
        public void AddRGBtoGlobalHash(int rgb, int label_number)
        {
            if (!InGlobalNonRoadHash(rgb))
            {
                if (!label_text_color_hash_list[label_number].ContainsKey(rgb))
                    label_text_color_hash_list[label_number].Add(rgb, rgb);
                if (!global_road_color_hash.ContainsKey(rgb))
                    global_road_color_hash.Add(rgb, rgb);
            }
        }
        public bool InGlobalNonRoadHash(int rgb)
        {
            if (negtive_color.Contains(rgb))
                return true;
            else
                return false;
        }
        public int GenerateColorFromPositiveExamples(Bitmap srcimg, int label_number)
        {
            // case 0: non-solid chars and solid bg
            // case 1: non-solid chars and bg
            // case 2: solid chars and bg
            // case 3: solid chars and non-solid bg
            int mw = srcimg.Width;
            int mh = srcimg.Height;
            double fg_count_th = 0.05;
            ColorDecompositionFast cdf = new ColorDecompositionFast();
            List<Bitmap> decomposed_img_list = cdf.Apply(srcimg);
            int fg_pixel_count = mw * mh;
            // check for case 0 and 1
            for (int i = 0; i < decomposed_img_list.Count; i++)
            {
                int rgb = (int)cdf.fg_idx_color_hash[i];
                if (negtive_color.Contains(rgb)) fg_pixel_count -= cdf.fg_count_list[i];
            }
            // add all colors into global hash except the ones in neg lables
            if (fg_pixel_count < (double)(mw * mh) * fg_count_th * 3)
            {
                //Console.WriteLine("Too few fg pixels: " + fg_pixel_count);
                for (int i = 0; i < decomposed_img_list.Count; i++)
                    AddRGBtoGlobalHash((int)cdf.fg_idx_color_hash[i], label_number);
                return 0;
            }
            else
            {
                double[] avg_hough_distance = new double[decomposed_img_list.Count];
                double min_dist = Double.MaxValue;
                int min_idx = -1;
                for (int i = 0; i < decomposed_img_list.Count; i++)
                {
                    Log.WriteBitmap2Debug(decomposed_img_list[i], "label" + label_count + "_color_" + i.ToString());
                    int rgb = (int)cdf.fg_idx_color_hash[i];
                    if (InGlobalNonRoadHash(rgb))
                        continue;

                    if (cdf.fg_count_list[i] < (double)(mw * mh) * fg_count_th)
                        continue;

                    avg_hough_distance[i] = RunRunLengthSmoothing(decomposed_img_list[i], i);
                    //Console.WriteLine(i + "avg distance: " + avg_hough_distance[i]);
                    if (avg_hough_distance[i] < min_dist) { min_dist = avg_hough_distance[i]; min_idx = i; }
                }

                fg_pixel_count = 0;
                if (min_idx != -1) // found a solid color for chars
                {
                    int road_color_by_hough = (int)cdf.fg_idx_color_hash[min_idx];
                    AddRGBtoGlobalHash(road_color_by_hough, label_number);
                    fg_pixel_count += cdf.fg_count_list[min_idx];
                    // search for nearby colors in the YIQ domain
                    for (int i = 0; i < decomposed_img_list.Count; i++)
                    {
                        if (i == min_idx) continue;
                        int rgb = (int)cdf.fg_idx_color_hash[i];
                        if (InGlobalNonRoadHash(rgb)) continue;

                        if (ColorDistance(road_color_by_hough, rgb))
                        {
                            AddRGBtoGlobalHash(rgb, label_number);
                            fg_pixel_count += cdf.fg_count_list[i];
                        }
                    }
                }
                //Console.WriteLine("Fg pixels after Hough: " + (double)fg_pixel_count / (double)(mw * mh));
                if (fg_pixel_count < (double)(mw * mh) * fg_count_th * 3)
                {

                    for (int i = 0; i < decomposed_img_list.Count; i++)
                        AddRGBtoGlobalHash((int)cdf.fg_idx_color_hash[i], label_number);
                    return 0;
                }
                else if (fg_pixel_count < (double)(mw * mh) * fg_count_th * 10)
                    return 2;
                else
                    return 1; //partial
            }
        }
        public double RunRunLengthSmoothing(Bitmap srcimg, int i)
        {
            int mw = srcimg.Width;
            int mh = srcimg.Height;
            Bitmap hough_img = (Bitmap)(srcimg.Clone());
            hough_img = ImageUtils.toGray(hough_img);

            RunLengthSmoothing rlt = new RunLengthSmoothing();
            rlt.dir = CurrentState.debug_dir;
            rlt.num = i;
            int size = mw / mh;
            hough_img = rlt.Apply(hough_img, size);
            if (rlt.remaining_fp == 0)
                return Double.MaxValue;
            else
                return 1 / (double)(rlt.remaining_fp);// GetAVGDist(i, hough_img, line_list, 1);
        }

        public bool ColorDistance(int rgb1, int rgb2)
        {
            float[] yiq1 = ImageUtils.RGB2YIQ(rgb1);
            float[] yiq2 = ImageUtils.RGB2YIQ(rgb2);

            double distance = Math.Sqrt((yiq1[0] - yiq2[0]) * (yiq1[0] - yiq2[0]) + (yiq1[1] - yiq2[1]) * (yiq1[1] - yiq2[1]) +
            (yiq1[2] - yiq2[2]) * (yiq1[2] - yiq2[2]));

            if (distance < 2) return true;
            else return false;
        }
    }
}
