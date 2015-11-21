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
using System.Collections.ObjectModel;
using PapyrusDotNet.PapyrusAssembly.Implementations;

#endregion

namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusTypeDefinition : PapyrusTypeReference
    {
        public PapyrusTypeDefinition()
        {
            Fields = new Collection<PapyrusFieldDefinition>();
            NestedTypes = new Collection<PapyrusTypeDefinition>();
            Properties = new Collection<PapyrusPropertyDefinition>();
            States = new Collection<PapyrusStateDefinition>();
        }

        public int Size { get; set; }
        public string ParentClass { get; set; }
        public byte ConstFlag { get; set; }
        public string Documentation { get; set; }
        public int UserFlags { get; set; }
        public string AutoStateName { get; set; }
        public bool IsClass { get; set; }
        public bool IsStruct { get; set; }
        public Collection<PapyrusFieldDefinition> Fields { get; set; }
        public Collection<PapyrusTypeDefinition> NestedTypes { get; set; }
        public Collection<PapyrusPropertyDefinition> Properties { get; set; }
        public Collection<PapyrusStateDefinition> States { get; set; }
    }
}