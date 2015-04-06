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
    public class ComboBoxLayerSelector : ESRI.ArcGIS.Desktop.AddIns.ComboBox
    {
        private static ComboBoxLayerSelector s_comboBox;
        public string selected_layer_name;


        public ComboBoxLayerSelector()
        {
            s_comboBox = this;
            s_comboBox.Enabled = true;
        }

        internal static ComboBoxLayerSelector GetLayerNameComboBox()
        {
            return s_comboBox;
        }

        internal void AddItem(string itemName, IRasterLayer layer)
        {
            s_comboBox.Add(itemName, layer);
            if (s_comboBox.items.Count == 0)
            {
               
              // s_comboBox.Select(m_selAllCookie);
            }

            // Add each item to combo box.
            //  int cookie = s_comboBox.Add(itemName, layer);
        }

        internal void ClearAll()
        {
          //  m_selAllCookie = -1;
            s_comboBox.Clear();
          //s_comboBox.Add("Select a map");

        }

        protected override void OnUpdate()
        {
           // this.Enabled = true;
            Enabled = ArcMap.Application != null;
           
        }

        protected override void OnSelChange(int cookie)
        {
            //if (cookie == -1)
            //    return;

            foreach (ComboBox.Item item in this.items)
            {
                IRasterLayer fl = item.Tag as IRasterLayer;
                if (fl == null)
                    continue;

                if (cookie == item.Cookie)
                {
                    selected_layer_name = item.Caption;
                }
            }

            // Fire ContentsChanged event to cause TOC to refresh with new selected layers.
            ArcMap.Document.ActiveView.ContentsChanged(); ;

        }
        public IRasterLayer GetSelectedLayer()
        {
            foreach (ComboBox.Item item in this.items)
            {
                IRasterLayer fl = item.Tag as IRasterLayer;
                if (fl == null)
                    continue;

                if (item.Caption == selected_layer_name)
                {
                    return item.Tag as IRasterLayer;

                }

            }
            return null;
        }

     
       
    }

}
