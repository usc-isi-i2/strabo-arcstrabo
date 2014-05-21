using System;
using System.Collections.Generic;

namespace Strabo.Core.ColorSegmentation
{
    public class AutoLayerExtraction
    {
        public void apply(String directory)
        {
            if (directory[directory.Length - 1] != '\\')
            {
                directory += @"\";
            }

            string[] files = System.IO.Directory.GetFiles(directory, "*.png", System.IO.SearchOption.TopDirectoryOnly);

            if (files.Length == 0)
            {
                System.Console.WriteLine("No *.png file found in the specified directory! ");
            }

            foreach (string currentFile in files)
            {
                String dir = directory;
                String fn = System.IO.Path.GetFileName(currentFile);
                System.Console.WriteLine(Environment.NewLine + String.Format("Processing {0} ...", fn));

                //create new directory for processing and copy image to this new dir
                string oldDir = dir;
                dir += System.IO.Path.GetFileNameWithoutExtension(fn) + @"\";

                System.IO.Directory.CreateDirectory(dir);
                System.IO.File.Copy(oldDir + fn, dir + fn, true);

                string outPutDir = dir + @"output\";
                System.IO.Directory.CreateDirectory(outPutDir);


                //Find best K
                ColorSegmentationWorker csw = new ColorSegmentationWorker();
                int bestK = csw.Apply(dir + fn, 2, 40, true);

                /*
                Extract layers for the best K
                csw = new ColorSegmentationWorker();
                csw.Apply(dir + fn, bestK, bestK, true);
                */

                /*
                //Compare extracted layrers and save report
                CompareImageLayers cp = new CompareImageLayers();
                String fileFormat = System.IO.Path.GetFileNameWithoutExtension(fn) + "_ms_mc1024_k" + bestK + "_l{0}" + System.IO.Path.GetExtension(fn);
                List<KeyValuePair<int, int>> pairList = cp.analysisSimilarityOfLayers(bestK, dir, fileFormat);

                //Merge similar layers and copy distinct layers
                cp.generateOutput(pairList, bestK, dir, outPutDir, fileFormat);
                 */
            }
        }
    }
}