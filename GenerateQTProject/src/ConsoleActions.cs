/**
 *  Unreal Qt Project Generator
 *
 *  ConsoleActions.cs
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
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GenerateQTProject
{
    /// <summary>
    /// This class contains all methods related to console input and output
    /// </summary>
    class ConsoleActions
    {
        /// <summary>
        /// Asks for project directory, repeats until valid path is entered
        /// </summary>
        public static string InputProjectPath()
        {
            string projDir = "";
            Console.WriteLine("This small tool automates the process of generating a Qt project from an Unreal Engine 4 VS project.\n");
            Console.WriteLine("Before you procede make sure you have created an Unreal Engine build kit in QtCreator (and have completed debugger setup if you intend to debug with QtCreator)\n");
            Console.WriteLine("WARNING: The project folder inside the source folder and the .vcxproj file must use the same name as the .sln file of your project. (Which is default)\n\n");

            bool isValid = false;
            do
            {
                Console.WriteLine("Please enter path to the directory where your .uproject file is located (drag and drop folder here):");
                projDir = Console.ReadLine();

                projDir = projDir.Replace("\"", "");

                if (!projDir.EndsWith("\\"))
                    projDir += "\\";

                if (Directory.Exists(projDir))
                {
                    foreach (string file in Directory.GetFiles(projDir))
                    {
                        if (file.EndsWith(".uproject"))
                        {
                            isValid = true;
                            break;
                        }
                    }           
                }
                    
                if (!isValid)
                    Console.WriteLine("Invalid directory path\n");

            } while (!isValid);

            return projDir;
        }

        /// <summary>
        /// Run a small wizard which is able to autodetect the Qt Creator ids
        /// </summary>
        public static void StartConfigWizard()
        {
            PrintHeader();
            ConfigurationData newConfig = new ConfigurationData();

            Console.WriteLine("It seems that you run this program for the first time as no configuration file was found.\n");
            Console.WriteLine("This program now needs to detect your Qt environment id and the id of your Unreal Engine build kit (which you should already have created in QtCreator before).\n\n");

            /*Console.WriteLine("1. First you have to enter the path to your primary Unreal Engine installation directory.\nIn case of the launcher version please select the launcher directory which contains the different 4.x folders. If you want to use a custom engine build (from git), please select the base directory of the installation (which contains Engine, Templates, etc. folders).\n");
            Console.WriteLine("If you want to use both (launcher and custom builds) for project file generation, select the launcher path as you can add custom commands for git builds to the configuration file. (see comments in config file)");
            //Console.WriteLine("Custom commands consist of a command_name/engine_installation_directory pair (only for git engine builds). When you want to generate a project file for a custom engine build, it then suffices to launch the project generator with the command_name as argument.");
            //Console.WriteLine("\neg. if you have in your configuration file (CustomEngineProfiles section) the line: myAwesomeBuild=C:\\Unreal\\myAwesomeEngineBuildPath");
            //Console.WriteLine("You can generate the project files for myAwesomeEngineBuild by launching UnrealQtProjectGenerator with the argument myAwesomeBuild.");

            var path = InputEnginePath(true);
            newConfig.isLauncherPath = path.Item1;
            newConfig.defaultEnginePath = path.Item2;

            PrintHeader();*/

            Console.WriteLine("When you now press enter, an empty project will be opened in QtCreator.");
            Console.WriteLine("The only thing you have to do is:\n 1. select your Unreal Engine build kit when asked by QtCreator\n 2. Hit the configure project button\n 3. close QtCreator.\n");
            Console.WriteLine("Please make sure that QtCreator is not currently running, then press enter to proceed...");
            Console.ReadLine();

            File.WriteAllText(FileActions.PROGRAM_DIR + "temp.pro", "");
            Process qtCreatorProcess = Process.Start(FileActions.PROGRAM_DIR + "temp.pro");
            Console.WriteLine("QtCreator launched, waiting for user interaction...");
            qtCreatorProcess.WaitForExit();
            Console.WriteLine("QtCreator closed...\n");

            PrintHeader();

            if (!File.Exists(FileActions.PROGRAM_DIR + "temp.pro.user"))
            {
                Console.WriteLine("\nERROR: No .pro.user file was generated, cannot proceede (you can also edit the configuration file manually).");
                Console.WriteLine(" - press enter to quit...");
                Console.ReadLine();
                Environment.Exit(15);
            }

            string userContent = "";
            try
            {
                userContent = File.ReadAllText(FileActions.PROGRAM_DIR + "temp.pro.user");
            }
            catch
            {
                Console.WriteLine("\nERROR: Error while reading .pro.user file.");
                Console.WriteLine(" - press enter to quit...");
                Console.ReadLine();
                Environment.Exit(16);
            }

            try
            {
                File.Delete(FileActions.PROGRAM_DIR + "temp.pro.user");
                File.Delete(FileActions.PROGRAM_DIR + "temp.pro");
            }
            catch
            {
                Console.WriteLine("\nERROR: Error while deleting temporary pro file.");
            }

            const string middle_env_id_pattern = "\\{[0-9a-f]{8}\\-[0-9a-f]{4}\\-[0-9a-f]{4}\\-[0-9a-f]{4}\\-[0-9a-f]{12}\\}";

            var envMatch = Regex.Match(userContent, "\\<variable\\>EnvironmentId\\</variable\\>[\\s]*\n[\\s]*\\<value type=\"QByteArray\"\\>(?<id>" + middle_env_id_pattern + ")");
            if (envMatch.Success && Configuration.IsValidQtId(envMatch.Groups["id"].Value))
            {  
                newConfig.qtCreatorEnvironmentId = envMatch.Groups["id"].Value;
            }
            else
            {
                Console.WriteLine("\nERROR: Error while reading environment id from user file.");
                Console.WriteLine(" - press enter to quit...");
                Console.ReadLine();
                Environment.Exit(17);
            }

            var confMatch = Regex.Match(userContent, "key=\"ProjectExplorer.ProjectConfiguration.Id\"\\>(?<id>" + middle_env_id_pattern + ")");
            if (confMatch.Success && Configuration.IsValidQtId(confMatch.Groups["id"].Value))
            {
                newConfig.qtCreatorUnrealConfigurationId = confMatch.Groups["id"].Value;
            }
            else
            {
                Console.WriteLine("\nERROR: Error while reading configuration id from user file.");
                Console.WriteLine(" - press enter to quit...");
                Console.ReadLine();
                Environment.Exit(18);
            }

            if (!Configuration.writeWizardConfig(newConfig))
            {
                Console.WriteLine("\nERROR: Error while writing configuration file.");
                Console.WriteLine(" - press enter to quit...");
                Console.ReadLine();
                Environment.Exit(19);
            }
        }

        private static Tuple<bool, string> InputEnginePath(bool allowLauncher)
        {
            string uPath = "";
            bool launcherPath = false;

            bool success = false;
            do
            {
                Console.WriteLine("\nPlease enter Unreal Engine Path:");
                uPath = Console.ReadLine();
                uPath = uPath.Replace("\"", "");

                if (!uPath.EndsWith("\\"))
                    uPath = uPath + "\\";

                if (!Directory.Exists(uPath))
                {
                    Console.WriteLine("Invalid Directory\n");
                }
                else
                {
                    if (allowLauncher && uPath.Contains("Epic Games"))
                    {
                        foreach (string dir in Directory.GetDirectories(uPath))
                        {
                            if (dir.Substring(dir.LastIndexOf("\\") + 1).StartsWith("4.") && File.Exists(dir + @"\Engine\Build\BatchFiles\build.bat"))
                            {
                                success = true;
                                launcherPath = true;
                                break;
                            }
                        }
                    }

                    if (File.Exists(uPath + @"Engine\Build\BatchFiles\build.bat")) {
                        success = true;
                        break;
                    }


                    if (!success)
                        Console.WriteLine("Directory contains no Unreal Engine installations.\n");
                }

            } while (!success);

            return new Tuple<bool, string>(launcherPath, uPath);
        }

        /// <summary>
        /// If this is the first run, display the disclaimer, if accepted this is stored in a file in the AppData folder
        /// </summary>
        public static void DisplayFirstRunDisclaimer()
        {
            string savePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\UnrealQtGenerator";

            if (!File.Exists(savePath + "\\vars.conf"))
            {
                Console.WriteLine("This Software is provided free of charge.\n");
                Console.WriteLine("THIS SOFTWARE IS PROVIDED BY ME “AS IS” AND ANY EXPRESS OR " +
                "IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY" +
                " AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL I" +
                " BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES " +
                "(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, " +
                "OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, " +
                "WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING " +
                "IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.\n");
                Console.WriteLine("This Software is UNOFFICIAL, I am neither affiliated with Epic Games nor with the Qt-Project.\n"
                    + "\"Qt\" is a registered trademark of Digia Plc and/or its subsidiaries. \"Unreal\" is a registered trademark of Epic Games, Inc.\n");

                Console.Write(" - Press \"y\" to accept the disclaimer and to continue, or \"n\" to decline...");
                bool accepted = false;
                while (!accepted)
                {
                    ConsoleKeyInfo k = Console.ReadKey();
                    if (k.Key == ConsoleKey.Y)
                        accepted = true;
                    else if (k.Key == ConsoleKey.N)
                        Environment.Exit(-1);
                }

                if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);

                try
                {
                    File.WriteAllText(savePath + "\\vars.conf", "DisclaimerAccepted");
                }
                catch (Exception)
                {
                    Console.WriteLine("ERROR: Disclaimer accepted state couldn't be stored.");
                }
                
                // clear disclaimer text
                Console.Clear();
                PrintHeader();
            }
        }

        /// <summary>
        /// Prints application header to console
        /// </summary>
        public static void PrintHeader()
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("***********************************************");
            Console.WriteLine("****** Unreal Qt Project Generator v0.3  ******");
            Console.WriteLine("******            ©2015 AlphaN           ******");
            Console.WriteLine("******     Special Thanks to Antares     ******");
            Console.WriteLine("***********************************************");
            Console.WriteLine();
        }
    }
}
