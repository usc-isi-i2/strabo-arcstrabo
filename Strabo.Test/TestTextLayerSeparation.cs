using Strabo.Core.TextLayerExtraction;
using Strabo.Core.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strabo.Test
{
    public class TestTextLayerSeparation
    {
        
        public static void test()
        {
            string dir=@"..\..\data\";
            string fn = "US_U_12245_1_67_crop.png";
            string fnp = "US_U_12245_1_67_crop_p.png";
            string fnn = "US_U_12245_1_67_crop_n.png";

            Bitmap srcimg = new Bitmap(dir + fn);
            Bitmap imgfnp = new Bitmap(dir + fnp);
            Bitmap imgfnn = new Bitmap(dir + fnn);

            List<Bitmap> imgfnp_list = new List<Bitmap>();
            imgfnp_list.Add(imgfnp);
            List<Bitmap> imgfnn_list = new List<Bitmap>();
            imgfnn_list.Add(imgfnn);
            TextLayerExtractionTrainer tet = new TextLayerExtractionTrainer();
           
            Log.SetStartTime();

            List<Bitmap> result_list = new List<Bitmap>();
            for(int i=0;i<1;i++)
                result_list=tet.GUIProcessOneLayerOnly(srcimg, imgfnp_list, imgfnn_list, 4);
            Console.WriteLine(Log.GetDurationInSeconds());
           
            result_list[0].Save(dir + "rr.png");

        }
    }
}
