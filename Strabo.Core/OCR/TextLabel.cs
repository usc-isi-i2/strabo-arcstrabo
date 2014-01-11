using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Strabo.Core.OCR
{
    public class TextLabel
    {
        public Point mass_center;
        public string text="";
        public int char_count;
        public double orientation;
        public int id;
        public string fn;
        public string susp_text="";
        public int susp_char_count = 0; // from docx
        public int bbxx, bbxy, bbxw, bbxh;
        public double avg_h, avg_w;
        //public List<double> distance_to_road_list = new List<double>();

        public TextLabel(int id, int ch,int x, int y, double orientation,string fn, int bbxx,int bbxy, int bbxw, int bbxh)
        {
            this.id = id;
            this.char_count = ch;
            this.mass_center.X = x;
            this.mass_center.Y = y;
            this.orientation = orientation;
            this.fn=fn;
            this.bbxx = bbxx;
            this.bbxy = bbxy;
            this.bbxw = bbxw;
            this.bbxh = bbxh;
            this.avg_h = Convert.ToDouble(bbxh) / Convert.ToDouble(ch);
            this.avg_w = Convert.ToDouble(bbxw) / Convert.ToDouble(ch);
        }
        public TextLabel() { }
    }
}
