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


                    Console.Write("Size is :"+srcimg.Size);
                //    Console.Read();
                    //Crop crop = new Crop(rect);
                    Bitmap tile = srcimg.Clone(rect, srcimg.PixelFormat);
                    


                    results.Add(tile);
                }
            return results;
        }
    }
}
