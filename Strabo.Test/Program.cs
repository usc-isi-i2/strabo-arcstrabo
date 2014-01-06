using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;

namespace Strabo.Test
{
    /// <summary>
    /// Use this Main class to call your test classes
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            TestYourEmguCVSetting();
        }

        /// <summary>
        /// Test your EmguCV setting. You should be able to see the Lena picture if your EmguCV setting is correct.
        /// Check here for more information for the EmguCV installation: http://www.emgu.com/wiki/index.php/Download_And_Installation
        /// </summary>
        static void TestYourEmguCVSetting()
        {
            Image<Hsv, Byte> img1 = new Image<Hsv, Byte>(@"..\..\data\lena.jpg");
            ImageViewer.Show(img1, "Result Window");
        }
    }
}
