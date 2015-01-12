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
using System.Linq;
using System.Text;
using Emgu.CV.OCR;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using Strabo.Core.OCR;
using Strabo.Core.Utility;


namespace Strabo.Core.OCR
{
    /// <summary>
    /// Wrapper class for Tesseract OSC Engine
    /// </summary>
    public class WrapperTesseract
    {
        private Tesseract _ocr;
        
        /// <summary>
        /// Initializes and creates the tessearct engine
        /// </summary>
        public WrapperTesseract(string path, string lng)
        {
            // Variable value is set to parent directory of tessdata
            //string path = AppDomain.CurrentDomain.BaseDirectory;

            //Path should be same as TESSDATA folder
            //Environment.SetEnvironmentVariable("TESSDATA_PREFIX", path); //This path should always end with a "/" or "\", e.g., TESSDATA_PREFIX="/usr/share/tesseract-ocr/"

            _ocr = new Tesseract(path, lng, Tesseract.OcrEngineMode.OEM_TESSERACT_CUBE_COMBINED);
            ///_ocr = new Tesseract(path, "eng", Tesseract.OcrEngineMode.OEM_TESSERACT_CUBE_COMBINED);
        }

        private bool alphnumericratio(string temp)
        {
            string clean = Regex.Replace(temp, @"[^a-zA-Z0-9]", "");
            if ((double)clean.Length / (double)temp.Length <= 0.5)
                return false;
            else
                return true;
        }
        /// <summary>
        /// Extracts text from images using tesseract and creates a GEOJSON file with all the features
        /// </summary>
        /// <param name="dirPath">This is the path where all the image files will be present</param>
        public void ExtractTextToGEOJSON(string inputPath, string outputPath, string TesseractResultsJSONFileName)
        {
            string[] filePaths = Directory.GetFiles(inputPath, "*.png");
            if (filePaths.Length == 0)
                return;
            int noOfFiles = filePaths.Length;
            String[] text = new String[noOfFiles];

            float[] textCost = new float[noOfFiles];
            Bgr drawColor = new Bgr(Color.Gray);
            int[] imageIds = new int[noOfFiles];
            try
            {
                Console.WriteLine("Tessearct in progress...");
                for (int i = 0; i < noOfFiles; i++)
                {

                    string filename = Path.GetFileName(filePaths[i]);
                    String[] splitTokens = filename.Split('_');
                    if (splitTokens.Length != 11)
                        continue;
                    Image<Bgr, Byte> image = new Image<Bgr, byte>(filePaths[i]);

                    using (Image<Gray, byte> gray = image.Convert<Gray, Byte>())
                    {
                        int index = 0;
                        _ocr.Recognize(gray);
                        int charCount = 0;
                        textCost[i] = 0;
                        Tesseract.Charactor[] recog_char = _ocr.GetCharactors();

                        while (charCount < recog_char.Length)
                        {
                            text[i] += recog_char[charCount].Text;
                            textCost[i] += recog_char[charCount].Cost;
                            charCount++;
                        }
                        imageIds[i] = int.Parse(filename.Split('_')[0]);
                        index = Array.IndexOf(imageIds, imageIds[i]);
                        if (index != -1 && index != i)
                        {
                            if (textCost[index] < textCost[i])
                                imageIds[i] = -1;
                            else
                                imageIds[index] = -1;
                        }
                    }
                }
            }

            catch (Exception exception)
            {
                //MessageBox.Show(exception.Message);
                Log.WriteLine(exception.Message);
                Log.WriteLine(exception.ToString());
                throw (exception);
            }

            try
            {

                FeatureInJSON jsonFeatures = new FeatureInJSON();
                jsonFeatures.displayFieldName = "";
                jsonFeatures.geometryType = "esriGeometryPolygon";
                jsonFeatures.fieldAliases.OBJECTID = "OBJECTID";
                jsonFeatures.fieldAliases.Filename = "Filename";
                jsonFeatures.fieldAliases.Orientation = "Orientation";
                jsonFeatures.fieldAliases.Text = "Text";
                jsonFeatures.fieldAliases.Susp_char_count = "Susp_char_count";
                jsonFeatures.fieldAliases.Susp_text = "Susp_text";
                jsonFeatures.fieldAliases.Mass_centerX = "Mass_centerX";
                jsonFeatures.fieldAliases.Mass_centerY = "Mass_centerY";
                jsonFeatures.fieldAliases.Char_count = "Char_count";
                jsonFeatures.fieldAliases.DetectionCost = "DetectionCost";
                jsonFeatures.spatialReference.latestWkid = 0;
                jsonFeatures.spatialReference.wkid = 0;
                jsonFeatures.fields[0].name = "OBJECTID";
                jsonFeatures.fields[0].type = "esriFieldTypeOID";
                jsonFeatures.fields[0].alias = "OBJECTID";
                jsonFeatures.fields[0].length = 0;
                jsonFeatures.fields[1].name = "Text";
                jsonFeatures.fields[1].type = "esriFieldTypeString";
                jsonFeatures.fields[1].alias = "Text";
                jsonFeatures.fields[1].length = 80;
                jsonFeatures.fields[2].name = "Char_count";
                jsonFeatures.fields[2].type = "esriFieldTypeInteger";
                jsonFeatures.fields[2].alias = "Char_count";
                jsonFeatures.fields[2].length = 0;
                jsonFeatures.fields[3].name = "Orientation";
                jsonFeatures.fields[3].type = "esriFieldTypeDouble";
                jsonFeatures.fields[3].alias = "Orientation";
                jsonFeatures.fields[3].length = 0;
                jsonFeatures.fields[4].name = "Filename";
                jsonFeatures.fields[4].type = "esriFieldTypeString";
                jsonFeatures.fields[4].alias = "Filename";
                jsonFeatures.fields[4].length = 80;
                jsonFeatures.fields[5].name = "Susp_text";
                jsonFeatures.fields[5].type = "esriFieldTypeString";
                jsonFeatures.fields[5].alias = "Susp_text";
                jsonFeatures.fields[5].length = 80;
                jsonFeatures.fields[6].name = "Susp_char_count";
                jsonFeatures.fields[6].type = "esriFieldTypeInteger";
                jsonFeatures.fields[6].alias = "Susp_char_count";
                jsonFeatures.fields[6].length = 0;
                jsonFeatures.fields[7].name = "Mass_centerX";
                jsonFeatures.fields[7].type = "esriFieldTypeDouble";
                jsonFeatures.fields[7].alias = "Mass_centerX";
                jsonFeatures.fields[7].length = 0;
                jsonFeatures.fields[8].name = "Mass_centerY";
                jsonFeatures.fields[8].type = "esriFieldTypeDouble";
                jsonFeatures.fields[8].alias = "Mass_centerY";
                jsonFeatures.fields[8].length = 0;
                jsonFeatures.fields[9].name = "DetectionCost";
                jsonFeatures.fields[9].type = "esriFieldTypeDouble";
                jsonFeatures.fields[9].alias = "DetectionCost";
                jsonFeatures.fields[9].length = 0;

                Array filesSearchArray = (Array)filePaths;
                Console.WriteLine("Converting to JSON...");
                for (int i = 0; i < noOfFiles; i++)
                {
                    if (imageIds[i] == -1 || textCost[i] == 0 || filePaths[i] == "")
                        continue;
                    char[] separator = new char[] { '_', '.' };
                    string filename = Path.GetFileName(filePaths[i]);
                    String[] token = filename.Split(separator, StringSplitOptions.RemoveEmptyEntries);


                    /* Regular expression to match non-word characters more than 3 in the text
                     \p{L} matches any kind of letter from any langauge
                     \p{Nd} matches a digit zero through nine in any script except ideographic scripts*/
                    Regex nonWords = new Regex("[^\\p{L}\\p{Nd}]{3}[^\\p{L}\\p{Nd}]+");
                    text[i] = nonWords.Replace(text[i], "");
                    if (text[i].Contains("1 1 1")) // dashed lines
                        continue;
                    if(alphnumericratio(text[i])==false)
                        continue;
                    Features ftr = new Features();
                    ftr.attributes.OBJECTID = Convert.ToInt16(token[0]);
                    ftr.attributes.Text = text[i];
                    ftr.attributes.Char_count = Convert.ToInt16(token[2]);
                    ftr.attributes.Orientation = Convert.ToInt16(token[6]);
                    ftr.attributes.Filename = filename;
                    ftr.attributes.Susp_char_count = 0;
                    ftr.attributes.Susp_text = "";
                    ftr.attributes.Mass_centerX = Convert.ToInt16(token[3]);
                    ftr.attributes.Mass_centerY = Convert.ToInt16(token[4]);
                    ftr.attributes.DetectionCost = textCost[i];
                    int x = Convert.ToInt16(token[7]);
                    int y = Convert.ToInt16(token[8]);
                    int w = Convert.ToInt16(token[9]);
                    int h = Convert.ToInt16(token[10]);
                    ftr.geometry.rings[0, 0, 0] = x;
                    ftr.geometry.rings[0, 0, 1] = -y;
                    ftr.geometry.rings[0, 1, 0] = x + w;
                    ftr.geometry.rings[0, 1, 1] = -y;
                    ftr.geometry.rings[0, 2, 0] = x + w;
                    ftr.geometry.rings[0, 2, 1] = -y - h;
                    ftr.geometry.rings[0, 3, 0] = x;
                    ftr.geometry.rings[0, 3, 1] = -y - h;
                    ftr.geometry.rings[0, 4, 0] = x;
                    ftr.geometry.rings[0, 4, 1] = -y;


                    jsonFeatures.features.Add(ftr);


                    GeoJson geoJson = new GeoJson();
                    geoJson.featureInJson = jsonFeatures;
                    geoJson.writeJsonFile(outputPath + "\\" + TesseractResultsJSONFileName);
                }

            }
            catch (Exception exception)
            {
                //MessageBox.Show(exception.Message);
                Log.WriteLine(exception.Message);
                Log.WriteLine(exception.ToString());
                throw (exception);
            }
        }
    }


}
