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
using System.Text;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Desktop;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.DataSourcesFile;
using System.Drawing;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Editor;
using System.Windows;
using Strabo.Core.OCR;
using System.Windows.Forms;
using Strabo.Core.Utility;



namespace ArcStrabo
{
    public class ButtonTextExtraction : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        protected override void OnClick()
        {
            string straboPath = Environment.GetEnvironmentVariable(ArcStrabo2Extension.EnvironmentVariableSTRABO_HOME, EnvironmentVariableTarget.Machine);
            string tessPath = Environment.GetEnvironmentVariable(ArcStrabo2Extension.EnvironmentVariableTESS_DATA, EnvironmentVariableTarget.Machine);

            if (String.IsNullOrEmpty(straboPath) == true)
            {
                MessageBox.Show(ArcStrabo2Extension.ErrorMsgNoStraboHome);
                return;
            }
            if (String.IsNullOrEmpty(tessPath) == true)
            {
                MessageBox.Show(ArcStrabo2Extension.ErrorMsgNoTess_Data);
                return;
            }

            bool Initialize_straboPath_Correct = initialize_straboPath_directories(straboPath);

            if (Initialize_straboPath_Correct == false)
            {
                MessageBox.Show(ArcStrabo2Extension.ErrorMsgNoStraboHomeWritePermission);
                return;
            }

            

            #region Text Recognition
            ////Save Positive and Negative Layer and making GeoJason File
            ComboBoxLayerSelector layerNameCobo = ComboBoxLayerSelector.GetLayerNameComboBox();

            RasterLayer rasterlayer = new RasterLayer();
            rasterlayer = ((RasterLayer)layerNameCobo.GetSelectedLayer());

            ArcStraboObject arcStraboObject = new ArcStraboObject();
            string input_data_source_directory = rasterlayer.FilePath;// arcStraboObject.findRasterLayerPath();

            //string rasterPath = arcStraboObject.CreateDirectory(dir, "Data");
            //string logPath = arcStraboObject.CreateDirectory(dir, "Log");

            //Log.SetLogDir(System.IO.Path.GetTempPath());
            Log.SetLogDir(ArcStrabo2Extension.Log_Path);
            //Log.SetOutputDir(System.IO.Path.GetTempPath());
            Log.SetOutputDir(ArcStrabo2Extension.Log_Path);

            Log.WriteLine(DateTime.Now+": MakingGeoJsonFile Mathod Start  SIMA");

            IMap map = ArcMap.Document.FocusMap;
            //arcStraboObject.MakingTextGeoJsonFile(rasterPath);
            arcStraboObject.MakingTextLabelGeoJsonFile(ArcStrabo2Extension.Text_Result_Path);
            Log.WriteLine("MakingGeoJsonFile Mathod Finish");

            ////run TextExtraction Layer from Strao.core and load raster Layer
            Log.WriteLine("textLayerExtract Mathod Start SIMA");
            arcStraboObject.textLayerExtract(input_data_source_directory, ArcStrabo2Extension.Text_Result_Path);
            Log.WriteLine("textLayerExtract Mathod Finish");

            Log.WriteLine("AddRasterLayer Mathod Start SIMA");
            //arcStraboObject.AddRasterLayer(rasterPath, "Result.png");
            arcStraboObject.AddRasterLayer(ArcStrabo2Extension.Text_Result_Path, ArcStrabo2Extension.TextLayerPNGFileName);
            //string fn = "\\Result.png";
            Log.WriteLine("AddRasterLayer Mathod Finish");

            ///run TextIdentifier Method
            Log.WriteLine("textIndentification Mathod Start SIMA");
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;
            //arcStraboObject.textIndentification(rasterPath + "\\", fn);
            arcStraboObject.textIndentification(ArcStrabo2Extension.Text_Result_Path + "\\", ArcStrabo2Extension.Intermediate_Result_Path + "\\", ArcStrabo2Extension.TextLayerPNGFileName);
            System.Windows.Forms.Cursor.Current = Cursors.Default;
            Log.WriteLine("textIndentification Mathod Finish");

            ///OCR Part
            Log.WriteLine("ExtractTextToGEOJSON Mathod Start SANJUALI");
            System.Windows.Forms.Cursor.Current = Cursors.AppStarting;
            Strabo.Core.OCR.WrapperTesseract eng = new Strabo.Core.OCR.WrapperTesseract(tessPath);
            //string OCRPath = rasterPath + "\\Results";
            //string OCRPath = ArcStrabo2Extension.Text_Result_Path;
            eng.ExtractTextToGEOJSON(ArcStrabo2Extension.Intermediate_Result_Path,ArcStrabo2Extension.Text_Result_Path,ArcStrabo2Extension.TesseractResultsJSONFileName);
            Log.WriteLine("ExtractTextToGEOJSON Mathod Finish");
            System.Windows.Forms.Cursor.Current = Cursors.Default;

            /////Add Polygon of OCR Layer
            Log.WriteLine("CreateFeatureClassWithFields Mathod Start SIMA");
            //IWorkspace workspace = arcStraboObject.CreateShapefileWorkspace(rasterPath);
            IWorkspace workspace = arcStraboObject.CreateShapefileWorkspace(ArcStrabo2Extension.Text_Result_Path);
            IFeatureWorkspace featureworkspace = (IFeatureWorkspace)workspace;
            string tesseDataPath = ArcStrabo2Extension.Text_Result_Path + "\\" + ArcStrabo2Extension.TesseractResultsJSONFileName;
            IFeatureClass featureClass = arcStraboObject.CreateFeatureClassWithFields("OCRLayer", featureworkspace, tesseDataPath);
            IFeatureLayer featureLayer = arcStraboObject.CreateFeatureLayer(featureClass);
            Log.WriteLine("CreateFeatureClassWithFields Mathod Finish");

            Log.WriteLine("AddPolygon Mathod Start");
            arcStraboObject.AddPolygon(featureLayer, featureworkspace, tesseDataPath);
            Log.WriteLine("AddPolygon Mathod Finish");

            Log.ArchiveLog();
            #endregion
        }

        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }

        protected bool initialize_straboPath_directories(string _straboPath)
        {
            // check whether straboPath exists
            if (string.IsNullOrEmpty(_straboPath))
            {
                return false;
            }
            ArcStrabo2Extension.Output_Path = _straboPath + ArcStrabo2Extension.Output_Path;
            ArcStrabo2Extension.Text_Result_Path = ArcStrabo2Extension.Output_Path + ArcStrabo2Extension.Text_Result_Path;
            ArcStrabo2Extension.Log_Path = ArcStrabo2Extension.Output_Path + ArcStrabo2Extension.Log_Path;
            ArcStrabo2Extension.Intermediate_Result_Path = ArcStrabo2Extension.Output_Path + ArcStrabo2Extension.Intermediate_Result_Path;
            try
            {
                // check Output_Path
                if (Directory.Exists(ArcStrabo2Extension.Output_Path))
                {
                    DirectoryInfo TheFolder = new DirectoryInfo(ArcStrabo2Extension.Output_Path);
                    if (TheFolder.GetFiles() != null)
                        foreach (FileInfo NextFile in TheFolder.GetFiles())
                            File.Delete(NextFile.FullName);
                }
                else
                {
                    Directory.CreateDirectory(ArcStrabo2Extension.Output_Path);
                }
                // check Text_Result_Path
                if (Directory.Exists(ArcStrabo2Extension.Text_Result_Path))
                {
                    // delete any existing files
                    DirectoryInfo TheFolder = new DirectoryInfo(ArcStrabo2Extension.Text_Result_Path);
                    if (TheFolder.GetFiles() != null)
                        foreach (FileInfo NextFile in TheFolder.GetFiles())
                            File.Delete(NextFile.FullName);
                }
                else
                {
                    // create folder
                    Directory.CreateDirectory(ArcStrabo2Extension.Text_Result_Path);
                }
                // check Log_Path
                if (Directory.Exists(ArcStrabo2Extension.Log_Path))
                {
                    DirectoryInfo TheFolder = new DirectoryInfo(ArcStrabo2Extension.Log_Path);
                    if (TheFolder.GetFiles() != null)
                        foreach (FileInfo NextFile in TheFolder.GetFiles())
                            File.Delete(NextFile.FullName);
                }
                else
                {
                    Directory.CreateDirectory(ArcStrabo2Extension.Log_Path);
                }
                // check Intermediate_Result_Path
                if (Directory.Exists(ArcStrabo2Extension.Intermediate_Result_Path))
                {
                    // delete any existing files
                    DirectoryInfo TheFolder = new DirectoryInfo(ArcStrabo2Extension.Intermediate_Result_Path);
                    if (TheFolder.GetFiles() != null)
                        foreach (FileInfo NextFile in TheFolder.GetFiles())
                            File.Delete(NextFile.FullName);
                }
                else
                {
                    // create folder
                    Directory.CreateDirectory(ArcStrabo2Extension.Intermediate_Result_Path);
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected bool initialize_tessPath_directories(string _tessPath)
        {
            // check whether tessPath exists
            if (string.IsNullOrEmpty(_tessPath))
            {
                return false;
            }

            try
            {
                if (Directory.Exists(ArcStrabo2Extension.Tessdata_Path))
                {
                    DirectoryInfo TheFolder = new DirectoryInfo(ArcStrabo2Extension.Tessdata_Path);
                    if (TheFolder.GetFiles() != null)
                        foreach (FileInfo NextFile in TheFolder.GetFiles())
                            File.Delete(NextFile.FullName);
                }
                else
                {
                    ArcStrabo2Extension.Tessdata_Path = _tessPath + "\\tessdata";
                    Directory.CreateDirectory(ArcStrabo2Extension.Tessdata_Path);
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

    }
}
