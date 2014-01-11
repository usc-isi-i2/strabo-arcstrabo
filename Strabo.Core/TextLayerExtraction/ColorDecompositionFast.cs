using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using Strabo.Core.ImageProcessing;

namespace Strabo.Core.TextLayerExtraction
{
    public class ColorDecompositionFast
    {
        public List<int> fg_count_list = new List<int>();
        public Hashtable fg_color_idx_hash = new Hashtable();
        public Hashtable fg_idx_color_hash = new Hashtable();
        public ColorDecompositionFast() { }
        public List<Bitmap> Apply(Bitmap srcimg)
        {
            int mw = srcimg.Width;
            int mh = srcimg.Height;
            int[] pixels = ImageUtils.BitmapToArray1DIntRGB(srcimg);
            ColorHistogram colorHist = new ColorHistogram(pixels);
            int[] color_array = colorHist.getColorArray();
            List<bool[,]> img_list = new List<bool[,]>();
            for (int i = 0; i < color_array.Length; i++)
            {
                img_list.Add(new bool[mh, mw]);
                fg_count_list.Add(0);
                fg_idx_color_hash.Add(i, color_array[i]);
                fg_color_idx_hash.Add(color_array[i], i);
            }
            
            for(int i=0;i<mw;i++)
                for (int j = 0; j < mh; j++)
                {
                    int idx = (int)fg_color_idx_hash[pixels[j * mw + i]];
                    (img_list[idx])[j,i] = true;
                    fg_count_list[idx]++;
                }
          
            List<Bitmap> bitmap_list = new List<Bitmap>();
            for (int i = 0; i < img_list.Count; i++)
                bitmap_list.Add(ImageUtils.ArrayBool2DToBitmap(img_list[i]));
            return bitmap_list;
        }
        public void ApplySaveInd(Bitmap srcimg, string dir, string fn)
        {
            int mw = srcimg.Width;
            int mh = srcimg.Height;
            int[] pixels = ImageUtils.BitmapToArray1DIntRGB(srcimg);
            ColorHistogram colorHist = new ColorHistogram(pixels);
            int[] color_array = colorHist.getColorArray();
            //List<bool[,]> img_list = new List<bool[,]>();
            //for (int i = 0; i < color_array.Length; i++)
            //{
            //    img_list.Add(new bool[mh, mw]);
            //    fg_count_list.Add(0);
            //    fg_idx_color_hash.Add(i, color_array[i]);
            //    fg_color_idx_hash.Add(color_array[i], i);
            //}
            for (int c = 0; c < color_array.Length; c++)
            {
                int rgb = color_array[c];
                bool[,] bool_img = new bool[mh, mw];
                for (int i = 0; i < mw; i++)
                    for (int j = 0; j < mh; j++)
                    {
                        if(pixels[j * mw + i]==rgb)
                            bool_img[j, i] = true;
                    }
                ImageUtils.ArrayBool2DToBitmap(bool_img).Save(dir + fn+c+".png");
            }
           
        }
    }
}
