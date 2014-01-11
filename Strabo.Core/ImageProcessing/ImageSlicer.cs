using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Strabo.Core.ImageProcessing
{
    public class ImageSlicer
    {
        public List<int[]> xy_offset_list = new List<int[]>();
        public ImageSlicer() { }
        public List<Bitmap> Apply(int row, int col, int overlap, Bitmap srcimg)
        {
            int num = row * col;
            List<Bitmap> results = new List<Bitmap>();
            int width = srcimg.Width;
            int height = srcimg.Height;

            int twidth = width / col + overlap * 2;
            int theight = height / row + overlap * 2;
            for (int j = 0; j < row; j++)
                for (int i = 0; i < col; i++)
                {
                    int row_step = height / row;
                    int col_step = width / col;

                    int x = col_step * i;
                    int y = row_step * j;
                    int xwidth = twidth;
                    int yheight = theight;
                    if (i == col - 1)
                        xwidth = width - x;
                    if (j == row - 1)
                        yheight = height - y;
                    int[] xy_offset = new int[2];
                    xy_offset[0] = x; xy_offset[1] = y;
                    xy_offset_list.Add(xy_offset);
                    Rectangle rect = new Rectangle(x, y, xwidth, yheight);
                    //Crop crop = new Crop(rect);
                    Bitmap tile = srcimg.Clone(rect, srcimg.PixelFormat);
                    results.Add(tile);
                }
            return results;
        }
    }
}
