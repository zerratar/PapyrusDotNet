using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PapyrusDotNet.Common;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly.Classes;
using PapyrusDotNet.PapyrusAssembly.Enums;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations.Processors
{
    public class PapyrusBranchInstructionProcessor : IPapyrusInstructionProcessor
    {
        private readonly Clr2PapyrusInstructionProcessor mainInstructionProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PapyrusBranchInstructionProcessor"/> class.
        /// </summary>
        /// <param name="mainInstructionProcessor">The main instruction processor.</param>
        public PapyrusBranchInstructionProcessor(Clr2PapyrusInstructionProcessor mainInstructionProcessor)
        {
            this.mainInstructionProcessor = mainInstructionProcessor;
        }

        /// <summary>
        /// Parses the instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public IEnumerable<PapyrusInstruction> Process(Instruction instruction, MethodDefinition targetMethod, TypeDefinition type)
        {
            List<PapyrusInstruction> instructions = new List<PapyrusInstruction>();
            if (InstructionHelper.IsBranchConditional(instruction.OpCode.Code))
            {
                var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
                if (mainInstructionProcessor.EvaluationStack.Count >= popCount)
                {
                    var obj1 = mainInstructionProcessor.EvaluationStack.Pop();
                    var obj2 = mainInstructionProcessor.EvaluationStack.Pop();
                    // gets or create a temp boolean variable we can use to store the conditional check on.
                    var temp = mainInstructionProcessor.GetTargetVariable(instruction, null, "Bool");

                    var allVars = mainInstructionProcessor.PapyrusMethod.GetVariables();

                    var tempVar = allVars.FirstOrDefault(v => v.Name.Value == temp);

                    var destinationInstruction = instruction.Operand;

                    if (InstructionHelper.IsBranchConditionalEq(instruction.OpCode.Code))
                        instructions.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCode.CmpEq, tempVar, obj1, obj2));
                    else if (InstructionHelper.IsBranchConditionalLt(instruction.OpCode.Code))
                        instructions.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCode.CmpLt, tempVar, obj1, obj2));
                    else if (InstructionHelper.IsBranchConditionalGt(instruction.OpCode.Code))
                        instructions.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCode.CmpGt, tempVar, obj1, obj2));
                    else if (InstructionHelper.IsBranchConditionalGe(instruction.OpCode.Code))
                        instructions.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCode.CmpGte, tempVar, obj1, obj2));
                    else if (InstructionHelper.IsBranchConditionalGe(instruction.OpCode.Code))
                        instructions.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCode.CmpLte, tempVar, obj1, obj2));

                    instructions.Add(mainInstructionProcessor.ConditionalJump(PapyrusOpCode.Jmpt, tempVar, destinationInstruction));
                    return instructions;
                }
            }

            if (InstructionHelper.IsBranch(instruction.OpCode.Code))
            {
                var stack = mainInstructionProcessor.EvaluationStack;
                var targetInstruction = instruction.Operand;

                if (stack.Count > 0)
                {
                    var conditionalVariable = stack.Pop();
                    if (InstructionHelper.IsBranchTrue(instruction.OpCode.Code))
                    {
                        var jmpOp = mainInstructionProcessor.TryInvertJump(PapyrusOpCode.Jmpt);
                        var jmp = mainInstructionProcessor.CreatePapyrusInstruction(jmpOp, conditionalVariable, targetInstruction);
                        jmp.Operand = targetInstruction;
                        instructions.Add(jmp);
                        return instructions;
                    }
                    if (InstructionHelper.IsBranchFalse(instruction.OpCode.Code))
                    {
                        var jmpOp = mainInstructionProcessor.TryInvertJump(PapyrusOpCode.Jmpf);
                        var jmp = mainInstructionProcessor.CreatePapyrusInstruction(jmpOp, conditionalVariable, targetInstruction);
                        jmp.Operand = targetInstruction;
                        instructions.Add(jmp);
                        return instructions;
                    }
                }

                var jmpInst = mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCode.Jmp, targetInstruction);
                jmpInst.Operand = targetInstruction;
                instructions.Add(jmpInst);
            }
            return instructions;
        }
    }
}