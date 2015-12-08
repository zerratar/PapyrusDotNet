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
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PapyrusDotNet.Converters.Clr2Papyrus;
using PapyrusDotNet.Converters.Clr2Papyrus.Enums;
using PapyrusDotNet.Converters.Clr2Papyrus.Implementations;
using PapyrusDotNet.Converters.Papyrus2Clr.Implementations;
using PapyrusDotNet.Converters.Papyrus2CSharp;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Enums;

#endregion

namespace PapyrusDotNet.ConsoleTests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var converter = new Clr2PapyrusConverter(new Clr2PapyrusInstructionProcessor(), PapyrusCompilerOptions.Strict);
            var value = converter.Convert(
                new ClrAssemblyInput(
                    AssemblyDefinition.ReadAssembly(
                        @"D:\Git\PapyrusDotNet\Examples\Fallout4Example\bin\Debug\fallout4example.dll"),
                    PapyrusVersionTargets.Fallout4)) as PapyrusAssemblyOutput;

            JsonConvert.DefaultSettings = (() =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                return settings;
            });

            var val = JsonConvert.SerializeObject(value.Assemblies, Formatting.Indented);

            var dummy = val;



            var folder = @"D:\Spel\Fallout 4 Scripts\scripts\";
            var pexFile1 = folder + @"companionaffinityeventquestscript.pex";
            var pexFile2 = folder + @"BobbleheadStandContainerScript.pex";
            var pexFile3 = folder + @"mq203script.pex";

            var pexAssemblies = new PapyrusAssemblyDefinition[0];
            //{
            //    PapyrusAssemblyDefinition.ReadAssembly(pexFile1),
            //    PapyrusAssemblyDefinition.ReadAssembly(pexFile2),
            //    PapyrusAssemblyDefinition.ReadAssembly(pexFile3)
            //};

            var asm = value.Assemblies;

            var defs = new List<PapyrusAssemblyDefinition>(pexAssemblies);
            defs.AddRange(asm);

            //var pexFile2Size = new FileInfo(pexFile2).Length;

            //pexAssemblies[1].Write(pexFile2 + ".new");

            //var pexFile2SizeAfter = new FileInfo(pexFile2 + ".new").Length;

            //if (pexFile2Size == pexFile2SizeAfter)
            //{

            //}
            //else
            //{
            //    throw new Exception("ERROR ERROR! FILE SIZE MISMATCH!");
            //}

            var clrNamespaceResolver = new ClrNamespaceResolver();
            var csharpConverter = new Papyrus2CSharpConverter(clrNamespaceResolver,
                new ClrTypeReferenceResolver(clrNamespaceResolver, new ClrTypeNameResolver()));


            var output = csharpConverter.Convert(new PapyrusAssemblyInput(defs.ToArray())) as MultiCSharpOutput;

            var targetOutputFolder = "c:\\PapyrusDotNet\\Output";
            if (!Directory.Exists(targetOutputFolder))
            {
                Directory.CreateDirectory(targetOutputFolder);
            }

            output.Save(targetOutputFolder);
            value.Save(targetOutputFolder);



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