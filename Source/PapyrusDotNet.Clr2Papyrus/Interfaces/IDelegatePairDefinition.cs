using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Interfaces
{
    public interface IDelegatePairDefinition
    {
        Dictionary<MethodDefinition, List<FieldDefinition>> DelegateMethodFieldPair { get; set; }
        Dictionary<MethodDefinition, List<VariableReference>> DelegateMethodLocalPair { get; set; }

        List<TypeDefinition> DelegateTypeDefinitions { get; set; }
        List<FieldDefinition> DelegateFields { get; set; }
        List<MethodDefinition> DelegateMethodDefinitions { get; set; }
    }
}