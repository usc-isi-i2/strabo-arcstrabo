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
//using DBpediaTest;


namespace ArcStrabo10
{
    public class ButtonSymbolRecognition : ESRI.ArcGIS.Desktop.AddIns.Button
    {

        protected override void OnClick()
        {
            #region Symbel Recognition
            ArcStrabo10.ArcStraboObject.RasterMapInfo rasterInfo = new ArcStrabo10.ArcStraboObject.RasterMapInfo();
            string dir = "";
            ArcStraboObject arcStraboObject = new ArcStraboObject();
            arcStraboObject.symbolFindRasterLayerPath(rasterInfo);
                       
            string logPath = arcStraboObject.CreateDirectory(rasterInfo.rasterPath, "Log");

            Log.SetLogDir(System.IO.Path.GetTempPath());
            Log.SetOutputDir(System.IO.Path.GetTempPath());
            Log.SetStartTime();
            Log.WriteLine("Start");
            Log.WriteLine("MakingGeoJsonFile Mathod Start  SIMA");
            IMap map = ArcMap.Document.FocusMap;
            arcStraboObject.MakingSymbolGeoJsonFile();
            Log.WriteLine("MakingGeoJsonFile Mathod Finish");

            ////run TextExtraction Layer from Strao.core and load raster Layer
            Log.WriteLine("textLayerExtract Mathod Start SIMA");

            dir =rasterInfo.ratserImgPath;
            arcStraboObject.SymbolExtraction();
            Log.WriteLine("textLayerExtract Mathod Finish");

            /////Add Polygon of OCR Layer
            Log.WriteLine("CreateFeatureClassWithFields Mathod Start SIMA");
            IWorkspace workspace = arcStraboObject.symbolCreateShapefileWorkspace();
            IFeatureWorkspace featureworkspace = (IFeatureWorkspace)workspace;

            IFeatureClass featureClass = arcStraboObject.CreateFeatureClassWithFieldsGeoRef(ArcStrabo10Extension.TextLayerOCRShapefile, featureworkspace);
            IFeatureLayer featureLayer = arcStraboObject.CreateFeatureLayer(featureClass);
            Log.WriteLine("CreateFeatureClassWithFields Mathod Finish");

            Log.WriteLine("AddPolygon Mathod Start");
            arcStraboObject.addPoligonGeorefrenced(featureLayer, featureworkspace);
            Log.WriteLine("AddPolygon Mathod Finish");


            #endregion
        }

        protected override void OnUpdate()
        {
        }
        public ButtonSymbolRecognition()
        {

        }
    }
}
