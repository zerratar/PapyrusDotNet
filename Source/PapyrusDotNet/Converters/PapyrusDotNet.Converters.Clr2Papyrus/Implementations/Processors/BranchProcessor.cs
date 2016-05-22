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

using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PapyrusDotNet.Common;
using PapyrusDotNet.Common.Utilities;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations.Processors
{
    public interface IBranchProcessor : ISubInstructionProcessor { }

    public class BranchProcessor : IBranchProcessor
    {
        /// <summary>
        /// Parses the instruction.
        /// </summary>
        /// <param name="mainProcessor">The main instruction processor.</param>
        /// <param name="asmCollection">The papyrus assembly collection.</param>
        /// <param name="instruction">The instruction.</param>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public IEnumerable<PapyrusInstruction> Process(
            IClrInstructionProcessor mainProcessor,
            IReadOnlyCollection<PapyrusAssemblyDefinition> asmCollection, 
            Instruction instruction,
            MethodDefinition targetMethod, TypeDefinition type)
        {
            bool isStructAccess;
            var instructions = new List<PapyrusInstruction>();
            if (InstructionHelper.IsBranchConditional(instruction.OpCode.Code))
            {
                var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
                if (mainProcessor.EvaluationStack.Count >= popCount)
                {
                    var obj1 = mainProcessor.EvaluationStack.Pop();
                    var obj2 = mainProcessor.EvaluationStack.Pop();
                    // gets or create a temp boolean variable we can use to store the conditional check on.
                    var temp = mainProcessor.GetTargetVariable(instruction, null, out isStructAccess, "Bool");

                    var allVars = mainProcessor.PapyrusMethod.GetVariables();

                    var tempVar = allVars.FirstOrDefault(v => v.Name.Value == temp);

                    var destinationInstruction = instruction.Operand;

                    if (InstructionHelper.IsBranchConditionalEq(instruction.OpCode.Code))
                        instructions.Add(mainProcessor.CreatePapyrusInstruction(PapyrusOpCodes.CmpEq, tempVar,
                            obj1, obj2));
                    else if (InstructionHelper.IsBranchConditionalLt(instruction.OpCode.Code))
                        instructions.Add(mainProcessor.CreatePapyrusInstruction(PapyrusOpCodes.CmpLt, tempVar,
                            obj1, obj2));
                    else if (InstructionHelper.IsBranchConditionalGt(instruction.OpCode.Code))
                        instructions.Add(mainProcessor.CreatePapyrusInstruction(PapyrusOpCodes.CmpGt, tempVar,
                            obj1, obj2));
                    else if (InstructionHelper.IsBranchConditionalGe(instruction.OpCode.Code))
                        instructions.Add(mainProcessor.CreatePapyrusInstruction(PapyrusOpCodes.CmpGte,
                            tempVar, obj1, obj2));
                    else if (InstructionHelper.IsBranchConditionalGe(instruction.OpCode.Code))
                        instructions.Add(mainProcessor.CreatePapyrusInstruction(PapyrusOpCodes.CmpLte,
                            tempVar, obj1, obj2));

                    instructions.Add(mainProcessor.ConditionalJump(PapyrusOpCodes.Jmpt, tempVar,
                        destinationInstruction));
                    return instructions;
                }
            }

            if (InstructionHelper.IsBranch(instruction.OpCode.Code))
            {
                var stack = mainProcessor.EvaluationStack;
                var targetInstruction = instruction.Operand;

                if (stack.Count > 0)
                {
                    var conditionalVariable = stack.Pop();
                    if (InstructionHelper.IsBranchTrue(instruction.OpCode.Code))
                    {
                        var jmpOp = mainProcessor.TryInvertJump(PapyrusOpCodes.Jmpt);
                        var jmp = mainProcessor.CreatePapyrusInstruction(jmpOp, conditionalVariable,
                            targetInstruction);
                        jmp.Operand = targetInstruction;
                        instructions.Add(jmp);
                        return instructions;
                    }
                    if (InstructionHelper.IsBranchFalse(instruction.OpCode.Code))
                    {
                        var jmpOp = mainProcessor.TryInvertJump(PapyrusOpCodes.Jmpf);
                        var jmp = mainProcessor.CreatePapyrusInstruction(jmpOp, conditionalVariable,
                            targetInstruction);
                        jmp.Operand = targetInstruction;
                        instructions.Add(jmp);
                        return instructions;
                    }
                }

                var jmpInst = mainProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Jmp, targetInstruction);
                jmpInst.Operand = targetInstruction;
                instructions.Add(jmpInst);
            }
            return instructions;
        }
    }
}