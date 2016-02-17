/**
 *  Unreal Qt Project Generator
 *
 *  VCX_Parser.cs
 *
 *  Copyright (c) 2016 N. Baumann
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

namespace GenerateQTProject
{
    /// <summary>
    /// Implementation of ProjectFileParser for Visual Studio project files
    /// Error handling is done in base class
    /// </summary>
    public class VCX_Parser : ProjectFileParser
    {
        public VCX_Parser(string projectFile, string projectName) : base(projectFile, projectName) { }

        protected override string ConvertDefinesToQtFormat(string defines)
        {
            defines = "DEFINES += \"" + defines;
            defines = defines.Replace(";", "\"\nDEFINES += \"");
            defines = defines + "\"";

            return defines;
        }

        protected override string ConvertIncludesToQtFormat(string includes)
        {
            includes = "INCLUDEPATH += \"" + includes;
            includes = includes.Replace(";", "\"\nINCLUDEPATH += \"");
            includes = includes + "\"";

            return includes;
        }

        protected override string ExtractEngineDefines()
        {
            string definesString = projectFileContent;
            definesString = definesString.Substring(definesString.IndexOf("<NMakePreprocessorDefinitions>$(NMakePreprocessorDefinitions);") + "<NMakePreprocessorDefinitions>$(NMakePreprocessorDefinitions);".Length);
            definesString = definesString.Remove(definesString.LastIndexOf("</NMakePreprocessorDefinitions>"));

            return definesString;
        }

        protected override string ExtractEngineIncludes()
        {
            string includesString = projectFileContent;
            includesString = includesString.Substring(includesString.IndexOf("<NMakeIncludeSearchPath>$(NMakeIncludeSearchPath);") + "<NMakeIncludeSearchPath>$(NMakeIncludeSearchPath);".Length);
            includesString = includesString.Remove(includesString.LastIndexOf("</NMakeIncludeSearchPath>"));

            return includesString;
        }

        protected override string GetUnrealPathRegexPattern()
        {
            return "\\<NMakeBuildCommandLine\\>\\\"?(?<path>.*)\\\\Engine\\\\Build\\\\BatchFiles\\\\Build.bat";
        }

        protected override string LoadProjectFile(string path, string projectName)
        {
            return File.ReadAllText(path + "Intermediate\\ProjectFiles\\" + projectName + ".vcxproj");
        }
    }
}
