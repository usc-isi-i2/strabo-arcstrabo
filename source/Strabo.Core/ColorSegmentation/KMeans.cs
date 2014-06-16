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
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Strabo.Core.ColorSegmentation
{
    /// <summary>
    /// KMeans based on EmguCV
    /// </summary>
    public class KMeans
    {
        //public static KDetector kdetector;
        public string Apply(int k, string fn, string outImagePath)
        {
            Image<Bgr, float> src = new Image<Bgr, float>(fn);
            Matrix<float> samples = new Matrix<float>(src.Rows * src.Cols, 1, 3);
            Matrix<int> finalClusters = new Matrix<int>(src.Rows * src.Cols, 1);

            //Convert image to a sample matrix that its rows equal to width*height of image and its 
            //column equals to 3 feature (R/G/B) or (H/L/S)
            for (int y = 0; y < src.Rows; y++)
            {
                for (int x = 0; x < src.Cols; x++)
                {
                    samples.Data[y + x * src.Rows, 0] = (float)src[y, x].Blue;
                    samples.Data[y + x * src.Rows, 1] = (float)src[y, x].Green;
                    samples.Data[y + x * src.Rows, 2] = (float)src[y, x].Red;
                }
            }

            MCvTermCriteria term = new MCvTermCriteria(10000, 0.0001);
            term.type = TERMCRIT.CV_TERMCRIT_ITER | TERMCRIT.CV_TERMCRIT_EPS;

            int clusterCount = k;
            int attempts = 10;

            //center matrix after call k-means function holds the cluster value
            Matrix<float> centers;
            centers = new Matrix<float>(clusterCount, 3);

            int mm = CvInvoke.cvKMeans2(samples, clusterCount, finalClusters, term, attempts, IntPtr.Zero, KMeansInitType.PPCenters, centers, IntPtr.Zero);

            Image<Bgr, float> new_image = new Image<Bgr, float>(src.Size);

            //find color of cluster values
            Bgr[] clusterColors = new Bgr[clusterCount];
            for (int i = 0; i < clusterCount; i++)
            {
                Bgr b = new Bgr(centers[i, 0], centers[i, 1], centers[i, 2]);

                clusterColors[i] = b;
            }

            //Draw a image based on cluster color
            for (int y = 0; y < src.Rows; y++)
            {
                for (int x = 0; x < src.Cols; x++)
                {
                    PointF p = new PointF(x, y);
                    new_image.Draw(new CircleF(p, 1.0f), clusterColors[finalClusters[y + x * src.Rows, 0]], 1);
                }
            }

            new_image.Save(outImagePath);

            return outImagePath;
        }


        /// <summary>
        /// Ramtin's code
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="srcImg">The source img.</param>
        /// <param name="outImg">The out img.</param>
        /// <param name="k">The k.</param>
        /// <param name="report">The report.</param>
        /// <returns></returns>
        public String Apply(string dir, string srcImg, string outImg, int k, ref Dictionary<int, double> report)
        {
            Image<Bgr, float> src = new Image<Bgr, float>(dir + srcImg);
            Matrix<float> samples = new Matrix<float>(src.Rows * src.Cols, 1, 3);
            Matrix<int> finalClusters = new Matrix<int>(src.Rows * src.Cols, 1);

            //Convert image to a sample matrix that its rows equal to width*height of image and its 
            //column equals to 3 feature (R/G/B) or (H/L/S)
            for (int y = 0; y < src.Rows; y++)
            {
                for (int x = 0; x < src.Cols; x++)
                {
                    samples.Data[y + x * src.Rows, 0] = (float)src[y, x].Blue;
                    samples.Data[y + x * src.Rows, 1] = (float)src[y, x].Green;
                    samples.Data[y + x * src.Rows, 2] = (float)src[y, x].Red;
                }
            }

            //if (kdetector == null)
            //{
            //     kdetector = new KDetector(src, samples, k);
            //}


            MCvTermCriteria term = new MCvTermCriteria(10000, 0.0001);
            term.type = TERMCRIT.CV_TERMCRIT_ITER | TERMCRIT.CV_TERMCRIT_EPS;

            int clusterCount = k;
            int attempts = 10;

            //center matrix after call k-means function holds the cluster value
            Matrix<float> centers;
            centers = new Matrix<float>(clusterCount, 3);

            //My unsuccessful code for prodving center seed for k-mean
            //if (kdetector.centers == null)
            //{
             //   centers = new Matrix<float>(clusterCount, 3);
            //}
            //else
            //{
            //    centers = kdetector.getNewCenterSeeds(k);
            //}           

            //call k-mean
            int mm = CvInvoke.cvKMeans2(samples, clusterCount, finalClusters, term, attempts, IntPtr.Zero, KMeansInitType.PPCenters, centers, IntPtr.Zero);


            //kdetector.saveClusteringInfo(centers, finalClusters);


            //Reporting
            String retSt = String.Empty;
            if (System.IO.File.Exists(dir + "kmeans.csv"))
            {
                retSt = System.IO.File.ReadAllText(dir + "kmeans.csv");
            }
            if (String.IsNullOrEmpty(retSt))
            {
                retSt += "k, intra, inter, validaity " + Environment.NewLine;
            }
            //retSt += String.Format("{0}, {1}, {2}, {3}",
            //    k,  kdetector.calIntraDistanceForAllCluster(),  kdetector.calInterDistance(),  kdetector.geValidity()) + Environment.NewLine;


            //System.IO.File.WriteAllText(dir + "kmeans.csv", retSt);

            //report.Add(k, kdetector.geValidity());

            Image<Bgr, float> new_image = new Image<Bgr, float>(src.Size);

            //find color of cluster values
            Bgr[] clusterColors = new Bgr[clusterCount];
            for (int i = 0; i < clusterCount; i++)
            {
                Bgr b = new Bgr(centers[i, 0], centers[i, 1], centers[i, 2]);

                clusterColors[i] = b;
            }


            //Draw a image based on cluster color
            for (int y = 0; y < src.Rows; y++)
            {
                for (int x = 0; x < src.Cols; x++)
                {
                    PointF p = new PointF(x, y);

                    //new_image.Draw(new CircleF(p, 1.0f), bb, 1);
                    new_image.Draw(new CircleF(p, 1.0f), clusterColors[finalClusters[y + x * src.Rows, 0]], 1);
                }
            }

            new_image.Save(outImg);


            return outImg;
        }

    }
}
