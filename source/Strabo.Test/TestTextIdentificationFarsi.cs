using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Strabo.Core.TextDetection;



namespace Strabo.Test
{
    class TestTextIdentificationFarsi
    {

        public static void textIndentification(string Input_dir,string Output_dir , string fn)
        {

            //TextDetectionWorker trw = new TextDetectionWorker();
            //trw.Apply(dir, fn, 2.5, false);

            TextDetectionWorkerFarsi TestWorker = new TextDetectionWorkerFarsi();
            TestWorker.Apply(Input_dir , Output_dir , fn , 2 , false);


        }


    }
}
