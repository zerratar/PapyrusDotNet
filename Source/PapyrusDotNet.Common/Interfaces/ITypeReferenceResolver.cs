using System.Collections;
using System.Collections.Generic;
using Mono.Cecil;

namespace PapyrusDotNet.Common.Interfaces
{
    public interface ITypeReferenceResolver
    {
        TypeReference Resolve(ref IList<string> reservedNames, ref IList<TypeReference> referenceList, ModuleDefinition mainModule, TypeDefinition newType, string fallbackTypeName = null);
    }
}