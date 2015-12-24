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
using Mono.Cecil;
using Mono.Cecil.Cil;
using PapyrusDotNet.Common;
using PapyrusDotNet.Common.Utilities;
using PapyrusDotNet.Converters.Clr2Papyrus.Enums;
using PapyrusDotNet.Converters.Clr2Papyrus.Exceptions;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Extensions;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations.Processors
{
    public class ConditionalInstructionProcessor : IInstructionProcessor
    {
        private readonly IClr2PapyrusInstructionProcessor mainInstructionProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalInstructionProcessor"/> class.
        /// </summary>
        /// <param name="clr2PapyrusInstructionProcessor">The CLR2 papyrus instruction processor.</param>
        public ConditionalInstructionProcessor(IClr2PapyrusInstructionProcessor clr2PapyrusInstructionProcessor)
        {
            mainInstructionProcessor = clr2PapyrusInstructionProcessor;
        }

        /// <summary>
        /// Processes the conditional instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="overrideOpCode">The override op code.</param>
        /// <param name="tempVariable">The temporary variable.</param>
        /// <returns></returns>
        public IEnumerable<PapyrusInstruction> Process(Instruction instruction, Code overrideOpCode = Code.Nop, string tempVariable = null)
        {
            bool isStructAccess;
            var output = new List<PapyrusInstruction>();

            //cast = null;

            var heapStack = mainInstructionProcessor.EvaluationStack;

            // TODO: GetConditional only applies on Integers and must add support for Float further on.

            var papyrusOpCode = Utility.GetPapyrusMathOrEqualityOpCode(overrideOpCode != Code.Nop ? overrideOpCode : instruction.OpCode.Code, false);

            if (heapStack.Count >= 2) //Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
            {
                var numeratorObject = heapStack.Pop();
                var denumeratorObject = heapStack.Pop();
                var vars = mainInstructionProcessor.PapyrusMethod.GetVariables();
                int varIndex;

                object numerator;
                object denumerator;


                if (numeratorObject.Value is PapyrusFieldDefinition)
                {
                    numeratorObject.Value = (numeratorObject.Value as PapyrusFieldDefinition).FieldVariable;
                }

                if (denumeratorObject.Value is PapyrusFieldDefinition)
                {
                    denumeratorObject.Value = (denumeratorObject.Value as PapyrusFieldDefinition).FieldVariable;
                }

                if (numeratorObject.Value is PapyrusVariableReference)
                {
                    var varRef = numeratorObject.Value as PapyrusVariableReference;
                    var refTypeName = varRef.TypeName?.Value;

                    if (refTypeName == null)
                        refTypeName = "Int";

                    numerator = varRef;

                    // if not int or string, we need to cast it.
                    if (!refTypeName.ToLower().Equals("int") && !refTypeName.ToLower().Equals("system.int32") &&
                        !refTypeName.ToLower().Equals("system.string") && !refTypeName.ToLower().Equals("string"))
                    {
                        var typeVariable = mainInstructionProcessor.GetTargetVariable(instruction, null,
                            out isStructAccess, "Int");
                        output.Add(mainInstructionProcessor.CreatePapyrusCastInstruction(typeVariable, varRef));
                        // cast = "Cast " + typeVariable + " " + value1;
                    }
                }
                else if (numeratorObject.Value is FieldReference)
                {
                    numerator =
                        mainInstructionProcessor.CreateVariableReferenceFromName(
                            (numeratorObject.Value as FieldReference).Name);
                }
                else
                {
                    numerator = mainInstructionProcessor.CreateVariableReference(
                        Utility.GetPrimitiveTypeFromValue(numeratorObject.Value),
                        numeratorObject.Value);
                }

                if (denumeratorObject.Value is PapyrusVariableReference)
                {
                    var varRef = denumeratorObject.Value as PapyrusVariableReference;
                    var refTypeName = varRef.TypeName.Value;
                    denumerator = varRef;

                    // if not int or string, we need to cast it.
                    if (!refTypeName.ToLower().Equals("int") && !refTypeName.ToLower().Equals("system.int32") &&
                        !refTypeName.ToLower().Equals("system.string") && !refTypeName.ToLower().Equals("string"))
                    {
                        // CAST BOOL TO INT
                        // var typeVariable = GetTargetVariable(instruction, null, "Int");
                        // cast = "Cast " + typeVariable + " " + value2;

                        var typeVariable = mainInstructionProcessor.GetTargetVariable(instruction, null,
                            out isStructAccess, "Int");
                        output.Add(mainInstructionProcessor.CreatePapyrusCastInstruction(typeVariable, varRef));

                    }
                }
                else if (denumeratorObject.Value is FieldReference)
                {
                    denumerator =
                        mainInstructionProcessor.CreateVariableReferenceFromName(
                            (denumeratorObject.Value as FieldReference).Name);
                }
                else
                {
                    denumerator =
                        mainInstructionProcessor.CreateVariableReference(
                            Utility.GetPrimitiveTypeFromValue(denumeratorObject.Value), denumeratorObject.Value);
                }

                if (!string.IsNullOrEmpty(tempVariable))
                {
                    output.Add(mainInstructionProcessor.CreatePapyrusInstruction(papyrusOpCode,
                        mainInstructionProcessor.CreateVariableReferenceFromName(tempVariable), denumerator, numerator));
                    return output;
                }

                // if (Utility.IsGreaterThan(code) || Utility.IsLessThan(code))
                {
                    var next = instruction.Next;

                    // If the next one is a switch, it most likely 
                    // means that we want to apply some mathematical stuff
                    // on our constant value so that we can properly do an equality
                    // comparison.
                    if (InstructionHelper.IsSwitch(next.OpCode.Code))
                    {
                        var newTempVariable = mainInstructionProcessor.GetTargetVariable(instruction, null,
                            out isStructAccess, "Int", true);
                        mainInstructionProcessor.SwitchConditionalComparer =
                            mainInstructionProcessor.CreateVariableReferenceFromName(newTempVariable);
                        mainInstructionProcessor.SwitchConditionalComparer.ValueType = PapyrusPrimitiveType.Reference;
                        mainInstructionProcessor.SwitchConditionalComparer.TypeName =
                            "Int".Ref(mainInstructionProcessor.PapyrusAssembly);

                        output.Add(mainInstructionProcessor.CreatePapyrusInstruction(papyrusOpCode,
                            mainInstructionProcessor.SwitchConditionalComparer, denumerator, numerator));
                        return output;
                    }

                    while (next != null &&
                           !InstructionHelper.IsStoreLocalVariable(next.OpCode.Code) &&
                           !InstructionHelper.IsStoreField(next.OpCode.Code) &&
                           !InstructionHelper.IsCallMethod(next.OpCode.Code))
                    {
                        next = next.Next;
                    }

                    if (next != null && next.Operand is MethodReference)
                    {
                        // if none found, create a temp one.
                        var methodRef = next.Operand as MethodReference;

                        var tVar = mainInstructionProcessor.CreateTempVariable(
                            methodRef.MethodReturnType.ReturnType.FullName != "System.Void"
                                ? methodRef.MethodReturnType.ReturnType.FullName
                                : "System.int");

                        var targetVar = tVar;

                        mainInstructionProcessor.EvaluationStack.Push(new EvaluationStackItem
                        {
                            Value = tVar.Value,
                            TypeName = tVar.TypeName.Value
                        });

                        output.Add(mainInstructionProcessor.CreatePapyrusInstruction(papyrusOpCode, targetVar,
                            denumerator, numerator));
                        return output;
                    }


                    if (next == null)
                    {
                        // No intentions to store this value into a variable, 
                        // Its to be used in a function call.
                        //return "NULLPTR " + denumerator + " " + numerator;
                        return output;
                    }

                    mainInstructionProcessor.SkipToOffset = next.Offset;
                    if (next.Operand is FieldReference)
                    {
                        var field = mainInstructionProcessor.GetFieldFromStfld(next);
                        var structRef = field as PapyrusStructFieldReference;
                        if (structRef != null)
                        {
                            // output.Add(mainInstructionProcessor.CreatePapyrusInstruction(papyrusOpCode, field, denumerator, numerator));
                            // structRef.
                        }
                        else if (field != null)
                        {
                            output.Add(mainInstructionProcessor.CreatePapyrusInstruction(papyrusOpCode, field,
                                denumerator, numerator));
                            return output;
                            // LastSaughtTypeName = fieldData.TypeName;
                        }

                        //if (field != null)
                        //{
                        //    output.Add(mainInstructionProcessor.CreatePapyrusInstruction(papyrusOpCode, field, denumerator, numerator));
                        //    return output;
                        //    //return field.Name + " " + denumerator + " " + numerator;
                        //}
                    }


                    var numericValue = mainInstructionProcessor.GetNumericValue(next);
                    varIndex = (int)numericValue;
                }

                output.Add(mainInstructionProcessor.CreatePapyrusInstruction(papyrusOpCode, vars[varIndex], denumerator,
                    numerator));
                //return vars[varIndex].Name + " " + denumerator + " " + numerator;
            }
            else if (mainInstructionProcessor.PapyrusCompilerOptions == PapyrusCompilerOptions.Strict)
            {
                throw new StackUnderflowException();
            }
            return output;
        }

        /// <summary>
        /// Processes the specified instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<PapyrusInstruction> Process(IReadOnlyCollection<PapyrusAssemblyDefinition> papyrusAssemblyCollection, Instruction instruction, MethodDefinition targetMethod, TypeDefinition type)
        {
            throw new System.NotImplementedException();
        }
    }
}