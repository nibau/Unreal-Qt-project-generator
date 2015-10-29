using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace GenerateQTProject
{
    public struct ConfigurationData
    {
        // this information has to be copied over from a .pro.user file once
        public string qtCreatorEnvironmentId;
        public string qtCreatorUnrealConfigurationId;
    }

    public static class Configuration
    {
        public static string CONFIG_FILE_NAME { get; } = "config.ini";
        public static ConfigurationData data;

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

        public static bool writeWizardConfig(ConfigurationData data)
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
