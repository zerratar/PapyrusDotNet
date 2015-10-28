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

using PapyrusDotNet.Common;
using PapyrusDotNet.CoreBuilder.Interfaces;
using PapyrusDotNet.CoreBuilder.Papyrus.Assembly;
using PapyrusDotNet.CoreBuilder.Papyrus.Script;

namespace PapyrusDotNet.CoreBuilder
{
    using System;

    using PowerArgs;

    public enum PapyrusParseType
    {
        Script,
        Assembly
    }

    class Program
    {
        private static IPapyrusNameResolver nameResolver;
        private static IPapyrusScriptParser scriptParser;
        private static IPapyrusAssemblyParser assemblyParser;
        private static ICoreLibraryGenerator coreGenerator;


        static void Main(string[] args)
        {
            var ParseType = PapyrusParseType.Assembly;

            Console.Title = "PapyrusDotNet";

            string inputDirectory = @"C:\CreationKit\Data\Scripts\Source";
            string searchFor = "*.pas";
            string typeName = "assembly";

            PapyrusDotNetArgs parsed = new PapyrusDotNetArgs()
            {
                InputFolder = inputDirectory,
                InputType = "script"
            };
            try
            {
                if (parsed == null)
                    parsed = Args.Parse<PapyrusDotNetArgs>(args);
                if (parsed.InputFolder != null)
                {
                    // Console.WriteLine("You entered string '{0}' and int '{1}'", parsed.StringArg, parsed.IntArg);
                    if (parsed.InputType.ToLower().Contains("script") || parsed.InputType.ToLower().Contains("psc"))
                    {
                        ParseType = PapyrusParseType.Script;
                        searchFor = "*.psc";
                        typeName = "script";
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
                    Console.WriteLine("Type: " + typeName);
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


            nameResolver = new DictionaryPapyrusNameResolver();
            scriptParser = new PapyrusScriptParser(nameResolver);
            assemblyParser = new PapyrusAssemblyParser(nameResolver);
            coreGenerator = new CoreLibraryGenerator(new ConsoleStatusCallbackService(), scriptParser, assemblyParser);

            coreGenerator.GenerateCoreLibrary(typeName, inputDirectory, searchFor);

            Console.ReadKey();
        }
    }
}
