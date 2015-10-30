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
        /// <param name="projData">Reference to project parser</param>
        /// <returns>success</returns>
        public static void GenerateProFile(ProjectFileParser projData)
        {
            /* These lists will store all source and header files 
            which are found in the source directory of your project */
            List<string> SourceFilePaths;
            List<string> HeaderFilePaths;

            ConsoleActions.PrintHeader();

            Console.WriteLine("Generating .pro file...");
            SourceFilePaths = new List<string>();
            HeaderFilePaths = new List<string>();

            string sourcePath = projData.projectPath + "Source\\" + projData.projectName;

            FileActions.ScanDirectoryForFiles(SourceFilePaths, HeaderFilePaths, sourcePath, projData.projectName);

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
            if (File.Exists(sourcePath + "\\" + projData.projectName + ".Build.cs"))
            {
                qtProFile = qtProFile + "\n\n" +
                "DISTFILES += \\\n\t" +
                "../../Source/" + projData.projectName + "/" + projData.projectName + ".Build.cs";
            }

            try
            {
                File.WriteAllText(projData.projectPath + "Intermediate\\ProjectFiles\\" + projData.projectName + ".pro", qtProFile);
            }
            catch
            {
                Errors.ErrorExit(Errors.PROJECT_FILE_WRITE_FAILED);
            }
        }

        /// <summary>
        /// Generates defines.pri and includes.pri files (with data from YourProject.vcxproj)
        /// </summary>
        /// <param name="projData">Reference to project parser</param>
        /// <returns>success</returns>
        public static void GenerateDefinesAndInclude(ProjectFileParser projData)
        {
            Console.WriteLine("Generating defines.pri and includes.pri...\n");
            // Write files
            try
            {
                File.WriteAllText(projData.projectPath + "Intermediate\\ProjectFiles\\defines.pri", projData.GetEngineDefines());
                File.WriteAllText(projData.projectPath + "Intermediate\\ProjectFiles\\includes.pri", projData.GetEngineIncludes());
            }
            catch
            {
                Errors.ErrorExit(Errors.DEFINES_AND_INCLUDES_WRITE_FAILED);
            }
        }


        /// <summary>
        /// This method will generate and apply a modified qt.pro.user file, which contains build presets for UE4
        /// </summary>
        /// <param name="projData">Reference to project parser</param>
        /// <returns>success</returns>
        public static void GenerateQtBuildPreset(ProjectFileParser projData)
        {
            // Helper variable which stores the retrieved Unreal Engine Version (currently not needed)
            //string UnrealVersion;

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
            PROJECT_NAME = projData.projectName;

            // Set project directory
            PROJECT_DIR = projData.projectPath;

            // Set project file path
            UPROJ_FILE = projData.uprojectFilePath;

            // set engine path
            UNREAL_PATH = projData.GetEnginePath();

            // Load user file preset
            String qtBuildPreset = "";
            try
            {
                qtBuildPreset = File.ReadAllText(FileActions.PROGRAM_DIR + "qtBuildPreset.xml");
            }
            catch
            {
                Errors.ErrorExit(Errors.BUILD_PRESET_READ_FAILED);
            }
            

            // Replace preset variables with actual values
            qtBuildPreset = qtBuildPreset.Replace("$PROJECT_NAME", PROJECT_NAME);
            qtBuildPreset = qtBuildPreset.Replace("$PROJECT_DIR", PROJECT_DIR);
            qtBuildPreset = qtBuildPreset.Replace("$UPROJ_FILE", UPROJ_FILE);
            qtBuildPreset = qtBuildPreset.Replace("$UNREAL_PATH", UNREAL_PATH);

            qtBuildPreset = qtBuildPreset.Replace("$QT_ENV_ID", QT_ENV_ID);
            qtBuildPreset = qtBuildPreset.Replace("$QT_CONF_ID", QT_CONF_ID);

            // remove -rocket for custom engine builds
            if (!projData.IsLauncherBuild())
            {
                qtBuildPreset = qtBuildPreset.Replace("-rocket", "");
            }

            // Write new user file
            try
            {
                File.WriteAllText(projData.projectPath + "Intermediate\\ProjectFiles\\" + projData.projectName + ".pro.user", qtBuildPreset);
            }
            catch
            {
                Errors.ErrorExit(Errors.QT_PRO_USERFILE_WRITE_FAILED);
            }

            Console.WriteLine("User file written sucessfully.");
        }
    }
}
