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
namespace ArcStrabo10
{
    public class ComboBoxLanguageSelector : ESRI.ArcGIS.Desktop.AddIns.ComboBox
    {
        private static ComboBoxLanguageSelector s_comboBox;
        public string selected_language;


        public ComboBoxLanguageSelector()
        {
            s_comboBox = this;
            s_comboBox.Enabled = true;
        
            
        }


        internal static ComboBoxLanguageSelector GetLanguageNameComboBox()
        {
            return s_comboBox;
        }


        private static Dictionary<string, string> languages = new Dictionary<string, string>()
        {
            {"English", "eng"},
            {"Farsi", "per"},
            {"Chinese", "chi_sim"}
        };


        internal void AddItem(string lng)
        {
            s_comboBox.Add(lng);
        }


        internal void ClearAll()
        {
            s_comboBox.Clear();
        }


        protected override void OnUpdate()
        {
           // this.Enabled = true;
            Enabled = ArcMap.Application != null;

        }


        protected override void OnSelChange(int cookie)
        {

            foreach (ComboBox.Item item in this.items)
            {
                if (cookie == item.Cookie)
                {
                    selected_language = item.Caption;

                }
            }

            // Fire ContentsChanged event to cause TOC to refresh with new selected layers.
            ArcMap.Document.ActiveView.ContentsChanged(); ;

        }


        public string Get_selected_language()
        {
            if (selected_language == null)
            {
                
                return null;
            }

            return languages[selected_language];
        }

    }
}