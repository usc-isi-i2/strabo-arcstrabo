using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ArcStrabo
{
    public partial class ProgressForm : Form
    {
        public ProgressForm()
        {
            InitializeComponent();
            
        }
        public void setProgress(int min, int max)
        {
            progressBar1.Minimum = min;
            progressBar1.Maximum = max;
            progressBar1.Step = 1;
        }
        public void stepProgress()
        {
            progressBar1.PerformStep();
        }
    }
}
