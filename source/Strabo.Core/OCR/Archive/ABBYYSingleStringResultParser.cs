using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Strabo.Core.OCR
{
    public class ABBYYSingleStringResultParser
    {
        public List<TextLabel> textlabel_list = new List<TextLabel>();

        public ABBYYSingleStringResultParser() { }
        public void ReadResult(string dir, string fn)
        {
            double avg_h = 0;
            double avg_w = 0;
            int char_count = 0;
            StreamReader sr = new StreamReader(dir + fn);
            string line = sr.ReadLine();
            while (line != null)
            {
                //char[] ch = ';';
                string[] token = line.Split(';');
                //135; 974; 354; 33; John Av;
                
                TextLabel textlabel = new TextLabel();
                textlabel.id =  Convert.ToInt16(token[0]);
                textlabel.mass_center = new System.Drawing.Point(Convert.ToInt16(token[1]), Convert.ToInt16(token[2]));
                textlabel.orientation = Convert.ToDouble(token[3]);
                textlabel.text = token[4].Trim();
                //if(textlabel.text.Length <=3) 

                textlabel.susp_text = token[5];
                textlabel.bbxx = Convert.ToInt16(token[6]); 
                textlabel.bbxy = Convert.ToInt16(token[7]);
                textlabel.bbxw = Convert.ToInt16(token[8]);
                textlabel.bbxh = Convert.ToInt16(token[9]);
                //textlabel.mass_center = new System.Drawing.Point(Convert.ToInt16(textlabel.bbxx + textlabel.bbxw / 2), Convert.ToInt16(textlabel.bbxy + textlabel.bbxh / 2));
                textlabel.avg_h = Convert.ToDouble(textlabel.bbxh) / Convert.ToDouble(textlabel.text.Length);
                textlabel.avg_w = Convert.ToDouble(textlabel.bbxw) / Convert.ToDouble(textlabel.text.Length);
                textlabel_list.Add(textlabel);
                line = sr.ReadLine();
                char_count += textlabel.text.Length;
                avg_h += textlabel.bbxh;
                avg_w += textlabel.bbxw;
            }
            avg_h /= char_count;
            avg_w /= char_count;
            Console.WriteLine("Size:" +avg_h + ";" + avg_w +";"+ Convert.ToString((avg_h + avg_w /2)));
        }
        public void ReadOCRResults(string dir)
        {
            DirectoryInfo dirinfo = new DirectoryInfo(dir);

            foreach (FileInfo NextFile in dirinfo.GetFiles()) // 
            {
                if (NextFile.Extension == ".docx"
                    && !NextFile.Name.Contains('~'))
                {
                    string fn = Path.GetFileNameWithoutExtension(NextFile.Name);
                    string[] token = fn.Split('_');

                    int num = Convert.ToInt16(token[0]);
                    int ch = Convert.ToInt16(token[2]);
                    int x = Convert.ToInt16(token[3]);
                    int y = Convert.ToInt16(token[4]);

                    double slope = Convert.ToInt16(token[6]);
                    int bbxx = Convert.ToInt16(token[7]);
                    int bbxy = Convert.ToInt16(token[8]);
                    int bbxw = Convert.ToInt16(token[9]);
                    int bbxh = Convert.ToInt16(token[10]);
                    
                    textlabel_list.Add(new TextLabel(num,ch,x,y,slope,fn, bbxx,bbxy,bbxw,bbxh));
                }
            }
            for (int i = 0; i < textlabel_list.Count; i++)
            {
                TextLabel textlabel = textlabel_list[i];
                DocxToText dtt = new DocxToText(dir + textlabel.fn + ".docx");
                textlabel.text = dtt.ExtractText();
                textlabel.susp_char_count = dtt.susp_char_count; 
                //if (textlabel.text == "" || textlabel.text == null ||suscharratio(textlabel))// ||  line_counter > 1)
                //{
                //    textlabel_list.RemoveAt(i);
               //     i--;
                //}
            }
            ResultMerger();
        }
        private string ReadWord(string filePath)
        {
            DocxToText dtt = new DocxToText(filePath);
            return dtt.ExtractText();
        }
        private void ResultMerger()
        {
            List<TextLabel> merged_textlabel_list = new List<TextLabel>();
            for (int i = 0; i < textlabel_list.Count; i++)
            {
                TextLabel textlabel = textlabel_list[i];
                List<TextLabel> subtextlabel_list = new List<TextLabel>();
                //subtextlabel_list.Add(textlabel);
                for (int j = i+1; j < textlabel_list.Count; j++)
                {
                    TextLabel textlabel2 = textlabel_list[j];
                    if (textlabel.id == textlabel2.id)
                    {
                        subtextlabel_list.Add(textlabel2);
                        textlabel_list.RemoveAt(j);
                        j--;
                    }
                }
                int min_susp_count = Int16.MaxValue;
                if (subtextlabel_list.Count > 0)
                    subtextlabel_list.Add(textlabel);
                for (int j = 0; j < subtextlabel_list.Count; j++)
                {
                    if (subtextlabel_list[j].susp_char_count < min_susp_count)
                    {
                        min_susp_count = subtextlabel_list[j].susp_char_count;
                        textlabel = subtextlabel_list[j];
                        textlabel.susp_char_count = min_susp_count;
                    }
                }
                merged_textlabel_list.Add(textlabel);
            }
            textlabel_list.Clear();
            textlabel_list.AddRange(merged_textlabel_list);
            
        }
        private bool alphnumericratio(string temp)
        {
            string clean = Regex.Replace(temp, @"[^a-zA-Z0-9]", "");
            if ((double)clean.Length / (double)temp.Length <= 0.5)
                return false;
            else
                return true;
        }
        private bool suscharratio(TextLabel textlabel)
        {
            if (textlabel.susp_char_count == textlabel.text.Length)
                return true;
            else
                return false;
        }
    }
}
