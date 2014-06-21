using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Strabo.Core.OCR;
using Emgu.CV.OCR;

namespace Strabo.Test
{
    public class TestOCR
    {
        public static void test()
        {
            //Tesseract _ocr = new Tesseract(@"C:\Emgu\emgucv-windows-universal-cuda 2.9.0.1922\bin\", "eng", Tesseract.OcrEngineMode.OEM_TESSERACT_CUBE_COMBINED);
        
            string dir = @"..\..\data\Results\";
            string fn = "US_U_12245_1_67_crop.png";
            //TextRecognitionWorker trw = new TextRecognitionWorker();
            //trw.ApplyABBYYOCR(dir, fn);
        }
    }
}
