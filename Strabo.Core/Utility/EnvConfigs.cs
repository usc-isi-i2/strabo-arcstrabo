/*******************************************************************************
 * Copyright 2010 University of Southern California
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * 	http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * This code was developed as part of the Strabo map processing project 
 * by the Spatial Sciences Institute and by the Information Integration Group 
 * at the Information Sciences Institute of the University of Southern 
 * California. For more information, publications, and related projects, 
 * please see: http://yoyoi.info and http://www.isi.edu/integration
 ******************************************************************************/
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
