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

            //temp editing yaoyi
            string[] mcqImagePaths = null;// mcq.quantizeImageMT(8, Path.GetDirectoryName(fullFilePath) + "\\", Path.GetFileNameWithoutExtension(changeFileName("c", fullFilePath)));
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
