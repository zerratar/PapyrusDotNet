#region License

//     This file is part of PapyrusDotNet.
// 
//     PapyrusDotNet is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     PapyrusDotNet is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with PapyrusDotNet.  If not, see <http://www.gnu.org/licenses/>.
//  
//     Copyright 2015, Karl Patrik Johansson, zerratar@gmail.com

#endregion

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.Converters.Clr2Papyrus;
using PapyrusDotNet.Converters.Clr2Papyrus.Implementations;
using PapyrusDotNet.Converters.Papyrus2Clr;
using PapyrusDotNet.Converters.Papyrus2Clr.Implementations;
using PapyrusDotNet.Old;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Enums;
using PowerArgs;

#endregion

namespace PapyrusDotNet
{
    public class PapyrusDotNetArgs
    {
        [ArgShortcut("o")]
        [ArgPosition(1)]
        public string OutputFolder { get; set; }

        [ArgShortcut("i")]
        [ArgPosition(0)]
        public string InputFile { get; set; }
    }

    public class Program
    {
        public static bool HandleConstructorAsOnInit = false;

        public static AssemblyDefinition CurrentAssembly;

        public static string OutputFolder;

        public static string InputFile;

        private static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("PapyrusDotNet v0.2");

            IAssemblyConverter converter;
            IAssemblyInput inputData;

            var clr2Papyrus = true;
            var output = "";
            var input = "";

            if (args.Length < 2)
            {
                Console.WriteLine("Missing Important Arguments.");
                PrintHelp();
                return;
            }

            input = args[0];
            output = args[1];

            if (args.Length >= 3)
            {
                clr2Papyrus = args[2].ToLower() != "-clr";
            }

            if (clr2Papyrus)
            {
                var targetVersion = PapyrusVersionTargets.Fallout4;
                if (args.Length >= 4)
                {
                    targetVersion = args[3].ToLower() == "-fo4"
                        ? PapyrusVersionTargets.Fallout4
                        : PapyrusVersionTargets.Skyrim;
                }

                converter = new Clr2PapyrusConverter(new Clr2PapyrusInstructionProcessor());
                inputData = new ClrAssemblyInput(AssemblyDefinition.ReadAssembly(input), targetVersion);
            }
            else
            {
                var nsResolver = new ClrNamespaceResolver();

                converter = new Papyrus2ClrConverter(nsResolver,
                    new ClrTypeReferenceResolver(nsResolver, new ClrTypeNameResolver()));

                inputData = new PapyrusAssemblyInput(
                        Directory.GetFiles(input, "*.pex", SearchOption.AllDirectories)
                        .Select(PapyrusAssemblyDefinition.ReadAssembly)
                        .ToArray()
                    );
            }

            var outputData = converter.Convert(inputData);

            if (outputData != null)
            {
                // Do something...
            }

            // MainOld(args);
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Usage: PapyrusDotNet.exe <input> <output> [option] [<target papyrus version (-fo4 | -skyrim)>]");
            Console.WriteLine("Options:");
            Console.WriteLine("\t-papyrus :: [Default] Converts a .NET .dll into .pex files. Each class will be a separate .pex file.");
            Console.WriteLine("\t\t<input> :: file (.dll)");
            Console.WriteLine("\t\t<output> :: folder");
            Console.WriteLine("\t\t<target version> :: [Fallout 4 is default] -fo4 or -skyrim");
            Console.WriteLine("\t-clr :: Converts a .pex or folder containg .pex files into a .NET library usable when modding.");
            Console.WriteLine("\t\t<input> :: .pex file or folder");
            Console.WriteLine("\t\t<output> :: folder (File will be named PapyrusDotNet.Core.dll)");
        }

        #region Old Program Start
        private static void MainOld(string[] args)
        {
            OutputFolder = @".\output";
            InputFile = @"..\Examples\Example1\bin\Debug\Example1.dll";
            try
            {
                var parsed = Args.Parse<PapyrusDotNetArgs>(args);
                if (parsed.InputFile != null)
                {
                    // Console.WriteLine("You entered string '{0}' and int '{1}'", parsed.StringArg, parsed.IntArg);
                    if (parsed.OutputFolder != null)
                        OutputFolder = parsed.OutputFolder;
                    InputFile = parsed.InputFile;
                    if (OutputFolder.Contains("\""))
                    {
                        OutputFolder = OutputFolder.Replace("\"", "");
                    }

                    if (InputFile.Contains("\""))
                    {
                        InputFile = InputFile.Replace("\"", "");
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
            CurrentAssembly = AssemblyDefinition.ReadAssembly(InputFile);

            var asmReferences = AssemblyHelper.GetAssemblyReferences(CurrentAssembly);

            /* Generate all CallStacks */
            foreach (var asm in asmReferences)
            {
                PapyrusAsmWriter.GenerateCallStack(asm);
            }
            PapyrusAsmWriter.GenerateCallStack(CurrentAssembly);

            /* Generate all papyrus code */
            foreach (var asm in asmReferences)
            {
                PapyrusAsmWriter.GeneratePapyrusFromAssembly(asm, ref outputPasFiles, CurrentAssembly);
            }

            PapyrusAsmWriter.GeneratePapyrusFromAssembly(CurrentAssembly, ref outputPasFiles);

            foreach (var pas in outputPasFiles)
            {
                try
                {
                    if (!Directory.Exists(OutputFolder)) Directory.CreateDirectory(OutputFolder);

                    Console.WriteLine("Saving " + Path.Combine(OutputFolder, pas.Key) + "...");
                    File.WriteAllText(Path.Combine(OutputFolder, pas.Key), pas.Value);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("----------------");
                    Console.WriteLine(OutputFolder);
                    Console.WriteLine(pas.Key);
                    Console.WriteLine(exc);
                    Console.ReadKey();
                }
            }
        }
        #endregion
        // ld is to load from stack and assign its value to either function, variable or return

        // st codes are to store to the stack
    }

}