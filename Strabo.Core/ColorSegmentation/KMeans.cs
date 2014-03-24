using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;

namespace Strabo.Core.ColorSegmentation
{
    /// <summary>
    /// KMeans based on EmguCV
    /// </summary>
    class KMeans
    {
        public String apply(string dir, string srcImg, string outImg, int k)
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


            MCvTermCriteria term = new MCvTermCriteria(10000, 0.0001);
            term.type = TERMCRIT.CV_TERMCRIT_ITER | TERMCRIT.CV_TERMCRIT_EPS;

            int clusterCount = k;
            int attempts = 10;

            //center matrix after call k-means function holds the cluster value
            Matrix<float> centers = new Matrix<float>(clusterCount, 3);

            //call k-mean
            int mm = CvInvoke.cvKMeans2(samples, clusterCount, finalClusters, term, attempts, IntPtr.Zero, KMeansInitType.PPCenters, centers, IntPtr.Zero);

            Image<Bgr, float> new_image = new Image<Bgr, float>(src.Size);

            //find color of cluster values
            Bgr[] clusterColors = new Bgr[clusterCount];
            for (int i = 0; i < clusterCount; i++)
            {
                Bgr b = new Bgr(centers[i, 0], centers[i, 1], centers[i, 2]);

                clusterColors[i] = b;
            }

            double intra = 0;
            for (int i = 0; i < clusterCount; i++)
            {
                float zR = centers[i, 2], zG = centers[i, 1], zB = centers[i, 0];

                for (int y = 0; y < src.Rows; y++)
                {
                    for (int x = 0; x < src.Cols; x++)
                    {
                        if (finalClusters[y + x * src.Rows, 0] == i)
                        {
                            float xR = samples.Data[y + x * src.Rows, 2];
                            float xG = samples.Data[y + x * src.Rows, 1];
                            float xB = samples.Data[y + x * src.Rows, 0];

                            intra += Math.Sqrt(Math.Pow(zR - xR, 2) + Math.Pow(zG - xG, 2) + Math.Pow(zB - xB, 2));
                        }
                    }
                }
            }
            intra = intra / samples.Data.Length;



            double minInterDistance = int.MaxValue;
            for (int i = 0; i < clusterCount - 1; i++)
            {
                for (int j = i + 1; j < clusterCount; j++)
                {

                    float zR1 = centers[i, 2], zG1 = centers[i, 1], zB1 = centers[i, 0];
                    float zR2 = centers[j, 2], zG2 = centers[j, 1], zB2 = centers[j, 0];

                    double d = Math.Sqrt(Math.Pow(zR1 - zR2, 2) + Math.Pow(zG1 - zG2, 2) + Math.Pow(zB1 - zB2, 2));
                    if (d < minInterDistance)
                    {
                        minInterDistance = d;
                    }
                }
            }

            double validity = intra / minInterDistance;


            String retSt = String.Empty;
            if (System.IO.File.Exists(dir + "kmeans.csv"))
            {
                retSt = System.IO.File.ReadAllText(dir + "kmeans.csv");
            }
            if (String.IsNullOrEmpty(retSt))
            {
                //retSt += "n, compactness, attempts, intra, inter, validaity, center_R, center_G, center_B, colorCode" + Environment.NewLine;
                retSt += "k, compactness, attempts, intra, inter, validaity " + Environment.NewLine;
            }
            retSt += String.Format("{0}, {1}, {2}, {3}, {4}, {5}",
                k, mm, attempts, intra, minInterDistance, validity) + Environment.NewLine;

            //for (int i = 0; i < k; i++)
            //{
            //    String colorCode = ((int)clusterColors[i].Red).ToString("X2") + ((int)clusterColors[i].Green).ToString("X2") + ((int)clusterColors[i].Blue).ToString("X2");
            //    retSt += String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, #{9}", k, mm, attempts, intraDistance, minInterDistance, validity, (int)clusterColors[i].Red, (int)clusterColors[i].Green, (int)clusterColors[i].Blue, colorCode) + Environment.NewLine;
            //}
            System.IO.File.WriteAllText(dir + "kmeans.csv", retSt);


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
