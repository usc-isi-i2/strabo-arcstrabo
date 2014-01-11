using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Strabo.Core.ImageProcessing
{
    public class ConnectingGap
    {
        public ConnectingGap() { }
        public Bitmap Apply(Bitmap srcimg)
        {
            bool[,] image = ImageUtils.BitmapToBoolArray2D(srcimg, 0);
            bool[,] tmp = new bool[srcimg.Height,srcimg.Width];
            for (int i = 1; i < srcimg.Width - 1; i++)
                for (int j = 1; j < srcimg.Height - 1; j++)
                {
                    if (image[j, i] == false)
                    {
                        tmp[j,i]=image[j,i];
                        //if (j == 40 && i == 46)
                        //    Console.WriteLine("debug");
                        bool[] marks = new bool[8];
                        int nei_count = 0;
                        // 0 1 2
                        // 3 x 4
                        // 5 6 7
                        int count = 0;
                        for (int r = -1; r < 2; r++)
                            for (int c = -1; c < 2; c++)
                            {

                                if (r == 0 && c == 0) 
                                    continue;
                                
                                if (image[j + r, i + c]) 
                                    count++;
                                marks[nei_count] = image[j + r, i + c];
                                nei_count++;
                            }
                        //if (count == 2 || count ==3)
                        {
                            if ((marks[0] && marks[7]) || (marks[1] && marks[6]) ||
                                (marks[2] && marks[5]) || (marks[3] && marks[4]))
                                tmp[j, i] = true;
                        }
                      
                    }
                        else 
                            tmp[j,i]=image[j,i];
                }
            for (int i = 1; i < srcimg.Width - 1; i++)
                for (int j = 1; j < srcimg.Height - 1; j++)
                    image[j, i] = tmp[j, i];
            return ImageUtils.ArrayBool2DToBitmap(image);
        }
    }
}
