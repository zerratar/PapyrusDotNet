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

using PapyrusDotNet.PapyrusAssembly.Extensions;

namespace PapyrusDotNet.PapyrusAssembly.Classes
{
    public class PapyrusFieldDefinition : PapyrusFieldReference
    {
        private readonly PapyrusAssemblyDefinition assembly;

        public PapyrusStringRef Name { get; set; }
        public int UserFlags { get; set; }
        public PapyrusTypeDefinition DeclaringType { get; set; }
        public string Documentation { get; set; }
        internal string TypeName { get; set; }
        public bool IsConst { get; set; }

        public PapyrusFieldDefinition(PapyrusAssemblyDefinition assembly)
        {
            this.assembly = assembly;
        }

        public PapyrusFieldDefinition(PapyrusAssemblyDefinition assembly, string name, string typeName)
            : this(assembly)
        {
            Name = name.Ref(assembly);
            TypeName = typeName;
        }
    }
}