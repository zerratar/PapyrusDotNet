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
//     Copyright 2015, Karl Patrik Johansson, zerratar@gmail.com

using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using PapyrusDotNet.Common;
using PapyrusDotNet.Converters.Clr2Papyrus.Enums;
using PapyrusDotNet.Converters.Clr2Papyrus.Exceptions;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Extensions;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations.Processors
{
    public class CallInstructionProcessor : IInstructionProcessor
    {
        private readonly IClr2PapyrusInstructionProcessor mainInstructionProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallInstructionProcessor"/> class.
        /// </summary>
        /// <param name="clr2PapyrusInstructionProcessor">The CLR2 papyrus instruction processor.</param>
        public CallInstructionProcessor(IClr2PapyrusInstructionProcessor clr2PapyrusInstructionProcessor)
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
        public IEnumerable<PapyrusInstruction> Process(Instruction instruction, MethodDefinition targetMethod, TypeDefinition targetType)
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

                if (methodDefinition != null && methodDefinition.IsConstructor) return processInstruction;

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

                    mainInstructionProcessor.InvertedBranch = methodRef.Name.ToLower().Contains("op_inequal");

                    if (!InstructionHelper.IsStore(instruction.Next.OpCode.Code))
                    {
                        mainInstructionProcessor.SkipToOffset = instruction.Next.Offset;
                        return processInstruction;
                    }
                    // EvaluationStack.Push(new EvaluationStackItem { IsMethodCall = true, Value = methodRef, TypeName = methodRef.ReturnType.FullName });

                    processInstruction.AddRange(mainInstructionProcessor.ProcessConditionalInstruction(instruction, Code.Ceq));
                }
                else
                {
                    if (methodRef.Name.ToLower().Contains("get_") || methodRef.Name.ToLower().Contains("set_"))
                    {
                        processInstruction.AddRange(ProcessPropertyAccess(instruction, methodRef, methodDefinition, parameters));

                        return processInstruction;
                    }


                    if (isStaticCall)
                    {
                        var destinationVariable = mainInstructionProcessor.GetTargetVariable(instruction, methodRef);
                        {
                            processInstruction.Add(CreatePapyrusCallInstruction(PapyrusOpCode.Callstatic, methodRef,
                                callerLocation,
                                destinationVariable, parameters));
                            return processInstruction;

                        }
                    }
                    if (isThisCall)
                    {
                        var destinationVariable = mainInstructionProcessor.GetTargetVariable(instruction, methodRef);
                        {
                            processInstruction.Add(CreatePapyrusCallInstruction(PapyrusOpCode.Callmethod, methodRef, callerLocation,
                                destinationVariable, parameters));
                            return processInstruction;
                        }
                    }
                }
            }
            return processInstruction;
        }

        private IEnumerable<PapyrusInstruction> ProcessPropertyAccess(Instruction instruction, MethodReference methodRef, MethodDefinition methodDefinition,
            List<object> parameters)
        {
            var instructions = new List<PapyrusInstruction>();

            if (methodRef is MethodDefinition)
            {
                methodDefinition = methodRef as MethodDefinition;
            }
            if (methodDefinition != null)
            {
                var matchingProperty = mainInstructionProcessor.PapyrusType.Properties.FirstOrDefault(
                    p => p.SetMethod.Name.Value.ToLower().Equals(methodRef.Name.ToLower()) ||
                         p.GetMethod.Name.Value.ToLower().Equals(methodRef.Name.ToLower()));
                if (matchingProperty != null)
                {
                    if (methodDefinition.IsSetter)
                    {
                        var param = parameters;

                        instructions.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCode.PropSet,
                                mainInstructionProcessor.CreateVariableReferenceFromName(matchingProperty.Name.Value),
                                mainInstructionProcessor.CreateVariableReferenceFromName("self"),
                                param.First()
                            ));
                    }
                    else if (methodDefinition.IsGetter)
                    {
                        var destinationVariable = mainInstructionProcessor.GetTargetVariable(instruction, methodRef);

                        instructions.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCode.PropSet,
                                mainInstructionProcessor.CreateVariableReferenceFromName(matchingProperty.Name.Value),
                                mainInstructionProcessor.CreateVariableReferenceFromName("self"),
                                 mainInstructionProcessor.CreateVariableReferenceFromName(destinationVariable)
                            ));
                    }
                }
            }
            return instructions;
        }

        /// <summary>
        /// Creates a papyrus call instruction.
        /// </summary>
        /// <param name="callOpCode">The call op code.</param>
        /// <param name="methodRef">The method reference.</param>
        /// <param name="callerLocation">The caller location.</param>
        /// <param name="destinationVariable">The destination variable.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public PapyrusInstruction CreatePapyrusCallInstruction(PapyrusOpCode callOpCode, MethodReference methodRef, string callerLocation, string destinationVariable, List<object> parameters)
        {
            var inst = new PapyrusInstruction { OpCode = callOpCode };
            if (callOpCode == PapyrusOpCode.Callstatic)
            {
                inst.Arguments.AddRange(mainInstructionProcessor.ParsePapyrusParameters(new object[] {
                mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference, callerLocation),
                mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference, methodRef.Name),
                mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference, destinationVariable)}));
            }
            else
            {
                inst.Arguments.AddRange(mainInstructionProcessor.ParsePapyrusParameters(new object[] {
                mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference, methodRef.Name),
                mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference, callerLocation),
                mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference, destinationVariable)}));
            }
            inst.OperandArguments.AddRange(EnsureParameterTypes(methodRef.Parameters, mainInstructionProcessor.ParsePapyrusParameters(parameters.ToArray())));
            inst.Operand = methodRef;
            return inst;
        }

        /// <summary>
        /// Ensures the parameter types.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="papyrusParams">The papyrus parameters.</param>
        /// <returns></returns>
        public IEnumerable<PapyrusVariableReference> EnsureParameterTypes(Collection<ParameterDefinition> parameters, List<PapyrusVariableReference> papyrusParams)
        {
            var varRefs = new List<PapyrusVariableReference>();
            var i = 0;
            foreach (var p in parameters)
            {
                var papyrusReturnType = Utility.GetPapyrusReturnType(p.ParameterType);
                if (p.ParameterType.IsValueType
                    && Utility.PapyrusValueTypeToString(papyrusParams[i].ValueType) != papyrusReturnType
                    && papyrusParams[i].ValueType != PapyrusPrimitiveType.Reference)
                {
                    papyrusParams[i].TypeName = papyrusReturnType.Ref(mainInstructionProcessor.PapyrusAssembly);
                    papyrusParams[i].ValueType = Utility.GetPrimitiveTypeFromType(p.ParameterType);
                    papyrusParams[i].Value = Utility.TypeValueConvert(papyrusReturnType, papyrusParams[i].Value);
                    varRefs.Add(papyrusParams[i]);
                }
                else
                {
                    varRefs.Add(papyrusParams[i]);
                }
                i++;
            }

            return varRefs;
        }

    }
}