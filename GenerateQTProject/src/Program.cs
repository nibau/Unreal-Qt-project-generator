/**
 *  Unreal Qt Project Generator
 *
 *  Program.cs
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

using System;
using System.Diagnostics;

namespace GenerateQTProject
{
    class Program
    {
        static void Main(string[] args)
        {
            // program cannot run without qtBuildPreset.xml
            FileActions.CheckIfPresetFilePresent();

            ConsoleActions.DisplayFirstRunDisclaimer();

            string projectDir = "";
            string projectName = "";

            if (!Configuration.HasConfigurationFile())
            {
                ConsoleActions.StartConfigWizard();
                ConsoleActions.PrintHeader();
                Console.WriteLine("Configuration file written successfully.\n");
                Console.WriteLine("From now on you have two options to create project files:\n");
                Console.WriteLine("1. Run the tool from anywhere and enter the path to the project folder manually");
                Console.WriteLine("2. Run it from the command-line inside the project folder. (tool will detect everything from the working directory)");
                Console.WriteLine("\n\nIf you prefer the second option, I recommend adding the uProGen folder\nto the PATH variable.");
                Console.WriteLine("\n\n\t-Press Enter to quit...");
                Console.ReadLine();
                return;
            }
            else if (!Configuration.LoadConfiguration())
            {
                ConsoleActions.PrintHeader();
                Console.WriteLine("Invalid configuration found.\nThe configuration file will now open so you can fix the values.\nYou can also delete the file and rerun the tool (this will reinvoke the wizard).");
                Console.ReadLine();
                FileActions.OpenConfigFile();
                Environment.Exit(0);
            }

            // search working directory for project files
            projectDir = FileActions.LookForProjectInWD();
            projectDir += "\\";

            if (projectDir == "\\") // working directory isn't a project directory
            {
                ConsoleActions.PrintHeader();
                
                projectDir = ConsoleActions.InputProjectPath();
                Console.WriteLine();
            }

            projectName = FileActions.ExtractProjectName(projectDir);

            ProjectFileParser projectParser = new VCX_Parser(projectDir, projectName);

            Generator.GenerateProFile(projectParser);
         
            Generator.GenerateDefinesAndInclude(projectParser);

            Generator.GenerateQtBuildPreset(projectParser);

            Console.WriteLine("\nQt Project generation successful.\n");
            Console.WriteLine("Do you want to open your project now (y/n)?");
            string answer = Console.ReadLine();
            if (answer.ToLower() == "y" || answer.ToLower() == "yes")
            {
                // opens your newly generated project (if .pro is associated with QtCreator)
                Process.Start(projectDir + "\\Intermediate\\ProjectFiles\\" + projectName + ".pro");
            }
        }    
    }
}