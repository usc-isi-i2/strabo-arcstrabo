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
using Emgu.CV;
using Emgu.CV.Structure;
using Strabo.Core.TextLayerExtraction;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strabo.Core.ColorSegmentation
{
    /// <summary>
    /// Starting class for color segmentation
    /// </summary>
    public class ColorSegmentationWorker
    {
        public ColorSegmentationWorker() { }

        //#region Ramtin Code
        //[Obsolete("Ramtin version")]
        //public int Apply(String fullFilePath, int fromK, int toK, bool doExtraction)
        //{
        //    PreProcessing pp = new PreProcessing();
        //    fullFilePath = pp.doPerProccessing(fullFilePath);

        //    Dictionary<int, double> report = new Dictionary<int, double>();

        //    int uc = PreProcessing.countUniqueColorNumber(fullFilePath);
        //    if ( uc < toK)
        //    {
        //        System.Console.WriteLine(String.Format(" number of unique colors less than {0} so do only  {1}-mean", toK,uc));
        //        fromK = uc;
        //        toK = uc;
        //    }


        //    for (int i = fromK; i <= toK; i++)
        //    {
               
        //        System.Console.WriteLine(String.Format("k-mean {0} ... from {1}", i, toK));

        //        //K-mean
        //        string kfullFilePath = doKmean(fullFilePath, i, ref report);
        //        //Color extraction
        //        if (doExtraction)
        //        {
        //            String[] pathList = doColorExctraction(kfullFilePath);
        //            /*
        //            foreach (String s in pathList)
        //            {
        //                Connected Components
        //                doConnectedComponent(s);
        //            }*/

        //            CompareImageLayers cp = new CompareImageLayers();
        //            String fileFormat = System.IO.Path.GetFileNameWithoutExtension(fullFilePath) + "_k" + i + "_l{0}" + System.IO.Path.GetExtension(fullFilePath);
        //            List<List<double>> normalizedSpatialList = cp.analysisSpatialSimilarityOfLayers(i, System.IO.Path.GetDirectoryName(fullFilePath) + "\\", fileFormat);
        //            List<List<double>> normalizedColorList = cp.analysisColorSimilarityOfLayers(i, System.IO.Path.GetDirectoryName(fullFilePath) + "\\", fileFormat);

        //            //save Output
        //            StringBuilder sbTxt = new StringBuilder();
        //            for (int m = 0; m < normalizedColorList.Count; m++)
        //            {
        //                sbTxt.Append(string.Format("l{0},", m));
        //            }
        //            sbTxt = sbTxt.Remove(sbTxt.Length - 1, 1);
        //            sbTxt.Append(Environment.NewLine);

        //            //merge Color and Spatial half half
        //            List<List<double>> colorSpatialList = new List<List<double>>();
        //            for (int m = 0; m < normalizedColorList.Count; m++)
        //            {
        //                List<double> ls = new List<double>();
        //                for (int n = 0; n < normalizedColorList[0].Count; n++)
        //                {
        //                   //double d = normalizedSpatialList[m][n] + normalizedColorList[m][n];
        //                    double d =  normalizedColorList[m][n];
        //                    ls.Add(d);

        //                    sbTxt.Append(d + ",");
        //                }
        //                sbTxt = sbTxt.Remove(sbTxt.Length - 1, 1);
        //                sbTxt.Append(Environment.NewLine);

        //                colorSpatialList.Add(ls);
        //            }

        //            //Save the output
        //            System.IO.File.WriteAllText(System.IO.Path.GetDirectoryName(fullFilePath) + "\\" + "layersColorSpatialComparison_" + normalizedColorList.Count + ".txt", sbTxt.ToString());
        //        }
        //    }

        //    int bestK = 0;
        //    if (!doExtraction)
        //    {
        //        bestK = new KDetector().findBestK(report);
        //    }

        //    return bestK;
        //}

        //private String doKmean(String fullFilePath, int k,ref Dictionary<int, double> report)
        //{
        //    KMeans kmean = new KMeans();

        //    String latestFullFilePath = kmean.Apply(Path.GetDirectoryName(fullFilePath) + "\\", Path.GetFileName(fullFilePath), changeFileName("_k" + k, fullFilePath), k, ref report);

        //    KMeans.kdetector = null;
        //    kmean = null;

        //    return latestFullFilePath;
        //}

        //private String[] doColorExctraction(String fullFilePath)
        //{
        //    ColorExtraction ce = new ColorExtraction();
        //    Dictionary<int, Color> tbl = ce.ApplyFast(fullFilePath, changeFileName("_l{0}", fullFilePath));

        //    String[] returnPath = new String[tbl.Count];
        //    for (int i = 0; i < tbl.Count; i++)
        //    {
        //        returnPath[i] = changeFileName("_l" + i, fullFilePath);
        //    }
        //    return returnPath;
        //}

        //private String doConnectedComponent(String fullFilePath)
        //{
        //    String path = changeFileName("_cc", fullFilePath);

        //    ConnectedComponent cc = new ConnectedComponent();
        //    int ccCount = cc.apply(fullFilePath, path);

        //    return path;
        //}

        //private string changeFileName(String postFix, String fullFilePath)
        //{
        //    return fullFilePath.Insert(fullFilePath.LastIndexOf('.'), postFix);
        //}
        //#endregion
        
        public string[] Apply(string dir, string fn)
        {
            List<String> outImagePaths = new List<string>();
            try
            {
                string fn_only = Path.GetFileNameWithoutExtension(fn);

                //MeanShift
                MeanShiftMultiThreads mt = new MeanShiftMultiThreads();
                outImagePaths.Add(mt.ApplyYIQMT(dir + fn, 8, 3, 15, dir + fn_only + "_ms.png"));
                mt = null;
                GC.Collect();

                // Median Cut
                MedianCutMultiThreads mcq = new MedianCutMultiThreads();
                List<int> qnum_list = new List<int>();
                qnum_list.Add(1024);
                mcq.GenerateColorPalette(dir + fn_only + "_ms.png", qnum_list);
                string[] mcqImagePaths = mcq.quantizeImageMT(8, dir, fn_only + "_mc");
                mcq = null;
                GC.Collect();

                /* Parin 
                List<int> qnum_list = new List<int>();
                qnum_list.Add(1024);
                for (int i = 0; i < qnum_list.Count; i++)
                {
                    Console.WriteLine(dir + fn_only + "_ms.png");

                    Image<Bgr, Byte> inpImage = new Image<Bgr, Byte>(dir + fn_only + "_ms.png");

                    Image<Rgb, Byte> dst = new Image<Rgb, Byte>(inpImage.Size);
                    CvInvoke.cvSmooth(inpImage.Ptr, dst.Ptr, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_MEDIAN, 31, 31, 1.5, 1);
                    dst.ToBitmap().Save(dir + fn_only + "_mc" + qnum_list[i] + ".png", ImageFormat.Png);
                }
                */
                for (int i = 0; i < mcqImagePaths.Length; i++)
                    outImagePaths.Add(mcqImagePaths[i]);

                // KMeans
                for (int i = 0; i < qnum_list.Count; i++)
                {
                    int km = 128;//qnum_list[i];
                    for (int k = 0; k < 10; k++)
                    {
                        MyKMeans kmeans = new MyKMeans();
                        km /= 2;
                        if (km < 4) break;
                        Console.WriteLine(km);
                        outImagePaths.Add(kmeans.Apply(km, dir + fn_only + "_mc" + qnum_list[i] + ".png", dir + fn_only + "_mc" + qnum_list[i] + "_k" + km + ".png"));
                        kmeans = null;
                        GC.Collect();
                    }
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString()); 
            }

            return outImagePaths.ToArray();
        }
        //#region Obsolete Code
        //[Obsolete("Old version")]
        ////New apply using openCV for k-means, color extraction and connected component
        // [Obsolete("Old version")]
        //public string[] MyApplyOld(string dir, string fn)
        //{

        //    List<String> outImagePaths = new List<string>();
         
        //    string fn_only = Path.GetFileNameWithoutExtension(fn);

        //    //MeanShift
        //    MeanShiftMultiThreads mt = new MeanShiftMultiThreads();
        //    outImagePaths.Add(mt.ApplyYIQMT(dir + fn, 8, 3, 15, dir + fn_only + "_ms.png"));
        //    mt = null;
        //    GC.Collect();

        //    // Median Cut
        //    MedianCutMultiThreads mcq = new MedianCutMultiThreads();
        //    List<int> qnum_list = new List<int>();
        //    qnum_list.Add(1024);
        //    mcq.GenerateColorPalette(dir + fn_only + "_ms.png", qnum_list);
        //    string[] mcqImagePaths = mcq.quantizeImageMT(8, dir, fn_only + "_mc");
        //    mcq = null;
        //    GC.Collect();

        //    //Emgu_CV K-Means
        //     Dictionary<int,double> rep = new Dictionary<int,double>();
        //    String report = "file, k-means, layer, CC, Color, R, G, B" + Environment.NewLine;
        //    string medianCutImage = mcqImagePaths[0].Replace(dir, "");
        //    for (int k = 2; k <= 2; k++)
        //    {
        //        KMeans kmean = new KMeans();
        //        String kmeanFileImage = medianCutImage.Insert(medianCutImage.IndexOf('.'), "_k{0}");

        //        string kmeanSavedFile = kmean.Apply(Path.GetFullPath(dir), medianCutImage, Path.GetFullPath(dir) + string.Format(kmeanFileImage, k), k, ref rep);

        //        String ceFileImage = kmeanSavedFile.Insert(kmeanSavedFile.IndexOf('.'), "_l{0}");
        //        ColorExtraction ce = new ColorExtraction();
        //        Dictionary<int, Color> tbl = ce.ApplyFast(kmeanSavedFile, ceFileImage);

        //        for (int l = 0; l < tbl.Count; l++)
        //        {
        //            ConnectedComponent cc = new ConnectedComponent();
        //            String ccFileImage = String.Format(ceFileImage, l).Insert(String.Format(ceFileImage, l).IndexOf("."), "_cc");
        //            int ccCount = cc.apply(string.Format(ceFileImage, l), ccFileImage);

        //            report += String.Format("{0}, {1}, {2}, {3}, #{4}, {5}, {6}, {7} ", fn, k, l, ccCount, tbl[l].R.ToString("X2") + tbl[l].G.ToString("X2") + tbl[l].B.ToString("X2"), tbl[l].R, tbl[l].G, tbl[l].B) + Environment.NewLine;
        //        }
        //    }

        //    File.WriteAllText(dir + "report.csv", report);

        //    return outImagePaths.ToArray();
        //}
        //#endregion
    }
}
