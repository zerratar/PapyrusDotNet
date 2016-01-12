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

using System.Linq;
using Mono.Cecil;
using Mono.Collections.Generic;
using PapyrusDotNet.Common.Interfaces;
using FieldAttributes = PapyrusDotNet.Common.Papyrus.FieldAttributes;

#endregion

namespace PapyrusDotNet.Common.Utilities
{
    public class PapyrusAttributeReader : IPapyrusAttributeReader
    {
        private readonly IValueTypeConverter papyrusValueTypeConverter;

        public PapyrusAttributeReader(IValueTypeConverter papyrusValueTypeConverter)
        {
            this.papyrusValueTypeConverter = papyrusValueTypeConverter;
        }

        public string GetCustomAttributeValue(CustomAttribute varAttr)
        {
            var ctrArg = varAttr.ConstructorArguments.FirstOrDefault();
            if (ctrArg.Value != null)
            {
                if (ctrArg.Value is CustomAttributeArgument)
                {
                    var arg = (CustomAttributeArgument) ctrArg.Value;
                    var val = arg.Value;

                    return papyrusValueTypeConverter.Convert(arg.Type.Name, val).ToString();
                }
                return ctrArg.Value.ToString();
            }
            return null;
        }

        public FieldAttributes ReadPapyrusAttributes(TypeDefinition typeDef)
        {
            var attributes = ReadPapyrusAttributes(typeDef.CustomAttributes);

            if (typeDef.HasGenericParameters)
            {
                attributes.IsGeneric = true;
                var genericParameters = typeDef.GenericParameters;
            }

            foreach (var varAttr in typeDef.CustomAttributes)
            {
                if (varAttr.AttributeType.Name.Equals("InitialValueAttribute"))
                {
                    attributes.InitialValue = GetCustomAttributeValue(varAttr);
                }
            }
            return attributes;
        }

        public FieldAttributes ReadPapyrusAttributes(FieldDefinition fieldDef)
        {
            var attributes = ReadPapyrusAttributes(fieldDef.CustomAttributes);

            if (fieldDef.FieldType.Name == "T") attributes.IsGeneric = true;

            return attributes;
        }

        public FieldAttributes ReadPapyrusAttributes(PropertyDefinition propertyDef)
        {
            var attributes = ReadPapyrusAttributes(propertyDef.CustomAttributes);

            if (propertyDef.PropertyType.Name == "T") attributes.IsGeneric = true;

            return attributes;
        }

        public FieldAttributes ReadPapyrusAttributes(MethodDefinition methodDef)
        {
            var attributes = ReadPapyrusAttributes(methodDef.CustomAttributes);

            //if (variable.FieldType.Name == "T") attributes.IsGeneric = true;

            return attributes;
        }

        public FieldAttributes ReadPapyrusAttributes(Collection<CustomAttribute> customAttributes)
        {
            string initialValue = null, docString = null;
            bool isProperty = false,
                isAuto = false,
                isAutoReadOnly = false,
                isHidden = false,
                isConditional = false,
                isGeneric = false;

            foreach (var varAttr in customAttributes)
            {
                if (varAttr.AttributeType.Name.Equals("PropertyAttribute"))
                    isProperty = true;
                if (varAttr.AttributeType.Name.Equals("GenericMemberAttribute"))
                {
                    isGeneric = true;
                    foreach (var arg in varAttr.ConstructorArguments)
                    {
                        // Not implemented yet
                    }
                }
                if (varAttr.AttributeType.Name.Equals("DocStringAttribute"))
                {
                    docString = GetCustomAttributeValue(varAttr);
                }
                if (varAttr.AttributeType.Name.Equals("InitialValueAttribute"))
                {
                    var ctrArg = varAttr.ConstructorArguments.FirstOrDefault();
                    if (ctrArg.Value != null)
                    {
                        if (ctrArg.Value is CustomAttributeArgument)
                        {
                            var arg = (CustomAttributeArgument) ctrArg.Value;
                            var val = arg.Value;

                            initialValue = papyrusValueTypeConverter.Convert(arg.Type.Name, val).ToString();
                        }
                        else
                            initialValue = ctrArg.Value.ToString();
                    }
                }
                if (varAttr.AttributeType.Name.Equals("AutoAttribute"))
                    isAuto = true;

                if (varAttr.AttributeType.Name.Equals("AutoReadOnlyAttribute"))
                    isAutoReadOnly = true;

                if (varAttr.AttributeType.Name.Equals("HiddenAttribute"))
                    isHidden = true;

                if (varAttr.AttributeType.Name.Equals("ConditionalAttribute"))
                    isConditional = true;
            }
            return new FieldAttributes
            {
                IsGeneric = isGeneric,
                InitialValue = initialValue,
                DocString = docString,
                IsAuto = isAuto,
                IsAutoReadOnly = isAutoReadOnly,
                IsConditional = isConditional,
                IsHidden = isHidden,
                IsProperty = isProperty
            };
        }
    }
}