using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Strabo.Core.OCR;

namespace Strabo.Test
{
    public class TestOCR
    {
        public static void test()
        {
            string dir = @"..\..\data\Results\";
            string fn = "US_U_12245_1_67_crop.png";
            TextRecognitionWorker trw = new TextRecognitionWorker();
            trw.ApplyABBYYOCR(dir, fn);
        }
    }
}
