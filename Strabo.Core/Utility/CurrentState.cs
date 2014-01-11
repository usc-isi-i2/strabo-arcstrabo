using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Strabo.Core.DataType;

namespace Strabo.Core.Utility
{
    public class CurrentState
    {
        public static string map_fn = "map.png";
        public static string text_layer_fn = "text.png";
        public static string text_extraction_results_fn = "text_extraction_results.png";
        public static string text_recognition_results_fn = "text_recognition_results.png";
        //public static MPProfiles MPProfile;// = new MPProfiles();
        //public static RoadIntersection[] ints;
        public static List<int> road_vectors;//=new List<int>();
        public static int mw, mh;
        public static string process_id;
        public static int process_counter;
        public static Bitmap map;
        public static Bitmap intermediateBitmap;
        public static Hashtable bitmaps;// = new Hashtable();
        public static string input_fn;
        public static string output_fn_without_ext;
        public static double scalar=1;
        public static int margin = 20;
        public static string bin_dir;
        public static string profile_dir;
        public static string input_dir;
        public static string output_dir;
        public static string profile_filename;
        public static string debug_dir;
        public static string log_dir;
        public static string temp_dir;
        public static string tgsp_exe_filename = "text.exe";
        public static string imagej_home;
        public static string imagej_macro;
        public static int java_max_mem=512;
        // supervised method 
        public static int SPV_UL_BOX_SIZE = 20;
        public static int SPV_UL_BOX_W = 1;//40;
        public static int SPV_UL_BOX_H  =1;//20;
        public static int SPV_UL_BOX_R = 0;
        public static int SPV_UL_NUM_Char = 0;
        public static int x_offset=0;
        public static int y_offset = 0;
        public static bool use_preprocessed_ppt = false;
        public static int preprocessed_road_width;
        //public static RoadFormat preprocessed_road_format;
        public static List<int> road_width_list = new List<int>();
        //public static List<RoadFormat> road_format_list = new List<RoadFormat>();
        public CurrentState(){}
        public static void Clear()
        {
            //MPProfile = null;// = new MPProfiles();
            //ints = null;
            road_vectors = null;//=new List<int>();
            process_counter = 0;
            map = null;
            intermediateBitmap = null;
            bitmaps = null;
            GC.Collect();
        }
        public static void Clear4NewMap()
        {
            road_width_list.Clear();
            //road_format_list.Clear();
        }
        public static void LoadEnvConfig(string config_fn)
        {
            EnvConfigs.Load(config_fn);
        }
        public static void LoadMPProfile(string mapsource)
        {
            
            //MPProfile = new MPProfiles();
            //MPProfile.Load(mapsource);
            road_vectors = new List<int>();
            bitmaps= new Hashtable();
        }
        
        public static bool inROI(int x, int y)
        {
            if (x > margin && x < mw - margin && y > margin && y < mh - margin)
                return true;
            else
                return false;
        }
        public static void deleteState(string process_id)
        {
            if (bitmaps.ContainsKey(process_id))
                bitmaps.Remove(process_id);
        }
        public static Bitmap getState(string process_id)
        {
            return (Bitmap)((Bitmap)bitmaps[process_id]).Clone();
        }
        public static void saveState(string process_id, bool writelog)
        {
            process_counter++;
            if (bitmaps.ContainsKey(process_id))
                bitmaps.Remove(process_id);
            bitmaps.Add(process_id, (Bitmap)(intermediateBitmap.Clone()));
            //if(writelog) Log.WriteBitmap2Debug(intermediateBitmap, output_fn_without_ext + process_counter + process_id+".png");
            process_counter++;
        }
        public static void saveState(string process_id, Bitmap image, bool writelog)
        {
            process_counter++;
            intermediateBitmap = (Bitmap)image.Clone();
            if (bitmaps.ContainsKey(process_id))
                bitmaps.Remove(process_id);
            bitmaps.Add(process_id, (Bitmap)(intermediateBitmap.Clone()));
            //if (writelog) Log.WriteBitmap2Debug(intermediateBitmap, output_fn_without_ext + process_counter + process_id + ".png");
        }
        public static void saveStateWithoutBitmapHash(string process_id, Bitmap image, bool writelog)
        {
            process_counter++;
            intermediateBitmap = (Bitmap)image.Clone();
            //if (writelog) Log.WriteBitmap2Debug(intermediateBitmap, output_fn_without_ext + process_counter + process_id + ".png");
        }
        public static void saveStateWithoutBitmapHash(string process_id, bool writelog)
        {
            process_counter++;
            //if (writelog) Log.WriteBitmap2Debug(intermediateBitmap, output_fn_without_ext + process_counter + process_id + ".png");
        }
        public static void DeleteTMPFiles()
        {
            DirectoryInfo dir = new DirectoryInfo(bin_dir);
            foreach (FileInfo NextFile in dir.GetFiles()) // 
            {
                if (NextFile.Extension == ".png" || NextFile.Extension == ".tif")
                    NextFile.Delete();
            }
            if (File.Exists(imagej_macro + "thinning.bat")) File.Delete(imagej_macro + "thinning.bat");
            if (File.Exists(imagej_macro + "input.png")) File.Delete(imagej_macro + "input.png");
            if (File.Exists(imagej_macro + "thinning.png")) File.Delete(imagej_macro + "thinning.png");
        }
    }
}
