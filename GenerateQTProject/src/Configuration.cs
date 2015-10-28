using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace GenerateQTProject
{
    public struct ConfigurationData
    {
        // path can either be launcher path (folder with 4.x folders) or a concrete Unreal Engine path
        public string defaultEnginePath;

        // true if launcher path (contains 4.x folders), false if normal engine folder
        public bool isLauncherPath;

        // this information has to be copied over from a .pro.user file once
        public string qtCreatorEnvironmentId;
        public string qtCreatorUnrealConfigurationId;

        public Dictionary<string, string> customEngines;
    }

    public static class Configuration
    {
        public static string CONFIG_FILE_NAME { get; } = "UnrealProjectGenerator.ini";
        public static ConfigurationData data;

        const string ENV_ID_PATTERN = "^\\{[0-9a-f]{8}\\-[0-9a-f]{4}\\-[0-9a-f]{4}\\-[0-9a-f]{4}\\-[0-9a-f]{12}\\}$";

        public static bool IsValidQtId(string id)
        {
            return Regex.Match(id, ENV_ID_PATTERN).Success;
        }

        public static bool HasConfigurationFile()
        {
            return File.Exists(FileActions.PROGRAM_DIR + CONFIG_FILE_NAME);
        }

        public static bool IsValidCustomCommand(string command)
        {
            return data.customEngines != null && data.customEngines.ContainsKey(command);
        }

        public static bool LoadConfiguration()
        {
            data = new ConfigurationData();
            data.customEngines = new Dictionary<string, string>();

            if (!File.Exists(FileActions.PROGRAM_DIR + CONFIG_FILE_NAME))
            {
                return false;
            }

            string[] config = File.ReadAllLines(FileActions.PROGRAM_DIR + CONFIG_FILE_NAME);

            string line = "";
            Boolean customSection = false;

            for (int i = 0; i < config.Length; ++i)
            {
                line = config[i];

                // ignore comments
                line = line.Remove(line.IndexOf('\'')).Trim();

                if (line.StartsWith("[CustomEngineProfiles]"))
                    customSection = true;

                string[] key_val_pair = line.Split('=');

                // skip line if no key value pair was found
                if (line == "" || key_val_pair.Length != 2)
                    continue;

                key_val_pair[0] = key_val_pair[0].Trim();
                key_val_pair[1] = key_val_pair[1].Trim();

                switch (key_val_pair[0])
                {
                    case "default_engine_path":
                        data.defaultEnginePath = key_val_pair[1].Replace("\"", "");
                        if (!data.defaultEnginePath.EndsWith("\\"))
                            data.defaultEnginePath += "\\";         
                        break;
                    case "launcher_path":
                        try
                        {
                            data.isLauncherPath = Convert.ToBoolean(key_val_pair[1].ToLower());
                        }
                        catch
                        {
                            Console.WriteLine("CONFIG ERROR: Invalid launcher_path value in configuration file. (has to be TRUE or FALSE)");
                            return false;
                        }
                        break;
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
                        if (customSection)
                            data.customEngines.Add(key_val_pair[0], key_val_pair[1]);
                        else
                        {
                            Console.WriteLine("CONFIG ERROR: Configuration parse error.\n");
                            return false;
                        }            
                        break;
                }
            }


            // check if engine path is valid
            bool valid = false;
            if (data.isLauncherPath)
            {           
                foreach (string dir in Directory.GetDirectories(data.defaultEnginePath))
                {
                    if (dir.StartsWith("4."))
                    {
                        valid = true;
                        break;
                    }
                } 
            }
            else
            {
                if (Directory.Exists(data.defaultEnginePath + "Engine"))
                    valid = true;
            }

            if (!valid)
            {
                Console.WriteLine("CONFIG ERROR: Invalid default_engine_path set.");
                if (data.isLauncherPath)
                    Console.WriteLine("\tNo 4.x folders found at the provided location.");
                else
                    Console.WriteLine("\tEngine folder not found.");

                return false;
            }

            if (data.defaultEnginePath == "" || data.qtCreatorEnvironmentId == "" || data.qtCreatorUnrealConfigurationId == "")
                return false;

            return true;
        }

        public static bool writeWizardConfig(ConfigurationData data)
        {
            string file = "";
            file += "' This sets the settings for the default Unreal Engine installation (either launcher or git version)\n";
            file += "[Default]\n\n";
            file += "' If launcher_path = TRUE, default_engine_path should point to the launcher path with the 4.x folders in it\n";
            file += "' If launcher_path = FALSE (useful if you want to use a custom engine build), default_engine_path should point\n";
            file += "' to the base directory of the Unreal Engine installation (contains Engine, FeaturePacks, Samples, Templates, ... folders)\n";
            file += "launcher_path = " + data.isLauncherPath.ToString() + "\n";
            file += "default_engine_path = " + data.defaultEnginePath + "\n\n";
            file += "' These values can be found in a .pro.user file of a project on this computer which uses (exclusively) your Unreal Engine build kit.\n";
            file += "qt_environment_id = " + data.qtCreatorEnvironmentId + "\n";
            file += "unreal_project_configuration_id = " + data.qtCreatorUnrealConfigurationId + "\n\n";
            file += "' Use this section if you want to use additional git versions (without launcher) of Unreal Engine 4 for some projects.\n";
            file += "' Each entry consists of a command name and a path\n";
            file += "' The name has to be entered as argument when you run the project generator in a project folder\n\n";
            file += "[CustomEngineProfiles]\n\n";
            file += "' example:\n";
            file += "'\tcustomEngineBuild = C:\\UnrealEngine\\myBuild\n";
            file += "' ->can be used with command UnrealProjectGenerator customEngineBuild";

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
