using Mono.Cecil;
using Mono.Collections.Generic;

namespace PapyrusDotNet.Common.Interfaces
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