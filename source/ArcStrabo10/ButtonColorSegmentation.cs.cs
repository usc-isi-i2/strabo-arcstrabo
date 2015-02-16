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
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

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
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Editor;

using Strabo.Core.ColorSegmentation;

namespace ArcStrabo10
{
    public class ButtonColorSegmentation : ESRI.ArcGIS.Desktop.AddIns.Button
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

                bool Initialize_straboPath_Correct = ArcStrabo10Extension.initialize_straboPath_directories(straboPath);

                if (Initialize_straboPath_Correct == false)
                {
                    MessageBox.Show(ArcStrabo10Extension.ErrorMsgNoStraboHomeWritePermission);
                    return;
                }

                ArcStrabo10Extension.PathSet = true;
            }
            //
            //  TODO: Sample code showing how to access button host
            //
            ArcMap.Application.CurrentTool = null;
            ComboBoxLayerSelector layerNameCombo = ComboBoxLayerSelector.GetLayerNameComboBox();

            RasterLayer rasterlayer = new RasterLayer();
            rasterlayer = ((RasterLayer)layerNameCombo.GetSelectedLayer());
            //raster.Raster
            //RasterLayer raster2 = new RasterLayer();
            //raster2.CreateFromRaster(raster.Raster);
            //IMap map = ArcMap.Document.FocusMap;
            //map.AddLayer((ILayer)raster2);
            //MessageBox.Show(layerNameCombo.selected_layer_name + " " + raster2.RowCount + " " + raster2.ColumnCount + " " + raster2.BandCount);
            ColorSegmentationWorker cs = new ColorSegmentationWorker();
            try
            {
                IRaster2 iraster2 = rasterlayer.Raster as IRaster2;
                string[] bitmap_fns = cs.Apply(System.IO.Path.GetDirectoryName(iraster2.RasterDataset.CompleteName) + "\\", ArcStrabo10Extension.Text_Result_Path + "\\", System.IO.Path.GetFileName(iraster2.RasterDataset.CompleteName));
                IMap map = ArcMap.Document.FocusMap;
                foreach (string path in bitmap_fns)
                {
                    //RasterDataset rds = new RasterDataset();
                    //rds.OpenFromFile(path);
                    RasterLayer rasterlayer2 = new RasterLayer();
                    rasterlayer2.CreateFromFilePath(path);
                    map.AddLayer(rasterlayer2);
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }

        }
        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }
    }
}
