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

namespace ArcStrabo
{
    class ArcStraboObject
    {

        FeatureInJSON _featureInJSON;


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
                        if (rasterLayer.Name != "PositiveLabel" && rasterLayer.Name != "NegativeLabel" && rasterLayer.Name != "OCRLayer")

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

        /// <summary>
        /// access to the positive and negative layer and save them on the GeoJson File
        /// </summary>
        public void MakingGeoJsonFile(string dir)
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




                    if (layer.Name == "PositiveLabel" || layer.Name == "NegativeLabel")
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

                        if (layer.Name == "PositiveLabel")
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
                            geoJson.featureInJson.features[i].geometry.rings[0, 0, 0] = int.Parse(Math.Round(iFeature.Extent.UpperLeft.X).ToString());
                            geoJson.featureInJson.features[i].geometry.rings[0, 0, 1] = int.Parse(Math.Round(iFeature.Extent.UpperLeft.Y).ToString());
                            geoJson.featureInJson.features[i].geometry.rings[0, 1, 0] = int.Parse(Math.Round(iFeature.Extent.UpperRight.X).ToString());
                            geoJson.featureInJson.features[i].geometry.rings[0, 1, 1] = int.Parse(Math.Round(iFeature.Extent.UpperRight.Y).ToString());
                            geoJson.featureInJson.features[i].geometry.rings[0, 2, 0] = int.Parse(Math.Round(iFeature.Extent.LowerRight.X).ToString());
                            geoJson.featureInJson.features[i].geometry.rings[0, 2, 1] = int.Parse(Math.Round(iFeature.Extent.LowerRight.Y).ToString());
                            geoJson.featureInJson.features[i].geometry.rings[0, 3, 0] = int.Parse(Math.Round(iFeature.Extent.LowerLeft.X).ToString());
                            geoJson.featureInJson.features[i].geometry.rings[0, 3, 1] = int.Parse(Math.Round(iFeature.Extent.LowerLeft.Y).ToString());
                            geoJson.featureInJson.features[i].geometry.rings[0, 4, 0] = int.Parse(Math.Round(iFeature.Extent.UpperLeft.X).ToString());
                            geoJson.featureInJson.features[i].geometry.rings[0, 4, 1] = int.Parse(Math.Round(iFeature.Extent.UpperLeft.Y).ToString());

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
        /// ////////TextExtraction and TextIdentifier Methods
        /// </summary>
        /// <param name="sourceImageDir"></param>
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
                    int x1, x2, y1, y2;
                    x1 = _featureInJSON.features[j].geometry.rings[0, 0, 0];
                    y1 = _featureInJSON.features[j].geometry.rings[0, 0, 1];
                    x2 = _featureInJSON.features[j].geometry.rings[0, 1, 0];
                    y2 = _featureInJSON.features[j].geometry.rings[0, 2, 1];

                    Rectangle rec = new Rectangle(x1, y1 * -1, x2 - x1, y1 - y2);
                    //Image<Bgr, Byte> test = srcImage.GetSubRect(rec);
                    Bitmap img = srcImage.Bitmap;
                    Bitmap cropedImage = img.Clone(rec, img.PixelFormat);
                    //cropedImage.Save(path+"\\img" + j.ToString());
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
    }
}
