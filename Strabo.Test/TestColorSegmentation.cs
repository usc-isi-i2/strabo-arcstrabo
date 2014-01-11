using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Strabo.Core.ColorSegmentation;

namespace Strabo.Test
{
    public class TestColorSegmentation
    {
        public static void test()
        {
            string dir=@"..\..\data\";
            string fn = "lena.jpg";

            ColorSegmentationWorker csw = new ColorSegmentationWorker();
            csw.Apply(dir, fn);
        }
    }
}
