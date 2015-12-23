using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using PapyrusDotNet.Common;
using PapyrusDotNet.Converters.Clr2Papyrus.Enums;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Test
{
    public class MockClr2PapyrusInstructionProcessor : IClr2PapyrusInstructionProcessor
    {
        public Stack<EvaluationStackItem> EvaluationStack { get; set; }
        public PapyrusMethodDefinition PapyrusMethod { get; set; }
        public PapyrusAssemblyDefinition PapyrusAssembly { get; set; }
        public PapyrusTypeDefinition PapyrusType { get; set; }
        public PapyrusCompilerOptions PapyrusCompilerOptions { get; set; }
        public bool SkipNextInstruction { get; set; }
        public int SkipToOffset { get; set; }
        public bool InvertedBranch { get; set; }
        public PapyrusVariableReference SwitchConditionalComparer { get; set; }

        public IEnumerable<PapyrusInstruction> ProcessInstructions(PapyrusAssemblyDefinition targetPapyrusAssembly, PapyrusTypeDefinition targetPapyrusType,
            PapyrusMethodDefinition targetPapyrusMethod, MethodDefinition method, MethodBody body, Collection<Instruction> instructions,
            PapyrusCompilerOptions options)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<PapyrusInstruction> ProcessInstructions(IEnumerable<PapyrusAssemblyDefinition> papyrusAssemblyCollection, PapyrusAssemblyDefinition targetPapyrusAssembly,
            PapyrusTypeDefinition targetPapyrusType, PapyrusMethodDefinition targetPapyrusMethod, MethodDefinition method,
            MethodBody body, Collection<Instruction> instructions, PapyrusCompilerOptions options)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<PapyrusInstruction> ProcessInstructions(IEnumerable<PapyrusAssemblyDefinition> papyrusAssemblyCollection, IDelegatePairDefinition delegatePairDef,
            PapyrusAssemblyDefinition targetPapyrusAssembly, PapyrusTypeDefinition targetPapyrusType,
            PapyrusMethodDefinition targetPapyrusMethod, MethodDefinition method, MethodBody body, Collection<Instruction> instructions,
            PapyrusCompilerOptions options)
        {
            throw new System.NotImplementedException();
        }

        public string GetTargetVariable(Instruction instruction, MethodReference methodRef, out bool isStructAccess,
            string fallbackType = null, bool forceNew = false)
        {
            throw new System.NotImplementedException();
        }

        public string GetTargetVariable(Instruction instruction, MethodReference methodRef, string fallbackType = null,
            bool forceNew = false)
        {
            throw new System.NotImplementedException();
        }

        public PapyrusInstruction CreatePapyrusInstruction(PapyrusOpCodes papyrusOpCode, params object[] values)
        {
            throw new System.NotImplementedException();
        }

        public PapyrusInstruction ConditionalJump(PapyrusOpCodes jumpType, PapyrusVariableReference conditionalVar,
            object destinationInstruction)
        {
            throw new System.NotImplementedException();
        }

        public PapyrusOpCodes TryInvertJump(PapyrusOpCodes jmpt)
        {
            throw new System.NotImplementedException();
        }

        public MethodDefinition TryResolveMethodReference(MethodReference methodRef)
        {
            return null;
        }

        public List<PapyrusInstruction> ProcessStringConcat(Instruction instruction, MethodReference methodRef, List<object> parameters)
        {
            return new List<PapyrusInstruction>();
        }

        public IEnumerable<PapyrusInstruction> ProcessConditionalInstruction(Instruction instruction, Code overrideOpCode = Code.Nop,
            string tempVariable = null)
        {
            return new List<PapyrusInstruction>();
        }

        public PapyrusVariableReference CreateVariableReference(PapyrusPrimitiveType papyrusPrimitiveType, object value)
        {
            return new PapyrusVariableReference() { ValueType = papyrusPrimitiveType, Value = value };
        }

        public PapyrusVariableReference CreateVariableReferenceFromName(string varName)
        {
            return new PapyrusVariableReference() { ValueType = PapyrusPrimitiveType.Reference, Value = varName };
        }

        public Instruction GetNextStoreLocalVariableInstruction(Instruction input, out int varIndex)
        {
            varIndex = 0;
            return null;
        }

        public object GetNumericValue(Instruction instruction)
        {
            return 0;
        }

        public PapyrusInstruction CreatePapyrusCastInstruction(string destinationVariable,
            PapyrusVariableReference variableToCast)
        {
            return new PapyrusInstruction() { OpCode = PapyrusOpCodes.Cast };
        }

        public PapyrusFieldDefinition GetFieldFromStfld(Instruction whereToPlace)
        {
            return null;
        }

        public PapyrusVariableReference CreateTempVariable(string variableName, MethodReference methodRef = null)
        {
            return CreateVariableReferenceFromName(variableName + "1");
        }

        public List<PapyrusVariableReference> ParsePapyrusParameters(object[] values)
        {
            return values.Select(i => new PapyrusVariableReference() { Value = i }).ToList();
        }
    }
}