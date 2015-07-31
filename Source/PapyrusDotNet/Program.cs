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
	
	Copyright 2014, Karl Patrik Johansson, zerratar@gmail.com
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace PapyrusDotNet
{
    using System.IO;

    using PapyrusDotNet.Common;

    using PowerArgs;

    public class PapyrusDotNetArgs
    {
        [ArgShortcut("o")]
        [ArgPosition(1)]
        public string OutputFolder { get; set; }

        [ArgShortcut("i")]
        [ArgPosition(0)]
        public string InputFile { get; set; }
    }
    class Program
    {
        public static bool HandleConstructorAsOnInit = false;
        
        public static AssemblyDefinition CurrentAssembly;

        public static string outputFolder;

        public static string inputFile;

        static void Main(string[] args)
        {

            outputFolder = @".\output";
            inputFile = @"..\Examples\Example1\bin\Debug\Example1.dll";
            try
            {
                var parsed = Args.Parse<PapyrusDotNetArgs>(args);
                if (parsed.InputFile != null)
                {
                    // Console.WriteLine("You entered string '{0}' and int '{1}'", parsed.StringArg, parsed.IntArg);
                    if (parsed.OutputFolder != null)
                        outputFolder = parsed.OutputFolder;
                    inputFile = parsed.InputFile;
                    if (outputFolder.Contains("\""))
                    {
                        outputFolder = outputFolder.Replace("\"", "");
                    }

                    if (inputFile.Contains("\""))
                    {
                        inputFile = inputFile.Replace("\"", "");
                    }

                    Console.WriteLine("You entered string '{0}' and int '{1}'", parsed.InputFile, parsed.OutputFolder);
                }
            }
            catch (ArgException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ArgUsage.GetUsage<PapyrusDotNetArgs>());
                // return;
            }


            var outputPasFiles = new Dictionary<string, string>();
            CurrentAssembly = AssemblyDefinition.ReadAssembly(inputFile);

            var asmReferences = AssemblyHelper.GetAssemblyReferences(CurrentAssembly);

            foreach (var asm in asmReferences)
            {
                PapyrusAsmWriter.GeneratePapyrusFromAssembly(asm, ref outputPasFiles, CurrentAssembly);
            }


            PapyrusAsmWriter.GeneratePapyrusFromAssembly(CurrentAssembly, ref outputPasFiles);

            foreach (var pas in outputPasFiles)
            {
                try
                {
                    if (!System.IO.Directory.Exists(outputFolder)) System.IO.Directory.CreateDirectory(outputFolder);

                    Console.WriteLine("Saving " + System.IO.Path.Combine(outputFolder, pas.Key) + "...");
                    System.IO.File.WriteAllText(System.IO.Path.Combine(outputFolder, pas.Key), pas.Value);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("----------------");
                    Console.WriteLine(outputFolder);
                    Console.WriteLine(pas.Key);
                    Console.WriteLine(exc);
                    Console.ReadKey();
                }
            }
        }

        // st codes are to store to the stack
        // ld is to load from stack and assign its value to either function, variable or return
    }
}
