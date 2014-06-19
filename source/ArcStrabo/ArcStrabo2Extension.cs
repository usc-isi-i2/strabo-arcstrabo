using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ArcStrabo
{
    public class ArcStrabo2Extension : ESRI.ArcGIS.Desktop.AddIns.Extension
    {
        public static string ErrorMsgNoTable = "Please upload a data table for transformation";
        public static string ErrorMsgNoStraboHome = "Unable to access the environment variable " + EnvironmentVariableSTRABO_HOME;
        public static string ErrorMsgNoTess_Data = "Unable to access the environment variable "+ EnvironmentVariableTESS_DATA;
        public static string ErrorMsgNoStraboHomeWritePermission = "Unable to write files to "+EnvironmentVariableSTRABO_HOME+". Please make sure you have the write permission.";

        public static string EnvironmentVariableSTRABO_HOME = "STRABO_HOME";
        public static string EnvironmentVariableTESS_DATA = "TESSDATA_PREFIX";

        // Renuka: adding folders
        public static string Text_Result_Path = "\\result";
        public static string Result_Shapefile_Folder_Name = "ocr_shapefile";
        public static string Symbol_Result_Path = "\\result";
        public static string Log_Path = "\\log";
        public static string Output_Path = "\\output";
        public static string Tessdata_Path;
        public static string Intermediate_Result_Path = "\\intermediate_result";

        public static string TextPositiveLabelLayerName = "TextPositiveLabel";
        public static string TextNegtiveLabelLayerName = "TextNegativeLabel";

        public static string TextPositiveLabelLayerJSONFileName ="PositiveLayerInfo.json";
        public static string TextNegtiveLabelLayerJSONFileName ="NegativeLayerInfo.json";
        public static string TesseractResultsJSONFileName = "tesseract_geojson.json";
        public static string TextLayerPNGFileName = "Result.png";

        public static ProgressForm pForm = new ProgressForm();

        public ArcStrabo2Extension()
        {
        }

        protected override void OnStartup()
        {
            //
            // TODO: Uncomment to start listening to document events
            //
            // WireDocumentEvents();
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

    }

}
