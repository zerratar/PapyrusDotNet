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

using PapyrusDotNet.PapyrusAssembly.Enums;
using PapyrusDotNet.PapyrusAssembly.Extensions;

#endregion

namespace PapyrusDotNet.PapyrusAssembly.Classes
{
    public class PapyrusPropertyDefinition : PapyrusPropertyReference
    {
        private readonly PapyrusAssemblyDefinition assembly;

        public PapyrusPropertyDefinition(PapyrusAssemblyDefinition assembly)
        {
            this.assembly = assembly;
        }

        public PapyrusPropertyDefinition(PapyrusAssemblyDefinition assembly, string name, string typeName)
            : base(new PapyrusStringRef(assembly, name), null, PapyrusPrimitiveType.None)
        {
            this.assembly = assembly;
            TypeName = typeName.Ref(assembly);
        }

        public PapyrusStringRef TypeName { get; set; }
        public PapyrusStringRef Documentation { get; set; }
        public int Userflags { get; set; }
        public byte Flags { get; set; }

        public bool IsAuto => (Flags & 4) > 0;
        public bool HasGetter => (Flags & 1) > 0;
        public bool HasSetter => (Flags & 2) > 0;


        public string AutoName { get; set; }
        public PapyrusMethodDefinition GetMethod { get; set; }
        public PapyrusMethodDefinition SetMethod { get; set; }
    }
}