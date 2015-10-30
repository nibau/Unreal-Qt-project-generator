/**
 *  Unreal Qt Project Generator
 *
 *  Errors.cs
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

namespace GenerateQTProject
{
    /// <summary>
    /// Contains all the different error codes.
    /// </summary>
    public static class Errors
    {
        /// <summary>
        /// Prints the error message which corresponds to the error code,
        /// waits for user to press enter and terminates program with given error code.
        /// </summary>
        /// <param name="code">Error code</param>
        public static void ErrorExit(int code)
        {
            Console.WriteLine("\nERROR: " + ErrorMsg[code - 1] + "\n");
            Console.WriteLine(" - Press Enter to quit application...");
            Console.ReadLine();
            Environment.Exit(code);
        }

        /* ERRORS */

        public const int ENGINE_PATH_NOT_FOUND_IN_PROJECT_FILE = 1;
        public const int INVALID_ENGINE_PATH_FOUND = 2;
        public const int DEFINES_AND_INCLUDES_READ_FAILED = 3;
        public const int PROJECT_FILE_WRITE_FAILED = 4;
        public const int CODE_PROJECT_FILE_READ_FAILED = 5;
        public const int DEFINES_AND_INCLUDES_WRITE_FAILED = 6;
        public const int QT_PRO_USERFILE_WRITE_FAILED = 7;
        public const int QT_PRO_USERFILE_MISSING = 8;
        public const int QT_PRO_USERFILE_READ_FAILED = 9;
        public const int ENVIRONMENT_ID_NOT_FOUND = 10;
        public const int CONFIGURATION_ID_NOT_FOUND = 11;
        public const int CONFIGURATION_WRITE_FAILED = 12;
        public const int UPROJECT_NOT_FOUND = 13;
        public const int BUILD_PRESET_MISSING = 14;
        public const int CONFIG_OPEN_ERROR = 15;
        public const int BUILD_PRESET_READ_FAILED = 16;

        /* ERROR MESSAGES*/

        private static string[] ErrorMsg =
        {
            "Unreal Engine path not found in vcxproj file.",
            "Invalid engine path found in vcxproj. Maybe you have no longer installed the Unreal Engine build with which you created this project?",
            "Defines or includes couldn't be retrieved from project file.",
            "Couldn't write project file.",
            "The project file couldn't be read.",
            "Couldn't write defines and include files.",
            "Couldn't write .pro.user file.",
            "No .pro.user file was generated, cannot proceede (you can also edit the configuration file manually).",
            "Error while reading .pro.user file.",
            "Error while reading environment id from user file.",
            "Error while reading configuration id from user file.",
            "Error while writing configuration file.",
            "The .uproject file was not found.",
            "qtBuildPreset.xml (has to be in same folder as this .exe) file is missing.",
            "An error occurred while trying to open the configuration file.",
            "Error while reading qtBuildPreset.xml."
        };
    }
}
