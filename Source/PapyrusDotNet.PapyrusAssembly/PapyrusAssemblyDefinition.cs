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

using System.Collections.ObjectModel;
using PapyrusDotNet.PapyrusAssembly.Enums;
using PapyrusDotNet.PapyrusAssembly.Implementations;
using PapyrusDotNet.PapyrusAssembly.Interfaces;

#endregion

namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusAssemblyDefinition
    {
        private readonly IPapyrusAssemblyWriter writer;

        private PapyrusVersionTargets versionTarget;

        internal PapyrusAssemblyDefinition()
        {
            writer = new PapyrusAssemblyWriter();
        }

        internal PapyrusAssemblyDefinition(PapyrusVersionTargets versionTarget) : this()
        {
            this.versionTarget = versionTarget;
        }

        public PapyrusHeader Header { get; internal set; }

        public PapyrusTypeDescriptionTable DescriptionTable { get; set; }

        public Collection<PapyrusTypeDefinition> Types { get; set; }

        public static PapyrusAssemblyDefinition CreateAssembly(PapyrusVersionTargets versionTarget)
        {
            return new PapyrusAssemblyDefinition(versionTarget);
        }

        public static PapyrusAssemblyDefinition LoadAssembly(string pexFile)
        {
            using (var reader = new PapyrusAssemblyReader(pexFile))
            {
                return reader.Read();
            }
        }

        public void Write(string outputFile)
        {
            writer.Write(outputFile);
        }
    }
}