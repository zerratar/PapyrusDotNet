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

using PapyrusDotNet.PapyrusAssembly.Extensions;

#endregion

namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusFieldDefinition : PapyrusFieldReference
    {
        public readonly PapyrusAssemblyDefinition DeclaringAssembly;

        public PapyrusFieldDefinition(PapyrusAssemblyDefinition declaringAssembly, PapyrusTypeDefinition declaringType)
        {
            DeclaringType = declaringType;
            this.DeclaringAssembly = declaringAssembly;
        }

        public PapyrusFieldDefinition(PapyrusAssemblyDefinition declaringAssembly, PapyrusTypeDefinition declaringType, string name, string typeName)
            : this(declaringAssembly, declaringType)
        {
            name = "::" + name.Replace('<', '_').Replace('>', '_');
            name = name.Replace("::::", "::");
            Name = name.Ref(declaringAssembly);
            TypeName = typeName;
        }

        public PapyrusStringRef Name { get; set; }
        public int UserFlags { get; set; }
        public PapyrusTypeDefinition DeclaringType { get; set; }
        public string Documentation { get; set; }
        public string TypeName { get; set; }
        public byte Flags { get; set; }

        public override string ToString()
        {
            return Name.Value + " : " + TypeName;
            // return "FieldDef: " + TypeName + " " + Name;
        }
    }
}