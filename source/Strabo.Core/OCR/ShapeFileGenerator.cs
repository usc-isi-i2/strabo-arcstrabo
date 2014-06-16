using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapTools;

namespace Strabo.Core.OCR
{
    public class ShapeFileGenerator
    {
        public ShapeFileGenerator() { }
        public void createSHP()
        {
            ShapeLib.ShapeType shpType = ShapeLib.ShapeType.Polygon;
            IntPtr hShp;
            // create a new shapefile
            hShp = ShapeLib.SHPCreate(@"C:\Users\san28\Source\Repos\Strabo\data\Results", shpType);
            if (hShp.Equals(IntPtr.Zero))
                return;

            int NVERTICES = 5;
            // create an arbitrary geometric figure
            // note that our boundary is defined clockwise, according
            // to the ESRI shapefile rule that the neighborhood to the right 
            // of an observer walking along the ring in vertex order is
            // the neighborhood inside the polygon.  In contrast, holes are 
            // defined in counterclockwise order.
           
                double[] xCoord = new double[5];
                double[] yCoord = new double[5];
                double bbxx=1240;
                double bbxy=531;
                double bbxw=20;
                double bbxh=213;
                xCoord[0] = bbxx;
                yCoord[0] = -bbxy;

                xCoord[1] = bbxx + bbxw;
                yCoord[1] = -bbxy;

                xCoord[2] = bbxx + bbxw;
                yCoord[2] = -bbxy - bbxh;

                xCoord[3] = bbxx;
                yCoord[3] = -bbxy - bbxh;
                // ensure start and end point are equal (some roundoff err occurs in Sin(2PI))
                xCoord[4] = xCoord[0];
                yCoord[4] = yCoord[0];

                // add three shapes
                IntPtr pshpObj = ShapeLib.SHPCreateSimpleObject(shpType, NVERTICES,
                    xCoord, yCoord, new double[NVERTICES]);

                int iRet = ShapeLib.SHPWriteObject(hShp, -1, pshpObj);
                ShapeLib.SHPDestroyObject(pshpObj);
           

            // we want to test SHPOpen, so we will close hShp then reopen it
            ShapeLib.SHPClose(hShp);

        }
    }
}
