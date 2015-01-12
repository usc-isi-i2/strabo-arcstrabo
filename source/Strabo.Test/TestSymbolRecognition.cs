using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Strabo.Core.SymbolRecognition;

namespace Strabo.Test
{
        public class CoordinatePoint
        {
            public double lat;
            public double lng;
            public string URI;
            public string type;
            public CoordinatePoint()
            {
                lat = lng = 0;
                URI = type = "";
            }
        }
        public class Points
        {
            public double leftTopX, leftTopY;
            public double rightTopX, rightTopY;
            public double leftDownX, leftDownY;
            public double rightDownX, rightDownY;

            public string URI;
            public Points()
            {
                leftDownY = leftDownX = leftTopX = leftTopY = rightDownX = rightDownY =
                    rightTopX = rightTopY = 0;
                URI = "";
            }
        }
    class TestSymbolRecognition
    {
        public void test()
        {
            List<CoordinatePoint> pointURIs = new List<CoordinatePoint>();

            List<DBpedia.DbpediaInfo> dbpediaResults = DBpedia.getDbpediaInfo(44.346, 33.356, 44.464, 33.284);
            DBpedia.DbpediaInfo answer = new DBpedia.DbpediaInfo();

            int x = 0;
            
            //foreach (CoordinatePoint item in coordinatePoints)
            //{
            //    answer = findMinDistance(item, dbpediaResults);
            //    CoordinatePoint pnt = new CoordinatePoint();
            //    pnt.lat = item.lat;
            //    pnt.lng = item.lng;
            //    item.URI = pnt.URI = answer.uri;
            //    item.type = pnt.type = answer.type;
            //    dbpediaResults.Remove(answer);
            //    pointURIs.Add(pnt);
            //}
   


        }
    }
}
