using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Strabo.Core.ColorSegmentation
{
    public class CompareImageLayers
    {
        public Dictionary<int, Matrix<float>> dic;

        public double findCosineSimilarity(string layer1Fn, int ln, string layer2Fn, int rn)
        {
            Emgu.CV.Image<Bgr, int> imgLayer1 = new Emgu.CV.Image<Bgr, int>(layer1Fn);
            Emgu.CV.Image<Bgr, int> imgLayer2 = new Emgu.CV.Image<Bgr, int>(layer2Fn);

            int ratio = (int)Math.Max(imgLayer1.Width, imgLayer1.Height) / 100;

            if (dic == null)
            {
                dic = new Dictionary<int, Matrix<float>>();
            }

            Matrix<float> l;
            if (dic.ContainsKey(ln))
            {
                l = dic[ln];
            }
            else
            {
                l = imageToMatrix(imgLayer1, ratio,ln);
                dic.Add(ln, l);
            }

          
            Matrix<float> r;
            if (dic.ContainsKey(rn))
            {
                r = dic[rn];
            }
            else
            {
                r = imageToMatrix(imgLayer2, ratio,rn);
                dic.Add(rn, r);
            }

            return CosineCompareFeatureVectors(l, r);

            //unsucccessful try of earth mover's distance
            //double compareResult = 0;
            //try
            //{
            //    compareResult = CvInvoke.cvCalcEMD2(l.Ptr, r.Ptr, Emgu.CV.CvEnum.DIST_TYPE.CV_DIST_L2, null, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            //}
            //catch (Exception ex)
            //{
            //    int k = 0;
            //}
            //return compareResult;
        }

        public double CosineCompareFeatureVectors(Matrix<float> fv1, Matrix<float> fv2) // the smaller the higher similarity
        {
            double compareResult = 0;
            double aSquare = 0;
            double bSquare = 0;
            for (int i = 0; i < fv1.Cols; i++)
            {
                compareResult += fv1[0, i] * fv2[0, i];
                aSquare += fv1[0, i] * fv1[0, i];
                bSquare += fv2[0, i] * fv2[0, i];
            }
            return compareResult / (Math.Sqrt(aSquare) * Math.Sqrt(bSquare)) * -1 + 1; // translate to 0 (the same) - 2 (the opposite)
        }

        private Matrix<float> imageToMatrix(Emgu.CV.Image<Bgr, int> img, int ratio, int n)
        {
            int h = img.Height / ratio;
            if (img.Height % ratio > 0) h++;
            int w = img.Width / ratio;
            if (img.Width % ratio > 0) w++;

            int hw = h * w;

            Matrix<float> m = new Matrix<float>(1, hw);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    Bgr b = new Bgr(img[i, j].Blue, img[i, j].Green, img[i, j].Red);
                    if (!(b.Red == 0 && b.Green == 0 && b.Blue == 0))
                    {
                        int indx = ((int)(i / ratio)) * ((int)(j / ratio));
                        m.Data[0, indx] = m.Data[0, indx] + 1;
                    }
                }
            }

            //for (int i = 0; i < m.Data.Length; i++)
            //{
            //    sb.Append(m.Data[0, i] + ",");
            //}

            //System.IO.File.WriteAllText( @"C:\Users\ramtinb\Source\Strabo\data\5" + n + ".csv",sb.ToString());

            return m;
        }

        public List<KeyValuePair<int, int>> analysisSimilarityOfLayers(int bestK, string dir, string fileFormat)
        {
            //extract similarity between layers
            StringBuilder sbCsv = new StringBuilder();
            sbCsv.Append("i,j,val" + Environment.NewLine);

            StringBuilder sbTxt = new StringBuilder();
            
            for (int i = 0; i < bestK; i++)
            {
               sbTxt.Append(string.Format("l{0},", i));
            }
            sbTxt = sbTxt.Remove(sbTxt.Length - 1, 1);
            sbTxt.Append(Environment.NewLine);


            List<KeyValuePair<int, int>> pairList = new List<KeyValuePair<int, int>>(bestK);
            for (int i = 0; i < bestK; i++)
            {
                double min = double.MaxValue;
                int minI = i;

                for (int j = 0; j < bestK; j++)
                {
                    //if (i == j) continue;
                    string lfn = dir + String.Format(fileFormat, i);
                    string rgn = dir + String.Format(fileFormat, j); 
                    double d = findCosineSimilarity(lfn, i, rgn, j);

                    d = (d < 0.0000000001) ? 0 : d;

                    sbCsv.Append(i + "," + j + "," + d + Environment.NewLine);
                    sbTxt.Append(d + ",");

                    if (d < min && d > 0)
                    {
                        min = d;
                        minI = j;
                    }
                }
                sbTxt = sbTxt.Remove(sbTxt.Length - 1, 1);
                sbTxt.Append(Environment.NewLine);

                pairList.Add(new KeyValuePair<int, int>(i, minI));
            }

            //Save report and export
            System.IO.File.WriteAllText(dir + "layersComparison.csv", sbCsv.ToString());
            System.IO.File.WriteAllText(dir + "layersComparison.txt", sbTxt.ToString());

            return pairList;
        }

        public void generateOutput(List<KeyValuePair<int, int>> pairList, int bestK, string dir, string outPutDir,string fileFormat)
        {
            //Find similar data
            List<KeyValuePair<int, int>> blendList = new List<KeyValuePair<int, int>>(bestK);
            List<int> lonelyList = new List<int>(bestK);

            for (int i = 0; i < pairList.Count; i++)
            {
                for (int j = 0; j < pairList.Count; j++)
                {
                    if (pairList[i].Key == pairList[j].Value && pairList[j].Key == pairList[i].Value)
                    {
                        if (!(blendList.Contains(pairList[i]) || blendList.Contains(pairList[j])))
                        {
                            blendList.Add(pairList[i]);
                        }
                    }
                }
            }

            for (int i = 0; i < pairList.Count; i++)
            {
                bool key = true;
                for (int j = 0; j < blendList.Count; j++)
                {
                    if (pairList[i].Key == blendList[j].Key || pairList[i].Key == blendList[j].Value)
                    {
                        key = false;
                    }
                }

                if (key)
                {
                    if (!lonelyList.Contains(pairList[i].Key))
                    {
                        lonelyList.Add(pairList[i].Key);
                    }
                }
            }


            //move not changed layers
            for (int i = 0; i < lonelyList.Count; i++)
            {
                string fileName = String.Format(fileFormat, lonelyList[i]);
                System.IO.File.Copy(dir + fileName, outPutDir + fileName, true);
            }

            //blend and move
            for (int j = 0; j < blendList.Count; j++)
            {
                string fileName1 = String.Format(fileFormat, blendList[j].Key);
                Emgu.CV.Image<Bgr, int> img1 = new Emgu.CV.Image<Bgr, int>(dir + fileName1);


                string fileName2 = String.Format(fileFormat, blendList[j].Value);
                Emgu.CV.Image<Bgr, int> img2 = new Emgu.CV.Image<Bgr, int>(dir + fileName2);

                string fileName3 = String.Format(fileFormat, blendList[j].Key + "_" + blendList[j].Value);

                Emgu.CV.Image<Bgr, int> img3 = new Emgu.CV.Image<Bgr, int>(img1.Width, img1.Height);
                Emgu.CV.CvInvoke.cvAdd(img1, img2, img3, IntPtr.Zero);
                img3.Save(outPutDir + fileName3);

            }
        }
    
    }
}
