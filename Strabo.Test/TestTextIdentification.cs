using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Strabo.Core.TextDetection;

namespace Strabo.Test
{
    public class TestTextIdentification
    {
        public static void textIndentification(string dir, string fn)
        {
           
            TextDetectionWorker trw = new TextDetectionWorker();
            trw.Apply(dir, fn, 2.5, false);
        }
    }
}
//#Region Back up
//string dir=@"..\..\data\";
//string fn = "text.png";
//TextDetectionWorker trw = new TextDetectionWorker();
//trw.Apply(dir, fn, 2.5, false);
