using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PapyrusDotNet.Common;
using PapyrusDotNet.Converters.Clr2Papyrus.Enums;
using PapyrusDotNet.Converters.Clr2Papyrus.Exceptions;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly.Classes;
using PapyrusDotNet.PapyrusAssembly.Enums;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations.Processors
{
    public class PapyrusCallInstructionProcessor : IPapyrusInstructionProcessor
    {
        private readonly Clr2PapyrusInstructionProcessor mainInstructionProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PapyrusCallInstructionProcessor"/> class.
        /// </summary>
        /// <param name="clr2PapyrusInstructionProcessor">The CLR2 papyrus instruction processor.</param>
        public PapyrusCallInstructionProcessor(Clr2PapyrusInstructionProcessor clr2PapyrusInstructionProcessor)
        {
            mainInstructionProcessor = clr2PapyrusInstructionProcessor;
        }

        /// <summary>
        /// Parses the instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
        /// <exception cref="StackUnderflowException"></exception>
        public IEnumerable<PapyrusInstruction> ParseInstruction(Instruction instruction, MethodDefinition targetMethod, TypeDefinition targetType)
        {
            var processInstruction = new List<PapyrusInstruction>();
            var stack = mainInstructionProcessor.EvaluationStack;
            var methodRef = instruction.Operand as MethodReference;
            if (methodRef != null)
            {
                // What we need:
                // 1. Call Location (The name of the type that has this method)
                // 2. Method Name (The name of the method, duh)
                // 3. Method Parameters (The parameters that we need to pass to the method)
                // 4. Destination Variable (The variable needed to store the return value of the method)
                //      - Destination Variable must always exist, the difference between a function returning a object
                //        and a void, is that we "assign" the destination to a ::nonevar temp variable of type None (void)

                var name = methodRef.FullName;
                var itemsToPop = instruction.OpCode.StackBehaviourPop == StackBehaviour.Varpop
                    ? methodRef.Parameters.Count
                    : Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);

                if (stack.Count < itemsToPop)
                {
                    if (mainInstructionProcessor.PapyrusCompilerOptions == PapyrusCompilerOptions.Strict)
                    {
                        throw new StackUnderflowException(targetMethod, instruction);
                    }
                    return processInstruction;
                }

                // Check if the current invoked Method is inside the same instance of this type
                // by checking if it is using "this.Method(params);" << "this."...
                var isThisCall = false;
                var isStaticCall = false;
                var callerLocation = "";
                var parameters = new List<object>();

                if (methodRef.HasThis)
                {
                    callerLocation = "self";
                    isThisCall = true;
                }

                for (var paramIndex = 0; paramIndex < itemsToPop; paramIndex++)
                {
                    var parameter = stack.Pop();
                    if (parameter.IsThis && mainInstructionProcessor.EvaluationStack.Count > methodRef.Parameters.Count
                        || methodRef.CallingConvention == MethodCallingConvention.ThisCall)
                    {
                        isThisCall = true;
                        callerLocation = "self"; // Location: 'self' is the same as 'this'
                    }
                    parameters.Insert(0, parameter);
                }

                var methodDefinition = mainInstructionProcessor.TryResolveMethodReference(methodRef);
                if (methodDefinition != null)
                {
                    isStaticCall = methodDefinition.IsStatic;
                }

                if (methodDefinition == null)
                {
                    isStaticCall = name.Contains("::");
                }

                if (isStaticCall)
                {
                    callerLocation = name.Split("::")[0];
                }

                if (callerLocation.Contains("."))
                {
                    callerLocation = callerLocation.Split('.').LastOrDefault();
                }

                if (methodRef.Name.ToLower().Contains("concat"))
                {
                    processInstruction.AddRange(mainInstructionProcessor.ProcessStringConcat(instruction, methodRef, parameters));
                }
                else if (methodRef.Name.ToLower().Contains("op_equal") ||
                         methodRef.Name.ToLower().Contains("op_inequal"))
                {
                    // TODO: Add Equality comparison

                    mainInstructionProcessor.invertedBranch = methodRef.Name.ToLower().Contains("op_inequal");

                    if (!InstructionHelper.IsStore(instruction.Next.OpCode.Code))
                    {
                        mainInstructionProcessor.SkipToOffset = instruction.Next.Offset;
                        return processInstruction;
                    }
                    // EvaluationStack.Push(new EvaluationStackItem { IsMethodCall = true, Value = methodRef, TypeName = methodRef.ReturnType.FullName });

                    processInstruction.AddRange(mainInstructionProcessor.GetConditional(instruction, Code.Ceq));
                }
                else
                {
                    if (isStaticCall)
                    {
                        var destinationVariable = mainInstructionProcessor.GetTargetVariable(instruction, methodRef);
                        {
                            processInstruction.Add(mainInstructionProcessor.CreatePapyrusCallInstruction(PapyrusOpCode.Callstatic, methodRef,
                                callerLocation,
                                destinationVariable, parameters));
                            return processInstruction;

                        }
                    }
                    if (isThisCall)
                    {
                        var destinationVariable = mainInstructionProcessor.GetTargetVariable(instruction, methodRef);
                        {
                            processInstruction.Add(mainInstructionProcessor.CreatePapyrusCallInstruction(PapyrusOpCode.Callmethod, methodRef, callerLocation,
                                destinationVariable, parameters));
                            return processInstruction;
                        }
                    }
                }
            }
            return processInstruction;
        }
    }
}