using Strabo.Core.Utility;
using Strabo.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Strabo.Core.ImageProcessing;

namespace Strabo.Core.OCR
{
    public class TextRecognitionWorker
    {
        public TextRecognitionWorker() { }
        public void ApplyABBYYOCR(string dir, string fn)
        {
            try
            {
                ABBYYSingleStringResultParser assrp = new ABBYYSingleStringResultParser();
                StreamWriter sw = new StreamWriter(dir + "OCR_results.txt");
                assrp.ReadOCRResults(dir);
                //sw.WriteLine("Text ID" + ";"
                //             + "Recognized Text" + ";" + "Suspicious Text" + ";" + "BoundingBox Top-Left X" + ";" + "BoundingBox Top-Left Y" + ";" + "BoundingBox Width" + ";" + "BoundingBox Height" + ";" + "Position X" + ";" + "Position Y" + ";" + "Orientation");
                sw.WriteLine("ID" + ","
                            + "Text" + "," + "BoundingBoxTopLeftX" + "," + "BoundingBoxTopLeftY" + "," + "BoundingBoxWidth" + "," + "BoundingBoxHeight");
                Bitmap srcimg = new Bitmap(dir+fn);
                srcimg = ImageUtils.AnyToFormat24bppRgb(srcimg);

                Graphics g = Graphics.FromImage(srcimg);
                for (int i = 0; i < assrp.textlabel_list.Count; i++)
                {
                    TextLabel textlabel = assrp.textlabel_list[i];
                    Font font = new Font("Arial", 10);
                    try
                    {
                        SolidBrush sb = new SolidBrush(Color.FromArgb(50, 0, 0, 0));
                        g.DrawRectangle(
                            new Pen(Color.Black, 3), new Rectangle(textlabel.bbxx, textlabel.bbxy, textlabel.bbxw, textlabel.bbxh));
                        g.FillRectangle(
                           sb, new Rectangle(textlabel.bbxx, textlabel.bbxy, textlabel.bbxw, textlabel.bbxh));

                        //g.DrawString(textlabel.id.ToString(), font, Brushes.Black, textlabel.bbxx, textlabel.bbxy);
                        g.DrawString(textlabel.text, font, Brushes.Black, textlabel.bbxx, textlabel.bbxy + textlabel.bbxh);
                    }
                    catch { }
                    if (i < assrp.textlabel_list.Count - 1)
                        sw.WriteLine(textlabel.id + ","
                        + textlabel.text + "," + textlabel.bbxx + "," + -1 * textlabel.bbxy + "," + textlabel.bbxw + "," + textlabel.bbxh);
                    else
                        sw.Write(textlabel.id + ","
                       + textlabel.text + "," + textlabel.bbxx + "," + -1 * textlabel.bbxy + "," + textlabel.bbxw + "," + textlabel.bbxh);
                    //srcimg.Save(CurrentState.output_dir + CurrentState.text_recognition_results_fn, ImageFormat.Png);
                }
                sw.Close();
                g.Dispose();

                //ShapefileUtils sut = new ShapefileUtils();
                //sut.WrtieText2Shp(assrp, dir, "OCR_results");
            }
            catch (Exception exception)
            {
                Log.WriteLine("GenerateOCRResults: " + exception.Message);
            }
        }
    }
}
