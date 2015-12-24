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

#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Mono.Cecil;
using PapyrusDotNet.Common;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.Common.Utilities;
using PapyrusDotNet.Converters.Clr2Papyrus;
using PapyrusDotNet.Converters.Clr2Papyrus.Enums;
using PapyrusDotNet.Converters.Clr2Papyrus.Implementations;
using PapyrusDotNet.Converters.Papyrus2Clr;
using PapyrusDotNet.Converters.Papyrus2Clr.Implementations;
using PapyrusDotNet.PapyrusAssembly;
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

        [Obsolete]
        public static string InputFile { get; set; }

        //public static string OutputFolder;

        //public static string InputFile;

        private static readonly ConsoleUiRenderer ui = new ConsoleUiRenderer();

        private static void Main(string[] args)
        {

            IAssemblyConverter converter;
            IAssemblyInput inputData;

            if (args.Length < 2)
            {
                Console.WriteLine("  Missing Important Arguments.");
                ui.DrawHelp();
                return;
            }

            ui.DrawInterface("Magic is about to happen!");

            var clr2Papyrus = !Enumerable.Contains(args, "-clr");
            var input = args[0];
            var output = args[1];

            if (clr2Papyrus)
            {
                var targetVersion = Enumerable.Contains(args, "-skyrim") ? PapyrusVersionTargets.Skyrim
                        : PapyrusVersionTargets.Fallout4;

                var compilerOptions = !Enumerable.Contains(args, "-easy") ? PapyrusCompilerOptions.Strict
                    : PapyrusCompilerOptions.Easy;
                var readerParameters = new ReaderParameters { ReadSymbols = true };
                converter = new Clr2PapyrusConverter(new Clr2PapyrusInstructionProcessor(), compilerOptions);
                inputData = new ClrAssemblyInput(AssemblyDefinition.ReadAssembly(input, readerParameters), targetVersion);
            }
            else
            {
                var nsResolver = new NamespaceResolver();
                var camelCaseResolver = new PascalCaseNameResolver(ui, "wordlist-fo4.txt");
                // Papyrus2ClrCecilConverter
                converter = new Papyrus2ClrCecilConverter(
                    ui, camelCaseResolver, nsResolver,
                    new TypeReferenceResolver(nsResolver, new TypeNameResolver(camelCaseResolver)));

                var pexFiles = Directory.GetFiles(input, "*.pex", SearchOption.AllDirectories);

                ui.DrawInterface("(1/3) Reading Papyrus Assemblies.");

                var papyrusAssemblyDefinitions = pexFiles.Select(f => ReadPapyrusAssembly(f, pexFiles.Length)).ToArray();

                inputData = new PapyrusAssemblyInput(
                    papyrusAssemblyDefinitions
                    );
            }

            var outputData = converter.Convert(inputData);

            if (outputData != null)
            {
                outputData.Save(output);

                // Do something...
            }

            while (true)
            {
                var openTargetDir = new Hotkeys("Open target directory", ConsoleKey.A, () =>
                {
                    Process.Start(output);
                });

                var exit = new Hotkeys("Exit", ConsoleKey.X, () => { });

                ui.DrawHotkeys(openTargetDir, exit);

                var posx = Console.CursorLeft;

                var k = Console.ReadKey();

                Console.CursorLeft = posx;
                Console.Write(" ");

                if (k.Key == exit.Key)
                    return;

                if (k.Key == openTargetDir.Key)
                {
                    openTargetDir.Action();
                }
            }
            // MainOld(args);
        }

        private static int assembliesReadTick;
        private static int assembliesRead;
        private static PapyrusAssemblyDefinition ReadPapyrusAssembly(string arg, int maxCount)
        {
            assembliesReadTick++;
            assembliesRead++;
            if (assembliesReadTick >= 100 || assembliesRead == maxCount || maxCount < 1000)
            {
                ui.DrawProgressBarWithInfo(assembliesRead, maxCount);
                assembliesReadTick = 0;
            }
            return PapyrusAssemblyDefinition.ReadAssembly(arg);
        }

        #region Old Program Start

        //        private static void MainOld(string[] args)
        //        {
        //            OutputFolder = @".\output";
        //            InputFile = @"..\Examples\Example1\bin\Debug\Example1.dll";
        //            try
        //            {
        //                var parsed = Args.Parse<PapyrusDotNetArgs>(args);
        //                if (parsed.InputFile != null)
        //                {
        //                    // Console.WriteLine("You entered string '{0}' and int '{1}'", parsed.StringArg, parsed.IntArg);
        //                    if (parsed.OutputFolder != null)
        //                        OutputFolder = parsed.OutputFolder;
        //                    InputFile = parsed.InputFile;
        //                    if (OutputFolder.Contains("\""))
        //                    {
        //                        OutputFolder = OutputFolder.Replace("\"", "");
        //                    }
        //
        //                    if (InputFile.Contains("\""))
        //                    {
        //                        InputFile = InputFile.Replace("\"", "");
        //                    }
        //
        //                    Console.WriteLine("You entered string '{0}' and int '{1}'", parsed.InputFile, parsed.OutputFolder);
        //                }
        //            }
        //            catch (ArgException ex)
        //            {
        //                Console.WriteLine(ex.Message);
        //                Console.WriteLine(ArgUsage.GetUsage<PapyrusDotNetArgs>());
        //                // return;
        //            }
        //
        //
        //            var outputPasFiles = new Dictionary<string, string>();
        //            CurrentAssembly = AssemblyDefinition.ReadAssembly(InputFile);
        //
        //            var asmReferences = AssemblyHelper.GetAssemblyReferences(CurrentAssembly);
        //
        //            /* Generate all CallStacks */
        //            foreach (var asm in asmReferences)
        //            {
        //                PapyrusAsmWriter.GenerateCallStack(asm);
        //            }
        //            PapyrusAsmWriter.GenerateCallStack(CurrentAssembly);
        //
        //            /* Generate all papyrus code */
        //            foreach (var asm in asmReferences)
        //            {
        //                PapyrusAsmWriter.GeneratePapyrusFromAssembly(asm, ref outputPasFiles, CurrentAssembly);
        //            }
        //
        //            PapyrusAsmWriter.GeneratePapyrusFromAssembly(CurrentAssembly, ref outputPasFiles);
        //
        //            foreach (var pas in outputPasFiles)
        //            {
        //                try
        //                {
        //                    if (!Directory.Exists(OutputFolder)) Directory.CreateDirectory(OutputFolder);
        //
        //                    Console.WriteLine("Saving " + Path.Combine(OutputFolder, pas.Key) + "...");
        //                    File.WriteAllText(Path.Combine(OutputFolder, pas.Key), pas.Value);
        //                }
        //                catch (Exception exc)
        //                {
        //                    Console.WriteLine("----------------");
        //                    Console.WriteLine(OutputFolder);
        //                    Console.WriteLine(pas.Key);
        //                    Console.WriteLine(exc);
        //                    Console.ReadKey();
        //                }
        //            }
        //        }

        #endregion

        // st codes are to store to the stack
        // ld is to load from stack and assign its value to either function, variable or return
    }
}