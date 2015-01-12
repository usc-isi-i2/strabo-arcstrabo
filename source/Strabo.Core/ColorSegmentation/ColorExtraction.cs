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
using Emgu.CV.Structure;
using Strabo.Core.ImageProcessing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace Strabo.Core.ColorSegmentation
{
    public class ColorExtraction
    {
        public int applyOld(String inputName, String saveName)
        {

            Emgu.CV.Image<Bgr, int> img = new Emgu.CV.Image<Bgr, int>(inputName);

            Bgr d = new Bgr(0, 0, 0);
            List<Bgr> colorCat = new List<Bgr>();

            for (int i = 0; i < img.Data.GetLength(0); i++)
            {
                for (int j = 0; j < img.Data.GetLength(1); j++)
                {

                    Bgr b = new Bgr(img[i, j].Blue, img[i, j].Green, img[i, j].Red);

                    if (!colorCat.Contains(b))
                    {
                        colorCat.Add(b);
                    }

                }
            }

            for (int k = 0; k < colorCat.Count; k++)
            {
                Emgu.CV.Image<Bgr, int> imgS = new Emgu.CV.Image<Bgr, int>(img.Width, img.Height);
                for (int i = 0; i < img.Data.GetLength(0); i++)
                {
                    for (int j = 0; j < img.Data.GetLength(1); j++)
                    {

                        Bgr b = new Bgr(img[i, j].Blue, img[i, j].Green, img[i, j].Red);

                        if (colorCat[k].Blue == b.Blue && colorCat[k].Green == b.Green && colorCat[k].Red == b.Red)
                        {
                            imgS[i, j] = b;
                        }
                        else
                        {
                            imgS[i, j] = d;
                        }

                    }
                }

                imgS.Save(String.Format(saveName, k));

            }

            return colorCat.Count;

        }

        public Color getColor(int num)
        {
            int r = num / (256 * 256);
            int b = (num - (r * 256 * 256)) / 256;
            int g = num - (r * 256 * 256) - (b * 256);

            Color c = Color.FromArgb(r, g, b);

            return c;
        }

        public Dictionary<int, Color> ApplyFast(String inputName, String saveName)
        {
            Dictionary<int, Color> tbl = new Dictionary<int, Color>();

            Bitmap srcimg = new Bitmap(inputName);

            List<int> fg_count_list = new List<int>();
            Hashtable fg_color_idx_hash = new Hashtable();

            int mw = srcimg.Width;
            int mh = srcimg.Height;
            int[] pixels = ImageUtils.BitmapToArray1DIntRGB(srcimg);
            ColorHistogram colorHist = new ColorHistogram(pixels);
            int[] color_array = colorHist.getColorArray();
            List<int[,]> img_list = new List<int[,]>();
            List<Bitmap> bitmap_list = new List<Bitmap>();
            for (int i = 0; i < color_array.Length; i++)
            {
                bitmap_list.Add(new Bitmap(mw, mh));
                img_list.Add(new int[mh, mw]);
                fg_count_list.Add(0);
                fg_color_idx_hash.Add(color_array[i], i);
                tbl.Add(i, getColor(color_array[i]));
            }

            for (int i = 0; i < mw; i++)
            {
                for (int j = 0; j < mh; j++)
                {
                    int idx = (int)fg_color_idx_hash[pixels[j * mw + i]];
                    bitmap_list[idx].SetPixel(i, j, getColor(pixels[j * mw + i]));
                    fg_count_list[idx]++;
                }
            }

            for (int k = 0; k < img_list.Count; k++)
            {
                bitmap_list[k].Save(string.Format(saveName, k));
            }

            return tbl;

        }
 
    }
}