using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using Strabo.Core.DataType;
using Strabo.Core.TextDetection;


namespace Strabo.Core.ImageProcessing
{
    public class LineOfBestFit // minimize the verticle distance between pixels and the line function
    {
        // y = m*x+b
        public double m=0;
        public double b=0;
        public Rectangle bbx=new Rectangle(0,0,0,0);
        //n = number of points
        //Sx = the SUM of all the X coordinates
        //Sy = the SUM of all the Y coordinates
        //Sxy = the SUM of all (x * y)
        //Sx2 = the SUM of all (x * x) 
        bool[,] floodfill;
        int n=0; 
        Int64 Sx=0, Sy=0, Sxy=0, Sx2=0;
        public int pixels = 0;
        public int max_pixels = 10;//20;
        public int min_pixels = 1;
        public int x1 = 10000, y1 = 10000, x2 = 0, y2 = 0;
        
        //M = (n *Sxy - Sx * Sy) / (n * Sx2 - Sx * Sx)
        //B = (Sy - M * Sx) / n 
        //public void Print()
        //{
        //    int width = floodfill.GetLength(1);
        //    int height = floodfill.GetLength(0);

        //    Bitmap rimage = new Bitmap(width, height);
        //    for (int y = 0; y < height; y++)
        //    {
        //        for (int x = 0; x < width; x++)
        //        {
        //            if (floodfill[y, x] < 0)
        //                rimage.SetPixel(x, y, Color.White);
        //            if (floodfill[y, x] == 0)
        //                rimage.SetPixel(x, y, Color.Red);
        //            if (floodfill[y, x] > 0)
        //                rimage.SetPixel(x, y, Color.FromArgb(floodfill[y, x], floodfill[y, x], floodfill[y, x]));
        //        }
        //    }
        //    rimage.Save(@"D:\yaoyic\Data\wwwroot\output\extend2.bmp");
        //}
        public LineOfBestFit(bool[,] floodfill) { this.floodfill = floodfill; }
        public LineOfBestFit() { }
        /*
        public double Update(RoadPoint rp)
        {
            return Update(rp.x,rp.y);
        }*/
        public double Update(double x, double y)
        {
            return Update(Convert.ToInt16(x), Convert.ToInt16(y));
        }
        public double Update(int x, int y)
        {
            if (x1 == 10000)
            {
                x1 = x;
                y1 = y;
            }
            x2 = x;
            y2 = y;
            if (bbx.Width == 0)
                bbx = new Rectangle(x, y, 1, 1);
            else
            {
                int xx = bbx.X;
                int yy = bbx.Y;
                int xx2 = bbx.X + bbx.Width;
                int yy2 = bbx.Y + bbx.Height;

                if (x < xx) xx = x;
                if (x > xx2) xx2 = x;
                if (y > yy2) yy2 = y;
                if (y < yy) yy = y;
                bbx = new Rectangle(xx, yy, xx2 - xx, yy2 - yy);
            }
            
            y = -1 * y;

            Sx += x;
            Sy += y;
            Sxy += (x * y);
            Sx2 += (x * x);
            n++;
            m = (double)(n * Sxy - Sx * Sy) / (double)(n * Sx2 - Sx * Sx);
            //b = (double)(Sy - (double)(m * (double)Sx)) / (double)(n);
            b = (double)(Sy - (double)(m * (double)Sx)) / (double)(n);

            //m = (double)(x2 - x1) / (double)(y2 - y1);
            //Console.WriteLine(m+";"+Sxy+";"+Sx+";"+Sy+";"+Sx2+";"+Sx*Sx);
            double alpha = Math.Atan(m);
            if(alpha < 0 ) alpha+=360;
            return alpha;
        }
        public double GetOrientation()
        {
            if (bbx.Width <= 3)
            {
                return 90;
            }
            else if (bbx.Height == 1||Double.IsNaN(m))
            {
                
                return 0;
            }
            else
            {
                //double mm = Math.Abs
                //if (m > 0)
                {
                    double alpha = 180 / Math.PI * Math.Atan(m);
                    if (alpha < 0) alpha += 180;
                    if (alpha >= 360) alpha = 0;
                    return alpha;
                }
                //else // (m < 0)
                //{
                //    double alpha = 180 / Math.PI * Math.Atan(-1 * m);

                //    return (90 + 90 - alpha);
                //}
            }
        }
        public void FloodFill8(int x, int y, bool newColor, bool oldColor)
        {
            if (x >= 0 && x < floodfill.GetLength(1) && y >= 0 && y < floodfill.GetLength(0) 
                && floodfill[y, x] == oldColor && pixels < max_pixels)
            {
                pixels++;
                floodfill[y, x] = newColor; //set color before starting recursion!
                Update(x,floodfill.GetLength(0)- y);
                FloodFill8(x + 1, y, newColor, oldColor);
                FloodFill8(x - 1, y, newColor, oldColor);
                FloodFill8(x, y + 1, newColor, oldColor);
                FloodFill8(x, y - 1, newColor, oldColor);
                FloodFill8(x + 1, y + 1, newColor, oldColor);
                FloodFill8(x - 1, y - 1, newColor, oldColor);
                FloodFill8(x - 1, y + 1, newColor, oldColor);
                FloodFill8(x + 1, y - 1, newColor, oldColor);

                if (x < x1)
                    x1 = x;
                if (x > x2)
                    x2 = x;
                if (y < y1)
                    y1 = y;
                if (y > y2)
                    y2 = y;
            }
        }
        public LineFunction ComputeLine(int height)
        {
            bbx = new Rectangle(x1, y1, x2 - x1 + 1, y2 - y1 + 1);
            if (bbx.Width == 1)
            {
                //b.orientation = 90;
                m = Double.NaN;
                b = x1; // x = 4
            }
            else if (bbx.Height == 1)
            {
                //b.orientation = 180;
                m = 0;
                b = height - y1; // y = 4
            }
            if (pixels < min_pixels)
                return null;
            else
            {
                LineFunction line = new LineFunction(m, b, GetOrientation());
                return line;
            }
        }
    }
}
