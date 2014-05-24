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

            #region Text Recognition
            ////Save Positive and Negative Layer and making GeoJason File

            ArcStraboObject arcStraboObject = new ArcStraboObject();
            string dir = arcStraboObject.findRasterLayerPath();
            string rasterPath = arcStraboObject.CreateDirectory(dir, "Data");
            string logPath = arcStraboObject.CreateDirectory(dir, "Log");

            Log.SetLogDir(System.IO.Path.GetTempPath());
            Log.SetOutputDir(System.IO.Path.GetTempPath());
            Log.SetStartTime();
            Log.WriteLine("Start");
            Log.WriteLine("MakingGeoJsonFile Mathod Start  SIMA");
            IMap map = ArcMap.Document.FocusMap;
            arcStraboObject.MakingTextGeoJsonFile(rasterPath);
            Log.WriteLine("MakingGeoJsonFile Mathod Finish");

            ////run TextExtraction Layer from Strao.core and load raster Layer
            Log.WriteLine("textLayerExtract Mathod Start SIMA");
            arcStraboObject.textLayerExtract(dir);
            Log.WriteLine("textLayerExtract Mathod Finish");

            Log.WriteLine("AddRasterLayer Mathod Start SIMA");
            arcStraboObject.AddRasterLayer(rasterPath, "Result.png");
            string fn = "\\Result.png";
            Log.WriteLine("AddRasterLayer Mathod Finish");

            ///run TextIdentifier Method
            Log.WriteLine("textIndentification Mathod Start SIMA");
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;
            arcStraboObject.textIndentification(rasterPath + "\\", fn);
            System.Windows.Forms.Cursor.Current = Cursors.Default;
            Log.WriteLine("textIndentification Mathod Finish");

            ///OCR Part
            Log.WriteLine("ExtractTextToGEOJSON Mathod Start SANJUALI");
            System.Windows.Forms.Cursor.Current = Cursors.AppStarting;
            WrapperTesseract eng = new WrapperTesseract();
            string OCRPath = rasterPath + "\\Results";
            eng.ExtractTextToGEOJSON(OCRPath);
            Log.WriteLine("ExtractTextToGEOJSON Mathod Finish");
            System.Windows.Forms.Cursor.Current = Cursors.Default;

            /////Add Polygon of OCR Layer
            Log.WriteLine("CreateFeatureClassWithFields Mathod Start SIMA");
            IWorkspace workspace = arcStraboObject.CreateShapefileWorkspace(rasterPath);
            IFeatureWorkspace featureworkspace = (IFeatureWorkspace)workspace;
            string tesseDataPath = OCRPath + "\\tessearct_geojson.json";
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
    }
}
