using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ArcStrabo
{
    public class ArcStrabo2Extension : ESRI.ArcGIS.Desktop.AddIns.Extension
    {
        public static string ErrorMsgNoTable = "Please upload a data table for transformation";
        public static string ErrorMsgNoStraboHome = "Unable to access the environment variable STRABO_HOME";
        public static string ErrorMsgNoKarmaHomeWritePermission = "Unable to write files to STRABO_HOME. Please make sure you have the write permission to ";

        public static string EnvironmentVariableSTRABO_HOME = "STRABO_HOME";


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
