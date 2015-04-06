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
namespace Strabo.Test
{
   public class LayerAccessing
    {
       public void test(string resultFileDir)
       {

           const string DirSource = "#SourceImagePath:";
           const string Positive = "#PositiveLabel";
           const string Negative = "#NegativeLabel";
           const string X = "X=";
           const string Y = "Y=";
           const string Width = "Width=";
           const string Height = "Height=";

           Boolean positiveLb, negativeLb, feature = false;
           int x, y, width, height;
           StreamReader sr = new StreamReader(resultFileDir);
           string sourceImageDir = "";
           string temp = "";
           List<Bitmap> imgfnp_list = new List<Bitmap>();
           List<Bitmap> imgfnn_list = new List<Bitmap>();


           //////////////////get SourceImageDir///////////////
           temp = sr.ReadLine();
           sourceImageDir = temp.Substring(DirSource.Length, temp.Length);
           temp = sr.ReadLine();
           while (temp != null)
           {
               positiveLb = temp.Contains(Positive);
               negativeLb = temp.Contains(Negative);
               temp = sr.ReadLine();
               if (positiveLb || negativeLb || feature)
               {


                   temp = sr.ReadLine();
                   x = int.Parse(temp.Substring(X.Length, temp.Length));
                   temp = sr.ReadLine();
                   y = int.Parse(temp.Substring(Y.Length, temp.Length));
                   temp = sr.ReadLine();
                   width = int.Parse(temp.Substring(Width.Length, temp.Length));
                   temp = sr.ReadLine();
                   height = int.Parse(temp.Substring(Height.Length, temp.Length));
                   Rectangle rec = new Rectangle(x, y, width, height);
                   Image<Bgr, Byte> srcImage = new Image<Bgr, byte>(sourceImageDir);
                   Bitmap img = srcImage.GetSubRect(rec).Bitmap;

                   if (positiveLb)
                       imgfnp_list.Add(img);
                   else
                       imgfnn_list.Add(img);


                   temp = sr.ReadLine();
                   positiveLb = temp.Contains(Positive);
                   negativeLb = temp.Contains(Negative);
                   feature = temp.Contains("Feature");


               }
           }




           string dir = @"..\..\data\";
           //string fn = "US_U_12245_1_67_crop.png";
           //string fnp = "US_U_12245_1_67_crop_p.png";
           //string fnn = "US_U_12245_1_67_crop_n.png";

           Bitmap srcimg = new Bitmap(sourceImageDir);
           //Bitmap imgfnp = new Bitmap(dir + fnp);
           //Bitmap imgfnn = new Bitmap(dir + fnn);

           //List<Bitmap> imgfnp_list = new List<Bitmap>();
           //imgfnp_list.Add(imgfnp);
           //List<Bitmap> imgfnn_list = new List<Bitmap>();
           //imgfnn_list.Add(imgfnn);
           TextLayerExtractionTrainer tet = new TextLayerExtractionTrainer();

           Log.SetStartTime();

           List<Bitmap> result_list = new List<Bitmap>();
           for (int i = 0; i < 1; i++)
               result_list = tet.GUIProcessOneLayerOnly(srcimg, imgfnp_list, imgfnn_list, 4);
           Console.WriteLine(Log.GetDurationInSeconds());

           result_list[0].Save(dir + "rr.png");
       }
    }
}
