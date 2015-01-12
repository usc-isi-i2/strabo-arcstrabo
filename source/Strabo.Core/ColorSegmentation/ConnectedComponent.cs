/*******************************************************************************
 * Copyright 2010 University of Southern California
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * 	http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * This code was developed as part of the Strabo map processing project 
 * by the Spatial Sciences Institute and by the Information Integration Group 
 * at the Information Sciences Institute of the University of Southern 
 * California. For more information, publications, and related projects, 
 * please see: http://yoyoi.info and http://www.isi.edu/integration
 ******************************************************************************/
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;

namespace Strabo.Core.ColorSegmentation
{
    class ConnectedComponent
    {
        public int apply(string fileName, string output)
        {
            int counter = 0;

            Emgu.CV.Image<Bgr, Byte> imgS = new Emgu.CV.Image<Bgr, Byte>(fileName);

            Emgu.CV.Image<Gray, Byte> img = new Emgu.CV.Image<Gray, Byte>(fileName);

            //Emgu.CV.Image<Gray, Byte> imgGray = new Image<Gray, byte>(img.Width, img.Height);
            //CvInvoke.cvCvtColor(img, imgGray, COLOR_CONVERSION.BGR2GRAY);

            int thresh = 1;
            int max_thresh = 255;
            img = img.ThresholdBinary(new Gray(thresh), new Gray(max_thresh));

            img.Save(output.Replace(".", "_binary."));


            Contour<Point> contur = img.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_CCOMP);
            Emgu.CV.CvInvoke.cvDrawContours(imgS, contur, new MCvScalar(0, 0, 255), new MCvScalar(0, 0, 255), 1, 1, LINE_TYPE.EIGHT_CONNECTED, new Point(0, 0));


            contur = img.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_CCOMP);


            while (contur != null && contur.HNext != null)
            {
                if (counter == 0) { counter++; }

                contur = contur.HNext;
                counter++;
            }


            MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_SIMPLEX, 0.8f, 0.8f);
            MCvScalar color = new MCvScalar(255, 255, 255);

            CvInvoke.cvPutText(imgS, "counter:" + counter, new Point(10, 20), ref font, color);

            imgS.Save(output);



            return counter;

        }
    }
}
