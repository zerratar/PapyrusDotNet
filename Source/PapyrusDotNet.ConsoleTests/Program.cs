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
            var fallout4ScriptFolder = @"D:\Spel\Fallout 4 Scripts\scripts\";
            var fallout4Script = @"D:\Spel\Fallout 4 Scripts\scripts\companionaffinityeventquestscript.pex";
            var skyrimScript = @"C:\CreationKit\Data\scripts\activemagiceffect.pex";

            // var assembly = PapyrusAssemblyDefinition.LoadAssembly(fallout4Script);

            var allScriptFiles = Directory.GetFiles(fallout4ScriptFolder, "*.pex", SearchOption.AllDirectories);

            List<PapyrusAssemblyDefinition> assemblies = new List<PapyrusAssemblyDefinition>();
            var loadCount = 1;
            var corruptedCount = 0;
            foreach (var s in allScriptFiles)
            {
                var asm = PapyrusAssemblyDefinition.LoadAssembly(s);
                if (asm.IsCorrupted)
                {
                    corruptedCount++;
                }
                assemblies.Add(asm);
                loadCount++;
            }

            // var assemblies = allScriptFiles.Select(PapyrusAssemblyDefinition.LoadAssembly);

            var namespaceResolver = new ClrNamespaceResolver();
            var converter = new PapyrusToClrConverter(namespaceResolver, new ClrTypeReferenceResolver(namespaceResolver, new ClrTypeNameResolver()));
            var output = converter.Convert(new PapyrusAssemblyInput(assemblies.ToArray()));
        }
    }
}