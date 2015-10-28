/**
 *  Unreal Qt Project Generator
 *
 *  Generator.cs
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
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;

namespace GenerateQTProject
{
    /// <summary>
    /// Allows generation of .pro and defines\includes.pri file as well as injection of build/launch targets in proj.pro.user file
    /// </summary>
    class Generator
    {
        private const string ENTER_QUIT_MSG = " - Press Enter to quit...";

        /// <summary>
        /// Generates the Qt project file
        /// </summary>
        /// <param name="projectDir">Directory which contains sln and uproject file</param>
        /// <param name="projectName">Name of your UE project</param>
        /// <returns>success</returns>
        public static bool GenerateProFile(string projectDir, string projectName)
        {
            /* These lists will store all source and header files 
            which are found in the source directory of your project */

            List<string> SourceFilePaths;
            List<string> HeaderFilePaths;

            if (projectName == "")
            {
                Console.WriteLine("\nERROR: No .sln file found.\n");
                return false;
            }
            else if (!File.Exists(projectDir + "Intermediate\\ProjectFiles\\" + projectName + ".vcxproj"))
            {
                Console.WriteLine("\nERROR: .vcxproj file not found, name doesn't match .sln file name.\n");
                return false;
            }
            else if (!File.Exists(projectDir + projectName + ".uproject"))
            {
                Console.WriteLine("\nERROR: .uproject file not found, name doesn't match .sln file name.\n");
                return false;
            }
            else
            {
                Console.Clear();
                ConsoleActions.PrintHeader();

                Console.WriteLine("Generating .pro file...");
                SourceFilePaths = new List<string>();
                HeaderFilePaths = new List<string>();

                FileActions.ScanDirectoryForFiles(SourceFilePaths, HeaderFilePaths, projectDir + "Source\\" + projectName, projectName);
            }


            // Add some useful configuration options and include all UE defines
            string qtProFile = "TEMPLATE = app\n" +
            "CONFIG += console\n" +
            "CONFIG -= app_bundle\n" +
            "CONFIG -= qt\n" +
            "CONFIG += c++11\n" +
            " \n" +
            "# All the defines of your project will go in this file\n" +
            "# You can put this file on your repository, but you will need to remake it once you upgrade the engine.\n" +
            "include(defines.pri)\n" +
            " \n" +
            "# Qt Creator will automatically add headers and source files if you add them via Qt Creator.\n" +
            "HEADERS += ";

            // Add all found header files
            foreach (string headerFile in HeaderFilePaths)
            {
                qtProFile += headerFile + " \\\n\t";
            }

            qtProFile += "\nSOURCES += ";

            // Add all found source files
            foreach (string sourceFile in SourceFilePaths)
            {
                qtProFile += sourceFile + " \\\n\t";
            }

            // Add UE includes
            qtProFile = qtProFile + "\n# All your generated includes will go in this file\n" +
            "# You can not put this on the repository as this contains hard coded paths\n" +
            "# and is dependent on your windows install and engine version\n" +
            "include(includes.pri)";

            // Add build.cs as additional file
            if (File.Exists(projectDir + "Source\\" + projectName + "\\" + projectName + ".Build.cs"))
            {
                qtProFile = qtProFile + "\n\n" +
                "DISTFILES += \\\n\t" +
                "../../Source/" + projectName + "/" + projectName + ".Build.cs";
            }

            try
            {
                File.WriteAllText(projectDir + "Intermediate\\ProjectFiles\\" + projectName + ".pro", qtProFile);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nERROR: couldn't write project file.\n");
                Console.WriteLine(ex.StackTrace);
                Console.Write(ENTER_QUIT_MSG);
                Console.ReadLine();
                Environment.Exit(4);
                return false;
            }
        }

        /// <summary>
        /// Generates defines.pri and includes.pri files (with data from YourProject.vcxproj)
        /// </summary>
        /// <param name="projectDir">Directory which contains sln and uproject file</param>
        /// <param name="projectName">Name of your UE project</param>
        /// <returns>success</returns>
        public static bool GenerateDefinesAndInclude(string projectDir, string projectName)
        {
            string vcxText = "";
            try
            {
                vcxText = File.ReadAllText(projectDir + "Intermediate\\ProjectFiles\\" + projectName + ".vcxproj");
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nERROR: vcxproj file couldn't be read.\n");
                Console.WriteLine(ex.StackTrace);
                Console.Write(ENTER_QUIT_MSG);
                Console.ReadLine();
                Environment.Exit(5);
                return false;
            }

            // Read defines from vcxproj file
            string definesString = vcxText;
            definesString = definesString.Substring(definesString.IndexOf("<NMakePreprocessorDefinitions>$(NMakePreprocessorDefinitions);") + "<NMakePreprocessorDefinitions>$(NMakePreprocessorDefinitions);".Length);
            definesString = definesString.Remove(definesString.LastIndexOf("</NMakePreprocessorDefinitions>"));

            // Read includes from vcxproj file
            string includesString = vcxText;
            includesString = includesString.Substring(includesString.IndexOf("<NMakeIncludeSearchPath>$(NMakeIncludeSearchPath);") + "<NMakeIncludeSearchPath>$(NMakeIncludeSearchPath);".Length);
            includesString = includesString.Remove(includesString.LastIndexOf("</NMakeIncludeSearchPath>"));

            // Convert defines to Qt format
            definesString = "DEFINES += \"" + definesString;
            definesString = definesString.Replace(";", "\"\nDEFINES += \"");
            definesString = definesString + "\"";

            // Convert includes to Qt format
            includesString = "INCLUDEPATH += \"" + includesString;
            includesString = includesString.Replace(";", "\"\nINCLUDEPATH += \"");
            includesString = includesString + "\"";

            // Write files
            try
            {
                File.WriteAllText(projectDir + "Intermediate\\ProjectFiles\\defines.pri", definesString);
                File.WriteAllText(projectDir + "Intermediate\\ProjectFiles\\includes.pri", includesString);
            } catch(Exception ex)
            {
                Console.WriteLine("\nERROR: Couldn't write defines and include files.\n");
                Console.WriteLine(ex.StackTrace);
                Console.Write(ENTER_QUIT_MSG);
                Console.ReadLine();
                Environment.Exit(6);
                return false;
            }
                
            return true;
        }


        /// <summary>
        /// This method will generate and apply a modified qt.pro.user file, which contains build presets for UE4
        /// </summary>
        /// <param name="projectDir">Directory which contains sln and uproject file</param>
        /// <param name="projectName">Name of your UE project</param>
        /// <returns>success</returns>
        public static bool GenerateQtBuildPreset(string projectDir, string projectName, string customCommand)
        {
            // Helper variable which stores the retrieved Unreal Engine Version
            string UnrealVersion;

            // These variables are used to replace parts of the qtBuildPreset.xml file to match your project and Unreal Engine installation
            string UPROJ_FILE,
            UNREAL_PATH,
            PROJECT_DIR,
            PROJECT_NAME,
            QT_ENV_ID,
            QT_CONF_ID;

            QT_ENV_ID = Configuration.data.qtCreatorEnvironmentId;
            QT_CONF_ID = Configuration.data.qtCreatorUnrealConfigurationId;

            // Set project name
            PROJECT_NAME = projectName;

            // Set project directory
            PROJECT_DIR = projectDir;

            // Set project file path
            UPROJ_FILE = projectDir + projectName + ".uproject";
            if (!File.Exists(UPROJ_FILE))
            {
                Console.WriteLine("\nERROR: .uproject file not found.\n");
                return false;
            }

            // Retrieve engine version from .uproject file
            UnrealVersion = File.ReadAllText(UPROJ_FILE);
            UnrealVersion = UnrealVersion.Substring(UnrealVersion.IndexOf("\"EngineAssociation\": \"") + "\"EngineAssociation\": \"".Length);
            UnrealVersion = UnrealVersion.Remove(UnrealVersion.IndexOf("\","));

            bool isLauncherPath = false;

            // Retrieve Unreal Engine directory (defaultEnginePath if no customCommand is used, otherwise custom engine path)
            if (customCommand == "")
            {          
                UNREAL_PATH = Configuration.data.defaultEnginePath;

                // special handling of launcher version
                if (Configuration.data.isLauncherPath)
                {
                    isLauncherPath = true;

                    // Add version to path --> complete path
                    UNREAL_PATH += UnrealVersion;
                    if (!Directory.Exists(UNREAL_PATH))
                    {
                        Console.WriteLine("\nERROR: Your project was generated with Unreal Engine " + UnrealVersion + ", which apparently seems not to be installed at the moment (path: " + UNREAL_PATH + " doesn't exist.)\n");
                        Console.Write(ENTER_QUIT_MSG);
                        Console.ReadLine();
                        Environment.Exit(4);
                    }
                }
            }
            else // custom commands only for 
                UNREAL_PATH = Configuration.data.customEngines[customCommand];

            // Load user file preset
            String qtBuildPreset = File.ReadAllText("qtBuildPreset.xml");

            // Replace preset variables with actual values
            qtBuildPreset = qtBuildPreset.Replace("$PROJECT_NAME", PROJECT_NAME);
            qtBuildPreset = qtBuildPreset.Replace("$PROJECT_DIR", PROJECT_DIR);
            qtBuildPreset = qtBuildPreset.Replace("$UPROJ_FILE", UPROJ_FILE);
            qtBuildPreset = qtBuildPreset.Replace("$UNREAL_PATH", UNREAL_PATH);

            qtBuildPreset = qtBuildPreset.Replace("$QT_ENV_ID", QT_ENV_ID);
            qtBuildPreset = qtBuildPreset.Replace("$QT_CONF_ID", QT_CONF_ID);

            // remove -rocket for custom engine builds
            if (isLauncherPath) // || isCustomBuild
            {
                qtBuildPreset = qtBuildPreset.Replace("-rocket", "");
            }

            // Write new user file
            try
            {
                File.WriteAllText(projectDir + "Intermediate\\ProjectFiles\\" + projectName + ".pro.user", qtBuildPreset);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nERROR: coudldn't write .pro.user file.");
                Console.WriteLine(ex.StackTrace);
                Console.Write(ENTER_QUIT_MSG);
                Console.ReadLine();
                Environment.Exit(3);
            }

            Console.WriteLine("User file written sucessfully.");

            return true;
        }
    }
}
