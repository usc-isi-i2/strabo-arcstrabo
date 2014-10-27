using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Strabo.Core.ImageProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Threading;

using Emgu.CV.Structure;
using Emgu.CV;

using Strabo.Core.Utility;
using Strabo.Core.ImageProcessing;




namespace Strabo.Core.TextDetection
{
    class DetectTextStringsFarsi
    {

         int width, height;
        int min_width = 10, min_height = 10;
        int max_width = 500, max_height = 500;
        Font font2 = new Font("Arial", 10);
        public DetectTextStringsFarsi() { }
       
        public List<TextString> Apply(Bitmap srcimg, Bitmap dilatedimg)
        {
            width = srcimg.Width;
            height = srcimg.Height;
            max_width = width / 2;
            max_height = height / 2;
            //ashish
            
            srcimg = ImageUtils.ConvertGrayScaleToBinary(srcimg, threshold: 128);
            srcimg = ImageUtils.InvertColors(srcimg);
            dilatedimg = ImageUtils.ConvertGrayScaleToBinary(dilatedimg, threshold: 128);
            dilatedimg = ImageUtils.InvertColors(dilatedimg);

            MyConnectedComponentsAnalysisFast.MyBlobCounter char_bc = new MyConnectedComponentsAnalysisFast.MyBlobCounter();
            List<MyConnectedComponentsAnalysisFast.MyBlob> char_blobs = char_bc.GetBlobs(srcimg);
          //  List<MyConnectedComponentsAnalysisFast.MyBlob> char_blobs2 = char_bc.GetBlobs(srcimg);


            ushort[] char_labels = char_bc.objectLabels;
            
            MyConnectedComponentsAnalysisFast.MyBlobCounter string_bc = new MyConnectedComponentsAnalysisFast.MyBlobCounter();
            List<MyConnectedComponentsAnalysisFast.MyBlob> string_blobs = string_bc.GetBlobs(dilatedimg);
            ushort[] string_labels = string_bc.objectLabels;

            List<TextString> initial_string_list = new List<TextString>();

            int string_count = string_blobs.Count;
            for (int i = 0; i < string_count; i++)
            {
                initial_string_list.Add(new TextString());
                initial_string_list.Last().mass_center = string_blobs[i].mass_center;
            }

            for (int i = 0; i < char_blobs.Count; i++)
            {
                if (char_blobs[i].bbx.Width > 1 && char_blobs[i].bbx.Height > 1)
                {
                    char_blobs[i].string_id = string_labels[char_blobs[i].sample_y * width + char_blobs[i].sample_x] - 1;
                    initial_string_list[char_blobs[i].string_id].AddChar(char_blobs[i]);
                }
            }
/*
            String BlobInside = "";
            ////Added Part by Narges Honarvar Nazari ////////
          String  Output_dir = @"C:\Users\nhonarva\Desktop\strabohome\output\";
          Bitmap srcimg1 = new Bitmap(@"C:\Users\nhonarva\Desktop\MySamples\T-crop.png");
            ////////////  Show Blobs ///////////////////
            for (int i = 0; i < char_blobs.Count; i++)
            {
                Graphics g = Graphics.FromImage(srcimg1);
                g.DrawString(i.ToString(), font2, Brushes.Red, char_blobs[i].bbx.X, char_blobs[i].bbx.Y);
                g.DrawRectangle(new Pen(Color.Black, 3), char_blobs[i].bbx);
                Log.WriteBitmap2FolderExactFileName(Output_dir, srcimg1, "debug22.png");

            }


            ////////////  Show Blobs ///////////////////


            //////(char_blobs[i].string_id != char_blobs[j].string_id) 
          
            /////// Investigating which blobs can be dot ////////
            for (int i = 0; i < char_blobs.Count; i++)
            {
                BlobInside = BlobInside +"Bigger Blob is : "+ i+Environment.NewLine;
                
                  for (int j = 0; j < char_blobs.Count; j++)
                            {

                      
                                if (((i != j) && (char_blobs[j].bbx.X + char_blobs[j].bbx.Width >= char_blobs[i].bbx.X) &&
                                    (char_blobs[j].bbx.Y + char_blobs[j].bbx.Height >= char_blobs[i].bbx.Y) && (char_blobs[j].bbx.X + char_blobs[j].bbx.Width <= char_blobs[i].bbx.X + char_blobs[i].bbx.Width) &&
                                    (char_blobs[j].bbx.Y + char_blobs[j].bbx.Height <= char_blobs[i].bbx.Y + char_blobs[i].bbx.Height))
                                    || ((i != j) && (char_blobs[j].bbx.X >= char_blobs[i].bbx.X) &&
                                    (char_blobs[j].bbx.Y >= char_blobs[i].bbx.Y) && (char_blobs[j].bbx.X <= char_blobs[i].bbx.X + char_blobs[i].bbx.Width) &&
                                    (char_blobs[j].bbx.Y <= char_blobs[i].bbx.Y + char_blobs[i].bbx.Height))
                                    || ((i != j) && (char_blobs[j].bbx.X + char_blobs[j].bbx.Width >= char_blobs[i].bbx.X) &&
                                    (char_blobs[j].bbx.Y >= char_blobs[i].bbx.Y) && (char_blobs[j].bbx.X + char_blobs[j].bbx.Width <= char_blobs[i].bbx.X + char_blobs[i].bbx.Width) &&
                                    (char_blobs[j].bbx.Y <= char_blobs[i].bbx.Y + char_blobs[i].bbx.Height))
                                  || ((i != j) && (char_blobs[j].bbx.X  >= char_blobs[i].bbx.X) &&
                                    (char_blobs[j].bbx.Y + char_blobs[j].bbx.Height >= char_blobs[i].bbx.Y) && (char_blobs[j].bbx.X  <= char_blobs[i].bbx.X + char_blobs[i].bbx.Width) &&
                                    (char_blobs[j].bbx.Y + char_blobs[j].bbx.Height <= char_blobs[i].bbx.Y + char_blobs[i].bbx.Height)))
                                {


                                    BlobInside = BlobInside + j+ Environment.NewLine;
                                 
/*
                                    Rectangle FirstRectangle = new Rectangle();
                                    FirstRectangle.Location = new Point(char_blobs[i].bbx.X, char_blobs[i].bbx.Y);
                                    FirstRectangle.Size = new Size(char_blobs[i].bbx.Width, char_blobs[i].bbx.Height);

                                    Rectangle SecondRectangle = new Rectangle();
                                    SecondRectangle.Location = new Point(char_blobs[j].bbx.X, char_blobs[j].bbx.Y);
                                    SecondRectangle.Size = new Size(char_blobs[j].bbx.Width, char_blobs[j].bbx.Height);

                                 //            if (FirstRectangle.IntersectsWith(SecondRectangle))
                                  //          {


                                    

                                    int FirstBlobWidthSize = char_blobs[i].bbx.Width;
                                    int FirstBlobHeightSize = char_blobs[i].bbx.Height;
                                    int SecondBlobWidthSize = char_blobs[j].bbx.Width;
                                    int SecondBlobHeigthSize = char_blobs[j].bbx.Height;
                                    int FirstBlobSize= 0;
                                    int SecondBlobSize = 0;
                                    int SizeRatio = 0;

                                    if (FirstBlobHeightSize > FirstBlobWidthSize)
                                        FirstBlobSize = FirstBlobHeightSize;
                                    else
                                        FirstBlobSize = FirstBlobWidthSize;

                                    if (SecondBlobHeigthSize > SecondBlobWidthSize)
                                        SecondBlobSize = SecondBlobHeigthSize;

                                    else
                                        SecondBlobSize = SecondBlobWidthSize;

                                    SizeRatio=FirstBlobSize/SecondBlobWidthSize;

                                    if (SizeRatio<25)

                                        initial_string_list[char_blobs[i].string_id].AddChar(char_blobs[j]);
*/
/*            
                                    if ((char_blobs[j].bbx.Width < 10 || char_blobs[j].bbx.Height < min_height))
                                    {
                                        initial_string_list[char_blobs[i].string_id].AddChar(char_blobs[j]);
                                        char_blobs2[j].string_id = 33333;
                                    }
                                    //      }

                                    //          initial_string_list[char_blobs[i].string_id].AddChar(char_blobs[j]);




                                    // initial_string_list[char_blobs[i].string_id].AddChar(char_blobs[j]);
                                }
                }
            }



            System.IO.File.WriteAllText(@"C:\Users\nhonarva\Desktop\MySamples\WriteText.txt", BlobInside);

            BlobInside = "";

            for (int i = 0; i < initial_string_list.Count; i++)
            {
                BlobInside = BlobInside + "Bigger Blob is : " + i + Environment.NewLine;

                for (int j = 0; j < char_blobs2.Count; j++)
                {


                    if (( (char_blobs2[j].bbx.X + char_blobs2[j].bbx.Width >= initial_string_list[i].bbx.X) &&
                        (char_blobs2[j].bbx.Y + char_blobs2[j].bbx.Height >= initial_string_list[i].bbx.Y) && (char_blobs2[j].bbx.X + char_blobs2[j].bbx.Width <= initial_string_list[i].bbx.X + initial_string_list[i].bbx.Width) &&
                        (char_blobs2[j].bbx.Y + char_blobs2[j].bbx.Height <= initial_string_list[i].bbx.Y + initial_string_list[i].bbx.Height))
                        || ((char_blobs2[j].bbx.X >= initial_string_list[i].bbx.X) &&
                        (char_blobs2[j].bbx.Y >= initial_string_list[i].bbx.Y) && (char_blobs2[j].bbx.X <= initial_string_list[i].bbx.X + initial_string_list[i].bbx.Width) &&
                        (char_blobs2[j].bbx.Y <= initial_string_list[i].bbx.Y + initial_string_list[i].bbx.Height))
                        || ((char_blobs2[j].bbx.X + char_blobs2[j].bbx.Width >= initial_string_list[i].bbx.X) &&
                        (char_blobs2[j].bbx.Y >= initial_string_list[i].bbx.Y) && (char_blobs2[j].bbx.X + char_blobs2[j].bbx.Width <= initial_string_list[i].bbx.X + initial_string_list[i].bbx.Width) &&
                        (char_blobs2[j].bbx.Y <= initial_string_list[i].bbx.Y + initial_string_list[i].bbx.Height))
                      || ((char_blobs2[j].bbx.X >= initial_string_list[i].bbx.X) &&
                        (char_blobs2[j].bbx.Y + char_blobs2[j].bbx.Height >= initial_string_list[i].bbx.Y) && (char_blobs2[j].bbx.X <= initial_string_list[i].bbx.X + initial_string_list[i].bbx.Width) &&
                        (char_blobs2[j].bbx.Y + char_blobs2[j].bbx.Height <= initial_string_list[i].bbx.Y + initial_string_list[i].bbx.Height)))
                    {


                        BlobInside = BlobInside + j + Environment.NewLine;


                        if ((char_blobs2[j].string_id != 33333) && (char_blobs2[j].bbx.Width < 13) && (char_blobs2[j].bbx.Height <13))
                        {
                            initial_string_list[i].AddChar(char_blobs2[j]);
                        }


                   //     initial_string_list[i].AddChar(char_blobs[j]);
                    }
                }
            }
            
            
            System.IO.File.WriteAllText(@"C:\Users\nhonarva\Desktop\MySamples\WriteText1.txt", BlobInside);


            
         
            /////// Investigating which blobs can be dot ////////

            ////Added Part by Narges Honarvar Nazari ////////
*/
            for (int i = 0; i < initial_string_list.Count; i++)
            {
                if ((initial_string_list[i].char_list.Count == 0) )
                {
                    initial_string_list.RemoveAt(i);
                    
                    i--;
                }
            }
 
            for (int i = 0; i < initial_string_list.Count; i++)
            {
                PrintSubStringsSmall(char_labels, initial_string_list[i], 0);
            }
            return initial_string_list;
        }
        public void PrintSubStringsSmall(ushort[] char_labels, TextString ts, int margin)
        {
            bool[,] stringimg = new bool[ts.bbx.Height + margin, ts.bbx.Width + margin];
            for (int i = 0; i < ts.char_list.Count; i++)
            {
                for (int xx = ts.bbx.X; xx < ts.bbx.X + ts.bbx.Width; xx++)
                    for (int yy = ts.bbx.Y; yy < ts.bbx.Y + ts.bbx.Height; yy++)
                    {
                        if (char_labels[yy * width + xx] == ts.char_list[i].pixel_id)
                            stringimg[yy - ts.bbx.Y + margin / 2, xx - ts.bbx.X + margin / 2] = true;
                    }

            }
            if(ts.char_list.Count >0 )
            ts.srcimg = ImageUtils.ArrayBool2DToBitmap(stringimg);
         
        }
    }
}
