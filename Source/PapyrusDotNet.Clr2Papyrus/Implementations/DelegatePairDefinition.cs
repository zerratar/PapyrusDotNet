using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations
{
    public class DelegatePairDefinition : IDelegatePairDefinition
    {
        public Dictionary<MethodDefinition, List<FieldDefinition>> DelegateMethodFieldPair { get; set; } = new Dictionary<MethodDefinition, List<FieldDefinition>>();
        public Dictionary<MethodDefinition, List<VariableReference>> DelegateMethodLocalPair { get; set; } = new Dictionary<MethodDefinition, List<VariableReference>>();
        public List<MethodDefinition> DelegateMethodDefinitions { get; set; } = new List<MethodDefinition>();
        public List<FieldDefinition> DelegateFields { get; set; } = new List<FieldDefinition>();
        public List<TypeDefinition> DelegateTypeDefinitions { get; set; } = new List<TypeDefinition>();
    }
}