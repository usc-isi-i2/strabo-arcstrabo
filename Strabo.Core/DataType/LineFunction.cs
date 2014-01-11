using System;
using System.Collections.Generic;
using System.Text;

namespace Strabo.Core.DataType
{
    public class LineFunction
    {
        public double m = 0;
        public double b = 0;
        public double alph = 0;
        public LineFunction() { }
        public LineFunction(double m, double b, double a) { this.m = m; this.b = b; this.alph = a; }
        public double GetOrientation() { return alph; }
    }
}
