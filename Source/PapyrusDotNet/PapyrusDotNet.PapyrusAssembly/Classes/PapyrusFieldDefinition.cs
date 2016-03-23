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
//     Copyright 2016, Karl Patrik Johansson, zerratar@gmail.com

#region

using PapyrusDotNet.PapyrusAssembly.Extensions;

#endregion

namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusFieldDefinition : PapyrusMemberReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PapyrusFieldDefinition"/> class.
        /// </summary>
        /// <param name="declaringAssembly">The declaring assembly.</param>
        /// <param name="declaringType">Type of the declaring.</param>
        public PapyrusFieldDefinition(PapyrusAssemblyDefinition declaringAssembly, PapyrusTypeDefinition declaringType)
        {
            DeclaringType = declaringType;
            DeclaringAssembly = declaringAssembly;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PapyrusFieldDefinition"/> class.
        /// </summary>
        /// <param name="declaringAssembly">The declaring assembly.</param>
        /// <param name="declaringType">Type of the declaring.</param>
        /// <param name="name">The name.</param>
        /// <param name="typeName">Name of the type.</param>
        public PapyrusFieldDefinition(PapyrusAssemblyDefinition declaringAssembly, PapyrusTypeDefinition declaringType,
            string name, string typeName)
            : this(declaringAssembly, declaringType)
        {
            name = "::" + name.Replace('<', '_').Replace('>', '_');
            name = name.Replace("::::", "::");

            if (declaringType.IsStruct)
                name = name.Replace(":", "");
            Name = name.Ref(declaringAssembly);
            TypeName = typeName;
        }

        /// <summary>
        /// Gets the declaring assembly.
        /// </summary>
        public PapyrusAssemblyDefinition DeclaringAssembly { get; }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        public PapyrusVariableReference DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public PapyrusStringRef Name { get; set; }

        /// <summary>
        /// Gets or sets the user flags.
        /// </summary>
        public int UserFlags { get; set; }

        /// <summary>
        /// Gets or sets the type of the declaring.
        /// </summary>
        public PapyrusTypeDefinition DeclaringType { get; set; }

        /// <summary>
        /// Gets or sets the documentation.
        /// </summary>
        public string Documentation { get; set; }

        /// <summary>
        /// Gets or sets the name of the type.
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the flags.
        /// </summary>
        public byte Flags { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return Name.Value + " : " + TypeName;
            // return "FieldDef: " + TypeName + " " + Name;
        }
    }
}