using Emgu.CV;
using Emgu.CV.Structure;
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
    }
}
