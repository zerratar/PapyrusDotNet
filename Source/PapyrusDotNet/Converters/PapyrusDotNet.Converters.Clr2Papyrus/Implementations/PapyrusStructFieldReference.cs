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

using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations
{
    public class PapyrusStructFieldReference : PapyrusFieldDefinition
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PapyrusStructFieldReference" /> class.
        /// </summary>
        /// <param name="declaringAssembly">The assembly.</param>
        /// <param name="declaringType">(OPTIONAL) The declaring typedefinition. You can pass NULL</param>
        public PapyrusStructFieldReference(PapyrusAssemblyDefinition declaringAssembly,
            PapyrusTypeDefinition declaringType) : base(declaringAssembly, declaringType)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PapyrusStructFieldReference" /> class.
        /// </summary>
        /// <param name="declaringAssembly">The assembly.</param>
        /// <param name="declaringType">(OPTIONAL) The declaring typedefinition. You can pass NULL</param>
        /// <param name="name">The name.</param>
        /// <param name="typeName">Name of the type.</param>
        public PapyrusStructFieldReference(PapyrusAssemblyDefinition declaringAssembly,
            PapyrusTypeDefinition declaringType, string name, string typeName)
            : base(declaringAssembly, declaringType, name, typeName)
        {
        }

        public object StructSource { get; set; }
        public PapyrusVariableReference StructVariable { get; set; }

        public override string ToString()
        {
            return "StructRef: " + StructSource + " -> " + StructVariable;
        }
    }
}