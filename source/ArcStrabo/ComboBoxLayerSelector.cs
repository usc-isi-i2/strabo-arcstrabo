using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Desktop.AddIns;
namespace ArcStrabo
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
            //   if (s_comboBox.items.Count == 0)
            //  {
            s_comboBox.Add(itemName, layer);
            //s_comboBox.Select(m_selAllCookie);
            //   }

            // Add each item to combo box.
            //  int cookie = s_comboBox.Add(itemName, layer);
        }

        internal void ClearAll()
        {
            //   m_selAllCookie = -1;
            s_comboBox.Clear();
            //s_comboBox.Add("Select a map");

        }

        protected override void OnUpdate()
        {
            this.Enabled = ArcStrabo2Extension.IsExtensionEnabled();
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
