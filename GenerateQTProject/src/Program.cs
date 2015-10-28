/**
 *  Unreal Qt Project Generator
 *
 *  Program.cs
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
using System.Diagnostics;

namespace GenerateQTProject
{
    class Program
    {
        static void Main(string[] args)
        {         
            FileActions.CheckIfPresetFilePresent();

            string projectDir = "";
            string projectName = "";

            if (!Configuration.LoadConfiguration())
            {
                ConsoleActions.PrintHeader();
                Console.WriteLine("No valid configuration found. The configuration file will now open and you have to set at least the entries in the default section to match your configuration. After you have saved your configuration, please rerun the tool.\nTo open the configuration file, press enter...");
                Console.ReadLine();
                FileActions.OpenConfigFile();
                Environment.Exit(0);
            }

            projectDir = FileActions.lookForProjectInWD();

            if (projectDir == "")
            {
                ConsoleActions.PrintHeader();
                ConsoleActions.DisplayFirstRunDisclaimer();
                projectDir = ConsoleActions.InputProjectPath();            
            }

            projectName = FileActions.ExtractProjectName(projectDir);

            Console.WriteLine();
            if (!Generator.GenerateProFile(projectDir, projectName))
            {
                Console.WriteLine(" - Press Enter to quit application...");
                Console.ReadLine();
                Environment.Exit(1);
            }

            Console.WriteLine("Generating defines.pri and includes.pri...\n");
            if (!Generator.GenerateDefinesAndInclude(projectDir, projectName))
            {
                Console.WriteLine(" - Press Enter to quit application...");
                Console.ReadLine();
                Environment.Exit(2);
            }

            if (!Generator.GenerateQtBuildPreset(projectDir, projectName))
            {
                Console.WriteLine(" - Press Enter to quit application...");
                Console.ReadLine();
                Environment.Exit(3);
            }

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