/*
	This file is part of PapyrusDotNet.

	PapyrusDotNet is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	PapyrusDotNet is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with PapyrusDotNet.  If not, see <http://www.gnu.org/licenses/>.
	
	Copyright 2015, Karl Patrik Johansson, zerratar@gmail.com
 */

using System.IO;
using PapyrusDotNet.CoreBuilder.Interfaces;

namespace PapyrusDotNet.CoreBuilder
{
    using System;

    using PowerArgs;

    class Program
    {
        private static IPapyrusCilAssemblyBuilder coreAssemblyBuilder;

        static void Main(string[] args)
        {
            Console.Title = "PapyrusDotNet";

            var inputDirectory = @"C:\CreationKit\Data\Scripts\Source";
            var inputExtensionFilter = "*.pas";
            var inputSourceType = "assembly";

            PapyrusDotNetArgs parsed = new PapyrusDotNetArgs()
            {
                InputFolder = inputDirectory,
                InputType = "script"
            };
            try
            {
                // TODO: set the parsed = null; as default value.                
                if (parsed == null)
                    parsed = Args.Parse<PapyrusDotNetArgs>(args);

                if (parsed.InputFolder != null)
                {
                    // Console.WriteLine("You entered string '{0}' and int '{1}'", parsed.StringArg, parsed.IntArg);
                    if (parsed.InputType.ToLower().Contains("script") || parsed.InputType.ToLower().Contains("psc"))
                    {
                        inputExtensionFilter = "*.psc";
                        inputSourceType = "script";
                    }

                    inputDirectory = parsed.InputFolder;
                    if (inputDirectory.Contains("\""))
                    {
                        inputDirectory = inputDirectory.Replace("\"", "");
                    }
                    // Console.WriteLine("You entered string '{0}' and int '{1}'", parsed.InputFile, parsed.OutputFolder);
                }
                if (parsed.InputFolder == null)
                {
                    Console.WriteLine("No arguments were defined, using default.");
                    Console.WriteLine("Input: " + inputDirectory);
                    Console.WriteLine("Type: " + inputSourceType);
                    Console.WriteLine();
                    Console.WriteLine("If you are fine with this, press any key to continue.");
                    Console.WriteLine("Or hit CTRL+C to exit.");
                    Console.ReadKey();
                }
            }
            catch (ArgException ex)
            {
                Console.WriteLine("Crash on startup :(");
                Console.WriteLine("Exception thrown: " + ex);

                return;
            }


            var inputSourceFiles = Directory.GetFiles(inputDirectory, inputExtensionFilter, SearchOption.AllDirectories);

            coreAssemblyBuilder = new DefaultPapyrusCilAssemblyBuilder();

            coreAssemblyBuilder.BuildAssembly(inputSourceFiles);

            Console.ReadKey();
        }
    }
}
