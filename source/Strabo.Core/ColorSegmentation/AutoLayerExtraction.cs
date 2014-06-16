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
                int bestK = csw.Apply(dir, fn);

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