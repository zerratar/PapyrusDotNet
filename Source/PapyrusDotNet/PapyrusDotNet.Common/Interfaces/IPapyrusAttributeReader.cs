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

using Mono.Cecil;
using Mono.Collections.Generic;
using FieldAttributes = PapyrusDotNet.Common.Papyrus.FieldAttributes;

#endregion

namespace PapyrusDotNet.Common.Interfaces
{
    public interface IPapyrusAttributeReader : IUtility
    {
        string GetCustomAttributeValue(CustomAttribute varAttr);
        FieldAttributes ReadPapyrusAttributes(TypeDefinition typeDef);
        FieldAttributes ReadPapyrusAttributes(FieldDefinition fieldDef);
        FieldAttributes ReadPapyrusAttributes(PropertyDefinition propertyDef);
        FieldAttributes ReadPapyrusAttributes(MethodDefinition methodDef);
        FieldAttributes ReadPapyrusAttributes(Collection<CustomAttribute> customAttributes);
    }
}