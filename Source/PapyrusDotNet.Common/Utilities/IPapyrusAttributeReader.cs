using Mono.Cecil;
using Mono.Collections.Generic;
using PapyrusDotNet.Common.Interfaces;

namespace PapyrusDotNet.Common.Utilities
{
    public interface IPapyrusAttributeReader : IUtility
    {
        string GetCustomAttributeValue(CustomAttribute varAttr);
        Papyrus.FieldAttributes ReadPapyrusAttributes(TypeDefinition typeDef);
        Papyrus.FieldAttributes ReadPapyrusAttributes(FieldDefinition fieldDef);
        Papyrus.FieldAttributes ReadPapyrusAttributes(PropertyDefinition propertyDef);
        Papyrus.FieldAttributes ReadPapyrusAttributes(MethodDefinition methodDef);
        Papyrus.FieldAttributes ReadPapyrusAttributes(Collection<CustomAttribute> customAttributes);
    }
}