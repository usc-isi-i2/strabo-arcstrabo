using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Strabo.Core.Utility
{
    public class EnvConfigs
    {
        public static bool Load(string fn)
        {
            // fn = "config.txt";
            // Get current working directory
            // Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "app.config");
            string CurrentBinDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            CurrentBinDir = CurrentBinDir.Replace("file:\\", "");

            if (File.Exists(CurrentBinDir + @"\EnvConfigs\" + fn))
            {
                // Skip reading the config file for now, using implied directory structure
                CurrentState.bin_dir = CurrentBinDir + @"\";
                CurrentState.profile_dir = CurrentState.bin_dir + @"Profiles\";
                CurrentState.output_dir = CurrentState.bin_dir + @"Data\";
                CurrentState.input_dir = CurrentState.output_dir + @"input\";
                CurrentState.debug_dir = CurrentState.output_dir + @"debug\";
                CurrentState.log_dir = CurrentState.output_dir + @"log\";
                CurrentState.temp_dir = CurrentState.output_dir + @"temp\";

                CurrentState.profile_filename = "profile.xml";
                return true;
            }
            else
            {
                Console.WriteLine("Cannot find config file: " + CurrentBinDir + @"\EnvConfigs\" + fn);
                return false;
            }
        }
    }
}
