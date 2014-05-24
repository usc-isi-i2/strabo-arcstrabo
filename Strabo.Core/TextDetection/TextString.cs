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
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Strabo.Core.ImageProcessing;


namespace Strabo.Core.TextDetection
{
    public class TextString
    {
        public bool debug = true;
        public int id;
        public int pixel_id;
        public int char_width;
        public Rectangle bbx = new Rectangle(0, 0, 0, 0);
        public double mean_width=0;
        public double mean_height = 0;
        public int string_count;
        public List<MyConnectedComponentsAnalysisFast.MyBlob> char_list = new List<MyConnectedComponentsAnalysisFast.MyBlob>();
        public List<TextString> after_break_string_list = new List<TextString>();
        public List<TextString> final_string_list = new List<TextString>();
        public bool needsplit = false;
        List<int[,]> charimg_list = new List<int[,]>();
        List<int[,]> ivt_charimg_list = new List<int[,]>();
        int smaller_angle_threshold;
        int larger_angle_threshold;
        bool net = false;
        public string path = "";
        public Bitmap srcimg;
        public List<Bitmap> rotated_img_list = new List<Bitmap>();
        public List<double> orientation_list = new List<double>();

        public bool passed_first_run = false;
        public List<string> recognized_text_list = new List<string>();
        public double orientation = 0;

        public List<int> suspiciousSymbolsCount_list = new List<int>();
        //Count of all unrecognized symbols in layout
        public List<int> unrecognizedSymbolsCount_list = new List<int>();
        //Count of all nonspace symbols in layout
        public List<int> allSymbolsCount_list = new List<int>();
        //Count of all words in layout
        public List<int> allWordsCount_list = new List<int>();
        //Count of all not dictionary word in layout
        public List<int> notDictionaryWordsCount_list = new List<int>();

        public int x_offset = 0;
        public int y_offset = 0;

        public Point mass_center;
        public TextString() { }
        public double ShortestDistance(MyConnectedComponentsAnalysisFast.MyBlob char_blob)
        {
            double min_distance = Double.MaxValue;
            Point a = char_blob.mass_center;
            for (int i = 0; i < char_list.Count; i++)
            {
                Point b = char_list[i].mass_center;
                double distance = Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
                if (distance < min_distance)
                    min_distance = distance;
            }
            return min_distance;
        }
        public double Distance(Point a, Point b)
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }
        public void AddChar(MyConnectedComponentsAnalysisFast.MyBlob char_blob)
        {
            if (char_list.Contains(char_blob)) return;
            for (int i = 0; i < char_list.Count; i++)
                if (Distance(char_list[i].mass_center, char_blob.mass_center) < 3)
                    return;
            mean_height = (mean_height * char_list.Count + char_blob.bbx.Height);
            mean_width = (mean_width * char_list.Count + char_blob.bbx.Width);
            char_list.Add(char_blob);
            mean_height /= (double)char_list.Count;
            mean_width /= (double)char_list.Count;
            // Extend bbx
            if (bbx.Width == 0)
                bbx = char_blob.bbx;
            else
            {
                int x = bbx.X, y = bbx.Y, xx = bbx.X + bbx.Width - 1, yy = bbx.Y + bbx.Height - 1;
                int x1 = char_blob.bbx.X, y1 = char_blob.bbx.Y, xx1 = char_blob.bbx.X + char_blob.bbx.Width - 1, yy1 = char_blob.bbx.Y + char_blob.bbx.Height - 1;

                int x2, y2, xx2, yy2;

                if (x < x1) x2 = x;
                else x2 = x1;
                if (y < y1) y2 = y;
                else y2 = y1;

                if (xx < xx1) xx2 = xx1;
                else xx2 = xx;
                if (yy < yy1) yy2 = yy1;
                else yy2 = yy;

                bbx.X = x2; bbx.Y = y2;
                bbx.Width = xx2 - x2 + 1;
                bbx.Height = yy2 - y2 + 1;

            }
           

        }
        public int GetLargeCharCount()
        {
            int c=0;
            for (int i = 0; i < char_list.Count; i++)
                if (char_list[i].sizefilter_included)
                    c++;
            return c;
        }
        public void BBXConnect(int min_dist, int char_width, int[] labels, int label_width, int label_height)
        {
            int char_count = char_list.Count;

            for (int i = 0; i < char_count; i++)
            {
                if (char_list[i].bbx.Width < char_width / 2 && char_list[i].bbx.Height < char_width / 2) continue;
                for (int j = i + 1; j < char_count; j++)
                {

                    if (char_list[j].bbx.Width < char_width / 2 && char_list[j].bbx.Height < char_width / 2) continue;

                    int x1 = char_list[i].bbx.X;
                    int y1 = char_list[i].bbx.Y;
                    bool overlap = false;
                    if (char_list[i].bbx.IntersectsWith(char_list[j].bbx))
                        overlap = true;
                    else
                    {
                        Rectangle rect = new Rectangle(x1 - min_dist, y1 - min_dist, char_list[i].bbx.Width + min_dist * 2, char_list[i].bbx.Height + min_dist * 2);
                        if (rect.IntersectsWith(char_list[j].bbx))
                            overlap = true;
                    }
                    //double dist = Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));

                    //if (y1 == y2)
                    //{
                    //    dist -= char_list[i].bbx.Width / 2;
                    //    dist -= char_list[j].bbx.Width / 2;
                    //}
                    //else if (x1 == x2)
                    //{
                    //    dist -= char_list[i].bbx.Height / 2;
                    //    dist -= char_list[j].bbx.Height / 2;
                    //}
                    //else
                    //{
                    //    double dy = Math.Abs(y1 - y2);
                    //    double dx = Math.Abs(x1 - x2);

                    //    double w1 = char_list[i].bbx.Width / 2;
                    //    double h1 = w1 * dy / dx;

                    //    double w2 = char_list[j].bbx.Width / 2;
                    //    double h2 = w1 * dy / dx;

                    //}
                    if (overlap)//(dist <= min_dist)
                    {
                        char_list[i].neighbors.Add(j);
                        char_list[j].neighbors.Add(i);
                        char_list[i].neighbor_count++;
                        char_list[j].neighbor_count++;
                    }
                }
            }
            for (int i = 0; i < char_count; i++)
            {
                if (char_list[i].neighbors.Count > 2) needsplit = true;
                if (debug)
                {
                    Console.Write("NE: " + i + "--- ");

                    for (int j = 0; j < char_list[i].neighbors.Count; j++)
                        Console.Write(char_list[i].neighbors[j] + " ");
                    Console.WriteLine();
                }
            }
        }
        public void Split(int min_dist, int char_width, int[] labels, int label_width, int label_height, int smaller_angle_threshold, int larger_angle_threshold)
        {
            this.smaller_angle_threshold = smaller_angle_threshold;
            this.larger_angle_threshold = larger_angle_threshold;
            // Marking small CCs (might be dots or noise)
            int cc = 0;
            for (int i = 0; i < char_list.Count; i++)
            {
                if (char_list[i].bbx.Width < char_width / 2 && char_list[i].bbx.Height < char_width / 2)
                    char_list[i].sizefilter_included = false;
                else cc++;
            }
            // If the string has fewer than 3 CCs (non-small CCs), mark as "short string"
            if (cc <= 3) { Convert2FinalStringList(); return; }

            // Applying distance transformation (distance between characters)
            Connect(min_dist, char_width, labels, label_width, label_height);
            // Checking for "net" structure
            int connecting_node_count = 0;
            for (int i = 0; i < char_list.Count; i++)
            {
                if (char_list[i].neighbors.Count > 2)
                    connecting_node_count++;
                if (debug)
                {
                    Console.Write("NE: " + i + "--- ");
                    for (int j = 0; j < char_list[i].neighbors.Count; j++)
                        Console.Write(char_list[i].neighbors[j] + " ");
                    Console.WriteLine();
                }
            }
            if (connecting_node_count > char_list.Count / 2)
            { net = true; Convert2FinalStringList(); return; }
            // If no character has more than 2 connections and any set of three connected characters constitutes an acute angle, send the string to break
            if (connecting_node_count == 0)
                BreakIt(smaller_angle_threshold); // loose threshold
            else
                SplitIt();
        }
        public void Connect(int min_dist, int char_width, int[] labels, int label_width, int label_height)
        {
            int char_count = char_list.Count;
            int mw = bbx.Width + 2; int mh = bbx.Height + 2;// 1 pixel border
            List<int[,]> charimg_list = new List<int[,]>();
            List<int[,]> dist_charimg_list = new List<int[,]>();
            DistanceTransformation dist = new DistanceTransformation();
            for (int i = 0; i < char_count; i++)
            {
                int[,] charimg = new int[mh, mw];
                for (int xx = 1; xx < mw - 1; xx++)
                    for (int yy = 1; yy < mh - 1; yy++)
                        if (labels[(yy - 1 + bbx.Y) * label_width + (xx - 1 + bbx.X)] == char_list[i].pixel_id)
                            charimg[yy, xx] = 1; // 1 is fg
                        else
                            charimg[yy, xx] = 0;
                charimg_list.Add(charimg);
                dist_charimg_list.Add(dist.ApplyFGisZero(charimg));
            }
            for (int i = 0; i < char_count; i++)
                for (int j = i + 1; j < char_count; j++)
                {
                    int min = mw * mh;
                    for (int yy = 1; yy < mh - 1; yy++)
                    {
                        int[,] charimg = charimg_list[i];
                        for (int xx = 1; xx < mw - 1; xx++)
                        {
                            int[,] dist_charimg = dist_charimg_list[j];
                            if (charimg[yy, xx] == 1 && min > dist_charimg[yy, xx])
                                min = dist_charimg[yy, xx];
                        }
                    }
                    if (min <= min_dist)
                    {


                        if ((!char_list[j].sizefilter_included && !char_list[i].sizefilter_included) || //S S
                            (char_list[j].sizefilter_included && char_list[i].sizefilter_included))
                        {
                            char_list[i].neighbors.Add(j);
                            char_list[i].neighbor_count++;

                            char_list[j].neighbors.Add(i);
                            char_list[j].neighbor_count++;
                        }
                        else
                        {
                            if (!char_list[i].sizefilter_included) // S B
                            {
                                char_list[i].neighbors.Add(j);
                                char_list[i].neighbor_count++;
                            }
                            else
                            {
                                char_list[j].neighbors.Add(i);
                                char_list[j].neighbor_count++;
                            }
                        }
                    }
                }
        }
        public void BreakIt(int min_angel)
        {
            List<int> tmp_list4one = new List<int>();
            for (int i = 0; i < char_list.Count; i++)
            {
                if (char_list[i].sizefilter_included == false)
                {
                    tmp_list4one.Add(i); continue;
                }
                char_list[i].split_visited = 1;
                if (char_list[i].neighbor_count == 2)
                {
                    if(debug) Console.WriteLine("Break test: Points");
                    double angel = CosAngel(char_list[i].neighbors[0], char_list[i].neighbors[1], i);
                    if (debug) Console.WriteLine("Break test: Idx " + char_list[i].neighbors[0] + " " + i + " " + char_list[i].neighbors[1] + " _   " + angel);
                   
                    if (angel < min_angel) // acute
                    {
                        char_list[i].split_visited++;
                        char_list[i].split_here = true;
                        if(debug) Console.WriteLine("Break here @ " + i);
                    }
                    else
                        if(debug) Console.WriteLine("Break test passed");
                }
            }
            List<List<int>> tmp_list4two = new List<List<int>>();
            for (int i = 0; i < char_list.Count; i++)
            {
                if (char_list[i].sizefilter_included == false || char_list[i].split_visited <= 0 || char_list[i].split_here == true)
                    continue;
                List<int> substring_list = new List<int>();
                if(debug) Console.Write(" BreakItTrace: " );
                BreakItTrace(substring_list, i);
                if (debug) Console.WriteLine();
                
                if (substring_list.Count == 2)
                {
                    tmp_list4two.Add(substring_list);
                    if(debug) Console.WriteLine("                 Add tmp2ts:     " + substring_list[0] + "   " + substring_list[1]);
                }
                else
                    AddSubstring(substring_list);
            }
            MergeTwo(tmp_list4two);
            MergeOne(tmp_list4one);
        }
        public void BreakItTrace(List<int> substring_list, int i)
        {
            if (char_list[i].split_visited <= 0) return;
            if (debug) Console.Write(i + " ");
            substring_list.Add(i);
            char_list[i].split_visited--;
            if (char_list[i].split_here != true)
                for (int j = 0; j < char_list[i].neighbors.Count; j++)
                    BreakItTrace(substring_list, char_list[i].neighbors[j]);
        }
        public void MergeOne(List<int> tmp_list4one)
        {
            if (debug) Console.WriteLine("*******Merge One******");
            for (int i = 0; i < tmp_list4one.Count; i++)
            {
                // for small cc only since small cc still has neighbors
                int idx1 = tmp_list4one[i];
                MyConnectedComponentsAnalysisFast.MyBlob char1 = char_list[idx1];
                for (int ni = 0; ni < char1.neighbors.Count; ni++)
                {
                    int idx = char1.neighbors[ni];
                    for (int j = 0; j < final_string_list.Count; j++)
                    {
                        TextString ts = final_string_list[j];
                        if (ts.char_list.Contains(char_list[idx]))
                        {
                            char_list[idx1].included = true;
                            ts.AddChar(char_list[idx1]);
                            if (debug) Console.WriteLine("                 Add TS: " + idx1 + " to TS: " + j);
                        }
                    }
                }
                if (debug) Console.WriteLine(idx1 + " is not a small cc");
                if (!char_list[idx1].included)
                {
                    char_list[idx1].included = true;
                    List<int> tmp_list = new List<int>();
                    tmp_list.Add(idx1);
                    AddSubstring(tmp_list);
                }
            }
        }
        public void MergeTwo(List<List<int>> tmp_list4two)
        {
            if (debug) Console.WriteLine("identify non-connected two CC, add them to TS");
            for (int i = 0; i < tmp_list4two.Count; i++)
            {
                int idx1 = tmp_list4two[i][0];
                int idx2 = tmp_list4two[i][1];
                if (!char_list[idx1].included && !char_list[idx2].included)
                {
                    char_list[idx1].included = true;
                    char_list[idx2].included = true;
                    AddSubstring(tmp_list4two[i]);
                }
            }
            if (debug) Console.WriteLine("assign other two cc substring to each connected substring");
            for (int i = 0; i < tmp_list4two.Count; i++)
            {
                int idx1 = tmp_list4two[i][0];
                int idx2 = tmp_list4two[i][1];
                if (!char_list[idx1].included || !char_list[idx2].included)
                {
                    int idx = 0;
                    int idx3 = 0;
                    bool addtwo = false;
                    if (char_list[idx1].included && char_list[idx3].sizefilter_included == false)
                    { idx = idx1; idx3 = idx2; }
                    else if (char_list[idx3].included && char_list[idx1].sizefilter_included == false)
                    { idx = idx2; idx3 = idx1; }
                    else addtwo = true;
                    if (addtwo)
                    {
                        char_list[idx1].included = true;
                        char_list[idx2].included = true;
                        AddSubstring(tmp_list4two[i]);
                    }
                    else
                    {
                        for (int j = 0; j < final_string_list.Count; j++)
                        {
                            TextString ts = final_string_list[j];
                            if (ts.char_list.Contains(char_list[idx]))
                            {
                                ts.AddChar(char_list[idx3]);
                                char_list[idx3].included = true;
                                if (debug) Console.WriteLine("Add: " + idx3 + " to " + idx + "\'ts");
                            }
                        }
                    }
                }
                else
                {
                    if (debug) Console.WriteLine(idx1 + "   " + idx2 + "    added already!");

                }
            }
        }
        private void TraceCharacters(List<int> substring_list, int p1, int p3, int min_angle)
        {
            if (char_list[p3].neighbors.Count <= 0) return;

            double max_angle = -1;
            int max_angle_idx = 0;

            if (debug) Console.WriteLine("*****Trace:Char " + p1 + " and " + p3);
            MyConnectedComponentsAnalysisFast.MyBlob char1 = char_list[p3];
            for (int i = 0; i < char1.neighbors.Count; i++)
            {
                int idx = char1.neighbors[i];
                if (char_list[idx].sizefilter_included)
                {
                    double angel = CosAngel(p1, idx, p3);
                    if (angel > max_angle) { max_angle = angel; max_angle_idx = idx; }
                }
            }
            if (max_angle_idx >= 0 && max_angle > min_angle)
            {
                substring_list.Add(max_angle_idx);
                char_list[max_angle_idx].neighbors.Remove(p3);
                char_list[p3].neighbors.Remove(max_angle_idx);
                //TraceCharacters(substring_list, p3, max_angle_idx,min_angel);
                if (debug) Console.WriteLine("     Found: " + max_angle_idx + " Angle: " + max_angle + " Min Angle: " + min_angle);
                if (char_list[p3].split_here || char_list[max_angle_idx].split_here)
                    TraceCharacters(substring_list, p3, max_angle_idx, larger_angle_threshold); // strict
                else TraceCharacters(substring_list, p3, max_angle_idx, smaller_angle_threshold);


            }
            else
                if (debug) Console.WriteLine("     Found none: " + max_angle_idx + " Angle: " + max_angle + " Min Angle: " + min_angle);

        }
        //private int FindNextNeighbor(int p1, int p3)
        //{
        //    MyConnectedComponentsAnalysis.MyBlob char1 = char_list[p3];
        //    for (int i = 0; i < char1.neighbors.Count; i++)
        //    {
        //        int tmp_min_angle = min_angel;
        //        int idx = char1.neighbors[i];
        //        if (char_list[idx].visited == false && idx != p1 && char_list[idx].sizefilter_included)
        //        {
        //            if (char_list[idx].neighbor_count <= 2)
        //                tmp_min_angle = 100;


        //            double angel = CosAngel(p1, idx, p3);
        //            Console.WriteLine(p1 + " " + p3 + " " + idx + " _   " + angel);
        //            if (angel > min_angel)
        //            {
        //                char_list[idx].visited = true;
        //                return idx;
        //            }

        //        }
        //    }
        //    return -1;
        //}
        private int FindNextNeighbor(int c)
        {
            MyConnectedComponentsAnalysisFast.MyBlob char1 = char_list[c];
            for (int i = 0; i < char1.neighbors.Count; i++)
            {
                int idx = char1.neighbors[i];
                //if (char_list[idx].visited == false && char_list[idx].sizefilter_included)
                if (char_list[idx].sizefilter_included)
                {
                    //char_list[idx].visited = true;
                    return idx;
                }
            }
            return -1;
        }

        public void SplitIt()
        {
            if (debug) Console.WriteLine("************ SplitIt *****************");
            List<int> tmp_list4one = new List<int>();
            List<List<int>> tmp_list4two = new List<List<int>>();
            for (int i = 0; i < char_list.Count; i++)
                if (char_list[i].neighbor_count > 2)
                    char_list[i].split_here = true;
            for (int i = 0; i < char_list.Count; i++)
            {
                if (char_list[i].sizefilter_included == false)
                {
                    tmp_list4one.Add(i);
                    if (debug) Console.WriteLine(" Add " + i + " to one");
                    continue;
                }

                int next = FindNextNeighbor(i);
                
                if (next == -1 && char_list[i].included != true) // no neighbor
                {
                    tmp_list4one.Add(i);
                    if (debug) Console.WriteLine(" Add " + i + " to one");
                    continue;
                }
                while(next!=-1)
                {
                    List<int> substring_list = new List<int>();
                    if (debug) Console.WriteLine("     Start to trace from:" + i + " to " + next);

                    substring_list.Add(i);
                    substring_list.Add(next);
                    
                    char_list[i].neighbors.Remove(next);
                    char_list[next].neighbors.Remove(i);
                    if (char_list[next].split_here || char_list[i].split_here)
                        TraceCharacters(substring_list, i, next, larger_angle_threshold); // strict
                    else TraceCharacters(substring_list, i, next, smaller_angle_threshold);
                    if (char_list[next].split_here || char_list[i].split_here)
                        TraceCharacters(substring_list, next, i, larger_angle_threshold);
                    else TraceCharacters(substring_list, next, i, smaller_angle_threshold);

                    if (substring_list.Count == 2)
                    {
                        tmp_list4two.Add(substring_list);
                        if (debug) Console.WriteLine("                 Add tmp2ts:     " + substring_list[0] + "   " + substring_list[1]);
                    }
                    else
                        AddSubstring(substring_list);
                    next = FindNextNeighbor(i);
                }
            }
            //Console.WriteLine("identify non-connected two CC, add them to TS");
            //for (int i = 0; i < tmp_list4two.Count; i++)
            //{
            //    int idx1 = tmp_list4two[i][0];
            //    int idx2 = tmp_list4two[i][1];
            //    if (!char_list[idx1].included && !char_list[idx2].included)
            //    {
            //        char_list[idx1].included = true;
            //        char_list[idx2].included = true;
            //        AddSubstring(tmp_list4two[i]);
            //    }
            //}
            //Console.WriteLine("assign other two cc substring to each connected substring");
            //for (int i = 0; i < tmp_list4two.Count; i++)
            //{
            //    int idx1 = tmp_list4two[i][0];
            //    int idx2 = tmp_list4two[i][1];
            //    if (!char_list[idx1].included || !char_list[idx2].included)
            //    {
            //        int idx = 0;
            //        int idx3 = 0;
            //        if (char_list[idx1].included) { idx = idx1; idx3 = idx2; }
            //        else { idx = idx2; idx3 = idx1; }

            //        for (int j = 0; j < final_string_list.Count; j++)
            //        {
            //            TextString ts = final_string_list[j];
            //            if (ts.char_list.Contains(char_list[idx]))
            //            {
            //                ts.AddChar(char_list[idx3]);
            //                char_list[idx3].included = true;
            //                Console.WriteLine("Add: " + idx3 + " to " + idx + "\'ts");
            //            }
            //        }
            //    }
            //    else
            //    {
            //        Console.WriteLine(idx1 + "   " + idx2 + "    added already!");

            //    }
            //}
            MergeTwo(tmp_list4two);
            MergeOne(tmp_list4one);
            
        }

        private void ResetVisited()
        {
            for (int i = 0; i < char_list.Count; i++)
                char_list[i].visited = false;
        }
        private void ResetIncluded()
        {
            for (int i = 0; i < char_list.Count; i++)
                char_list[i].included = false;
        }
        

        public void Convert2FinalStringList()
        {
            TextString ts = new TextString();
            for (int i = 0; i < char_list.Count; i++)
                ts.AddChar(char_list[i]);
            final_string_list.Add(ts);
        }
        public void AddSubstring(List<int> substring_list)
        {
            if (debug) Console.Write("Final ST#: " + final_string_list.Count + "                 Add TS:     ");
            TextString ts = new TextString();
            for (int i = 0; i < substring_list.Count; i++)
            {
                ts.AddChar(char_list[substring_list[i]]);
                char_list[substring_list[i]].included = true;
                if (debug) Console.Write(substring_list[i] + " ");
            }

            final_string_list.Add(ts);
            if (debug) Console.WriteLine();
        }
        /*                      P3
         *                     / \ 
         *                    /   \
         *                  P1      P2
         */
        private double CosAngel(int idx1, int idx2, int i)
        {
            double error = 0.00001;
            //           Console.WriteLine("**************8CosAngel: " + idx1 + " " + i + " " + idx2);
            Point p3 = new Point(char_list[i].bbx.X + (char_list[i].bbx.Width) / 2, char_list[i].bbx.Y + (char_list[i].bbx.Height) / 2);
            Point p1 = new Point(char_list[idx1].bbx.X + (char_list[idx1].bbx.Width) / 2, char_list[idx1].bbx.Y + (char_list[idx1].bbx.Height) / 2);
            Point p2 = new Point(char_list[idx2].bbx.X + (char_list[idx2].bbx.Width) / 2, char_list[idx2].bbx.Y + (char_list[idx2].bbx.Height) / 2);
            if (debug) Console.WriteLine(p1.X + " " + p1.Y + ";" + p2.X + " " + p2.Y + ";" + p3.X + " " + p3.Y + ";");
            //if(p1.X==257&& p2.X==253 && p3.X==255)
            //    Console.WriteLine("debug");
            double x1 = p1.X;
            double x2 = p2.X;
            double x3 = p3.X;
            double y1 = p1.Y;
            double y2 = p2.Y;
            double y3 = p3.Y;
            if (x1 == x3 && x2 == x3)
                return 180;
            if (y1 == y3 && y2 == y3)
                return 180;
            double adotb = (x2 - x3) * (x1 - x3) + (y2 - y3) * (y1 - y3);
            double tmp1 = (Math.Sqrt((x2 - x3) * (x2 - x3) + (y2 - y3) * (y2 - y3)) * Math.Sqrt((x1 - x3) * (x1 - x3) + (y1 - y3) * (y1 - y3)));
            double tmp2 = adotb / tmp1;
            double angel = 0;
            if (Math.Abs(Math.Abs(tmp2) - 1) < error) angel = 180;
            else
            {
                angel = Math.Acos(adotb / tmp1);
                angel = angel * 180 / Math.PI;
            }
            if (angel < 0)
                return 180 + angel;

            else return angel;
        }
    }
}
