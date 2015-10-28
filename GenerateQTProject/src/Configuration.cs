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

        const string env_id_pattern = "^\\{[0-9a-f]{8}\\-[0-9a-f]{4}\\-[0-9a-f]{4}\\-[0-9a-f]{4}\\-[0-9a-f]{12}\\}$";

        public static void StoreDefaultUnrealPath(string path)
        {

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
                        catch (Exception e)
                        {
                            Console.WriteLine("CONFIG ERROR: Invalid launcher_path value in configuration file. (has to be TRUE or FALSE)");
                            return false;
                        }
                        break;
                    case "qt_environment_id":
                        data.qtCreatorEnvironmentId = key_val_pair[1];
                        if (!Regex.Match(data.qtCreatorEnvironmentId, env_id_pattern).Success)
                        {
                            Console.WriteLine("CONFIG ERROR: Invalid QtCreator environment ID set.");
                            return false;
                        }               
                        break;
                    case "unreal_project_configuration_id":
                        data.qtCreatorUnrealConfigurationId = key_val_pair[1];
                        if (!Regex.Match(data.qtCreatorUnrealConfigurationId, env_id_pattern).Success)
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

        public static string defaultConfigurationFile {get;} = "'For complete automation of the project file generation, engine path and QtCreator environment hash have to be filled in below manually for a single time\r\n\r\n' this sets the settings for the default Unreal Engine installation (either launcher or git version)\r\n[Default]\r\n'if launcher_path = TRUE, engine_path should point to the launcher path with the 4.x folders in it\r\n'if launcher_path = FALSE (useful if you want to use a custom engine build), engine_path should point to the base directory of the Unreal Engine installation\r\n' (contains Engine, FeaturePacks, Samples, Templates, ... folders)\r\nlauncher_path=TRUE\r\nengine_path=/path/to/unreal/installation\r\n\r\n'This hash can be found\r\nqt_environment_id={945faeb2-deb5-4c66-9adb-ee13b5882df2}\r\nunreal_project_configuration_name=Unreal Engine 4\r\nunreal_project_configuration_id={567d7386-8d59-448f-befc-8a074fa8675d}\r\n\r\n' Use this section if you want to use additional git versions (without launcher) of Unreal Engine 4 for some projects.\r\n' Each entry consists of a path and a name\r\n' The name has to be entered as argument when you run the project generator in a project folder\r\n[CustomEngineProfiles]\r\n'NAME=Path/to/git/engine (\r\n\r\n'eg.\r\n'customEngineBuild=C:/UnrealEngine/my build\r\n' -> can be used with command UnrealProjectGenerator customEngineBuild";
    }
}
