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
using PapyrusDotNet.PapyrusAssembly.Enums;
using PapyrusDotNet.PapyrusAssembly.Structs;

#endregion

namespace PapyrusDotNet.PapyrusAssembly.Classes
{
    public class PapyrusHeader
    {
        public const uint Fallout4PapyrusHeaderIdentifier = 0xFA57C0DE;

        public PapyrusSourceHeader SourceHeader { get; set; } = new PapyrusSourceHeader();
        public Dictionary<string, object> UserflagReferenceHeader { get; set; } = new Dictionary<string, object>();
        public PapyrusVersionTargets VersionTarget { get; set; }
        public uint HeaderIdentifier { get; set; }
        public bool HasDescriptionTable { get; set; }
    }
}