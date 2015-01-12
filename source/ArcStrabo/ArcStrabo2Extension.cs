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
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Desktop.AddIns;
using Strabo.Core.Utility;

namespace ArcStrabo
{
    public class ArcStrabo2Extension : ESRI.ArcGIS.Desktop.AddIns.Extension
    {
        public static string ErrorMsgNoInputMap = "No input map has been selected from the drop-down menu in the ArcStrabo2 Toolbar." + "\n \n" + "Please select an input map for text extraction.";
        public static string ErrorMsgNoInputLanguage = "No input language has been selected from the drop-down menu in the ArcStrabo2 Toolbar." + "\n \n" + "Please select a language for text extraction.";
        public static string ErrorMsgNameTextLayer = "Unable to create unique name for TextLayer.png file";
        public static string ErrorMsgNameOCRLayer = "Unable to create unique name for OCRLayer file";
        public static string ErrorMsgNoTable = "Please upload a data table for transformation";
        public static string ErrorMsgNoStraboHome = "Unable to access the environment variable " + EnvironmentVariableSTRABO_HOME;
        public static string ErrorMsgNoTess_Data = "Unable to access the environment variable " + EnvironmentVariableTESS_DATA;
        public static string ErrorMsgNoStraboHomeWritePermission = "Unable to write files to " + EnvironmentVariableSTRABO_HOME + ". Please make sure you have the write permission.";

        public static string EnvironmentVariableSTRABO_HOME = "STRABO_HOME";
        public static string EnvironmentVariableTESS_DATA = "TESSDATA_HOME";

        public static string Text_Result_Path = "\\result";
        public static string Result_Shapefile_Folder_Name = "ocr_shapefile";
        public static string Symbol_Result_Path = "\\result";
        public static string Log_Path = "\\log";
        public static string Output_Path = "\\output";
        public static string Output_SubPath = "\\output";
        public static string Tessdata_Path;
        public static string Intermediate_Result_Path = "\\intermediate_result";

        public static string TextPositiveLabelLayerName = "TextPositiveLabel";
        public static string TextNegtiveLabelLayerName = "TextNegativeLabel";

        public static string TextPositiveLabelLayerJSONFileName = "PositiveLayerInfo.json";
        public static string TextNegtiveLabelLayerJSONFileName = "NegativeLayerInfo.json";
        public static string TesseractResultsJSONFileName = "tesseract_geojson.json";
        public static string TextLayerPNGFileName = "TextLayer.png";
        public static string TextLayerOCRShapefile = "OCRLayer";

        public static bool PathSet = false;

        public static ProgressForm pForm = new ProgressForm();

        private IMap m_map;
        private static ArcStrabo2Extension s_extension;

        //http://help.arcgis.com/en/sdk/10.0/arcobjects_net/componenthelp/index.html#/ArcMap_Element/001v000001s7000000/

        public ArcStrabo2Extension()
        {
        }

        protected override void OnStartup()
        {
            s_extension = this;

            // Wire up events
            ArcMap.Events.NewDocument += ArcMap_NewOpenDocument;
            ArcMap.Events.OpenDocument += ArcMap_NewOpenDocument;
        }
        private void ArcMap_NewOpenDocument()
        {
            IActiveViewEvents_Event pageLayoutEvent = ArcMap.Document.PageLayout as IActiveViewEvents_Event;
            pageLayoutEvent.FocusMapChanged += new IActiveViewEvents_FocusMapChangedEventHandler(AVEvents_FocusMapChanged);

            Initialize();
        }
        private void WireDocumentEvents()
        {
            //
            // TODO: Sample document event wiring code. Change as needed
            //

            // Named event handler
            ArcMap.Events.NewDocument += delegate() { ArcMap_NewDocument(); };

            // Anonymous event handler
            ArcMap.Events.BeforeCloseDocument += delegate()
            {
                // Return true to stop document from closing
                ESRI.ArcGIS.Framework.IMessageDialog msgBox = new ESRI.ArcGIS.Framework.MessageDialogClass();
                return msgBox.DoModal("BeforeCloseDocument Event", "Abort closing?", "Yes", "No", ArcMap.Application.hWnd);
            };
        }

        void ArcMap_NewDocument()
        {
            // TODO: Handle new document event
        }
        private void Initialize()
        {
            // If the extension hasn't been started yet or it's been turned off, bail
            if (s_extension == null || this.State != ExtensionState.Enabled)
                return;

            // Reset event handlers
            IActiveViewEvents_Event avEvent = ArcMap.Document.FocusMap as IActiveViewEvents_Event;
            avEvent.ItemAdded += AvEvent_ItemAdded;
            avEvent.ItemDeleted += AvEvent_ItemAdded;

            avEvent.ContentsChanged += avEvent_ContentsChanged;

            // Update the UI
            m_map = ArcMap.Document.FocusMap;

            FillLayerComboBox();
            FillLanguageComboBox();
        }

        private void Uninitialize()
        {
            if (s_extension == null)
                return;

            // Detach event handlers
            IActiveViewEvents_Event avEvent = m_map as IActiveViewEvents_Event;
            avEvent.ItemAdded -= AvEvent_ItemAdded;
            avEvent.ItemDeleted -= AvEvent_ItemAdded;
            avEvent.ContentsChanged -= avEvent_ContentsChanged;
            avEvent = null;

            // Update UI
            ComboBoxLayerSelector layerNameCombo = ComboBoxLayerSelector.GetLayerNameComboBox();
            if (layerNameCombo != null)
                layerNameCombo.ClearAll();

        }
        internal static bool IsExtensionEnabled()
        {
            if (s_extension == null)
                GetExtension();

            if (s_extension == null)
                return false;

            return s_extension.State == ExtensionState.Enabled;
        }

        // Event handlers
        private void avEvent_ContentsChanged()
        {

        }

        private void AvEvent_ItemAdded(object Item)
        {
            m_map = ArcMap.Document.FocusMap;
            FillLayerComboBox();
        }

        private void AVEvents_FocusMapChanged()
        {
            Initialize();
        }

        private void FillLayerComboBox()
        {
            ComboBoxLayerSelector layerNameCombo = ComboBoxLayerSelector.GetLayerNameComboBox();
            if (layerNameCombo == null)
                return;

            layerNameCombo.ClearAll();

            IRasterLayer rasterLayer;
            // Loop through the layers in the map and add the layer's name to the combo box.
            for (int i = 0; i < m_map.LayerCount; i++)
            {
                if (m_map.get_Layer(i) is IRasterLayer)
                {
                    rasterLayer = m_map.get_Layer(i) as IRasterLayer;
                    if (rasterLayer == null)
                        break;

                    layerNameCombo.AddItem(rasterLayer.Name, rasterLayer);
                }
            }
        }

        private void FillLanguageComboBox()
        {
            ComboBoxLanguageSelector languageNameCombo = ComboBoxLanguageSelector.GetLanguageNameComboBox();
            if (languageNameCombo == null)
                return;

            languageNameCombo.ClearAll();

            languageNameCombo.AddItem("English");
            languageNameCombo.AddItem("Farsi");
            languageNameCombo.AddItem("Chinese");
        }

        private static ArcStrabo2Extension GetExtension()
        {
            return s_extension;
        }
        public static bool initialize_straboPath_directories(string _straboPath)
        {
            // check whether straboPath exists
            if (string.IsNullOrEmpty(_straboPath))
            {
                return false;
            }
            if (ArcStrabo2Extension.Output_Path == ArcStrabo2Extension.Output_SubPath)
            {
                ArcStrabo2Extension.Output_Path = _straboPath + ArcStrabo2Extension.Output_Path;
                ArcStrabo2Extension.Text_Result_Path = ArcStrabo2Extension.Output_Path + ArcStrabo2Extension.Text_Result_Path;
                ArcStrabo2Extension.Log_Path = ArcStrabo2Extension.Output_Path + ArcStrabo2Extension.Log_Path;
                ArcStrabo2Extension.Intermediate_Result_Path = ArcStrabo2Extension.Output_Path + ArcStrabo2Extension.Intermediate_Result_Path;
            }
            try
            {
                // check Output_Path
                if (Directory.Exists(ArcStrabo2Extension.Output_Path))
                {
                    Directory.Delete(ArcStrabo2Extension.Output_Path, true);
                }
                else
                    Directory.CreateDirectory(ArcStrabo2Extension.Output_Path);

                Directory.CreateDirectory(ArcStrabo2Extension.Text_Result_Path);
                // check Log_Path
                Directory.CreateDirectory(ArcStrabo2Extension.Log_Path);
                // check Intermediate_Result_Path
                Directory.CreateDirectory(ArcStrabo2Extension.Intermediate_Result_Path);
                return true;
            }
            catch (Exception e)
            {
                Log.WriteLine(e.Message);
                Log.WriteLine(e.ToString());
                return false;
            }
        }
    }

}
