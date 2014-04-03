using Strabo.Core.TextLayerExtraction;
using Strabo.Core.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing.Imaging;
using Newtonsoft.Json;
using Strabo.Core.OCR;


namespace Strabo.Test
{
    public class TestTextLayerSeparation
    {

        public static void textLayerExtract(string sourceImageDir)
        {
            if (sourceImageDir == "")
                return;
            
            List<Bitmap> imgfnp_list = new List<Bitmap>();
            List<Bitmap> imgfnn_list = new List<Bitmap>();

            Image<Bgr, Byte> srcImage = new Image<Bgr, byte>(sourceImageDir);

            fillImageList(@"C:\Users\simakmo\Documents\Visual Studio 2010\Projects\ArcStrabo\ArcStrabo\PositiveLayerInfo.json", imgfnp_list, srcImage);//   fillImageList(@"\..\..\PositiveLayerInfo.json", imgfnp_list, srcImage);
            fillImageList(@"C:\Users\simakmo\Documents\Visual Studio 2010\Projects\ArcStrabo\ArcStrabo\NegativeLayerInfo.json", imgfnn_list, srcImage);//   fillImageList(@"\..\..\NegativeLayerInfo.json", imgfnn_list, srcImage);


            //string dir = @"C:\Users\simakmo\Documents\Visual Studio 2010\Projects\LayerAccessingTest\LayerAccessingTest\data\";
            string dir = @"C:\Users\simakmo\Documents\Visual Studio 2010\Projects\ArcStrabo\ArcStrabo\data\"; //@"\..\..\data\";
            Bitmap srcimg = new Bitmap(sourceImageDir);
            TextLayerExtractionTrainer tet = new TextLayerExtractionTrainer();
            Log.SetStartTime();
            List<Bitmap> result_list = new List<Bitmap>();
            for (int i = 0; i < 1; i++)
                result_list = tet.GUIProcessOneLayerOnly(srcimg, imgfnp_list, imgfnn_list, 4);
            Console.WriteLine(Log.GetDurationInSeconds());

            result_list[0].Save(dir + "Result.png");

        }


        private static void fillImageList(string path, List<Bitmap> imgList, Image<Bgr, Byte> srcImage)
        {

            FeatureInJSON _featureInJSON;
            GeoJson geoJson = new GeoJson();
            
            if (!File.Exists(path))
                return;
            _featureInJSON = geoJson.readGeoJsonFile(path);

            for (int j = 0; j < _featureInJSON.features.Count; j++)
            {
                int x1, x2, y1, y2;
                x1 = _featureInJSON.features[j].geometry.rings[0, 0, 0];
                y1 =  _featureInJSON.features[j].geometry.rings[0, 0, 1];
                x2 = _featureInJSON.features[j].geometry.rings[0, 1, 0];
                y2 =  _featureInJSON.features[j].geometry.rings[0, 2, 1];

                Rectangle rec = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                //Image<Bgr, Byte> test = srcImage.GetSubRect(rec);
                Bitmap img = srcImage.Bitmap;
                Bitmap cropedImage = img.Clone(rec, img.PixelFormat);
                cropedImage.Save(@"C:\Users\simakmo\Documents\Visual Studio 2010\Projects\ArcStrabo\ArcStrabo\t" + j.ToString());
                imgList.Add(cropedImage);

            }




        }
    }
}
#region
//public  void textLayerExtract(string sourceImageDir )
//        {

//            List<Bitmap> imgfnp_list = new List<Bitmap>();
//            List<Bitmap> imgfnn_list = new List<Bitmap>();
//            Image<Bgr, Byte> srcImage = new Image<Bgr, byte>(sourceImageDir);
//            ////= @"..\..\Images\US_U_12245_1_67_crop.png";

//            GeoJson geoJson = new GeoJson();
//            _featureInJSON = geoJson.readGeoJsonFile(@"..\..\PositiveLayerInfo.json");


//string jsonString = File.ReadAllText(@"C:\Users\simakmo\Documents\Visual Studio 2010\Projects\LayerAccessingTest\LayerAccessingTest\ResultImageInfo.txt");
//Strabo.Test.Json.LayerFeatures lfList = jsonReader.layerFeatures = JsonConvert.DeserializeObject<Strabo.Test.Json.LayerFeatures>(jsonString);


//foreach (Strabo.Test.Json.LayerFeature lf in lfList)
//{
//    Rectangle rec = new Rectangle(Int32.Parse(lf.x.ToString()), Int32.Parse((lf.y * -1).ToString()),
//        Int32.Parse(lf.width.ToString()), Int32.Parse(lf.height.ToString()));
//    //Image<Bgr, Byte> test = srcImage.GetSubRect(rec);
//    Bitmap img = srcImage.Bitmap;
//    Bitmap cropedImage = img.Clone(rec, img.PixelFormat);


//    if (lf.type == Json.FeatureType.PositiveLabel)
//    {
//        imgfnp_list.Add(cropedImage);
//        //cropedImage.Save(@"C:\Users\simakmo\Documents\test1.jpg", ImageFormat.Jpeg);
//    }
//    else
//    {
//        imgfnn_list.Add(cropedImage);
//        //cropedImage.Save(@"C:\Users\simakmo\Documents\test2.jpg", ImageFormat.Jpeg);
//    }
//}


//string dir = @"C:\Users\simakmo\Documents\Visual Studio 2010\Projects\LayerAccessingTest\LayerAccessingTest\data\";
//Bitmap srcimg = new Bitmap(sourceImageDir);
//TextLayerExtractionTrainer tet = new TextLayerExtractionTrainer();
//Log.SetStartTime();
//List<Bitmap> result_list = new List<Bitmap>();
//for (int i = 0; i < 1; i++)
//    result_list = tet.GUIProcessOneLayerOnly(srcimg, imgfnp_list, imgfnn_list, 4);
//Console.WriteLine(Log.GetDurationInSeconds());

//result_list[0].Save(dir + "rr.png");



//  }
//string dir = @"..\..\data\";
//string fn = "US_U_12245_1_67_crop.png";
//string fnp = "US_U_12245_1_67_crop_p.png";
//string fnn = "US_U_12245_1_67_crop_n.png";

//Bitmap srcimg = new Bitmap(dir + fn);
//Bitmap imgfnp = new Bitmap(dir + fnp);
//Bitmap imgfnn = new Bitmap(dir + fnn);

//List<Bitmap> imgfnp_list = new List<Bitmap>();
//imgfnp_list.Add(imgfnp);
//List<Bitmap> imgfnn_list = new List<Bitmap>();
//imgfnn_list.Add(imgfnn);
//TextLayerExtractionTrainer tet = new TextLayerExtractionTrainer();

//Log.SetStartTime();

//List<Bitmap> result_list = new List<Bitmap>();
//for (int i = 0; i < 1; i++)
//    result_list = tet.GUIProcessOneLayerOnly(srcimg, imgfnp_list, imgfnn_list, 4);
//Console.WriteLine(Log.GetDurationInSeconds());

//result_list[0].Save(dir + "rr.png");

//Image<Gray, Byte> img1 = new Image<Gray, Byte>(@"C:\Users\simakmo\Source\Repos\Strabo\data\lena.jpg");
// ImageViewer.Show(img1, "Result Window");
#endregion