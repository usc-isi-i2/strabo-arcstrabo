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
    /// <summary>
    ///  Starting class for text recognition
    /// </summary>
    public class TextDetectionWorker
    {
        public bool debug = true;
        public string map_name;
        public int mw;
        public int mh;
        public string output_dir;
        public List<TextString> initial_string_list = new List<TextString>();
        public List<MyConnectedComponentsAnalysisFast.MyBlob> char_blobs;
        public int[] char_labels;
        public double angle_ratio=0.3;//0.19

        public string road_segmented_dir = "";
        public string road_segmented_fn = "";

        int tnum = 1;
        MergeTextStrings mts = new MergeTextStrings();

        public TextDetectionWorker()
        {
        }
        public void DetectOrientation()
        {
            Thread[] thread_array = new Thread[tnum];
            for (int i = 0; i < tnum; i++)
            {
                thread_array[i] = new Thread(new ParameterizedThreadStart(DetectOrientationThread));
                thread_array[i].Start(i);
            }
            for (int i = 0; i < tnum; i++)
                thread_array[i].Join();

        }
        public void DetectOrientationThread(object s)
        {
            int counter = 0;
            int start = (int)s;
            for (int i = start; i < mts.text_string_list.Count; i += tnum)
            {
                if (mts.text_string_list[i].char_list.Count > 2)
                {
                    double avg_size = 0;
                    for (int j = 0; j < mts.text_string_list[i].char_list.Count; j++)
                    {
                        avg_size += mts.text_string_list[i].char_list[j].bbx.Width + mts.text_string_list[i].char_list[j].bbx.Height;
                    }
                    counter++;
                    avg_size /= (double)(mts.text_string_list[i].char_list.Count * 2);
                    MultiThreadsSkewnessDetection mtsd = new MultiThreadsSkewnessDetection();
                    int[] idx = mtsd.Apply(1, mts.text_string_list[i].srcimg, (int)avg_size, 0, 180, 3);

                    if (idx[0] <= 90)
                    {
                        mts.text_string_list[i].orientation_list.Add(idx[0]);
                        mts.text_string_list[i].rotated_img_list.Add((Bitmap)mtsd.rotatedimg_table[idx[0]]);
                        if (Geometry.DiffSlope(idx[0], 90) < 5)
                        {
                            mts.text_string_list[i].orientation_list.Add(idx[1]);
                            mts.text_string_list[i].rotated_img_list.Add((Bitmap)mtsd.rotatedimg_table[idx[1]]);
                        }
                    }
                    else
                    {
                        mts.text_string_list[i].orientation_list.Add(idx[1]);
                        mts.text_string_list[i].rotated_img_list.Add((Bitmap)mtsd.rotatedimg_table[idx[1]]);
                        if (Geometry.DiffSlope(idx[1], 270) < 5)
                        {
                            mts.text_string_list[i].orientation_list.Add(idx[0]);
                            mts.text_string_list[i].rotated_img_list.Add((Bitmap)mtsd.rotatedimg_table[idx[0]]);
                        }
                    }

                }
            }

        }
        public string Apply(string dir, string fn, double size_ratio, bool preprocessing)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (!Directory.Exists(dir + "Results"))
                Directory.CreateDirectory(dir + "Results");
            output_dir = dir+"Results\\";
            Bitmap srcimg = new Bitmap(dir + fn);
            Log.DeleteAll(output_dir);
            Log.SetOutputDir(output_dir);

            // assigning parameters
            mw = srcimg.Width;
            mh = srcimg.Height;
            initial_string_list.Clear();
            char_blobs = null;

            ImageSlicer imgslicer = new ImageSlicer();
            List<TextString> text_string_list = new List<TextString>();
            List<Bitmap> img_list = imgslicer.Apply(4, 4, 100, srcimg);
            //srcimg.Dispose();
            //srcimg= null;

            //Log.SetStartTime();
            for (int s = 0; s < img_list.Count; s++)
            {
                RemoveBoarderCC removeBoarderCC = new RemoveBoarderCC();
                img_list[s] = removeBoarderCC.Apply(img_list[s]);
                ConditionalDilationAutomatic cda = new ConditionalDilationAutomatic();
                cda.ang_threshold = angle_ratio;

                //Log.SetStartTime();
                string outImagePath = output_dir+s+".png";
                cda.Apply(8, img_list[s], size_ratio, angle_ratio, preprocessing, outImagePath);
                Bitmap dilatedimg = new Bitmap(outImagePath);
                //time = Log.GetDurationInSeconds();
                //Log.WriteLine("CDA: "+s+": "+ time);

                //Log.SetStartTime();
                DetectTextStrings detectTS = new DetectTextStrings();
                List<TextString> string_list = detectTS.Apply(img_list[s], dilatedimg);
                //time = Log.GetDurationInSeconds();
                //Log.WriteLine("Detect: " + s + ": " + time);

                List<Bitmap> string_img_list = new List<Bitmap>();
                for (int i = 0; i < string_list.Count; i++)
                    string_img_list.Add(string_list[i].srcimg);
               
                int[] offset = imgslicer.xy_offset_list[s];

                for (int i = 0; i < string_list.Count; i++)
                {
                    string_list[i].x_offset = offset[0];
                    string_list[i].y_offset = offset[1];
                    mts.AddTextString(string_list[i]);
                }
                img_list[s].Dispose();
                img_list[s] = null;
            }
            //time = Log.GetDurationInSeconds();
            //Log.WriteLine("CDA time: "+time);
            //Log.WriteLine("Start to detect orientation");
            //Log.SetStartTime();
            tnum = 8;
            DetectOrientation();
            //time = Log.GetDurationInSeconds();
            //Log.WriteLine("Detect orientation time: " + time);

            //Log.WriteLine("Start to detect short string orientation");
            //Log.SetStartTime();
            for (int i = 0; i < mts.text_string_list.Count; i++)
            {
                if (mts.text_string_list[i].char_list.Count <= 3)
                {
                    List<int> nearest_string_list = findNearestSrings(i, mts.text_string_list);
                    int initial_orientation_count = mts.text_string_list[i].orientation_list.Count;
                    for (int j = 0; j < nearest_string_list.Count; j++)
                        mts.text_string_list[i].orientation_list.AddRange(mts.text_string_list[nearest_string_list[j]].orientation_list);
                    RotateImage rotate = new RotateImage();
                    for (int j = initial_orientation_count; j < mts.text_string_list[i].orientation_list.Count; j++)
                        mts.text_string_list[i].rotated_img_list.Add(rotate.Apply(mts.text_string_list[i].srcimg, (int)mts.text_string_list[i].orientation_list[j]));
                }
                
            }
            //time = Log.GetDurationInSeconds();
            //Log.WriteLine("Detect short string orientation time: " + time);

            //Log.WriteLine("Start to write results");
            //Log.SetStartTime();
            Graphics g = Graphics.FromImage(srcimg);
            for (int i = 0; i < mts.text_string_list.Count; i++)
            {
                List<Bitmap> all_string_img_list = new List<Bitmap>();
                    int x = mts.text_string_list[i].mass_center.X;
                    int y = mts.text_string_list[i].mass_center.Y;
                    for (int s = 0; s < mts.text_string_list[i].orientation_list.Count; s++)
                    {
                        string slope = Convert.ToInt16(mts.text_string_list[i].orientation_list[s]).ToString();
                         ImageStitcher imgstitcher1 = new ImageStitcher();
                         Bitmap single_img = imgstitcher1.ExpandCanvas(mts.text_string_list[i].rotated_img_list[s],20);
                        Log.WriteBitmap2FolderExactFileName(output_dir, single_img, i + "_p_" + mts.text_string_list[i].char_list.Count + "_" + x + "_" + y + "_s_" + slope + "_" + mts.text_string_list[i].bbx.X + "_" + mts.text_string_list[i].bbx.Y + "_" + mts.text_string_list[i].bbx.Width + "_" + mts.text_string_list[i].bbx.Height);
                    }
                    Font font2 = new Font("Arial", 20);
                    //g.DrawString(i.ToString(), font2, Brushes.Black, mts.text_string_list[i].bbx.X,mts.text_string_list[i].bbx.Y);
                    g.DrawRectangle(new Pen(Color.Black, 3), mts.text_string_list[i].bbx);
                    ImageStitcher imgstitcher2 = new ImageStitcher();
                    Bitmap srcimg2 = imgstitcher2.ExpandCanvas(mts.text_string_list[i].srcimg, 20);
                    Log.WriteBitmap2FolderExactFileName(output_dir, srcimg2, i + "_p_" + mts.text_string_list[i].char_list.Count + "_" + x + "_" + y + "_s_0" + "_" + mts.text_string_list[i].bbx.X + "_" + mts.text_string_list[i].bbx.Y + "_" + mts.text_string_list[i].bbx.Width + "_" + mts.text_string_list[i].bbx.Height);
            }
            g.Dispose();
            //time = Log.GetDurationInSeconds();
            //Log.WriteLine("Write results time: " + time);
            return null;
        }
        
        public List<int> findNearestSrings(int num, List<TextString> text_string_list)
        {
            //StreamWriter sw = new StreamWriter(output_dir + "log.txt");
            Rectangle rec = text_string_list[num].bbx;
            int x1 = rec.X;
            int y1 = rec.Y;
            Rectangle bbx = new Rectangle(x1 - rec.Width*2, y1 - rec.Height*2, rec.Width *4, rec.Height * 4);
            List<int> nearest_string_list = new List<int>();
            string line = "";
            for (int i = 0; i < text_string_list.Count; i++)
            {
                if (text_string_list[i].char_list.Count <= 3) continue;
                if (bbx.IntersectsWith(text_string_list[i].bbx))
                {
                    nearest_string_list.Add(i);
                    line += (i + 1) + "@" + text_string_list[i].rotated_img_list.Count +"@";
                }
            }
            //sw.WriteLine((num + 1) + "@" + line);
            //sw.Close();
            return nearest_string_list;
        }
        public Bitmap PrintSubStringBBXonMap(Bitmap srcimg)
        {
            //changed here - ASHISH

            srcimg = ImageUtils.InvertColors(srcimg);
            
            /*
            Invert ivt = new Invert();
            srcimg = ivt.Apply(srcimg);
            */
            
            srcimg = ImageUtils.toRGB(srcimg);
            Graphics g = Graphics.FromImage(srcimg);
            for (int i = 1; i < initial_string_list.Count; i++)
            {
                g.DrawRectangle(new Pen(Color.Red, 6), initial_string_list[i].bbx);
                //Font font = new Font("Arial", 20);
                //g.DrawString(i.ToString(), font, Brushes.Blue, initial_string_list[i].bbx.X, initial_string_list[i].bbx.Y);
                for (int j = 0; j < initial_string_list[i].final_string_list.Count; j++)
                    g.DrawRectangle(new Pen(Color.Green, 4), initial_string_list[i].final_string_list[j].bbx);

                //for (int j = 0; j < initial_string_list[i].char_list.Count; j++)
                //{
                //    Font font2 = new Font("Arial", 20);
                //    g.DrawString(j.ToString(), font2, Brushes.Red, initial_string_list[i].char_list[j].bbx.X, initial_string_list[i].char_list[j].bbx.Y);
                //   // g.DrawRectangle(new Pen(Color.Yellow, 2), initial_string_list[i].char_list[j].bbx);
                //}

            }
            g.Dispose();
            return srcimg;
        }
        public Bitmap PrintSubStringNumonMap(Bitmap srcimg)
        {
            //ASHISH
            srcimg = ImageUtils.InvertColors(srcimg);
            /*
            Invert ivt = new Invert();
            srcimg = ivt.Apply(srcimg);
            */
            srcimg = ImageUtils.toRGB(srcimg);
            Graphics g = Graphics.FromImage(srcimg);
            for (int i = 1; i < initial_string_list.Count; i++)
            {
                for (int j = 0; j < initial_string_list[i].char_list.Count; j++)
                {
                    int x = 0;
                    Font font2 = new Font("Arial", 20);
                    g.DrawString(j.ToString(), font2, Brushes.Red, initial_string_list[i].char_list[j].bbx.X, initial_string_list[i].char_list[j].bbx.Y);
                    // g.DrawRectangle(new Pen(Color.Yellow, 2), initial_string_list[i].char_list[j].bbx);
                }
            }
            g.Dispose();
            return srcimg;
        }
        public Bitmap PrintSubStringsSmall(TextString ts)
        {
            bool[,] stringimg = new bool[ts.bbx.Height + 100, ts.bbx.Width + 100];
            for (int i = 0; i < ts.char_list.Count; i++)
            {
                for (int xx = ts.bbx.X; xx < ts.bbx.X + ts.bbx.Width; xx++)
                    for (int yy = ts.bbx.Y; yy < ts.bbx.Y + ts.bbx.Height; yy++)
                    {
                        if (char_labels[yy * mw + xx] == ts.char_list[i].pixel_id)
                            stringimg[yy - ts.bbx.Y + 50, xx - ts.bbx.X + 50] = true;
                        //else
                        //   stringimg[yy - ts.bbx.Y, xx - ts.bbx.X] = false;
                    }

            }
            ts.srcimg = ImageUtils.ArrayBool2DToBitmap(stringimg); ;
            return ts.srcimg;
        }
        public Bitmap PrintSubStringsSmall(TextString ts, int margin)
        {
            bool[,] stringimg = new bool[ts.bbx.Height + margin, ts.bbx.Width + margin];
            for (int i = 0; i < ts.char_list.Count; i++)
            {
                for (int xx = ts.bbx.X; xx < ts.bbx.X + ts.bbx.Width; xx++)
                    for (int yy = ts.bbx.Y; yy < ts.bbx.Y + ts.bbx.Height; yy++)
                    {
                        if (char_labels[yy * mw + xx] == ts.char_list[i].pixel_id)
                            stringimg[yy - ts.bbx.Y + margin / 2, xx - ts.bbx.X + margin / 2] = true;
                        //else
                        //   stringimg[yy - ts.bbx.Y, xx - ts.bbx.X] = false;
                    }

            }
            ts.srcimg = ImageUtils.ArrayBool2DToBitmap(stringimg); ;
            return ts.srcimg;
        }

    }
}
