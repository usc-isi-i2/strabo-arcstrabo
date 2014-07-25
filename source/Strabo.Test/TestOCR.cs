using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Strabo.Core.OCR;
using Emgu.CV.OCR;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Strabo.Test
{
    public class TestOCR
    {
        public static void test()
        {
            Tesseract _ocr = new Tesseract(@"C:\Users\yaoyichi\Desktop\emgucv-windows-universal-cuda 2.9.0.1922\", "eng", Tesseract.OcrEngineMode.OEM_TESSERACT_CUBE_COMBINED);

            string dir = @"C:\Users\yaoyichi\Desktop\";
            string fn = "100890509_mc1024_k64.png";
            Image<Bgr, Byte> gray = new Image<Bgr, byte>(dir + fn);
            _ocr.Recognize(gray);
            //TextRecognitionWorker trw = new TextRecognitionWorker();
            //trw.ApplyABBYYOCR(dir, fn);
        }
    }
}
