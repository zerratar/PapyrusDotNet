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
//     Copyright 2016, Karl Patrik Johansson, zerratar@gmail.com'

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Mono.Cecil;
using PapyrusDotNet.Common;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.Converters.Clr2Papyrus;
using PapyrusDotNet.Converters.Clr2Papyrus.Enums;
using PapyrusDotNet.Converters.Clr2Papyrus.Implementations;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.Converters.Papyrus2Clr;
using PapyrusDotNet.Converters.Papyrus2Clr.Implementations;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet
{
    public class PapyrusDotNetApp : IApplication
    {
        public AssemblyDefinition CurrentAssembly;

        private readonly IUserInterface ui;
        private readonly IClrInstructionProcessor instructorProcessor;
        private readonly INameConventionResolver nameResolver;
        private readonly string[] args;

        private int assembliesReadTick;
        private int assembliesRead;

        public PapyrusDotNetApp(string[] args,
            IUserInterface ui,
            IClrInstructionProcessor instructorProcessor,
            INameConventionResolver nameResolver)
        {
            this.args = args;
            this.ui = ui;
            this.instructorProcessor = instructorProcessor;
            this.nameResolver = nameResolver;
        }

        public int Run()
        {
            IAssemblyConverter converter;
            IAssemblyInput inputData;

            if (args.Length < 2)
            {
                // ui.WriteLine("  Missing Important Arguments.");
                ui.DrawHelp();
                return 128;
            }

            ui.Clear();
            ui.DrawInterface("Magic is about to happen!");

            var clr2Papyrus = !Enumerable.Contains(args, "-clr");
            var input = args[0];
            if (args.Contains("-i"))
            {
                input = args[Array.IndexOf(args, "-i") + 1];
            }
            var output = args[1];
            if (args.Contains("-o"))
            {
                output = args[Array.IndexOf(args, "-o") + 1];
            }
            var autoClose = args.Contains("x") || args.Contains("X") || args.Contains("-x") || args.Contains("-X");
            if (clr2Papyrus)
            {
                var targetVersion = Enumerable.Contains(args, "-skyrim")
                    ? PapyrusVersionTargets.Skyrim
                    : PapyrusVersionTargets.Fallout4;

                var compilerOptions = !Enumerable.Contains(args, "-easy")
                    ? PapyrusCompilerOptions.Strict
                    : PapyrusCompilerOptions.Easy;
                var readerParameters = new ReaderParameters { ReadSymbols = true };

                converter = new Clr2PapyrusConverter(ui, instructorProcessor, compilerOptions);

                var assemblyDefinition = AssemblyDefinition.ReadAssembly(input, readerParameters);

                assemblyDefinition.MainModule.ReadSymbols();

                inputData = new ClrAssemblyInput(assemblyDefinition, targetVersion);
            }
            else
            {
                var nsResolver = new NamespaceResolver();
                //var camelCaseResolver = new PascalCaseNameResolver(ui, "wordlist-fo4.txt");
                // Papyrus2ClrCecilConverter
                converter = new Papyrus2ClrCecilConverter(
                    ui, nameResolver, nsResolver,
                    new TypeReferenceResolver(nsResolver, new TypeNameResolver(nameResolver)));

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

            if (autoClose)
            {
                return 0;
            }

            while (true)
            {
                var openTargetDir = new Hotkeys("Open target directory", ConsoleKey.A, () => { Process.Start(output); });

                var exit = new Hotkeys("Exit", ConsoleKey.X, () => { });

                ui.DrawHotkeys(openTargetDir, exit);

                var posx = Console.CursorLeft;

                var k = Console.ReadKey();

                Console.CursorLeft = posx;
                Console.Write(" ");

                if (k.Key == exit.Key)
                    return 0;

                if (k.Key == openTargetDir.Key)
                {
                    openTargetDir.Action();
                }
            }
        }

        private PapyrusAssemblyDefinition ReadPapyrusAssembly(string arg, int maxCount)
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

    }
}
