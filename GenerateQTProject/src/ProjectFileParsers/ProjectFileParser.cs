/**
 *  Unreal Qt Project Generator
 *
 *  ProjectFileParser.cs
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

using System.IO;
using System.Text.RegularExpressions;

namespace GenerateQTProject
{
    /// <summary>
    /// This class provides an abstract interface to load defines, includes and engine path from a project file
    /// </summary>
    public abstract class ProjectFileParser
    {
        protected string projectFileContent;

        public string projectPath { get; private set; }
        public string projectName { get; private set; }
        public string uprojectFilePath { get; private set; }

        /// <summary>
        /// Loads content of the code project file into projectFileContent
        /// </summary>
        /// <param name="projectPath">Path to the project folder</param>
        /// <param name="projectName">Name of .uproject file</param>
        public ProjectFileParser(string projectPath, string projectName)
        {
            this.projectPath = projectPath;
            this.projectName = projectName;

            try
            {
                projectFileContent = LoadProjectFile(projectPath, projectName);

                uprojectFilePath = projectPath + projectName + ".uproject";

                if (!File.Exists(uprojectFilePath))
                    Errors.ErrorExit(Errors.UPROJECT_NOT_FOUND);
            }
            catch
            {
                Errors.ErrorExit(Errors.CODE_PROJECT_FILE_READ_FAILED);
            }
        }

        /// <summary>
        /// Loads the project file content
        /// </summary>
        /// <param name="path">Path to the Unreal Engine project folder</param>
        /// <param name="projectName">Name of the .uproject file</param>
        /// <returns>project file content</returns>
        protected abstract string LoadProjectFile(string path, string projectName);

        /// <summary>
        /// Retrieve engine defines in QtCreator format
        /// </summary>
        /// <returns>Qt formatted defines list</returns>
        public string GetEngineDefines()
        {
            string defines  = "";
            try
            {
                defines = ExtractEngineDefines();
            }
            catch
            {
                Errors.ErrorExit(Errors.DEFINES_AND_INCLUDES_READ_FAILED);
            }

            return ConvertDefinesToQtFormat(defines);
        }

        /// <summary>
        /// Retrieve engine includes in QtCreator format
        /// </summary>
        /// <returns>Qt formatted includes list</returns>
        public string GetEngineIncludes()
        {
            string includes = "";
            try
            {
                includes = ExtractEngineIncludes();
            }
            catch
            {
                Errors.ErrorExit(Errors.DEFINES_AND_INCLUDES_READ_FAILED);
            }

            return ConvertIncludesToQtFormat(includes);
        }


        /// <summary>
        /// Read engine defines from project file (can be implemented with Regex or String operations)
        /// </summary>
        /// <returns>Defines in original format</returns>
        protected abstract string ExtractEngineDefines();

        /// <summary>
        /// Converts defines to Qt format
        /// </summary>
        /// <param name="defines">Defines in original format</param>
        /// <returns>Defines in Qt format</returns>
        protected abstract string ConvertDefinesToQtFormat(string defines);

        /// <summary>
        /// Read engine includes from project file (can be implemented with Regex or String operations)
        /// </summary>
        /// <returns>Includes in original format</returns>
        protected abstract string ExtractEngineIncludes();

        /// <summary>
        /// Converts includes to Qt format
        /// </summary>
        /// <param name="includes">Includes in original format</param>
        /// <returns>Includes in Qt format</returns>
        protected abstract string ConvertIncludesToQtFormat(string includes);

        /// <summary>
        /// Retrieve engine path from project file
        /// </summary>
        /// <returns></returns>
        public string GetEnginePath()
        {
            var match = Regex.Match(projectFileContent, GetUnrealPathRegexPattern());

            if (!match.Success || match.Groups["path"] == null)
                Errors.ErrorExit(Errors.ENGINE_PATH_NOT_FOUND_IN_PROJECT_FILE);

            string path = match.Groups["path"].Value;

            if (!File.Exists(path + @"\Engine\Build\BatchFiles\build.bat"))
                Errors.ErrorExit(Errors.INVALID_ENGINE_PATH_FOUND);

            path = path.Replace("\\", "/");

            return path;
        }

        /// <summary>
        /// Is the engine build which is associated with this project a launcher build?
        /// </summary>
        /// <returns>True if launcher build, false if git build</returns>
        public bool IsLauncherBuild()
        {
            return projectFileContent.Contains("-rocket");
        }

        /// <summary>
        /// Stores a regex which matches the Unreal Engine path in the project file.
        /// IMPORTANT: The part of the matching pattern which contains the actual string needs to be in the matching group "path"
        /// </summary>
        /// <returns>Unreal Engine path pattern</returns>
        protected abstract string GetUnrealPathRegexPattern();
    }
}
