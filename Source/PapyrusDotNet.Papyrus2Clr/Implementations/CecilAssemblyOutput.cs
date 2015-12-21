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

using System.IO;
using Mono.Cecil;
using PapyrusDotNet.Common.Interfaces;

#endregion

namespace PapyrusDotNet.Converters.Papyrus2Clr.Implementations
{
    public class CecilAssemblyOutput : IAssemblyOutput
    {
        public CecilAssemblyOutput(AssemblyDefinition clrAssembly)
        {
            OutputAssembly = clrAssembly;
        }

        public AssemblyDefinition OutputAssembly { get; }

        public void Save(string output)
        {
            OutputAssembly.Write(Path.Combine(output, "PapyrusDotNet.Core.dll")); // TODO: Change the name to match the target version, ex Skyrim or Fallout4
        }
    }
}