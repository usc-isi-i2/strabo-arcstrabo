using Strabo.Core.ColorSegmentation;
using System;

namespace Strabo.Test
{
    public class TestColorSegmentation
    {
        public static void test(String directory)
        {
            ColorSegmentationWorker cs = new ColorSegmentationWorker();
            cs.Apply(@"C:\Users\yaoyichi\Desktop\Images-22-10\", @"C:\Users\yaoyichi\Desktop\Images-22-10\", "Yao-Yi.png");
        }
    }
}