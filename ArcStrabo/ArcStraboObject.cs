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
/****************************************/
/** Project| connecting Symbol and text**/
/** Usage  | Extraction Component to   **/
/**        |  Strabo and ArcMap        **/
/**        |                           **/
/** Author | Sima Moghaddam            **/
/** Date   | January to May 2014       **/
/****************************************/
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
using System;
using System.IO;
using System.Text;
using Strabo.Core;
using Strabo.Core.TextLayerExtraction;
using Newtonsoft.Json;
using Strabo.Core.OCR;
using System.Collections.Generic;
using Emgu.CV;
using Strabo.Core.Utility;
using Emgu.CV.Structure;
using Strabo.Core.TextDetection;
using System.Diagnostics;
using System.Collections;
using Strabo.Core.SymbolRecognition;


namespace ArcStrabo
{
    class ArcStraboObject
    {
        public class RasterMapInfo
        {
            private string type;
            private string path;
            private int height;
            private int width;
            private double topleftX;
            private double topleftY;
            private double downrightX;
            private double downrightY;
            private string imgPath;
            private string dataPath;
            private string inPath;
            private int negativeY = 1;

            public string rasterType { get { return type; } set { type = value; } }
            public string rasterPath { get { return path; } set { path = value; } }
            public string rasterIn { get { return inPath; } set { inPath = value; } }
            public string rasterData { get { return dataPath; } set { dataPath = value; } }

            public int rasterHeight { get { return height; } set { height = value; } }
            public int rasterWidth { get { return width; } set { width = value; } }
            public double rasterTopLeftX { get { return topleftX; } set { topleftX = value; } }
            public double rasterTopLeftY { get { return topleftY; } set { topleftY = value; } }
            public double rasterDownRightX { get { return downrightX; } set { downrightX = value; } }
            public double rasterDownRightY { get { return downrightY; } set { downrightY = value; } }
            public string ratserImgPath { get { return imgPath; } set { imgPath = value; } }
            public int ratserNegative { get { return negativeY; } set { negativeY = value; } }

        }
        FeatureInJSON _featureInJSON;
        public class CoordinatePoint
        {
            public double lat;
            public double lng;
            public string URI;
            public string type;
            public CoordinatePoint()
            {
                lat = lng = 0;
                URI = type = "";
            }
        }
        public class Points
        {
            public double leftTopX, leftTopY;
            public double rightTopX, rightTopY;
            public double leftDownX, leftDownY;
            public double rightDownX, rightDownY;

            public string URI;
            public Points()
            {
                leftDownY = leftDownX = leftTopX = leftTopY = rightDownX = rightDownY =
                    rightTopX = rightTopY = 0;
                URI = "";
            }
        }
        public Points[] pointSet;
        public Points[] lngLatSet;


        private RasterMapInfo _rasterInfo;
        public string findRasterLayerPath()
        {
            string path = "";
            IMap map = ArcMap.Document.FocusMap;

            try
            {


                IEnumLayer enumLayer = map.get_Layers(null, true);
                ILayer layer = enumLayer.Next();
                while (layer != null)
                {
                    IRasterLayer rasterLayer;
                    //((IDataset)layer).Workspace.
                    try
                    {
                        rasterLayer = (IRasterLayer)layer;
                        if (rasterLayer.Name != "SymbolPositiveLabel" && rasterLayer.Name != "TextPositiveLabel" && rasterLayer.Name != "TextNegativeLabel" && rasterLayer.Name != "OCRLayer")

                            path = rasterLayer.FilePath;
                    }
                    catch (Exception e)
                    {

                        // throw;
                    }
                    layer = enumLayer.Next();
                }
            }
            catch (Exception e)
            {

                Log.WriteLine(e.Message);
            }
            return path;

        }
        public void symbolFindRasterLayerPath(RasterMapInfo rasterInfo)
        {
            IMap map = ArcMap.Document.FocusMap;

            try
            {


                IEnumLayer enumLayer = map.get_Layers(null, true);
                ILayer layer = enumLayer.Next();
                while (layer != null)
                {
                    IRasterLayer rasterLayer;
                    try
                    {
                        rasterLayer = (IRasterLayer)layer;
                        if (rasterLayer.Name!= "SymbolPositiveLabel" && rasterLayer.Name != "TextPositiveLabel" && rasterLayer.Name != "TextNegativeLabel" && rasterLayer.Name != "OCRLayer")
                        {
                            string dir = rasterInfo.rasterPath = rasterLayer.FilePath;
                            rasterInfo.rasterType = rasterLayer.VisibleExtent.SpatialReference.Name;
                            rasterInfo.rasterTopLeftX = rasterLayer.AreaOfInterest.UpperLeft.X;
                            rasterInfo.rasterTopLeftY = rasterLayer.AreaOfInterest.UpperLeft.Y;
                            rasterInfo.rasterDownRightX = rasterLayer.AreaOfInterest.LowerRight.X;
                            rasterInfo.rasterDownRightY = rasterLayer.AreaOfInterest.LowerRight.Y;

                            string sourceImageDir = CreateDirectory(dir, "Data");
                            rasterInfo.rasterData = sourceImageDir;
                            sourceImageDir = sourceImageDir + "//in";
                            rasterInfo.rasterIn = sourceImageDir;
                            if (!System.IO.Directory.Exists(sourceImageDir))
                                System.IO.Directory.CreateDirectory(sourceImageDir);
                            Image<Bgr, Byte> srcImage = new Image<Bgr, byte>(dir);
                            Bitmap srcimg = new Bitmap(dir);
                          

                            if (rasterInfo.rasterType == "Unknown")
                            {
                                rasterInfo.rasterWidth = int.Parse(Math.Round(rasterLayer.AreaOfInterest.Width).ToString());
                                rasterInfo.rasterHeight = int.Parse(Math.Round(rasterLayer.AreaOfInterest.Height).ToString());

                                rasterInfo.ratserImgPath = rasterLayer.FilePath;
                            }
                            else
                            {
                                rasterInfo.rasterWidth = srcimg.Width;
                                rasterInfo.rasterHeight = srcimg.Height;
                                srcimg.Save(sourceImageDir + "//test.png");
                                rasterInfo.ratserImgPath = sourceImageDir + "//test.png";
                            }

                            _rasterInfo = rasterInfo;

                        }

                    }
                    catch (Exception e)
                    {

                        // throw;
                    }
                    layer = enumLayer.Next();
                }
            }
            catch (Exception e)
            {

                Log.WriteLine(e.Message);
            }


        }

        /// <summary>
        /// access to the positive and negative layer and save them on the GeoJson File
        /// </summary>
        public void MakingTextGeoJsonFile(string dir)
        {


            ///////////////////////////////////////////////////////////////
            //Setting current Map to access Layers, Feature Class and Features
            //build string builder to write on the ImageResultfile
            ///////////////////////////////////////////////////////////////
            string path = "";
            IMap map = ArcMap.Document.FocusMap;
            //GeoJson geoJson;
            IEnumLayer enumLayer = map.get_Layers(null, true);
            ILayer layer = enumLayer.Next();
            //((IDataset)layer).Workspace.
            IFeatureLayer featureLayer;
            IFeature iFeature;
            try
            {
                while (layer != null)
                {




                    if (layer.Name == "TextPositiveLabel" || layer.Name == "TextNegativeLabel")
                    {
                        GeoJson geoJson = new GeoJson();
                        geoJson.featureInJson = new FeatureInJSON();
                        geoJson.featureInJson.displayFieldName = layer.Name;
                        geoJson.featureInJson.fieldAliases = new FieldAliases();

                        //////////////////////////Set Alias Feature of the Layer////////////////////
                        geoJson.featureInJson.fieldAliases.Mass_centerX = "Mass_centerX";
                        geoJson.featureInJson.fieldAliases.Mass_centerY = "Mass_centerY";
                        geoJson.featureInJson.fieldAliases.OBJECTID = "OBJECTID";
                        geoJson.featureInJson.fieldAliases.Orientation = "Orientation";
                        geoJson.featureInJson.fieldAliases.Susp_char_count = "Susp_char_count";
                        geoJson.featureInJson.fieldAliases.Susp_text = "Susp_text";
                        geoJson.featureInJson.fieldAliases.Text = "Text";
                        geoJson.featureInJson.fieldAliases.Char_count = "Char_count";
                        geoJson.featureInJson.fieldAliases.Filename = "Filename";

                        //////////////////////Set Fields of the current Layer///////////////////////

                        geoJson.featureInJson.fields = new Field_info[9];
                        setGeoJASONFeilds("OBJECTID", esriFieldType.esriFieldTypeOID, geoJson.featureInJson.fields[0]);
                        setGeoJASONFeilds("Text", esriFieldType.esriFieldTypeString, geoJson.featureInJson.fields[1]);
                        setGeoJASONFeilds("Char_count", esriFieldType.esriFieldTypeInteger, geoJson.featureInJson.fields[2]);
                        setGeoJASONFeilds("Orientation", esriFieldType.esriFieldTypeDouble, geoJson.featureInJson.fields[3]);
                        setGeoJASONFeilds("Filename", esriFieldType.esriFieldTypeString, geoJson.featureInJson.fields[4]);
                        setGeoJASONFeilds("Susp_text", esriFieldType.esriFieldTypeString, geoJson.featureInJson.fields[5]);
                        setGeoJASONFeilds("Susp_char_count", esriFieldType.esriFieldTypeInteger, geoJson.featureInJson.fields[6]);
                        setGeoJASONFeilds("Mass_centerX", esriFieldType.esriFieldTypeDouble, geoJson.featureInJson.fields[7]);
                        setGeoJASONFeilds("Mass_centerY", esriFieldType.esriFieldTypeDouble, geoJson.featureInJson.fields[8]);





                        //************************GeoProjectChange***SIIIIIIMA***********************************************************
                        if (layer.Name == "TextPositiveLabel")
                            path = dir + "\\PositiveLayerInfo.json";






                        else
                            path = dir + "\\NegativeLayerInfo.json";


                        featureLayer = (IFeatureLayer)layer;
                        int count = featureLayer.FeatureClass.FeatureCount(null);
                        for (int j = 1; j <= count; j++)
                        {
                            iFeature = featureLayer.FeatureClass.GetFeature(j);
                            geoJson.featureInJson.features.Add(new Features());
                            int i = j - 1;
                            geoJson.featureInJson.features[i].attributes = new Attributes();
                            geoJson.featureInJson.features[i].attributes.OBJECTID = int.Parse(iFeature.get_Value(0).ToString());
                            geoJson.featureInJson.features[i].geometry = new Strabo.Core.OCR.Geometry();
                            geoJson.featureInJson.features[i].geometry.rings[0, 0, 0] = iFeature.Extent.UpperLeft.X;
                            geoJson.featureInJson.features[i].geometry.rings[0, 0, 1] = iFeature.Extent.UpperLeft.Y;
                            geoJson.featureInJson.features[i].geometry.rings[0, 1, 0] = iFeature.Extent.UpperRight.X;
                            geoJson.featureInJson.features[i].geometry.rings[0, 1, 1] = iFeature.Extent.UpperRight.Y;
                            geoJson.featureInJson.features[i].geometry.rings[0, 2, 0] = iFeature.Extent.LowerRight.X;
                            geoJson.featureInJson.features[i].geometry.rings[0, 2, 1] = iFeature.Extent.LowerRight.Y;
                            geoJson.featureInJson.features[i].geometry.rings[0, 3, 0] = iFeature.Extent.LowerLeft.X;
                            geoJson.featureInJson.features[i].geometry.rings[0, 3, 1] = iFeature.Extent.LowerLeft.Y;
                            geoJson.featureInJson.features[i].geometry.rings[0, 4, 0] = iFeature.Extent.UpperLeft.X;
                            geoJson.featureInJson.features[i].geometry.rings[0, 4, 1] = iFeature.Extent.UpperLeft.Y;
                          


                        }
                        geoJson.writeJsonFile(path);

                    }
                    layer = enumLayer.Next();
                }

            }
            catch (Exception e)
            {

                Log.WriteLine(e.Message);
            }

        }

        /// <summary>
        /// Cearte shapefile layer(Raster Layer) to open new layer for Polygons
        /// </summary>
        /// <returns></returns>
      

        /// <summary>
        /// Cearte New FeatureLayer
        /// </summary>
        /// <param name="featureClass"></param>
        /// <returns></returns>
        public IFeatureLayer CreateFeatureLayer(IFeatureClass featureClass)
        {
            try
            {


                // Create a new FeatureLayer and assign a shapefile to it
                IFeatureLayer featureLayer = new ESRI.ArcGIS.Carto.FeatureLayer();
                featureLayer.FeatureClass = featureClass;
                ILayer layer = (ILayer)featureLayer;
                layer.Name = featureLayer.FeatureClass.AliasName;
                return (IFeatureLayer)layer;
            }
            catch (Exception e)
            {
                Log.WriteLine("CreateFeatureLayer:" + e.Message);
            }
            return null;
        }

        /// <summary>
        /// Read the GeoJason File and add new Polygongs to the vector layer
        /// the name of file should be newFile.json
        /// </summary>
        /// <param name="featureLayer"></param>
        /// <param name="featureworkspace"></param>
        public void AddPolygon(IFeatureLayer featureLayer, IFeatureWorkspace featureworkspace, string dir)
        {
            try
            {


                //Define vertices
                GeoJson geoJson = new GeoJson();
                _featureInJSON = geoJson.readGeoJsonFile(dir);
                IMap map = ArcMap.Document.FocusMap;
                AddGraphicToMap(map);
                IWorkspaceEdit editWorkspace = featureworkspace as IWorkspaceEdit;
                editWorkspace.StartEditing(true);
                editWorkspace.StartEditOperation();
                for (int j = 0; j < _featureInJSON.features.Count; j++)
                {


                    IPoint pPoint1 = new ESRI.ArcGIS.Geometry.Point();
                    IPoint pPoint2 = new ESRI.ArcGIS.Geometry.Point();
                    IPoint pPoint3 = new ESRI.ArcGIS.Geometry.Point();
                    IPoint pPoint4 = new ESRI.ArcGIS.Geometry.Point();

                    pPoint1.PutCoords(_featureInJSON.features[j].geometry.rings[0, 0, 0], _featureInJSON.features[j].geometry.rings[0, 0, 1]);
                    pPoint2.PutCoords(_featureInJSON.features[j].geometry.rings[0, 1, 0], _featureInJSON.features[j].geometry.rings[0, 1, 1]);
                    pPoint3.PutCoords(_featureInJSON.features[j].geometry.rings[0, 2, 0], _featureInJSON.features[j].geometry.rings[0, 2, 1]);
                    pPoint4.PutCoords(_featureInJSON.features[j].geometry.rings[0, 3, 0], _featureInJSON.features[j].geometry.rings[0, 3, 1]);

                    IPointCollection pPolygon = new Polygon();
                    IPointCollection pPointCollection = pPolygon as IPointCollection;
                    //Add the vertices of the polygon
                    pPointCollection.AddPoints(1, ref pPoint1);
                    pPointCollection.AddPoints(1, ref pPoint2);
                    pPointCollection.AddPoints(1, ref pPoint3);
                    pPointCollection.AddPoints(1, ref pPoint4);


                    ((IPolygon)pPolygon).Close();
                    IFeature iFeature = featureLayer.FeatureClass.CreateFeature();
                    iFeature.Shape = (IPolygon)pPolygon;
                    iFeature.Store();
                    //Feature cursor used to loop through all features in feature class, optionally a query filter can be used.

                    int num = featureLayer.FeatureClass.FindField(_featureInJSON.fieldAliases.Char_count);
                    iFeature.set_Value(num, _featureInJSON.features[j].attributes.Char_count.ToString());

                    num = featureLayer.FeatureClass.FindField(_featureInJSON.fieldAliases.Filename);
                    iFeature.set_Value(num, _featureInJSON.features[j].attributes.Filename);

                    //num = featureLayer.FeatureClass.FindField(_featureInJSON.fieldAliases.Mass_centerX);
                    iFeature.set_Value(8, _featureInJSON.features[j].attributes.Mass_centerX.ToString());

                    //num = featureLayer.FeatureClass.FindField(_featureInJSON.fieldAliases.Mass_centerY);
                    iFeature.set_Value(9, _featureInJSON.features[j].attributes.Mass_centerY.ToString());

                    //num = featureLayer.FeatureClass.FindField(_featureInJSON.fieldAliases.OBJECTID);
                    //iFeature.set_Value(num, _featureInJSON.features[j].attributes.OBJECTID.ToString());

                    //num = featureLayer.FeatureClass.FindField(_featureInJSON.fieldAliases.Orientation);
                    iFeature.set_Value(4, _featureInJSON.features[j].attributes.OBJECTID.ToString());

                    //num = featureLayer.FeatureClass.FindField(_featureInJSON.fieldAliases.Susp_char_count);
                    iFeature.set_Value(7, _featureInJSON.features[j].attributes.Susp_char_count.ToString());

                    num = featureLayer.FeatureClass.FindField(_featureInJSON.fieldAliases.Susp_text);
                    iFeature.set_Value(num, _featureInJSON.features[j].attributes.Susp_text.ToString());

                    num = featureLayer.FeatureClass.FindField(_featureInJSON.fieldAliases.Text);
                    iFeature.set_Value(num, _featureInJSON.features[j].attributes.Text);

                    iFeature.Store();
                }
                IFeatureCursor fcCursor = featureLayer.FeatureClass.Update(null, true);
                //Stop the operation and provide a name to add to the operation stack.
                editWorkspace.StartEditOperation();
                //Stop editing and save the edits.
                editWorkspace.StopEditing(true);

                map.AddLayer(featureLayer);
            }
            catch (Exception e)
            {
                Log.WriteLine("AddPolygon: " + e.Message);
            }

        }



        /// <summary>
        /// Add Raster Layer for the result of Extract textlayer
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        public void AddRasterLayer(string path, string fileName)
        {

            IMap map = ArcMap.Document.FocusMap;

            try
            {

                IWorkspaceFactory workspaceFactory = new RasterWorkspaceFactory();
                IRasterWorkspace rasterWorkspace = (ESRI.ArcGIS.DataSourcesRaster.IRasterWorkspace)(workspaceFactory.OpenFromFile(path, 0));
                IRasterDataset rasterDataset = rasterWorkspace.OpenRasterDataset(fileName);
                IGeoDataset geoDataset = (ESRI.ArcGIS.Geodatabase.IGeoDataset)rasterDataset;
                // Create a raster for viewing
                IRasterLayer rasterLayer = new RasterLayer();

                rasterLayer.CreateFromDataset(rasterDataset);

                // Add the raster to the map
                map.AddLayer(rasterLayer);

            }
            catch (System.Exception ex)
            {

                Log.WriteLine("AddRasterLayer: " + ex.Message);


            }

        }

        /// <summary>
        /// Add Shapefile layer
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        private void AddShapeFileLayer(string path, string fileName)
        {
            try
            {
                // Create a new ShapefileWorkspaceFactory object and
                // open a shapefile folder - the path works with standard 9.3 installation
                IWorkspaceFactory workspaceFactory = new ShapefileWorkspaceFactory();
                IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)
                  workspaceFactory.OpenFromFile(path, 0);
                // Create a new FeatureLayer and assign a shapefile to it
                IFeatureLayer featureLayer = new ESRI.ArcGIS.Carto.FeatureLayer();
                featureLayer.FeatureClass = featureWorkspace.OpenFeatureClass(fileName);
                ILayer layer = (ILayer)featureLayer;
                layer.Name = featureLayer.FeatureClass.AliasName;
                // Add the Layer to the focus map
                IMap map = ArcMap.Document.FocusMap;

                map.AddLayer(layer);
            }
            catch (Exception e)
            {

                Log.WriteLine("AddShapeFileLayer: " + e.Message);
            }

        }

        /// <summary>
        /// Add Feature class and its feilds
        /// </summary>
        /// <param name="featureClassName"></param>
        /// <param name="featureWorkspace"></param>
        /// <returns></returns>
        public IFeatureClass CreateFeatureClassWithFields(String featureClassName, IFeatureWorkspace featureWorkspace, string dir)
        {
            GeoJson geoJson = new GeoJson();
            try
            {


                _featureInJSON = geoJson.readGeoJsonFile(dir);

                // Instantiate a feature class description to get the required fields.
                IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
                IObjectClassDescription ocDescription = (IObjectClassDescription)
                  fcDescription;
                IFields fields = ocDescription.RequiredFields;
                IFieldsEdit fieldsEdit = (IFieldsEdit)fields;

                for (int i = 0; i < 9; i++)
                {
                    if (_featureInJSON.fields[i].name == "OBJECTID")
                        continue;
                    else
                        addFeatureFeild(_featureInJSON.fields[i].name, _featureInJSON.fields[i].alias, (esriFieldType)Enum.Parse(typeof(esriFieldType), _featureInJSON.fields[i].type), fieldsEdit);
                }
                // Use IFieldChecker to create a validated fields collection.
                IFieldChecker fieldChecker = new FieldCheckerClass();
                IEnumFieldError enumFieldError = null;
                IFields validatedFields = null;
                fieldChecker.ValidateWorkspace = (IWorkspace)featureWorkspace;
                fieldChecker.Validate(fields, out enumFieldError, out validatedFields);

                // The enumFieldError enumerator can be inspected at this point to determine 
                // which fields were modified during validation.
                // Create the feature class.
                IFeatureClass featureClass = featureWorkspace.CreateFeatureClass
                  (featureClassName, validatedFields, ocDescription.InstanceCLSID,
                  ocDescription.ClassExtensionCLSID, esriFeatureType.esriFTSimple,
                  fcDescription.ShapeFieldName, "");

                return featureClass;
            }
            catch (Exception e)
            {

                Log.WriteLine("CreateFeatureClassWithFields: " + e.Message);
            }
            return null;
        }



        /// <summary>
        /// add feature feild of each feature
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="aliasName"></param>
        /// <param name="type"></param>
        /// <param name="fieldsEdit"></param>
        private void addFeatureFeild(string fieldName, string aliasName, esriFieldType type, IFieldsEdit fieldsEdit)
        {
            IField field = new FieldClass();
            IFieldEdit fieldEdit = (IFieldEdit)field;
            fieldEdit.Name_2 = fieldName;
            fieldEdit.Type_2 = type;
            fieldEdit.AliasName_2 = aliasName;
            fieldsEdit.AddField(field);
        }

        /// <summary>
        /// set feature of GeoJson File
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="type"></param>
        /// <param name="feild"></param>
        private void setGeoJASONFeilds(string fieldName, esriFieldType type, Field_info feild)
        {
            try
            {


                feild.alias = feild.name = fieldName;
                feild.type = type.ToString();
            }
            catch (Exception e)
            {

                Log.WriteLine("setGeoJASONFeilds: " + e.Message);
            }
        }

        public void AddGraphicToMap(ESRI.ArcGIS.Carto.IMap map)
        {
            try
            {


                ESRI.ArcGIS.Carto.IGraphicsContainer graphicsContainer = (ESRI.ArcGIS.Carto.IGraphicsContainer)map; // Explicit Cast
                ESRI.ArcGIS.Carto.IElement element = null;

                // Polygon elements
                ESRI.ArcGIS.Display.ISimpleFillSymbol simpleFillSymbol = new ESRI.ArcGIS.Display.SimpleFillSymbol();
                simpleFillSymbol.Color.RGB = 0;
                simpleFillSymbol.Color.NullColor = false;
                simpleFillSymbol.Style = ESRI.ArcGIS.Display.esriSimpleFillStyle.esriSFSForwardDiagonal;
                ESRI.ArcGIS.Carto.IFillShapeElement fillShapeElement = new ESRI.ArcGIS.Carto.PolygonElementClass();
                fillShapeElement.Symbol = simpleFillSymbol;
                element = (ESRI.ArcGIS.Carto.IElement)fillShapeElement; // Explicit Cast
            }
            catch (Exception e)
            {

                Log.WriteLine("AddGraphicToMap: " + e.Message);
            }
        }

        /// <summary>
        ////////TextExtraction and TextIdentifier Methods
        //</summary>
        //<param name="sourceImageDir"></param>
        public void textLayerExtract(string sourceImageDir)
        {
            try
            {


                if (sourceImageDir == "")
                    return;

                List<Bitmap> imgfnp_list = new List<Bitmap>();
                List<Bitmap> imgfnn_list = new List<Bitmap>();

                Image<Bgr, Byte> srcImage = new Image<Bgr, byte>(sourceImageDir);
                Bitmap srcimg = new Bitmap(sourceImageDir);

                sourceImageDir = CreateDirectory(sourceImageDir, "Data");
                ////////////////////Geo Spatial PRoject Sima//////////////////////


                fillImageList(sourceImageDir + "\\PositiveLayerInfo.json", imgfnp_list, srcImage);
                fillImageList(sourceImageDir + "\\NegativeLayerInfo.json", imgfnn_list, srcImage);




                TextLayerExtractionTrainer tet = new TextLayerExtractionTrainer();
                Log.SetStartTime();
                List<Bitmap> result_list = new List<Bitmap>();
                for (int i = 0; i < 1; i++)
                    result_list = tet.GUIProcessOneLayerOnly(srcimg, imgfnp_list, imgfnn_list, 4);
                Console.WriteLine(Log.GetDurationInSeconds());

                result_list[0].Save(sourceImageDir + "\\Result.png");
            }
            catch (Exception e)
            {

                Log.WriteLine("textLayerExtract:  " + e.Message);
            }
        }

        private void fillImageList(string path, List<Bitmap> imgList, Image<Bgr, Byte> srcImage)
        {


            FeatureInJSON _featureInJSON;
            GeoJson geoJson = new GeoJson();

            try
            {


                if (!File.Exists(path))
                    return;
                _featureInJSON = geoJson.readGeoJsonFile(path);

                for (int j = 0; j < _featureInJSON.features.Count; j++)
                {
                    double x1, x2, y1, y2;
                    x1 = _featureInJSON.features[j].geometry.rings[0, 0, 0];
                    y1 = _featureInJSON.features[j].geometry.rings[0, 0, 1];
                    x2 = _featureInJSON.features[j].geometry.rings[0, 1, 0];
                    y2 = _featureInJSON.features[j].geometry.rings[0, 2, 1];

                    Rectangle rec = new Rectangle(int.Parse(Math.Round(x1).ToString()), int.Parse((Math.Round(y1 * -1)).ToString()), int.Parse(Math.Round((x2 - x1)).ToString()),
                        int.Parse(Math.Round((y1 - y2)).ToString()));
                    //Image<Bgr, Byte> test = srcImage.GetSubRect(rec);
                    Bitmap img = srcImage.Bitmap;
                    Bitmap cropedImage = img.Clone(rec, img.PixelFormat);
                    cropedImage.Save( CreateDirectory(path, "Data")+"\\img" + j.ToString());
                    imgList.Add(cropedImage);

                }

            }
            catch (Exception e)
            {

                Log.WriteLine("fillImageList: " + e.Message);
            }

        }

        public void textIndentification(string dir, string fn)
        {
            try
            {


                TextDetectionWorker trw = new TextDetectionWorker();
                trw.Apply(dir, fn, 2.5, false);
            }
            catch (Exception e)
            {

                Log.WriteLine("textIndentification: " + e.Message);
            }
        }



        public string CreateDirectory(string current, string subDir)
        {

            DirectoryInfo parentInfo = System.IO.Directory.GetParent(current).Parent;
            try
            {


                subDir = parentInfo.FullName + "\\" + subDir;

                if (!System.IO.Directory.Exists(subDir))
                    System.IO.Directory.CreateDirectory(subDir);
            }
            catch (Exception e)
            {
                Log.WriteLine("CreateDirectory: " + e.Message);
            }
            return subDir;
        }
        public IWorkspace CreateShapefileWorkspace(string dir)
        {
            try
            {


                IWorkspaceFactory2 workspaceFactory = (IWorkspaceFactory2)new
                  ShapefileWorkspaceFactory();

                if (System.IO.Directory.Exists(dir + "\\StraboToArcMap"))
                    System.IO.Directory.Delete(dir + "\\StraboToArcMap", true);

                IWorkspaceName workspaceName = workspaceFactory.Create(dir,
                  "StraboToArcMap", null, 0);

                IName name = (IName)workspaceName;
                IWorkspace workspace = (IWorkspace)name.Open();
                return workspace;
            }
            catch (Exception e)
            {

                Log.WriteLine("CreateShapefileWorkspace : " + e.Message);
            }
            return null;
        }

        ///////////////////////Symbol Recognition /////////

        public IWorkspace symbolCreateShapefileWorkspace()
        {
            try
            {


                IWorkspaceFactory2 workspaceFactory = (IWorkspaceFactory2)new
                  ShapefileWorkspaceFactory();

                if (System.IO.Directory.Exists(_rasterInfo.rasterData + "\\ArcMapSymbolRecognition"))
                    System.IO.Directory.Delete(_rasterInfo.rasterData + "\\ArcMapSymbolRecognition", true);

                IWorkspaceName workspaceName = workspaceFactory.Create(_rasterInfo.rasterData, "ArcMapSymbolRecognition", null, 0);

                IName name = (IName)workspaceName;
                IWorkspace workspace = (IWorkspace)name.Open();
                return workspace;
            }
            catch (Exception e)
            {

                Log.WriteLine("CreateShapefileWorkspace : " + e.Message);
            }
            return null;
        }
        public void MakingSymbolGeoJsonFile()
        {


            ///////////////////////////////////////////////////////////////
            //Setting current Map to access Layers, Feature Class and Features
            //build string builder to write on the ImageResultfile
            ///////////////////////////////////////////////////////////////
            string path = "";
            IMap map = ArcMap.Document.FocusMap;
            //GeoJson geoJson;
            IEnumLayer enumLayer = map.get_Layers(null, true);
            ILayer layer = enumLayer.Next();
            //((IDataset)layer).Workspace.
            IFeatureLayer featureLayer;
            IFeature iFeature;
            try
            {
                while (layer != null)
                {




                    if (layer.Name == "SymbolPositiveLabel")
                    {
                        GeoJson geoJson = new GeoJson();
                        geoJson.featureInJson = new FeatureInJSON();
                        geoJson.featureInJson.displayFieldName = layer.Name;
                        geoJson.featureInJson.fieldAliases = new FieldAliases();

                        //////////////////////////Set Alias Feature of the Layer////////////////////
                        geoJson.featureInJson.fieldAliases.Mass_centerX = "Mass_centerX";
                        geoJson.featureInJson.fieldAliases.Mass_centerY = "Mass_centerY";
                        geoJson.featureInJson.fieldAliases.OBJECTID = "OBJECTID";
                        geoJson.featureInJson.fieldAliases.Orientation = "Orientation";
                        geoJson.featureInJson.fieldAliases.Susp_char_count = "Susp_char_count";
                        geoJson.featureInJson.fieldAliases.Susp_text = "Susp_text";
                        geoJson.featureInJson.fieldAliases.Text = "Text";
                        geoJson.featureInJson.fieldAliases.Char_count = "Char_count";
                        geoJson.featureInJson.fieldAliases.Filename = "Filename";

                        //////////////////////Set Fields of the current Layer///////////////////////

                        geoJson.featureInJson.fields = new Field_info[9];
                        setGeoJASONFeilds("OBJECTID", esriFieldType.esriFieldTypeOID, geoJson.featureInJson.fields[0]);
                        setGeoJASONFeilds("Text", esriFieldType.esriFieldTypeString, geoJson.featureInJson.fields[1]);
                        setGeoJASONFeilds("Char_count", esriFieldType.esriFieldTypeInteger, geoJson.featureInJson.fields[2]);
                        setGeoJASONFeilds("Orientation", esriFieldType.esriFieldTypeDouble, geoJson.featureInJson.fields[3]);
                        setGeoJASONFeilds("Filename", esriFieldType.esriFieldTypeString, geoJson.featureInJson.fields[4]);
                        setGeoJASONFeilds("Susp_text", esriFieldType.esriFieldTypeString, geoJson.featureInJson.fields[5]);
                        setGeoJASONFeilds("Susp_char_count", esriFieldType.esriFieldTypeInteger, geoJson.featureInJson.fields[6]);
                        setGeoJASONFeilds("Mass_centerX", esriFieldType.esriFieldTypeDouble, geoJson.featureInJson.fields[7]);
                        setGeoJASONFeilds("Mass_centerY", esriFieldType.esriFieldTypeDouble, geoJson.featureInJson.fields[8]);

                        path =_rasterInfo.rasterData + "\\PositiveLayerInfo.json";//@"C:\Emgu\emgucv-windows-universal-cuda 2.9.0.1922\Emgu.CV.Example\Strabo_Map_Processing\Hollywood\Data\PositiveLayerInfo.json"; //

                        featureLayer = (IFeatureLayer)layer;
                        int count = featureLayer.FeatureClass.FeatureCount(null);
                        for (int j = 1; j <= count; j++)
                        {
                            iFeature = featureLayer.FeatureClass.GetFeature(j);
                            geoJson.featureInJson.features.Add(new Features());
                            int i = j - 1;
                            geoJson.featureInJson.features[i].attributes = new Attributes();
                            geoJson.featureInJson.features[i].attributes.OBJECTID = int.Parse(iFeature.get_Value(0).ToString());
                            geoJson.featureInJson.features[i].geometry = new Strabo.Core.OCR.Geometry();
                            geoJson.featureInJson.features[i].geometry.rings[0, 0, 0] = iFeature.Extent.UpperLeft.X;
                            geoJson.featureInJson.features[i].geometry.rings[0, 0, 1] = iFeature.Extent.UpperLeft.Y;
                            geoJson.featureInJson.features[i].geometry.rings[0, 1, 0] = iFeature.Extent.UpperRight.X;
                            geoJson.featureInJson.features[i].geometry.rings[0, 1, 1] = iFeature.Extent.UpperRight.Y;
                            geoJson.featureInJson.features[i].geometry.rings[0, 2, 0] = iFeature.Extent.LowerRight.X;
                            geoJson.featureInJson.features[i].geometry.rings[0, 2, 1] = iFeature.Extent.LowerRight.Y;
                            geoJson.featureInJson.features[i].geometry.rings[0, 3, 0] = iFeature.Extent.LowerLeft.X;
                            geoJson.featureInJson.features[i].geometry.rings[0, 3, 1] = iFeature.Extent.LowerLeft.Y;
                            geoJson.featureInJson.features[i].geometry.rings[0, 4, 0] = iFeature.Extent.UpperLeft.X;
                            geoJson.featureInJson.features[i].geometry.rings[0, 4, 1] = iFeature.Extent.UpperLeft.Y;
                            if (iFeature.Extent.UpperLeft.Y < 0 && _rasterInfo.ratserNegative> 0)
                                _rasterInfo.ratserNegative = -1;

                        }
                        geoJson.writeJsonFile(path);

                    }
                    layer = enumLayer.Next();
                }

            }
            catch (Exception e)
            {

                Log.WriteLine(e.Message);
            }

        }
        /// <summary>
        /// /////////////Converting pixels to Lat/Long
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="pixelX"></param>
        /// <param name="pixelY"></param>
        public void GetXY(double lat, double lng, out double pixelX, out double pixelY)
        {

            int MAP_HEIGHT = _rasterInfo.rasterHeight;
            int MAP_WIDTH = _rasterInfo.rasterWidth;

            //extents of bounding box
            double e = _rasterInfo.rasterDownRightX; //eastern boundary latitude
            double w = _rasterInfo.rasterTopLeftX;//western boundary latitude
            double n = _rasterInfo.rasterTopLeftY;//north boundary longitude
            double s = _rasterInfo.rasterDownRightY;//south boundary longitude

            //int MAP_HEIGHT = 2615; // 5952;
            //int MAP_WIDTH = 5103;// 8568;

            ////extents of bounding box
            //double e = 44.4491887;// 44.5273990; //eastern boundary latitude
            //double w = 44.385351;//44.273999; //western boundary latitude
            //double n = 33.335706;// 33.404047; //north boundary longitude
            //double s = 33.303235;//33.251287; //south boundary longitude

            //calculate extents based on differences, convert to positive values 
            double nsspan = Math.Abs(n - s);
            double ewspan = Math.Abs(w - e);

            double nspix = MAP_HEIGHT / nsspan; //gives you how many pixels in a lng point
            double ewpix = MAP_WIDTH / ewspan; //gives you how many pixels in a lat point

            pixelX = (Math.Abs(w - lat)) * ewpix; //the difference between the western (left) edge of the box and the point in question, multiplied by pixels per point
            pixelY = (Math.Abs(n - lng)) * nspix; //the difference between the northern (top) edge of the box and the point in question, multiplied by pixels per point

        }
        /// <summary>
        /// ////////////Converting Lat/long to X and Y 
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>

        public void getLngLat(double x, double y, out  double lng, out  double lat)
        {

            int MAP_HEIGHT = _rasterInfo.rasterHeight;
            int MAP_WIDTH = _rasterInfo.rasterWidth;

            //extents of bounding box
            double e = _rasterInfo.rasterTopLeftX; //eastern boundary latitude
            double w = _rasterInfo.rasterDownRightX;//western boundary latitude
            double n = _rasterInfo.rasterTopLeftY;//north boundary longitude
            double s = _rasterInfo.rasterDownRightY;//south boundary longitude

            //int MAP_HEIGHT = 1310;//2615; // 5952;
            //int MAP_WIDTH = 2558;// 8568;

            //extents of bounding box
            //double e = 44.4491887;// 44.5273990; //eastern boundary latitude
            //double w = 44.385351;//44.273999; //western boundary latitude
            //double n = 33.335706;// 33.404047; //north boundary longitude
            //double s = 33.303235;//33.251287; //south boundary longitude

            //calculate extents based on differences, convert to positive values 
            double nsspan = Math.Abs(n - s);
            double ewspan = Math.Abs(w - e);

            double nspix = MAP_HEIGHT / nsspan; //gives you how many pixels in a lng point
            double ewpix = MAP_WIDTH / ewspan; //gives you how many pixels in a lat point
            lng = w + x / ewpix;
            lat = n - y / nspix;
        }

        private CoordinatePoint[] coordinatePoints;

        public void SymbolExtraction()
        {

            try
            {



                List<Bitmap> imgfnp_list = new List<Bitmap>();
                List<Bitmap> imgfnn_list = new List<Bitmap>();

                

                Image<Bgr, Byte> srcImage = new Image<Bgr, byte>(_rasterInfo.ratserImgPath);


                //sourceImageDir = CreateDirectory(parentInfo.FullName, "Data");
                ////////////////////Geo Spatial PRoject Sima//////////////////////


                fillSymbolList( imgfnp_list, srcImage);


                Image<Gray, Byte> gElement = null;
                

                HashSet<float[]> hash = symbolRecognition(out gElement, _rasterInfo.rasterData);
               
                coordinatePoints = new CoordinatePoint[hash.Count];
                pointSet = new Points[hash.Count];

                //....................................................................
                int j = 0;

                foreach (float[] i in hash)
                {
                    double lng, lat, x, y;
                    int neg = 1;
                    if (i[1] > 0 && _rasterInfo.ratserNegative < 0)
                        neg = _rasterInfo.ratserNegative;
                    pointSet[j] = new Points();
                    pointSet[j].leftTopX = i[0];
                    pointSet[j].leftTopY = i[1]*neg;
                    pointSet[j].rightTopX = i[0] + gElement.Size.Width;
                    pointSet[j].rightTopY = i[1] * neg;
                    pointSet[j].leftDownX = i[0];
                    pointSet[j].leftDownY = (i[1] + gElement.Size.Height) * neg;
                    pointSet[j].rightDownX = i[0] + gElement.Size.Width;
                    pointSet[j].rightDownY = (i[1] + gElement.Size.Height) * neg;

                    if (_rasterInfo.rasterType != "Unknown")
                    {

                        x = i[0] + gElement.Size.Width / 2;
                        y = i[1] + gElement.Size.Height / 2;
                        getLngLat(x, y, out lng, out lat);
                        coordinatePoints[j] = new CoordinatePoint();
                        coordinatePoints[j].lng = lng;
                        coordinatePoints[j].lat = lat;
                    }

                    j++;



                }

                #region Geo-Spatial Project- Connection to DBpredia
                if (_rasterInfo.rasterType != "Unknown")
                {
                    bindURI();
                    int num = 0;
                    foreach (CoordinatePoint pn in coordinatePoints)
                    {
                        pointSet[num].URI = pn.URI;
                        num++;
                    }
                }
                #endregion
            }
            catch (Exception e)
            {

                Log.WriteLine("textLayerExtract:  " + e.Message);
            }
        }

        public void convertPolygon()
        {
            lngLatSet = new Points[pointSet.Length];

            for (int i = 0; i < pointSet.Length; i++)
            {
                lngLatSet[i] = new Points();
                getLngLat(pointSet[i].leftTopX, pointSet[i].leftTopY, out lngLatSet[i].leftTopX, out lngLatSet[i].leftTopY);
                getLngLat(pointSet[i].rightTopX, pointSet[i].rightTopY, out lngLatSet[i].rightTopX, out lngLatSet[i].rightTopY);
                getLngLat(pointSet[i].leftDownX, pointSet[i].leftDownY, out lngLatSet[i].leftDownX, out lngLatSet[i].leftDownY);
                getLngLat(pointSet[i].rightDownX, pointSet[i].rightDownY, out lngLatSet[i].rightDownX, out lngLatSet[i].rightDownY);
                lngLatSet[i].URI = pointSet[i].URI;
            }
        }
        private void fillSymbolList( List<Bitmap> imgList, Image<Bgr, Byte> srcImage)
        {

            FeatureInJSON _featureInJSON;
            GeoJson geoJson = new GeoJson();

            try
            {


                if (!File.Exists(_rasterInfo.rasterData + "\\PositiveLayerInfo.json"))
                    return;
                _featureInJSON = geoJson.readGeoJsonFile(_rasterInfo.rasterData + "\\PositiveLayerInfo.json");

                for (int j = 0; j < _featureInJSON.features.Count; j++)
                {
                    double x1, x2, y1, y2;
                    Rectangle rec;
                    x1 = _featureInJSON.features[j].geometry.rings[0, 0, 0];
                    y1 = _featureInJSON.features[j].geometry.rings[0, 0, 1];
                    x2 = _featureInJSON.features[j].geometry.rings[0, 1, 0];
                    y2 = _featureInJSON.features[j].geometry.rings[0, 2, 1];

                    if (_rasterInfo.rasterType != "Unknown")
                    {
                        double pixelX1, pixelY1, pixelX2, pixelY2;
                        ////////////////////geo Spatial Project Change/////////////////////
                        GetXY(x1, y1, out pixelX1, out pixelY1);
                        GetXY(x2, y2, out pixelX2, out pixelY2);
                        double r1 = pixelY2 - pixelY1;
                        double r2 = pixelX2 - pixelX1;


                        rec = new Rectangle((int.Parse(Math.Round(pixelX1).ToString())), int.Parse((Math.Round(pixelY1)).ToString()),
                           int.Parse(Math.Round(Math.Abs(r2)).ToString()), int.Parse(Math.Round(Math.Abs(r1)).ToString()));
                    }
                    else
                    {
                        rec = new Rectangle((int.Parse(Math.Round(Math.Abs(x1)).ToString())), int.Parse((Math.Round(Math.Abs(y1))).ToString()),
                                                  int.Parse(Math.Round(Math.Abs(x1 - x2)).ToString()), int.Parse(Math.Round(Math.Abs(y1 - y2)).ToString()));
                    }
                    //  Image<Bgr, Byte> test = srcImage.GetSubRect(rec);
                    Bitmap img = srcImage.Bitmap;
                    Bitmap cropedImage = img.Clone(rec, img.PixelFormat);
                   
                    if (!System.IO.Directory.Exists(_rasterInfo.rasterIn))
                        System.IO.Directory.CreateDirectory(_rasterInfo.rasterIn);
                    Bitmap resizedImage = new Bitmap(cropedImage, cropedImage.Width , cropedImage.Height );
                    resizedImage.Save(_rasterInfo.rasterIn +"\\element.png");
                    imgList.Add(cropedImage);

                }

            }
            catch (Exception e)
            {

                Log.WriteLine("fillImageList: " + e.Message);
            }


        }

        public void addPoligonGeorefrenced(IFeatureLayer featureLayer, IFeatureWorkspace featureworkspace)
        {
            try
            {


                //Define vertices

                if (_rasterInfo.rasterType != "Unknown")
                    convertPolygon();

                IMap map = ArcMap.Document.FocusMap;
                AddGraphicToMap(map);
                IWorkspaceEdit editWorkspace = featureworkspace as IWorkspaceEdit;
                editWorkspace.StartEditing(true);
                editWorkspace.StartEditOperation();
                int count = 0;
                if (_rasterInfo.rasterType != "Unknown")
                    count = lngLatSet.Length;
                else
                    count = pointSet.Length;

                for (int j = 0; j < count; j++)
                {


                    IPoint pPoint1 = new ESRI.ArcGIS.Geometry.Point();
                    IPoint pPoint2 = new ESRI.ArcGIS.Geometry.Point();
                    IPoint pPoint3 = new ESRI.ArcGIS.Geometry.Point();
                    IPoint pPoint4 = new ESRI.ArcGIS.Geometry.Point();

                    if (_rasterInfo.rasterType != "Unknown")
                    {
                        pPoint1.PutCoords(lngLatSet[j].leftTopX, lngLatSet[j].leftTopY);
                        pPoint2.PutCoords(lngLatSet[j].rightTopX, lngLatSet[j].rightTopY);
                        pPoint3.PutCoords(lngLatSet[j].rightDownX, lngLatSet[j].rightDownY);
                        pPoint4.PutCoords(lngLatSet[j].leftDownX, lngLatSet[j].leftDownY);
                    }
                    else
                    {
                       // if (pointSet[j].leftTopY < 0)
                        {
                            pPoint1.PutCoords(pointSet[j].leftTopX, pointSet[j].leftTopY);
                            pPoint2.PutCoords(pointSet[j].rightTopX, pointSet[j].rightTopY);
                            pPoint3.PutCoords(pointSet[j].rightDownX, pointSet[j].rightDownY);
                            pPoint4.PutCoords(pointSet[j].leftDownX, pointSet[j].leftDownY);
                        }
                        //else if (_rasterInfo.ratserNegative<0)
                        //{
                        //    pPoint1.PutCoords(pointSet[j].leftTopX, pointSet[j].leftTopY *-1);
                        //    pPoint2.PutCoords(pointSet[j].rightTopX, pointSet[j].rightTopY*-1);
                        //    pPoint3.PutCoords(pointSet[j].rightDownX, pointSet[j].rightDownY*-1);
                        //    pPoint4.PutCoords(pointSet[j].leftDownX, pointSet[j].leftDownY*-1);
                        //}

                    }

                    IPointCollection pPolygon = new Polygon();
                    IPointCollection pPointCollection = pPolygon as IPointCollection;
                    //Add the vertices of the polygon
                    pPointCollection.AddPoints(1, ref pPoint1);
                    pPointCollection.AddPoints(1, ref pPoint2);
                    pPointCollection.AddPoints(1, ref pPoint3);
                    pPointCollection.AddPoints(1, ref pPoint4);


                    ((IPolygon)pPolygon).Close();
                    IFeature iFeature = featureLayer.FeatureClass.CreateFeature();
                    iFeature.Shape = (IPolygon)pPolygon;
                    iFeature.Store();
                    //Feature cursor used to loop through all features in feature class, optionally a query filter can be used.
                    if (_rasterInfo.rasterType != "Unknown")
                    {
                        int num = featureLayer.FeatureClass.FindField("URI");
                        iFeature.set_Value(num, lngLatSet[j].URI.ToString());
                    }
                    iFeature.Store();
                }
                IFeatureCursor fcCursor = featureLayer.FeatureClass.Update(null, true);
                //Stop the operation and provide a name to add to the operation stack.
                editWorkspace.StartEditOperation();
                //Stop editing and save the edits.
                editWorkspace.StopEditing(true);

                map.AddLayer(featureLayer);
            }
            catch (Exception e)
            {
                Log.WriteLine("AddPolygon: " + e.Message);
            }
        }

        public IFeatureClass CreateFeatureClassWithFieldsGeoRef(String featureClassName, IFeatureWorkspace featureWorkspace)
        {
            GeoJson geoJson = new GeoJson();
            try
            {
                // Instantiate a feature class description to get the required fields.
                IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
                IObjectClassDescription ocDescription = (IObjectClassDescription)
                  fcDescription;
                IFields fields = ocDescription.RequiredFields;
                IFieldsEdit fieldsEdit = (IFieldsEdit)fields;

                addFeatureFeild("URI", "URI", esriFieldType.esriFieldTypeString, fieldsEdit);
                // Use IFieldChecker to create a validated fields collection.
                IFieldChecker fieldChecker = new FieldCheckerClass();
                IEnumFieldError enumFieldError = null;
                IFields validatedFields = null;
                fieldChecker.ValidateWorkspace = (IWorkspace)featureWorkspace;
                fieldChecker.Validate(fields, out enumFieldError, out validatedFields);

                // The enumFieldError enumerator can be inspected at this point to determine 
                // which fields were modified during validation.
                // Create the feature class.
                IFeatureClass featureClass = featureWorkspace.CreateFeatureClass
                  (featureClassName, validatedFields, ocDescription.InstanceCLSID,
                  ocDescription.ClassExtensionCLSID, esriFeatureType.esriFTSimple,
                  fcDescription.ShapeFieldName, "");

                return featureClass;
            }
            catch (Exception e)
            {

                Log.WriteLine("CreateFeatureClassWithFields: " + e.Message);
            }
            return null;
        }

        private HashSet<float[]> symbolRecognition(out Image<Gray, Byte> gElement, string path)
        {

            #region

            Stopwatch watch = Stopwatch.StartNew();
            ArrayList allMatches = new ArrayList();
            Tuple<Image<Bgr, byte>, float[]> drawResult;
            float[] recStat;
            // Handling file names.

            string topic = "";
            //TextWriter log = File.AppendText(path + topic + "/log.txt");
            //path = "C:/Emgu/emgucv-windows-universal-cuda 2.9.0.1922/Emgu.CV.Example/Strabo_Map_Processing/Hollywood";
            // Read images.
            Image<Bgr, Byte> element = new Image<Bgr, Byte>(string.Format("{0}{1}/element.png", _rasterInfo.rasterIn, topic));
            Image<Bgr, Byte> test = new Image<Bgr, Byte>(string.Format("{0}{1}", _rasterInfo.ratserImgPath, topic));
            Bitmap window = test.ToBitmap();

            // Convert to gray-level images and save.
            gElement = element.Convert<Gray, Byte>();
            Image<Gray, Byte> gTest = test.Convert<Gray, Byte>();
            gElement.Save(string.Format("{0}{1}/in/g-element.png", path, topic));
            gTest.Save(string.Format("{0}{1}/in/g-test.png", path, topic));

            // Get image dimensions.
            int wfactor = 2;
            // The size of the element image.
            int ex = element.Width;
            int ey = element.Height;
            // The size of the test image.
            int tx = test.Width;
            int ty = test.Height;
            // The distance that the sliding window shifts.
            int xshift = tx / ex / wfactor * 2 - 1;
            int yshift = ty / ey / wfactor * 2 - 1;
            Image<Bgr, Byte> pTest;
            //log.WriteLine(string.Format("Element Image: ({0}*{1})\nTest Image:({2}*{3})\n", ex, ey, tx, ty));
            try
            {


                for (int j = 0; j < yshift; j++)
                {
                    for (int i = 0; i < xshift; i++)
                    {
                        int xstart = i * ex * wfactor / 2;
                        int ystart = j * ey * wfactor / 2;
                        int counter = i + j * xshift;


                        Rectangle r = new Rectangle(xstart, ystart, ex * wfactor, ey * wfactor);
                        pTest = new Image<Bgr, Byte>(window.Clone(r, window.PixelFormat));
                        pTest.Save(string.Format("{0}{1}/in/part.jpg", path, topic));


                        try
                        {


                            drawResult = Strabo.Core.SymbolRecognition.DrawMatches.Draw(gElement, pTest.Convert<Gray, Byte>(), test, xstart, ystart, path, topic, counter, null);
                        }
                        catch (Exception)
                        {

                            throw;
                        }

                        #region Sima test
                        //long matchTime;
                        //Image<Bgr, byte> resultmatches = SURFFeatureExample.DrawMatches.Draw(gElement, pTest.Convert<Gray, Byte>(), out matchTime);
                        //resultmatches.Save(@"C:\result\"+i + ".png");
                        // ImageViewer.Show(resultmatches, String.Format("Matched using {0} in {1} milliseconds", GpuInvoke.HasCuda ? "GPU" : "CPU", matchTime));
                        #endregion


                       // log.WriteLine(string.Format("\n\nSub-image #{0}:\n\tLoop #({1}, {2})\n\tSW1 location: ({3}, {4})", counter, i, j, xstart, ystart));
                        test = drawResult.Item1;
                        recStat = drawResult.Item2;
                        if (recStat[2] > 0)
                        {
                            allMatches.Add(recStat);
                            //log.WriteLine(string.Format("\n\tSW2 location: ({0}, {1})\n\tHistogram score: {2}]", recStat[0], recStat[1], recStat[2]));
                        }



                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

           // log.WriteLine("The count before consolidation: " + allMatches.Count);

            HashSet<float[]> hash0 = consolidate(allMatches, gElement.Width - 1, gElement.Height - 1, null);
            ArrayList al = new ArrayList();

            foreach (float[] i in hash0)
            {
                al.Add(i);
            }

            HashSet<float[]> hash = consolidate(al, gElement.Width - 1, gElement.Height - 1, null);

           // log.WriteLine("The count after consolidation: " + hash.Count);
            //Blue 
            TextWriter coordinatesOnMapBlue = File.AppendText(path + topic + "/coordinatesOnMapBlue.txt");
            int k = 0;
            foreach (float[] i in hash)
            {
                test.Draw(new Rectangle(new System.Drawing.Point((int)i[0], (int)i[1]), gElement.Size), new Bgr(Color.Red), 5);
                coordinatesOnMapBlue.WriteLine("x[" + k + "]= " + (int)i[0] + ", y[" + k + "]= " + (int)i[1] + "");
                coordinatesOnMapBlue.WriteLine("x[" + k + "]= " + (int)i[0] + gElement.Width + ", y[" + k + "]= " + (int)i[1] + "");
                coordinatesOnMapBlue.WriteLine("x[" + k + "]= " + (int)i[0] + ", y[" + k + "]= " + (int)i[1] + gElement.Height + "");
                coordinatesOnMapBlue.WriteLine("x[" + k + "]= " + (int)i[0] + gElement.Width + ", y[" + k + "]= " + (int)i[1] + gElement.Height + "");
                k++;
            }
            coordinatesOnMapBlue.Close();
            test.Save(string.Format("{0}{1}/out.jpg", path, topic));

            watch.Stop();
            //log.WriteLine(watch.Elapsed);

            //log.Close();

            return hash;
            #endregion
        }

        public static HashSet<float[]> consolidate(ArrayList al, int w, int h, TextWriter log)
        {
            ArrayList al2 = new ArrayList();

            foreach (float[] i in al)
            {
                al2.Add(i);
            }

            foreach (float[] i in al)
            {
                foreach (float[] j in al)
                {
                    if (!((Math.Abs(i[0] - j[0]) > w) ||
                        (Math.Abs(i[1] - j[1]) > h) ||
                        (Math.Sqrt(Math.Pow(i[0] - j[0], 2) + Math.Pow(i[1] - j[1], 2)) > Math.Sqrt(Math.Pow(w, 2) + Math.Pow(h, 2)))))
                    {

                        if (i[2] > j[2])
                        {
                            al2[al.IndexOf(j)] = i;
                        }
                        else if (i[2] < j[2])
                        {
                            al2[al.IndexOf(i)] = j;
                        }
                    }
                }
            }

            HashSet<float[]> hash = new HashSet<float[]>();

            foreach (float[] i in al2)
            {
                hash.Add(i);
            }

            //log.WriteLine("The count of al2: " + al2.Count);
            //log.WriteLine("The count of hash: " + hash.Count);

            al.Clear();

            foreach (float[] i in hash)
            {
                al.Add(i);
            }

            //log.WriteLine("The count after hash: " + al.Count);

            return hash;
        }


        /****************************************************************************/
        /*************************** these functions belongs to Geo_Spatial Project**/
        /****************************************************************************/

#region if want to add DBpedia part enable this region and add all projects at this folder: Visual Studio 2010\Projects\LinkedDataTools.JenaDotNet.0.3\VS2010
        private double shortestDistance(double lngS, double latS, double lngD, double latD)
        {

            return Math.Sqrt((Math.Pow(lngS - lngD, 2)) + Math.Pow((latS - latD), 2));

        }
        private void bindURI()
        {
            List<CoordinatePoint> pointURIs = new List<CoordinatePoint>();

            List<DBpedia.DbpediaInfo> dbpediaResults = DBpedia.getDbpediaInfo(_rasterInfo.rasterTopLeftX , _rasterInfo.rasterTopLeftY, _rasterInfo.rasterDownRightX, _rasterInfo.rasterDownRightY);
            DBpedia.DbpediaInfo answer = new DBpedia.DbpediaInfo();

            foreach (CoordinatePoint item in coordinatePoints)
            {
                answer = findMinDistance(item, dbpediaResults);
                CoordinatePoint pnt = new CoordinatePoint();
                pnt.lat = item.lat;
                pnt.lng = item.lng;
                item.URI = pnt.URI = answer.uri;
                item.type = pnt.type = answer.type;
                dbpediaResults.Remove(answer);
                pointURIs.Add(pnt);
            }
            string type = votting();
            for (int i = 0; i < coordinatePoints.Length; i++)

                if (pointURIs[i].type != type)
                    pointURIs[i].type = "";


        }
        private DBpedia.DbpediaInfo findMinDistance(CoordinatePoint item, List<DBpedia.DbpediaInfo> dbpediaResults)
        {
            double dis = int.MaxValue;
            double xS, yS, xD, yD;
            DBpedia.DbpediaInfo min = new DBpedia.DbpediaInfo();
            foreach (DBpedia.DbpediaInfo node in dbpediaResults)
            {
                GetXY(item.lng, item.lat, out xS, out yS);
                GetXY(node.lng, node.lat, out xD, out yD);
                double d = shortestDistance(xS, yS, xD, yD);
                if (d < dis)
                {
                    dis = d;
                    min = node;
                }
            }
            return min;

        }
        private int countVote(string type)
        {

            int count = 0;
            foreach (CoordinatePoint cp in coordinatePoints)
            {
                if (cp.type == type)
                    count++;
            }
            return count;
        }
        public string votting()
        {
            int max = 0;
            string type = "";
            foreach (CoordinatePoint item in coordinatePoints)
            {
                int m = countVote(item.type);

                if (m > max)
                {
                    max = m;
                    type = item.type;

                }
            }
            return type;
        }
#endregion

    }
}
