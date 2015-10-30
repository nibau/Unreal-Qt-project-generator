/**
 *  Unreal Qt Project Generator
 *
 *  FileActions.cs
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
    /// IO methods (file search etc.)
    /// </summary>
    class FileActions
    {
        /// <summary>
        /// Directory where the uProGen executable is located.
        /// </summary>
        public static string PROGRAM_DIR { get; } = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Opens configuration file in default text editor.
        /// </summary>
        public static void OpenConfigFile()
        {
            try
            {
                System.Diagnostics.Process.Start(PROGRAM_DIR + Configuration.CONFIG_FILE_NAME);
            }
            catch
            {
                Errors.ErrorExit(Errors.CONFIG_OPEN_ERROR);
            }
        }

        /// <summary>
        /// Extract project name from .uproject filename
        /// </summary>
        /// <param name="projectDir">Directory which contains sln and uproject file</param>
        /// <returns>Project Name</returns>
        public static string ExtractProjectName(string projectDir)
        {
            string projectName = "";
            foreach (string file in Directory.GetFiles(projectDir))
            {
                if (file.EndsWith(".uproject")) // uproject file found
                {
                    projectName = file.Substring(file.LastIndexOf('\\') + 1).Replace(".uproject", "");
                    break;
                }
            }

            if (projectName == "")
            {
                Errors.ErrorExit(Errors.UPROJECT_NOT_FOUND);
            }

            return projectName;
        }

        /// <summary>
        /// Scan working directory for .uproject file
        /// </summary>
        /// <returns>Working directory if project file is found, empty string otherwise</returns>
        public static string LookForProjectInWD()
        {
            foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory()))
            {
                if (file.EndsWith(".uproject"))
                    return Directory.GetCurrentDirectory();
            }

            return "";
        }

        /// <summary>
        /// Terminate program if qtBuildPreset.xml not present
        /// </summary>
        public static void CheckIfPresetFilePresent()
        {
            if (!File.Exists(PROGRAM_DIR + "qtBuildPreset.xml"))
                Errors.ErrorExit(Errors.BUILD_PRESET_MISSING);
        }

        /// <summary>
        /// Recursive method to scan source directory for source and header files
        /// </summary>
        /// <param name="SourceFiles">List to store source files</param>
        /// <param name="HeaderFiles">List to store header files</param>
        /// <param name="scanDirectory">Directory to scan</param>
        /// <param name="projectName">Name of your UE project</param>
        public static void ScanDirectoryForFiles(List<string> SourceFiles, List<string> HeaderFiles, string scanDirectory, string projectName)
        {
            if (!Directory.Exists(scanDirectory)) // error
            {
                Console.WriteLine("Directory \"" + scanDirectory + "\" not found.");
                return;
            }

            // Recursive search
            foreach (string dir in Directory.GetDirectories(scanDirectory))
            {
                ScanDirectoryForFiles(SourceFiles, HeaderFiles, dir, projectName);
            }

            // Scan current directory
            foreach (string sfile in Directory.GetFiles(scanDirectory))
            {
                if (sfile.EndsWith(".cpp"))
                    SourceFiles.Add("../../" + sfile.Substring(sfile.LastIndexOf("\\Source\\" + projectName)+1).Replace("\\", "/"));
                else if (sfile.EndsWith(".h") || sfile.EndsWith(".hpp"))
                    HeaderFiles.Add("../../" + sfile.Substring(sfile.LastIndexOf("\\Source\\" + projectName)+1).Replace("\\", "/"));
            }
        }
    }
}
