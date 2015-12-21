using System.Linq;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace PapyrusDotNet.Common.Utilities
{
    public class PapyrusAttributeReader : IPapyrusAttributeReader
    {
        public PapyrusAttributeReader()
        {
        }

        public string GetCustomAttributeValue(CustomAttribute varAttr)
        {
            var ctrArg = varAttr.ConstructorArguments.FirstOrDefault();
            if (ctrArg.Value != null)
            {
                if (ctrArg.Value is CustomAttributeArgument)
                {
                    var arg = (CustomAttributeArgument)ctrArg.Value;
                    var val = arg.Value;

                    return ValueTypeConverter.Instance.Convert(arg.Type.Name, val).ToString();
                }
                return ctrArg.Value.ToString();
            }
            return null;
        }

        public Papyrus.FieldAttributes ReadPapyrusAttributes(TypeDefinition typeDef)
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

        public Papyrus.FieldAttributes ReadPapyrusAttributes(FieldDefinition fieldDef)
        {
            var attributes = ReadPapyrusAttributes(fieldDef.CustomAttributes);

            if (fieldDef.FieldType.Name == "T") attributes.IsGeneric = true;

            return attributes;
        }

        public Papyrus.FieldAttributes ReadPapyrusAttributes(PropertyDefinition propertyDef)
        {
            var attributes = ReadPapyrusAttributes(propertyDef.CustomAttributes);

            if (propertyDef.PropertyType.Name == "T") attributes.IsGeneric = true;

            return attributes;
        }

        public Papyrus.FieldAttributes ReadPapyrusAttributes(MethodDefinition methodDef)
        {
            var attributes = ReadPapyrusAttributes(methodDef.CustomAttributes);

            //if (variable.FieldType.Name == "T") attributes.IsGeneric = true;

            return attributes;
        }

        public Papyrus.FieldAttributes ReadPapyrusAttributes(Collection<CustomAttribute> customAttributes)
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
                            var arg = (CustomAttributeArgument)ctrArg.Value;
                            var val = arg.Value;

                            initialValue = ValueTypeConverter.Instance.Convert(arg.Type.Name, val).ToString();
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
            return new Papyrus.FieldAttributes
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