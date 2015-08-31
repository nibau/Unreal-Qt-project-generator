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

                if (projDir.EndsWith("\\"))
                    projDir = projDir.Remove(projDir.LastIndexOf("\\"));

                projDir = projDir.Replace("\"", "");

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
            Console.WriteLine();
            Console.WriteLine("***********************************************");
            Console.WriteLine("******  Unreal Qt Project Generator v0.2 ******");
            Console.WriteLine("******            ©2015 AlphaN           ******");
            Console.WriteLine("******     Special Thanks to Antares     ******");
            Console.WriteLine("***********************************************");
            Console.WriteLine();
        }
    }
}
