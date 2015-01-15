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



namespace ArcStrabo10
{
    public class ButtonTextExtraction : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        protected override void OnClick()
        {
            string straboPath = Environment.GetEnvironmentVariable(ArcStrabo10Extension.EnvironmentVariableSTRABO_HOME, EnvironmentVariableTarget.User);
            string tessPath = Environment.GetEnvironmentVariable(ArcStrabo10Extension.EnvironmentVariableTESS_DATA, EnvironmentVariableTarget.User);

            if (ArcStrabo10Extension.PathSet == false)
            {
               
                if (String.IsNullOrEmpty(straboPath) == true)
                {
                    MessageBox.Show(ArcStrabo10Extension.ErrorMsgNoStraboHome);
                    return;
                }
                if (String.IsNullOrEmpty(tessPath) == true)
                {
                    MessageBox.Show(ArcStrabo10Extension.ErrorMsgNoTess_Data);
                    return;
                }

                ////Initialize directories
                bool Initialize_straboPath_Correct = ArcStrabo10Extension.initialize_straboPath_directories(straboPath);

                if (Initialize_straboPath_Correct == false)
                {
                    MessageBox.Show(ArcStrabo10Extension.ErrorMsgNoStraboHomeWritePermission);
                    return;
                }
                ArcStrabo10Extension.PathSet = true;
            }    

            #region Text Recognition
            ////Save Positive and Negative Layer and making GeoJason File
            ComboBoxLayerSelector layerNameCombo = ComboBoxLayerSelector.GetLayerNameComboBox();
            
            ////Select correct raster map layer
            RasterLayer rasterlayer = new RasterLayer();
            rasterlayer = ((RasterLayer)layerNameCombo.GetSelectedLayer());
            string input_data_source_directory;
            try
            {
                input_data_source_directory = rasterlayer.FilePath; 
            }
            catch (Exception)
            {
                // Handle no input map error
                MessageBox.Show(ArcStrabo10Extension.ErrorMsgNoInputMap, "Input Map Error", MessageBoxButtons.OK);
                return;
            }

            ////Select language from combo box in toolbar
            ComboBoxLanguageSelector languageNameCombo = ComboBoxLanguageSelector.GetLanguageNameComboBox();
            string lng = languageNameCombo.Get_selected_language();
            if (lng == null)
            {
                MessageBox.Show(ArcStrabo10Extension.ErrorMsgNoInputLanguage, "Input Language Error", MessageBoxButtons.OK);
                return;
            }

            ////Set Log Directory Path
            Log.SetLogDir(ArcStrabo10Extension.Log_Path);
            Log.SetOutputDir(ArcStrabo10Extension.Log_Path);

            Log.WriteLine("MakingTextLabelGeoJsonFile Method Start SIMA");
            IMap map = ArcMap.Document.FocusMap;
            ArcStraboObject arcStraboObject = new ArcStraboObject();
            arcStraboObject.MakingTextLabelGeoJsonFile(ArcStrabo10Extension.Text_Result_Path);
            Log.WriteLine("MakingTextLabelGeoJsonFile Method Finish");

            ////Run TextExtraction Layer from Strabo.core and load raster Layer
            Log.WriteLine("textLayerExtract Medthod Start SIMA");
            arcStraboObject.textLayerExtract(input_data_source_directory, ArcStrabo10Extension.Text_Result_Path);
            Log.WriteLine("textLayerExtract Method Finish");

            Log.WriteLine("AddRasterLayer Method Start SIMA");
            arcStraboObject.AddRasterLayer(ArcStrabo10Extension.Text_Result_Path, ArcStrabo10Extension.TextLayerPNGFileName);
            Log.WriteLine("AddRasterLayer Method Finish");

            ////Run TextIdentifier Method
            Log.WriteLine("textIndentification Method Start SIMA");
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

            ///// Attempting to create cancel feature window
            //DialogResult result = MessageBox.Show("Text Recognition is running.", "Application Running", MessageBoxButtons.OKCancel);
            //if (result == DialogResult.Cancel)
            //{
            //    return;
            //}
            //else if (result == DialogResult.OK)

            arcStraboObject.textIndentification(ArcStrabo10Extension.Text_Result_Path + "\\", ArcStrabo10Extension.Intermediate_Result_Path + "\\", ArcStrabo10Extension.TextLayerPNGFileName);
            System.Windows.Forms.Cursor.Current = Cursors.Default;
            Log.WriteLine("textIndentification Method Finish");

            ////OCR Part
            Log.WriteLine("ExtractTextToGEOJSON Method Start SANJUALI");
            System.Windows.Forms.Cursor.Current = Cursors.AppStarting;

            //// Select language from combo box in toolbar
            //ComboBoxLanguageSelector languageNameCombo = ComboBoxLanguageSelector.GetLanguageNameComboBox();
            //string lng = languageNameCombo.Get_selected_language();
            //if (lng == null)
            //{
            //    MessageBox.Show(ArcStrabo10Extension.ErrorMsgNoInputLanguage, "Input Language Error", MessageBoxButtons.OK);
            //    return;
            //}
            Strabo.Core.OCR.WrapperTesseract language = new Strabo.Core.OCR.WrapperTesseract(tessPath, lng);
            /// Strabo.Core.OCR.WrapperTesseract language = new Strabo.Core.OCR.WrapperTesseract(tessPath);
            language.ExtractTextToGEOJSON(ArcStrabo10Extension.Intermediate_Result_Path,ArcStrabo10Extension.Text_Result_Path,ArcStrabo10Extension.TesseractResultsJSONFileName);
            Log.WriteLine("ExtractTextToGEOJSON Method Finish");
            System.Windows.Forms.Cursor.Current = Cursors.Default;

            ////Add Polygon of OCR Layer
            Log.WriteLine("CreateFeatureClassWithFields Method Start SIMA");
            IWorkspace workspace = arcStraboObject.CreateShapefileWorkspace(ArcStrabo10Extension.Text_Result_Path);
            IFeatureWorkspace featureworkspace = (IFeatureWorkspace)workspace;
            string tesseDataPath = ArcStrabo10Extension.Text_Result_Path + "\\" + ArcStrabo10Extension.TesseractResultsJSONFileName;

            IFeatureClass featureClass = arcStraboObject.CreateFeatureClassWithFields(ArcStrabo10Extension.TextLayerOCRShapefile, featureworkspace, tesseDataPath);
            IFeatureLayer featureLayer = arcStraboObject.CreateFeatureLayer(featureClass);
            Log.WriteLine("CreateFeatureClassWithFields Method Finish");

            Log.WriteLine("AddPolygon Method Start");
            arcStraboObject.AddPolygon(featureLayer, featureworkspace, tesseDataPath);
            Log.WriteLine("AddPolygon Method Finish");

            Log.ArchiveLog();
            MessageBox.Show("Text recognition finished!", "Done", MessageBoxButtons.OK);
            #endregion
        }

        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }

    }
}
