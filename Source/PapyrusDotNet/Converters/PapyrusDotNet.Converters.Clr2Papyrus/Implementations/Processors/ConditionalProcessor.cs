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
using Mono.Cecil;
using Mono.Cecil.Cil;
using PapyrusDotNet.Common;
using PapyrusDotNet.Common.Utilities;
using PapyrusDotNet.Converters.Clr2Papyrus.Enums;
using PapyrusDotNet.Converters.Clr2Papyrus.Exceptions;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Extensions;

#endregion

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations.Processors
{
    public interface IConditionalProcessor : ISubInstructionProcessor
    {
        IEnumerable<PapyrusInstruction> Process(
            IClrInstructionProcessor mainProcessor,
            Instruction instruction,
            Code overrideOpCode = Code.Nop,
            string tempVariable = null);
    }

    public class ConditionalProcessor : IConditionalProcessor
    {
        /// <summary>
        /// Processes the specified instruction.
        /// </summary>
        /// <param name="mainProcessor">The main processor.</param>
        /// <param name="asmCollection">The papyrus assembly collection.</param>
        /// <param name="instruction">The instruction.</param>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<PapyrusInstruction> Process(
            IClrInstructionProcessor mainProcessor,
            IReadOnlyCollection<PapyrusAssemblyDefinition> asmCollection, Instruction instruction,
            MethodDefinition targetMethod, TypeDefinition type)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Processes the conditional instruction.
        /// </summary>
        /// <param name="mainProcessor">The main instruction processor.</param>
        /// <param name="instruction">The instruction.</param>
        /// <param name="overrideOpCode">The override op code.</param>
        /// <param name="tempVariable">The temporary variable.</param>
        /// <returns></returns>
        public IEnumerable<PapyrusInstruction> Process(
            IClrInstructionProcessor mainProcessor,
            Instruction instruction,
            Code overrideOpCode = Code.Nop,
            string tempVariable = null)
        {
            bool isStructAccess;
            var output = new List<PapyrusInstruction>();

            //cast = null;

            var heapStack = mainProcessor.EvaluationStack;

            // TODO: GetConditional only applies on Integers and must add support for Float further on.

            var papyrusOpCode =
                Utility.GetPapyrusMathOrEqualityOpCode(
                    overrideOpCode != Code.Nop ? overrideOpCode : instruction.OpCode.Code, false);

            if (heapStack.Count >= 2) //Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
            {
                var numeratorObject = heapStack.Pop();
                var denumeratorObject = heapStack.Pop();
                var vars = mainProcessor.PapyrusMethod.GetVariables();
                int varIndex;

                object numerator;
                object denumerator;


                if (numeratorObject.Value is PapyrusFieldDefinition)
                {
                    numeratorObject.Value = (numeratorObject.Value as PapyrusFieldDefinition).DefaultValue;
                }

                if (denumeratorObject.Value is PapyrusFieldDefinition)
                {
                    denumeratorObject.Value = (denumeratorObject.Value as PapyrusFieldDefinition).DefaultValue;
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
                        var typeVariable = mainProcessor.GetTargetVariable(instruction, null,
                            out isStructAccess, "Int");
                        output.Add(mainProcessor.CreatePapyrusCastInstruction(typeVariable, varRef));
                        // cast = "Cast " + typeVariable + " " + value1;
                    }
                }
                else if (numeratorObject.Value is FieldReference)
                {
                    numerator =
                        mainProcessor.CreateVariableReferenceFromName(
                            (numeratorObject.Value as FieldReference).Name);
                }
                else
                {
                    numerator = mainProcessor.CreateVariableReference(
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

                        var typeVariable = mainProcessor.GetTargetVariable(instruction, null,
                            out isStructAccess, "Int");
                        output.Add(mainProcessor.CreatePapyrusCastInstruction(typeVariable, varRef));
                    }
                }
                else if (denumeratorObject.Value is FieldReference)
                {
                    denumerator =
                        mainProcessor.CreateVariableReferenceFromName(
                            (denumeratorObject.Value as FieldReference).Name);
                }
                else
                {
                    denumerator =
                        mainProcessor.CreateVariableReference(
                            Utility.GetPrimitiveTypeFromValue(denumeratorObject.Value), denumeratorObject.Value);
                }

                if (!string.IsNullOrEmpty(tempVariable))
                {
                    output.Add(mainProcessor.CreatePapyrusInstruction(papyrusOpCode,
                        mainProcessor.CreateVariableReferenceFromName(tempVariable), denumerator, numerator));
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
                        var newTempVariable = mainProcessor.GetTargetVariable(instruction, null,
                            out isStructAccess, "Int", true);
                        mainProcessor.SwitchConditionalComparer =
                            mainProcessor.CreateVariableReferenceFromName(newTempVariable);
                        mainProcessor.SwitchConditionalComparer.Type = PapyrusPrimitiveType.Reference;
                        mainProcessor.SwitchConditionalComparer.TypeName =
                            "Int".Ref(mainProcessor.PapyrusAssembly);

                        output.Add(mainProcessor.CreatePapyrusInstruction(papyrusOpCode,
                            mainProcessor.SwitchConditionalComparer, denumerator, numerator));
                        return output;
                    }

                    while (next != null &&
                           !InstructionHelper.IsStoreLocalVariable(next.OpCode.Code) &&
                           !InstructionHelper.IsStoreFieldObject(next.OpCode.Code) &&
                           !InstructionHelper.IsCallMethod(next.OpCode.Code))
                    {
                        next = next.Next;
                    }

                    if (next != null && next.Operand is MethodReference)
                    {
                        // if none found, create a temp one.
                        var methodRef = next.Operand as MethodReference;

                        var tVar = mainProcessor.CreateTempVariable(
                            methodRef.MethodReturnType.ReturnType.FullName != "System.Void"
                                ? methodRef.MethodReturnType.ReturnType.FullName
                                : "System.int");

                        var targetVar = tVar;

                        mainProcessor.EvaluationStack.Push(new EvaluationStackItem
                        {
                            Value = tVar.Value,
                            TypeName = tVar.TypeName.Value
                        });

                        output.Add(mainProcessor.CreatePapyrusInstruction(papyrusOpCode, targetVar,
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

                    mainProcessor.SkipToOffset = next.Offset;
                    if (next.Operand is FieldReference)
                    {
                        var field = mainProcessor.GetFieldFromStfld(next);
                        var structRef = field as PapyrusStructFieldReference;
                        if (structRef != null)
                        {
                            // output.Add(mainInstructionProcessor.CreatePapyrusInstruction(papyrusOpCode, field, denumerator, numerator));
                            // structRef.
                        }
                        else if (field != null)
                        {
                            output.Add(mainProcessor.CreatePapyrusInstruction(papyrusOpCode, field,
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


                    var numericValue = mainProcessor.GetNumericValue(next);
                    varIndex = (int)numericValue;
                }

                output.Add(mainProcessor.CreatePapyrusInstruction(papyrusOpCode, vars[varIndex], denumerator,
                    numerator));
                //return vars[varIndex].Name + " " + denumerator + " " + numerator;
            }
            else if (mainProcessor.PapyrusCompilerOptions == PapyrusCompilerOptions.Strict)
            {
                throw new StackUnderflowException();
            }
            return output;
        }
    }
}