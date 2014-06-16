using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Strabo.Core.OCR
{
    public class ReadOCRResults
    {
        static void Read(string dir)
        {
            DirectoryInfo dirinfo = new DirectoryInfo(dir);

            foreach (FileInfo NextFile in dirinfo.GetFiles()) // 
            {
                if (NextFile.Extension == ".txt")
                {
                    string fn = Path.GetFileNameWithoutExtension(NextFile.Name);
                    string[] token = fn.Split('_');
                    int num = Convert.ToInt16(token[0]);
                    double x = Convert.ToDouble(token[3]);
                    double y = Convert.ToDouble(token[4]);
                    List<Double> slope = new List<Double>();
                    for (int i = 6; i < token.Length; i++)
                        slope.Add(Convert.ToDouble(token[i]));
                }
            }

        }
    }
}
