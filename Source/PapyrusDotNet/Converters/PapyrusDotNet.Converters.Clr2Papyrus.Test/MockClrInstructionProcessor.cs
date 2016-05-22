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

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using PapyrusDotNet.Common;
using PapyrusDotNet.Converters.Clr2Papyrus.Enums;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Converters.Clr2Papyrus.Test
{
    public class MockClrInstructionProcessor : IClrInstructionProcessor
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

        public IEnumerable<PapyrusInstruction> ProcessInstructions(
            IEnumerable<PapyrusAssemblyDefinition> papyrusAssemblyCollection, IDelegatePairDefinition delegatePairDef,
            PapyrusAssemblyDefinition targetPapyrusAssembly, PapyrusTypeDefinition targetPapyrusType,
            PapyrusMethodDefinition targetPapyrusMethod, MethodDefinition method, MethodBody body,
            Collection<Instruction> instructions,
            PapyrusCompilerOptions options)
        {
            throw new NotImplementedException();
        }

        public string GetTargetVariable(Instruction instruction, MethodReference methodRef, out bool isStructAccess,
            string fallbackType = null, bool forceNew = false)
        {
            throw new NotImplementedException();
        }

        public PapyrusInstruction CreatePapyrusInstruction(PapyrusOpCodes papyrusOpCode, params object[] values)
        {
            throw new NotImplementedException();
        }

        public PapyrusInstruction ConditionalJump(PapyrusOpCodes jumpType, PapyrusVariableReference conditionalVar,
            object destinationInstruction)
        {
            throw new NotImplementedException();
        }

        public PapyrusOpCodes TryInvertJump(PapyrusOpCodes jmpt)
        {
            throw new NotImplementedException();
        }

        public MethodDefinition TryResolveMethodReference(MethodReference methodRef)
        {
            return null;
        }

        public List<PapyrusInstruction> ProcessStringConcat(Instruction instruction, MethodReference methodRef,
            List<object> parameters)
        {
            return new List<PapyrusInstruction>();
        }

        public IEnumerable<PapyrusInstruction> ProcessConditionalInstruction(Instruction instruction,
            Code overrideOpCode = Code.Nop,
            string tempVariable = null)
        {
            return new List<PapyrusInstruction>();
        }

        public PapyrusVariableReference CreateVariableReference(PapyrusPrimitiveType papyrusPrimitiveType, object value)
        {
            return new PapyrusVariableReference {Type = papyrusPrimitiveType, Value = value};
        }

        public PapyrusVariableReference CreateVariableReferenceFromName(string varName)
        {
            return new PapyrusVariableReference {Type = PapyrusPrimitiveType.Reference, Value = varName};
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
            return new PapyrusInstruction {OpCode = PapyrusOpCodes.Cast};
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
            return values.Select(i => new PapyrusVariableReference {Value = i}).ToList();
        }

        public IDelegatePairDefinition GetDelegatePairDefinition()
        {
            throw new NotImplementedException();
        }

        public PapyrusFieldDefinition GetDelegateField(FieldReference fieldRef)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PapyrusInstruction> ProcessInstructions(PapyrusAssemblyDefinition targetPapyrusAssembly,
            PapyrusTypeDefinition targetPapyrusType,
            PapyrusMethodDefinition targetPapyrusMethod, MethodDefinition method, MethodBody body,
            Collection<Instruction> instructions,
            PapyrusCompilerOptions options)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PapyrusInstruction> ProcessInstructions(
            IEnumerable<PapyrusAssemblyDefinition> papyrusAssemblyCollection,
            PapyrusAssemblyDefinition targetPapyrusAssembly,
            PapyrusTypeDefinition targetPapyrusType, PapyrusMethodDefinition targetPapyrusMethod,
            MethodDefinition method,
            MethodBody body, Collection<Instruction> instructions, PapyrusCompilerOptions options)
        {
            throw new NotImplementedException();
        }

        public string GetTargetVariable(Instruction instruction, MethodReference methodRef, string fallbackType = null,
            bool forceNew = false)
        {
            throw new NotImplementedException();
        }
    }
}