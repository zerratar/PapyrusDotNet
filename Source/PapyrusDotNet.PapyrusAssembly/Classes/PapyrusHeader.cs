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

#endregion

namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusHeader
    {
        public const uint Fallout4PapyrusHeaderIdentifier = 0xFA57C0DE;
        public const uint SkyrimPapyrusHeaderIdentifier = 0xDEC057FA;
        public static readonly Version SkyrimPapyrusVersion = new Version(3, 2);
        public static readonly Version Fallout4PapyrusVersion = new Version(3, 9);
        private readonly PapyrusAssemblyDefinition assembly;

        public PapyrusHeader(PapyrusAssemblyDefinition assembly)
        {
            this.assembly = assembly;
            UserflagReferenceHeader = new PapyrusHeaderUserflagCollection(assembly);
        }

        public PapyrusSourceHeader SourceHeader { get; set; } = new PapyrusSourceHeader();
        public PapyrusHeaderUserflagCollection UserflagReferenceHeader { get; set; }
        public uint HeaderIdentifier { get; set; }
    }
}