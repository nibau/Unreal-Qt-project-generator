/**
 *  Unreal Qt Project Generator
 *
 *  Configuration.cs
 *
 *  Copyright (c) 2015 N. Baumann
 *
 *  This software is licensed under MIT license
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
 *  and associated documentation files (the "Software"), to deal in the Software without restriction, 
 *  including without limitation the rights to use, copy, modify, merge,
 *  publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons 
 *  to whom the Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 *  OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
 *  LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR 
 *  IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.IO;
using System.Text.RegularExpressions;

namespace GenerateQTProject
{
    public struct ConfigurationData
    {
        // this information has to be copied over from a .pro.user file on this computer once
        public string qtCreatorEnvironmentId;
        public string qtCreatorUnrealConfigurationId;
    }

    public static class Configuration
    {
        public const string CONFIG_FILE_NAME = "config.ini";
        public static ConfigurationData data;

        // regex pattern for Qt environment and configuration ids
        const string ENV_ID_PATTERN = "^\\{[0-9a-f]{8}\\-[0-9a-f]{4}\\-[0-9a-f]{4}\\-[0-9a-f]{4}\\-[0-9a-f]{12}\\}$";

        /// <summary>
        /// Checks if a Qt id has a valid format
        /// </summary>
        /// <param name="id">Qt id (some combined hexadecimal values)</param>
        /// <returns>True if format is valid</returns>
        public static bool IsValidQtId(string id)
        {
            return Regex.Match(id, ENV_ID_PATTERN).Success;
        }

        /// <summary>
        /// Checks if configuration file is present
        /// </summary>
        /// <returns>True if configuration file exists</returns>
        public static bool HasConfigurationFile()
        {
            return File.Exists(FileActions.PROGRAM_DIR + CONFIG_FILE_NAME);
        }

        /// <summary>
        /// Load data from configuration file
        /// Atm the function only loads 2 values (because there are only two settings), but it could easily be extended to detect more settings.
        /// </summary>
        /// <returns>True if valid configuration was read and stored in data field</returns>
        public static bool LoadConfiguration()
        {
            data = new ConfigurationData();

            if (!File.Exists(FileActions.PROGRAM_DIR + CONFIG_FILE_NAME))
            {
                return false;
            }

            string[] config = File.ReadAllLines(FileActions.PROGRAM_DIR + CONFIG_FILE_NAME);

            string line = "";

            for (int i = 0; i < config.Length; ++i)
            {
                line = config[i];

                // ignore comments
                if (line.Contains("\'"))
                    line = line.Remove(line.IndexOf('\''));

                line = line.Trim();

                string[] key_val_pair = line.Split('=');

                // skip line if no key value pair was found
                if (line == "" || key_val_pair.Length != 2)
                    continue;

                key_val_pair[0] = key_val_pair[0].Trim();
                key_val_pair[1] = key_val_pair[1].Trim();

                switch (key_val_pair[0])
                {
                    case "qt_environment_id":
                        data.qtCreatorEnvironmentId = key_val_pair[1];
                        if (!Regex.Match(data.qtCreatorEnvironmentId, ENV_ID_PATTERN).Success)
                        {
                            Console.WriteLine("CONFIG ERROR: Invalid QtCreator environment ID set.");
                            return false;
                        }               
                        break;
                    case "unreal_project_configuration_id":
                        data.qtCreatorUnrealConfigurationId = key_val_pair[1];
                        if (!Regex.Match(data.qtCreatorUnrealConfigurationId, ENV_ID_PATTERN).Success)
                        {
                            Console.WriteLine("CONFIG ERROR: Invalid QtCreator configuration ID set.");
                            return false;
                        }
                        break;
                    default:
                        Console.WriteLine("CONFIG ERROR: Configuration parse error.\n");
                        return false;
                }
            }

            if (data.qtCreatorEnvironmentId == "" || data.qtCreatorUnrealConfigurationId == "")
                return false;

            return true;
        }

        /// <summary>
        /// Writes the initial configuration file
        /// </summary>
        /// <param name="data">Data to write</param>
        /// <returns>True if successful</returns>
        public static bool WriteWizardConfig(ConfigurationData data)
        {
            string file = "";
            file += "[General]\r\n\r\n";
            file += "' These values can be found in a .pro.user file of a project on this computer which uses (exclusively) your Unreal Engine build kit.\r\n";
            file += "qt_environment_id = " + data.qtCreatorEnvironmentId + "\r\n";
            file += "unreal_project_configuration_id = " + data.qtCreatorUnrealConfigurationId + "\r\n\r\n";

            try
            {
                File.WriteAllText(FileActions.PROGRAM_DIR + CONFIG_FILE_NAME, file);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }


}
