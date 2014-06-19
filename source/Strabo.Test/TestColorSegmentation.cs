using Strabo.Core.ColorSegmentation;
using System;

namespace Strabo.Test
{
    public class TestColorSegmentation
    {
        public static void test(String directory)
        {
            ColorSegmentationWorker cs = new ColorSegmentationWorker();
            cs.Apply(@"C:\Users\yaoyichi\Dropbox\Server\NGATextRecognitionRelease\Data\Examples\Indonesian_from Becker_300dpi jpg_400dpi tif\", @"C:\Users\yaoyichi\Dropbox\Server\NGATextRecognitionRelease\Data\Examples\Indonesian_from Becker_300dpi jpg_400dpi tif\","g1148879n845337x1517264_crop.png");
        }
    }
}