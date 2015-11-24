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
using PapyrusDotNet.Converters.Papyrus2Clr;
using PapyrusDotNet.Converters.Papyrus2Clr.Implementations;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.ConsoleTests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var startTime = DateTime.Now;
            var fallout4ScriptFolder = @"D:\Spel\Fallout 4 Scripts\scripts\";
            var fallout4Script = "D:\\Spel\\Fallout 4 Scripts\\scripts\\Actor.pex";
            var skyrimScript = @"C:\CreationKit\Data\scripts\activemagiceffect.pex";

            var assembly = PapyrusAssemblyDefinition.LoadAssembly(skyrimScript, true);

            var allScriptFiles = Directory.GetFiles(fallout4ScriptFolder, "*.pex", SearchOption.AllDirectories);

            var assemblies = allScriptFiles.Select(PapyrusAssemblyDefinition.LoadAssembly);

            var namespaceResolver = new ClrNamespaceResolver();
            var converter = new PapyrusToClrConverter(namespaceResolver, new ClrTypeReferenceResolver(namespaceResolver, new ClrTypeNameResolver()));
            var output = converter.Convert(new PapyrusAssemblyInput(assemblies.ToArray()));
            var clr = output as ClrAssemblyOutput;
            clr.OutputAssembly.Write(@"D:\Git\PapyrusDotNet\Source\PapyrusDotNet.ConsoleTests\bin\Debug\PapyrusDotNet.Core.dll");
            Console.WriteLine("Build Time: " + (DateTime.Now - startTime).TotalSeconds + " seconds.");
        }
    }
}