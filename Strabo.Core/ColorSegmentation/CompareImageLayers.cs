using Emgu.CV;
using Emgu.CV.Structure;
using Strabo.Core.ImageProcessing;
using System;
using System.Collections.Generic;
using System.Drawing;
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

           // int ratio = 25;// (int)Math.Max(imgLayer1.Width, imgLayer1.Height) / 100;
            int ratio = 25;

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

           // System.Diagnostics.Debug.WriteLine(ln + "," + rn+ "->" + unWightedHammingCompareFeatureVectors(l, r) + Environment.NewLine);
            //System.Diagnostics.Debug.WriteLine(ln + "," + rn + "->" + WightedHammingCompareFeatureVectors(l, r) + Environment.NewLine);
            double result = CosineCompareFeatureVectors(l, r) / (WightedHammingCompareFeatureVectors(l, r) + 1);
            return result;// *-1 + 1;
           // return EMDCompare(l, r);

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

        public double EMDCompare(Matrix<float> fv1, Matrix<float> fv2)
        {
           

            double compareResult = 0;
            try
            {
                compareResult = CvInvoke.cvCalcEMD2(fv1, fv2, Emgu.CV.CvEnum.DIST_TYPE.CV_DIST_L1, null, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                int k = 0;
            }

            return compareResult;
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
            //return compareResult / (Math.Sqrt(aSquare) * Math.Sqrt(bSquare)) * -1 + 1; // translate to 0 (the same) - 2 (the opposite)
            return compareResult / (Math.Sqrt(aSquare) * Math.Sqrt(bSquare)); // translate to 0 (the same) - 2 (the opposite)
        }

        public double unWightedHammingCompareFeatureVectors(Matrix<float> fv1, Matrix<float> fv2) 
        {
            double result = 0;
            double a = 0, b = 0, c = 0, d = 0;
            for (int i = 0; i < fv1.Cols; i++)
            {
                a += Math.Sign(fv1[0, i]) - Math.Sign(fv1[0, i] * fv2[0, i]);
                b += Math.Sign(fv2[0, i]) - Math.Sign(fv2[0, i] * fv1[0, i]);
                c += Math.Sign(fv1[0, i]);
                d += Math.Sign(fv2[0, i]);
            }
            double e = (a / c);
            double f = (b / d);
            result = e+f;
            return result;
        }

        public double WightedHammingCompareFeatureVectors(Matrix<float> fv1, Matrix<float> fv2) 
        {
            double result = 0;
            double a = 0, b = 0, c = 0, d = 0;
            for (int i = 0; i < fv1.Cols; i++)
            {
                a += fv1[0, i] * (1 - Math.Sign(fv1[0, i] * fv2[0, i]));
                b += fv2[0, i] * (1 - Math.Sign(fv2[0, i] * fv1[0, i]));
                c += fv1[0, i];
                d += fv2[0, i];
            }
            double e = (a / c);
            double f = (b / d);
            result = e + f;
            return result;
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

            for (int i = 0; i < m.Data.Length; i++)
            {
                sb.Append(m.Data[0, i] + ",");
            }

            System.IO.File.WriteAllText(@"C:\Users\ramtinb\Source\Strabo\data\_" + n + ".csv", sb.ToString());

            return m;
        }

        public List<List<double>> analysisColorSimilarityOfLayers(int bestK, string dir, string fileFormat)
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


            Dictionary<string, double[]> colorCompare = new Dictionary<string, double[]>();
            List<KeyValuePair<int, int>> pairList = new List<KeyValuePair<int, int>>(bestK);
            List<List<double>> unnormalizedList = new List<List<double>>();

            double globalMin = double.MaxValue;
            double globalMax = double.MinValue;

            for (int i = 0; i < bestK; i++)
            {
                double min = double.MaxValue;
                int minI = i;

                List<double> row = new List<double>();

                for (int j = 0; j < bestK; j++)
                {
                    string lfn = dir + String.Format(fileFormat, i);
                    string rgn = dir + String.Format(fileFormat, j);
                    double d = findColorSimilarity(lfn, i, rgn, j);

                    sbCsv.Append(i + "," + j + "," + d + Environment.NewLine);
                    sbTxt.Append(d + ",");

                    if (!colorCompare.ContainsKey(lfn))
                    {
                        colorCompare.Add(lfn, getLayerColorHSV(lfn, i));
                    }

                    row.Add(d);
                    if (d < globalMin)
                        globalMin = d;

                    if (d> globalMax)
                        globalMax = d;

                    if (d < min && d > 0)
                    {
                        min = d;
                        minI = j;
                    }
                }
                sbTxt = sbTxt.Remove(sbTxt.Length - 1, 1);
                sbTxt.Append(Environment.NewLine);

                unnormalizedList.Add(row);

                pairList.Add(new KeyValuePair<int, int>(i, minI));
            }
            //Save report and export
            System.IO.File.WriteAllText(dir + "layersColorComparison_" + bestK + ".csv", sbCsv.ToString());
            System.IO.File.WriteAllText(dir + "layersColorComparison_" + bestK + ".txt", sbTxt.ToString());

            List<List<double>> normalizedList = new List<List<double>>();
            for (int i = 0; i < bestK; i++)
            {
                normalizedList.Add(SimplyNormalization(unnormalizedList[i],globalMin,globalMax));
            }
            //String sss = "H,S,V" + Environment.NewLine;
            //var en = colorCompare.GetEnumerator();
            //while (en.MoveNext())
            //{
            //    sss += en.Current.Value[0] + "," + en.Current.Value[1] + "," + en.Current.Value[2] + Environment.NewLine;
            //}
            //System.IO.File.AppendAllText(dir + @"\\color.csv", sss);

            return normalizedList;
        }

        public List<List<double>> analysisSpatialSimilarityOfLayers(int bestK, string dir, string fileFormat)
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
            List<List<double>> unnormalizedList = new List<List<double>>();

            double globalMin = double.MaxValue;
            double globalMax = double.MinValue;

            for (int i = 0; i < bestK; i++)
            {
                double min = double.MaxValue;
                int minI = i;

                List<double> row = new List<double>();

                for (int j = 0; j < bestK; j++)
                {
                    //if (i == j) continue;
                    string lfn = dir + String.Format(fileFormat, i);
                    string rgn = dir + String.Format(fileFormat, j);
                    double d = findCosineSimilarity(lfn, i, rgn, j);

                    d = (d < 0.0000000001) ? 0 : d;
                    d = (double.IsNaN(d)) ? 0 : d;

                    sbCsv.Append(i + "," + j + "," + d + Environment.NewLine);
                    sbTxt.Append(d + ",");

                    row.Add(d);

                    if (d < globalMin)
                        globalMin = d;

                    if (d > globalMax)
                        globalMax = d;

                    if (d < min && d > 0)
                    {
                        min = d;
                        minI = j;
                    }
                }
                sbTxt = sbTxt.Remove(sbTxt.Length - 1, 1);
                sbTxt.Append(Environment.NewLine);

                unnormalizedList.Add(row);

                pairList.Add(new KeyValuePair<int, int>(i, minI));
            }

            List<List<double>> normalizedList = new List<List<double>>();
            for (int i = 0; i < bestK; i++)
            {
                normalizedList.Add(SimplyNormalization(unnormalizedList[i], globalMin, globalMax));
            }

            //Save report and export
            System.IO.File.WriteAllText(dir + "layersSpatialityComparison_" + bestK + ".csv", sbCsv.ToString());
            System.IO.File.WriteAllText(dir + "layersSpatialityComparison_" + bestK + ".txt", sbTxt.ToString());

            return normalizedList;
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

        public double findColorSimilarity(string layer1Fn, int ln, string layer2Fn, int rn)
        {
            double[] d1 = getLayerColorHSV(layer1Fn, ln);
            double[] d2 = getLayerColorHSV(layer2Fn, rn);
            double d = Math.Sqrt(Math.Pow(d2[0] - d1[0], 2) + Math.Pow(d2[1] - d1[1], 2) + Math.Pow(d2[2] - d1[2], 2));
            return d;
        }

        public double[] getLayerColorHSV(string layerPath, int n)
        {
            Bitmap srcimg = new Bitmap(layerPath);
            int[] pixels = ImageUtils.BitmapToArray1DIntRGB(srcimg);
            ColorHistogram colorHist = new ColorHistogram(pixels);
            int[] color_array = colorHist.getColorArray();

            String st = color_array[1].ToString();

            Color c = getColor(int.Parse(st));
            //Hsv hsv = new Hsv(c.GetHue(),c.GetSaturation(),c.GetBrightness());
            //double[] f = { hsv.Hue, hsv.Satuation, hsv.Value };
            //double[] f = { c.GetHue(), c.GetSaturation() ,c.GetBrightness() };




            int max = Math.Max(c.R, Math.Max(c.G, c.B));
            int min = Math.Min(c.R, Math.Min(c.G, c.B));

            double hue = c.GetHue();
            double saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            double value = max / 255d;

            double[] f = { hue, saturation * 100, value * 100 };
            return f;
        }

        public Color getColor(int num)
        {
            int r = num / (256 * 256);
            int b = (num - (r * 256 * 256)) / 256;
            int g = num - (r * 256 * 256) - (b * 256);

            Color c = Color.FromArgb(r,b,g);

            return c;
        }

        public List<double> GaussianNormalization(List<double> simList)
        {
            double mean = 0;
            for (int i = 0; i < simList.Count; i++)
            {
                mean += simList[i];
            }
            mean = mean / simList.Count;
            double std = 0;
            for (int i = 0; i < simList.Count; i++)
                std += Math.Pow(simList[i] - mean, 2);
            std = Math.Sqrt(std);

            for (int i = 0; i < simList.Count; i++)
                simList[i] = (simList[i] - mean) / (std);

            return simList;
        }

        public List<double> SimplyNormalization( List<double> simList,double min,double max)
        {
      
            for (int i = 0; i < simList.Count; i++)
                simList[i] = (simList[i] - min) / (max - min);

            return simList;
        }

    }
}
