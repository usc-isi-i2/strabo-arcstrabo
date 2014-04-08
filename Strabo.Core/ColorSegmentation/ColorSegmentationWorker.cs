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

        public int Apply(String fullFilePath, int fromK, int toK, bool doExtraction)
        {
            //MeanShift
            fullFilePath = doMeanShift(fullFilePath);
            ////Median Cut
            fullFilePath = doMeanCut(fullFilePath);

            Dictionary<int, double> report = new Dictionary<int, double>();

            for (int i = fromK; i <= toK; i++)
            {
                System.Console.WriteLine(String.Format("k-mean {0} ... from {1}", i,toK));

                //K-mean
                string kfullFilePath = doKmean(fullFilePath, i, ref report);
                //Color extraction
                if (doExtraction)
                {
                    String[] pathList = doColorExctraction(kfullFilePath);
                    /*
                    foreach (String s in pathList)
                    {
                        Connected Components
                        doConnectedComponent(s);
                    }*/
                }
            }

            int bestK = 0;
            if (!doExtraction)
            {
                bestK = new KDetector().findBestK(report);
            }

            return bestK;
        }

        
        private String doMeanShift(String fullFilePath)
        {
            //MeanShift
            MeanShiftMultiThreads mt = new MeanShiftMultiThreads();
            String latestFullFilePath = mt.ApplyYIQMT(fullFilePath, 8, 3, 15, changeFileName("_ms", fullFilePath));
            mt = null;
            GC.Collect();

            return latestFullFilePath;
        }

        private String doMeanCut(String fullFilePath)
        {
            // Median Cut
            MedianCutMultiThreads mcq = new MedianCutMultiThreads();
            List<int> qnum_list = new List<int>();
            qnum_list.Add(1024);
            mcq.GenerateColorPalette(fullFilePath, qnum_list);

            string[] mcqImagePaths = mcq.quantizeImageMT(8, Path.GetDirectoryName(fullFilePath) + "\\", Path.GetFileNameWithoutExtension(changeFileName("_mc", fullFilePath)));
            String latestFullFilePath = mcqImagePaths[0];
            mcq = null;
            GC.Collect();

            return latestFullFilePath;
        }

        private String doKmean(String fullFilePath, int k,ref Dictionary<int, double> report)
        {
            KMeans kmean = new KMeans();

            String latestFullFilePath = kmean.apply(Path.GetDirectoryName(fullFilePath) + "\\", Path.GetFileName(fullFilePath), changeFileName("_k" + k, fullFilePath), k, ref report);

            return latestFullFilePath;
        }

        private String[] doColorExctraction(String fullFilePath)
        {
            ColorExtraction ce = new ColorExtraction();
            Dictionary<int, Color> tbl = ce.ApplyFast(fullFilePath, changeFileName("_l{0}", fullFilePath));

            String[] returnPath = new String[tbl.Count];
            for (int i = 0; i < tbl.Count; i++)
            {
                returnPath[i] = changeFileName("_l" + i, fullFilePath);
            }
            return returnPath;
        }

        private String doConnectedComponent(String fullFilePath)
        {
            String path = changeFileName("_cc", fullFilePath);

            ConnectedComponent cc = new ConnectedComponent();
            int ccCount = cc.apply(fullFilePath, path);

            return path;
        }

        private string changeFileName(String postFix, String fullFilePath)
        {
            return fullFilePath.Insert(fullFilePath.LastIndexOf('.'), postFix);
        }

        #region Obsolete Code
        [Obsolete("Old version")]
        public string[] ApplyOld(string dir, string fn)
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
   
        //New apply using openCV for k-means, color extraction and connected component
         [Obsolete("Old version")]
        public string[] MyApplyOld(string dir, string fn)
        {

            List<String> outImagePaths = new List<string>();
         
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

            //Emgu_CV K-Means
             Dictionary<int,double> rep = new Dictionary<int,double>();
            String report = "file, k-means, layer, CC, Color, R, G, B" + Environment.NewLine;
            string medianCutImage = mcqImagePaths[0].Replace(dir, "");
            for (int k = 2; k <= 2; k++)
            {
                KMeans kmean = new KMeans();
                String kmeanFileImage = medianCutImage.Insert(medianCutImage.IndexOf('.'), "_k{0}");

                string kmeanSavedFile = kmean.apply(Path.GetFullPath(dir), medianCutImage, Path.GetFullPath(dir) + string.Format(kmeanFileImage, k), k, ref rep);

                String ceFileImage = kmeanSavedFile.Insert(kmeanSavedFile.IndexOf('.'), "_l{0}");
                ColorExtraction ce = new ColorExtraction();
                Dictionary<int, Color> tbl = ce.ApplyFast(kmeanSavedFile, ceFileImage);

                for (int l = 0; l < tbl.Count; l++)
                {
                    ConnectedComponent cc = new ConnectedComponent();
                    String ccFileImage = String.Format(ceFileImage, l).Insert(String.Format(ceFileImage, l).IndexOf("."), "_cc");
                    int ccCount = cc.apply(string.Format(ceFileImage, l), ccFileImage);

                    report += String.Format("{0}, {1}, {2}, {3}, #{4}, {5}, {6}, {7} ", fn, k, l, ccCount, tbl[l].R.ToString("X2") + tbl[l].G.ToString("X2") + tbl[l].B.ToString("X2"), tbl[l].R, tbl[l].G, tbl[l].B) + Environment.NewLine;
                }
            }

            File.WriteAllText(dir + "report.csv", report);

            return outImagePaths.ToArray();
        }
        #endregion
    }
}
