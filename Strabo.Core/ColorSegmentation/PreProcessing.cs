using Emgu.CV;
using Emgu.CV.Structure;
using Strabo.Core.ImageProcessing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strabo.Core.ColorSegmentation
{
    public class PreProcessing
    {
        public string doPerProccessing(String fullFilePath)
        {
            //Do not run meanshift and median cut if the image color is less than 1024
            if (countUniqueColorNumber(fullFilePath) < 1024)
            {
                System.Console.WriteLine("unique color number is less than 1024 so skip meanShift and medianCut");
                String newfullFilePath = System.IO.Path.GetDirectoryName(fullFilePath) + @"\" + System.IO.Path.GetFileNameWithoutExtension(fullFilePath) + "_ms_mc1024" + System.IO.Path.GetExtension(fullFilePath);
                System.IO.File.Copy(fullFilePath, newfullFilePath,true);
                return newfullFilePath;
            }

            //MeanShift
            fullFilePath = doMeanShift(fullFilePath);
            ////Median Cut
            fullFilePath = doMeanCut(fullFilePath);

            return fullFilePath;
        }

        public static int countUniqueColorNumber(String fullFilePath)
        {
            return 40;

            Bitmap srcimg = new Bitmap(fullFilePath);
            int[] pixels = ImageUtils.BitmapToArray1DIntRGB(srcimg);
            ColorHistogram colorHist = new ColorHistogram(pixels);
            int[] color_array = colorHist.getColorArray();

            return color_array.Length;
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

            string[] mcqImagePaths = mcq.quantizeImageMT(8, Path.GetDirectoryName(fullFilePath) + "\\", Path.GetFileNameWithoutExtension(changeFileName("c", fullFilePath)));
            String latestFullFilePath = mcqImagePaths[0];
            mcq = null;
            GC.Collect();

            return latestFullFilePath;
        }

        private string changeFileName(String postFix, String fullFilePath)
        {
            return fullFilePath.Insert(fullFilePath.LastIndexOf('.'), postFix);
        }
    }
}
