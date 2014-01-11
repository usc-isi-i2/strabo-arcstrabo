using System;
using MapTools;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;
using Strabo.Core.OCR;

namespace Strabo.Utility
{
    public class ShapefileUtils
    {
        public ShapefileUtils() { }
        public void WrtieText2Shp(ABBYYSingleStringResultParser assrp, string dir, string fn)// (ABBYYSingleStringResultParser assrp,string dir, string fn)
        { 
            CreateSHP(assrp,dir,fn);
            CreateDBF(assrp, dir, fn);
        }
        private void CreateSHP(ABBYYSingleStringResultParser assrp, string dir, string fn)
        {
            ShapeLib.ShapeType shpType = ShapeLib.ShapeType.Polygon;
            //Console.WriteLine("*****Creating {0}*****\n", ShapeLib.SHPTypeName(shpType));
            IntPtr hShp;
            // create a new shapefile
            hShp = ShapeLib.SHPCreate(dir + fn+"1", shpType);
            if (hShp.Equals(IntPtr.Zero))
                return;

            int NVERTICES = 5;
            // create an arbitrary geometric figure
            // note that our boundary is defined clockwise, according
            // to the ESRI shapefile rule that the neighborhood to the right 
            // of an observer walking along the ring in vertex order is
            // the neighborhood inside the polygon.  In contrast, holes are 
            // defined in counterclockwise order.
            for (int t = 0; t < assrp.textlabel_list.Count; t++)
            {
                double[] xCoord = new double[5];
                double[] yCoord = new double[5];
                xCoord[0] = assrp.textlabel_list[t].bbxx;
                yCoord[0] = -assrp.textlabel_list[t].bbxy;

                xCoord[1] = assrp.textlabel_list[t].bbxx + assrp.textlabel_list[t].bbxw;
                yCoord[1] = -assrp.textlabel_list[t].bbxy;

                xCoord[2] = assrp.textlabel_list[t].bbxx + assrp.textlabel_list[t].bbxw;
                yCoord[2] = -assrp.textlabel_list[t].bbxy - assrp.textlabel_list[t].bbxh;

                xCoord[3] = assrp.textlabel_list[t].bbxx;
                yCoord[3] = -assrp.textlabel_list[t].bbxy - assrp.textlabel_list[t].bbxh;
                // ensure start and end point are equal (some roundoff err occurs in Sin(2PI))
                xCoord[4] = xCoord[0];
                yCoord[4] = yCoord[0];

                // add three shapes
                IntPtr pshpObj = ShapeLib.SHPCreateSimpleObject(shpType, NVERTICES,
                    xCoord, yCoord, new double[NVERTICES]);

                int iRet = ShapeLib.SHPWriteObject(hShp, -1, pshpObj);
                ShapeLib.SHPDestroyObject(pshpObj);
            }
            
            // we want to test SHPOpen, so we will close hShp then reopen it
            ShapeLib.SHPClose(hShp);
        }

        private void CreateDBF(ABBYYSingleStringResultParser assrp, string dir, string fn)
        {
            //Console.WriteLine("\n*****Creating dbf*****\n");
            // create dbase file
            IntPtr hDbf = ShapeLib.DBFCreate(dir+fn+"1");
            if (hDbf.Equals(IntPtr.Zero))
            {
                //Console.WriteLine("Error:  Unable to create {0}.dbf!", dir+fn);
                return;
            }

            // add some fields 
            int iRet = ShapeLib.DBFAddField(hDbf, "ID", ShapeLib.DBFFieldType.FTInteger, 4, 0);
            iRet = ShapeLib.DBFAddField(hDbf, "TEXT", ShapeLib.DBFFieldType.FTString, 50, 0);

            for (int t = 0; t < assrp.textlabel_list.Count; t++)
            {
                int iField = 0;
                iRet = (ShapeLib.DBFWriteIntegerAttribute(hDbf, t, iField++, assrp.textlabel_list[t].id));
                iRet = (ShapeLib.DBFWriteStringAttribute(hDbf, t, iField++, assrp.textlabel_list[t].text));
            }

            // close the file handle then reopen (only so we can test DBFOpen)
            ShapeLib.DBFClose(hDbf);
        }
    }
}
