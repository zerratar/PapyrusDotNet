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
//     Copyright 2016, Karl Patrik Johansson, zerratar@gmail.com

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using PapyrusDotNet.Common.Utilities;
using PapyrusDotNet.Converters.Clr2Papyrus;
using PapyrusDotNet.Converters.Clr2Papyrus.Enums;
using PapyrusDotNet.Converters.Clr2Papyrus.Implementations;
using PapyrusDotNet.Converters.Papyrus2Clr.Implementations;
using PapyrusDotNet.Converters.Papyrus2CSharp;
using PapyrusDotNet.Decompiler;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.ConsoleTests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Decompile_FollowersScript();

            //ReadDelegatesPex();

            //DecompileAllFallout4Scripts();

            //ReadAndWritePex();
        }

        public static void Decompile_FollowersScript()
        {
            var asm = PapyrusAssemblyDefinition.ReadAssembly(
                @"C:\Users\Karl\Downloads\exampleBad\test.pex");

            var decompiler =
                new PapyrusDecompiler(asm);
            var ctx = decompiler.CreateContext();
            var methods =
                asm.Types.First()
                    .States.SelectMany(s => s.Methods)
                    .Where(m => m.Body.Instructions.Count > 2)
                    .OrderBy(m => m.Body.Instructions.Count);
            foreach (var method in methods)
            {
                var result = decompiler.Decompile(ctx, method);
                if (result.DecompiledSourceCode.Contains("If (") || result.DecompiledSourceCode.Contains("While ("))
                {
                    // 1. Conditionet i IF satserna uppdateras aldrig och stannar som "If (false) ..."
                    //      - Detta gör att värden som skall användas saknas (om de är en direkt referens till en boolean variable)
                    //      - Eller att Expressions används och "mergas", ex: A == B
                    // 2. Indenten på första raden i en If Sats är fel.
                    // 3. Return kan ibland ha en "Undefined = return EXPRESSION", varför den sista "assign" vid "CheckAssign" sker
                    //    är fortfarande ett frågetecken. Den bör inte göras om destinationen (GetResult()) är undefined. 
                    //    men eftersom detta inte händer i källan så är det något som kontrolleras fel eller så har inte alla noder rätt värden.
                }
            }
        }

        private static void DecompileAllFallout4Scripts()
        {
            var folder = @"D:\Spel\Fallout 4 Scripts\scripts\";

            var allScripts = Directory.GetFiles(folder, "*.pex", SearchOption.AllDirectories);

            var clrNamespaceResolver = new NamespaceResolver();
            var csharpConverter = new Papyrus2CSharpConverter(clrNamespaceResolver,
                new TypeReferenceResolver(clrNamespaceResolver,
                    new TypeNameResolver(new PascalCaseNameResolver(new ConsoleUserInterface(), new PascalCaseNameResolverSettings(null)))));

            var index = 1;
            foreach (var s in allScripts)
            {
                Console.SetCursorPosition(0, 0);
                var asm =
                    PapyrusAssemblyDefinition.ReadAssembly(s);

                var output = csharpConverter.Convert(new PapyrusAssemblyInput(asm)) as MultiCSharpOutput;

                var targetOutputFolder = "c:\\PapyrusDotNet\\Output\\Decompiled";
                if (!Directory.Exists(targetOutputFolder))
                {
                    Directory.CreateDirectory(targetOutputFolder);
                }

                output?.Save(targetOutputFolder);

                Console.WriteLine("Decompiled: " + index + "/" + allScripts.Length);
                index++;
            }


            //var pexAssemblies = new PapyrusAssemblyDefinition[]
            //{
            //    PapyrusAssemblyDefinition.ReadAssembly(pexFile1),
            //    PapyrusAssemblyDefinition.ReadAssembly(pexFile2),
            //    PapyrusAssemblyDefinition.ReadAssembly(pexFile3)
            //};
        }

        private static void ReadDelegatesPex()
        {
            var asm = PapyrusAssemblyDefinition.ReadAssembly(@"C:\PapyrusDotNet\Output\DelegateTests.pex");
        }


        private static void ReadAndWritePex()
        {
            var drive = "d";
            var dir = @":\Git\PapyrusDotNet\Examples\Fallout4Example\bin\Debug\";
            var targetFolder = drive + dir;

            if (!Directory.Exists(targetFolder))
            {
                targetFolder = "c" + dir;
            }

            //            var provider = new Mono.Cecil.Pdb.PdbReaderProvider();

            //            provider.GetSymbolReader()

            //            PdbFactory factory = new PdbFactory();
            //            ISymbolReader reader =
            //factory.CreateReader(assdef.MainModule, ass_file);

            var readerParameters = new ReaderParameters { ReadSymbols = true };

            var converter = new Clr2PapyrusConverter(new Clr2PapyrusInstructionProcessor(),
                PapyrusCompilerOptions.Strict);
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(
                targetFolder + "fallout4example.dll", readerParameters);

            try
            {
                assemblyDefinition.MainModule.ReadSymbols();
            }
            catch
            {
            }

            var value = converter.Convert(
                new ClrAssemblyInput(
                    assemblyDefinition,
                    PapyrusVersionTargets.Fallout4)) as PapyrusAssemblyOutput;
#if false
                        var folder = @"d:\git\PapyrusDotNet\Source\Test Scripts\Fallout 4\";
                        var pexFile1 = folder + @"AssaultronHeadModStealthScript.pex";
                        var pexFile2 = folder + @"BobbleheadStandContainerScript.pex";
                        var pexFile3 = folder + @"DN035QuestScript.pex";

                        var pexAssemblies = new PapyrusAssemblyDefinition[]
                        {
                            PapyrusAssemblyDefinition.ReadAssembly(pexFile1),
                            PapyrusAssemblyDefinition.ReadAssembly(pexFile2),
                            PapyrusAssemblyDefinition.ReadAssembly(pexFile3)
                        };
#else
            var pexAssemblies = new PapyrusAssemblyDefinition[0];
#endif
            var asm = value.Assemblies;

            var defs = new List<PapyrusAssemblyDefinition>(pexAssemblies);


            var targetOutputFolder = "c:\\PapyrusDotNet\\Output";
            if (!Directory.Exists(targetOutputFolder))
            {
                Directory.CreateDirectory(targetOutputFolder);
            }

            value.Save(targetOutputFolder);

            var scripts = Directory.GetFiles(targetOutputFolder, "*.pex").Select(PapyrusAssemblyDefinition.ReadAssembly);

            defs.AddRange(scripts);

            //defs.AddRange(asm);

            var clrNamespaceResolver = new NamespaceResolver();
            var csharpConverter = new Papyrus2CSharpConverter(clrNamespaceResolver,
                new TypeReferenceResolver(clrNamespaceResolver,
                    new TypeNameResolver(new PascalCaseNameResolver(new ConsoleUserInterface(), new PascalCaseNameResolverSettings(null)))));

            var output = csharpConverter.Convert(new PapyrusAssemblyInput(defs.ToArray())) as MultiCSharpOutput;

            output.Save(targetOutputFolder);


            //var sourceScript = "D:\\Spel\\Fallout 4 Scripts\\scripts\\Actor.pex";
            //var destinationScript = "D:\\Spel\\Fallout 4 Scripts\\scripts\\Actor.pex_new";

            //var src = PapyrusAssemblyDefinition.ReadAssembly(sourceScript);
            //Assert.IsNotNull(src);
            //Assert.IsNotNull(src.Header.SourceHeader.Source);

            //src.Write(destinationScript);

            //var dest = PapyrusAssemblyDefinition.ReadAssembly(destinationScript);
            //Assert.IsNotNull(src);
            //Assert.IsNotNull(dest.Header.SourceHeader.Source);

            //Assert.AreEqual(src.Header.SourceHeader.Source, dest.Header.SourceHeader.Source);


            //   TestManySkyrimPapyrus();

            //var startTime = DateTime.Now;
            //var fallout4ScriptFolder = @"D:\Spel\Fallout 4 Scripts\scripts\";
            //var fallout4Script = "D:\\Spel\\Fallout 4 Scripts\\scripts\\Actor.pex";
            //var skyrimScript = @"C:\CreationKit\Data\scripts\activemagiceffect.pex";

            ////var assembly = PapyrusAssemblyDefinition.LoadAssembly(skyrimScript, true);

            //var allScriptFiles = Directory.GetFiles(fallout4ScriptFolder, "*.pex", SearchOption.AllDirectories);

            //var assemblies = allScriptFiles.Select(PapyrusAssemblyDefinition.LoadAssembly);

            //var namespaceResolver = new ClrNamespaceResolver();
            //var converter = new PapyrusToClrConverter(namespaceResolver,
            //    new ClrTypeReferenceResolver(namespaceResolver, new ClrTypeNameResolver()));
            //var output = converter.Convert(new PapyrusAssemblyInput(assemblies.ToArray()));
            //var clr = output as ClrAssemblyOutput;
            //clr.OutputAssembly.Write(
            //    @"D:\Git\PapyrusDotNet\Source\PapyrusDotNet.ConsoleTests\bin\Debug\PapyrusDotNet.Core.dll");
            //Console.WriteLine("Build Time: " + (DateTime.Now - startTime).TotalSeconds + " seconds.");
        }

        public static void TestManySkyrimPapyrus()
        {
            var scripts = Directory.GetFiles(@"C:\CreationKit\Data\scripts\", "*.pex", SearchOption.AllDirectories);
            var success = 0;
            foreach (var script in scripts)
            {
                var assembly = PapyrusAssemblyDefinition.ReadAssembly(script);
                if (assembly == null || assembly.IsCorrupted)
                {
                    throw new Exception($"TEST FAILED AT {success}!");
                }
                success++;
            }
        }
    }
}