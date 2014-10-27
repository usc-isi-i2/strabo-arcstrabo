using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Strabo.Core.TextDetection;

namespace Strabo.Test
{
    public class TestTextIdentificationChinese
    {
        public static void textIndentification(string dir, string fn)
        {

            //TextDetectionWorker trw = new TextDetectionWorker();
            //trw.Apply(dir, fn, 2.5, false);

            TextDetectionWorkerChinese TestWorker = new TextDetectionWorkerChinese();
            TestWorker.Apply(Input_dir, Output_dir, fn, 2, false);
        }
    }
}
//#Region Back up
//string dir=@"..\..\data\";
//string fn = "text.png";
//TextDetectionWorker trw = new TextDetectionWorker();
//trw.Apply(dir, fn, 2.5, false);
