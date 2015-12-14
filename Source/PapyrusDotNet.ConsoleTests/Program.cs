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
using System.IO;
using PapyrusDotNet.Converters.Papyrus2Clr.Implementations;
using PapyrusDotNet.Converters.Papyrus2CSharp;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.ConsoleTests
{
    internal class Program
    {
        private static void DecompileAllFallout4Scripts()
        {
            var folder = @"D:\Spel\Fallout 4 Scripts\scripts\";

            var allScripts = Directory.GetFiles(folder, "*.pex", SearchOption.AllDirectories);

            var clrNamespaceResolver = new ClrNamespaceResolver();
            var csharpConverter = new Papyrus2CSharpConverter(clrNamespaceResolver,
                new ClrTypeReferenceResolver(clrNamespaceResolver, new ClrTypeNameResolver()));

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

        private static void Main(string[] args)
        {

            DecompileAllFallout4Scripts();


            //            var converter = new Clr2PapyrusConverter(new Clr2PapyrusInstructionProcessor(), PapyrusCompilerOptions.Strict);
            //            var value = converter.Convert(
            //                new ClrAssemblyInput(
            //                    AssemblyDefinition.ReadAssembly(
            //                        @"c:\Git\PapyrusDotNet\Examples\Fallout4Example\bin\Debug\fallout4example.dll"),
            //                    PapyrusVersionTargets.Fallout4)) as PapyrusAssemblyOutput;
            //#if false
            //            var folder = @"d:\git\PapyrusDotNet\Source\Test Scripts\Fallout 4\";
            //            var pexFile1 = folder + @"AssaultronHeadModStealthScript.pex";
            //            var pexFile2 = folder + @"BobbleheadStandContainerScript.pex";
            //            var pexFile3 = folder + @"DN035QuestScript.pex";

            //            var pexAssemblies = new PapyrusAssemblyDefinition[]
            //            {
            //                PapyrusAssemblyDefinition.ReadAssembly(pexFile1),
            //                PapyrusAssemblyDefinition.ReadAssembly(pexFile2),
            //                PapyrusAssemblyDefinition.ReadAssembly(pexFile3)
            //            };
            //#else
            //            var pexAssemblies = new PapyrusAssemblyDefinition[0];
            //#endif
            //            var asm = value.Assemblies;

            //            var defs = new List<PapyrusAssemblyDefinition>(pexAssemblies);
            //            defs.AddRange(asm);

            //            var clrNamespaceResolver = new ClrNamespaceResolver();
            //            var csharpConverter = new Papyrus2CSharpConverter(clrNamespaceResolver,
            //                new ClrTypeReferenceResolver(clrNamespaceResolver, new ClrTypeNameResolver()));


            //            var output = csharpConverter.Convert(new PapyrusAssemblyInput(defs.ToArray())) as MultiCSharpOutput;

            //            var targetOutputFolder = "c:\\PapyrusDotNet\\Output";
            //            if (!Directory.Exists(targetOutputFolder))
            //            {
            //                Directory.CreateDirectory(targetOutputFolder);
            //            }

            //            output.Save(targetOutputFolder);
            //            value.Save(targetOutputFolder);



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