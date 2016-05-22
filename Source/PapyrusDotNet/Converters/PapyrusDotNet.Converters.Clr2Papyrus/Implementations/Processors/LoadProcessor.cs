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
using PapyrusDotNet.Common;
using PapyrusDotNet.Common.Utilities;
using PapyrusDotNet.Converters.Clr2Papyrus.Enums;
using PapyrusDotNet.Converters.Clr2Papyrus.Exceptions;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations.Processors
{
    public interface ILoadProcessor : ISubInstructionProcessor { }
    public class LoadProcessor : ILoadProcessor
    {
        /// <summary>
        /// Parses the instruction.
        /// </summary>
        /// <param name="mainProcessor">The processor.</param>
        /// <param name="asmCollection">The papyrus assembly collection.</param>
        /// <param name="instruction">The instruction.</param>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <exception cref="MissingVariableException"></exception>
        public IEnumerable<PapyrusInstruction> Process(
            IClrInstructionProcessor mainProcessor,
            IReadOnlyCollection<PapyrusAssemblyDefinition> asmCollection, 
            Instruction instruction,
            MethodDefinition targetMethod, TypeDefinition type)
        {
            bool isStructAccess;
            var outputInstructions = new List<PapyrusInstruction>();

            if (InstructionHelper.NextInstructionIs(instruction, Code.Ldnull) &&
                InstructionHelper.NextInstructionIs(instruction.Next, Code.Cgt_Un))
            {
                var stack = mainProcessor.EvaluationStack;
                //var itemToCheck = stack.Pop().Value;
                var itemToCheck = instruction.Operand as FieldReference;

                if (itemToCheck != null)
                {
                    mainProcessor.EvaluationStack.Push(
                        new EvaluationStackItem
                        {
                            Value = itemToCheck,
                            TypeName = ""
                        }
                        );
                    mainProcessor.EvaluationStack.Push(
                        new EvaluationStackItem
                        {
                            Value = null,
                            TypeName = "none"
                        }
                        );

                    mainProcessor.SkipToOffset =
                        InstructionHelper.NextInstructionIsOffset(instruction.Next, Code.Cgt_Un) - 1;

                    //bool structAccess;
                    //var targetVar = mainInstructionProcessor.GetTargetVariable(tarInstruction, null, out structAccess, "Bool");

                    //if (mainInstructionProcessor.SkipNextInstruction)
                    //{
                    //    mainInstructionProcessor.SkipToOffset += 2;
                    //    mainInstructionProcessor.SkipNextInstruction = false;
                    //}

                    //outputInstructions.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Is,
                    //    mainInstructionProcessor.CreateVariableReferenceFromName(targetVar), fieldDef,
                    //    mainInstructionProcessor.CreateVariableReferenceFromName(typeToCheckAgainst.Name)));
                    return outputInstructions;
                }
            }

            if (InstructionHelper.IsLoadMethodRef(instruction.OpCode.Code))
            {
                // Often used for delegates or Action/func parameters, when loading a reference pointer to a method and pushing it to the stack.
                // To maintain the evaluation stack, this could add a dummy item, but for now im not going to do so.
            }

            if (InstructionHelper.IsLoadLength(instruction.OpCode.Code))
            {
                var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
                if (mainProcessor.EvaluationStack.Count >= popCount)
                {
                    var val = mainProcessor.EvaluationStack.Pop();
                    if (val.TypeName.EndsWith("[]"))
                    {
                        if (val.Value is PapyrusPropertyDefinition)
                        {
                            // for now, so if this exception is thrown, i will have to remember I have to fix it.
                            throw new NotImplementedException();
                        }

                        if (val.Value is PapyrusVariableReference || val.Value is PapyrusFieldDefinition ||
                            val.Value is PapyrusParameterDefinition)
                        {
                            int variableIndex;

                            var storeInstruction =
                                mainProcessor.GetNextStoreLocalVariableInstruction(instruction,
                                    out variableIndex);

                            if (storeInstruction != null ||
                                InstructionHelper.IsConverToNumber(instruction.Next.OpCode.Code))
                            {
                                if (InstructionHelper.IsConverToNumber(instruction.Next.OpCode.Code))
                                {
                                    mainProcessor.SkipNextInstruction = false;
                                    mainProcessor.SkipToOffset = 0;

                                    var targetVariableName = mainProcessor.GetTargetVariable(instruction,
                                        null, out isStructAccess, "Int", true);

                                    var allVars = mainProcessor.PapyrusMethod.GetVariables();
                                    var targetVariable = allVars.FirstOrDefault(v => v.Name.Value == targetVariableName);

                                    if (targetVariable == null &&
                                        mainProcessor.PapyrusCompilerOptions == PapyrusCompilerOptions.Strict)
                                    {
                                        throw new MissingVariableException(targetVariableName);
                                    }
                                    if (targetVariable != null)
                                    {
                                        mainProcessor.EvaluationStack.Push(new EvaluationStackItem
                                        {
                                            TypeName = targetVariable.TypeName.Value,
                                            Value = targetVariable
                                        });

                                        outputInstructions.Add(
                                            mainProcessor.CreatePapyrusInstruction(
                                                PapyrusOpCodes.ArrayLength,
                                                mainProcessor.CreateVariableReference(
                                                    PapyrusPrimitiveType.Reference, targetVariableName), val.Value));
                                    }
                                }
                                else
                                {
                                    var allVars = mainProcessor.PapyrusMethod.GetVariables();

                                    mainProcessor.EvaluationStack.Push(new EvaluationStackItem
                                    {
                                        TypeName = allVars[variableIndex].TypeName.Value,
                                        Value = allVars[variableIndex]
                                    });

                                    outputInstructions.Add(
                                        mainProcessor.CreatePapyrusInstruction(PapyrusOpCodes.ArrayLength,
                                            allVars[variableIndex], val.Value));

                                    //return "ArrayLength " + allVars[variableIndex].Name + " " +
                                    //       (val.Value as VariableReference).Name;
                                }
                            }
                        }
                    }
                }
                // ArrayLength <outputVariableName> <arrayName>
            }


            if (InstructionHelper.IsLoadArgs(instruction.OpCode.Code))
            {
                var index = (int) mainProcessor.GetNumericValue(instruction);
                if (targetMethod.IsStatic && index == 0 && targetMethod.Parameters.Count == 0)
                {
                    mainProcessor.EvaluationStack.Push(new EvaluationStackItem
                    {
                        IsThis = true,
                        Value = type,
                        TypeName = type.FullName
                    });
                }
                else
                {
                    if (targetMethod.HasThis && index == 0)
                    {
                        return outputInstructions;
                    }

                    if (!targetMethod.IsStatic && index > 0) index--;
                    if (index < mainProcessor.PapyrusMethod.Parameters.Count)
                    {
                        mainProcessor.EvaluationStack.Push(new EvaluationStackItem
                        {
                            Value = mainProcessor.PapyrusMethod.Parameters[index],
                            TypeName = mainProcessor.PapyrusMethod.Parameters[index].TypeName.Value
                        });
                    }
                }
            }
            if (InstructionHelper.IsLoadInteger(instruction.OpCode.Code))
            {
                var index = mainProcessor.GetNumericValue(instruction);
                mainProcessor.EvaluationStack.Push(new EvaluationStackItem {Value = index, TypeName = "Int"});
            }

            if (InstructionHelper.IsLoadNull(instruction.OpCode.Code))
            {
                mainProcessor.EvaluationStack.Push(new EvaluationStackItem
                {
                    Value = "None",
                    TypeName = "None"
                });
            }

            if (InstructionHelper.IsLoadLocalVariable(instruction.OpCode.Code))
            {
                var index = (int) mainProcessor.GetNumericValue(instruction);
                var allVariables = mainProcessor.PapyrusMethod.GetVariables();
                if (index < allVariables.Count)
                {
                    mainProcessor.EvaluationStack.Push(new EvaluationStackItem
                    {
                        Value = allVariables[index],
                        TypeName = allVariables[index].TypeName.Value
                    });
                }
            }

            if (InstructionHelper.IsLoadString(instruction.OpCode.Code))
            {
                var value = StringUtility.AsString(instruction.Operand);

                mainProcessor.EvaluationStack.Push(new EvaluationStackItem
                {
                    Value = value,
                    TypeName = "String"
                });
            }

            if (InstructionHelper.IsLoadFieldObject(instruction.OpCode.Code))
            {
                if (instruction.Operand is FieldReference)
                {
                    var fieldRef = instruction.Operand as FieldReference;


                    PapyrusFieldDefinition targetField = null;

                    targetField = mainProcessor.PapyrusType.Fields.FirstOrDefault(
                        f => f.Name.Value == "::" + fieldRef.Name.Replace('<', '_').Replace('>', '_'));

                    if (targetField == null)
                        targetField = mainProcessor.GetDelegateField(fieldRef);


                    if (targetField != null)
                    {
                        mainProcessor.EvaluationStack.Push(new EvaluationStackItem
                        {
                            Value = targetField,
                            TypeName = targetField.TypeName
                        });
                    }

                    if (InstructionHelper.PreviousInstructionWas(instruction, Code.Ldflda) &&
                        fieldRef.FullName.Contains("/"))
                    {
                        var targetStructVariable = mainProcessor.EvaluationStack.Pop().Value;

                        var structRef = new PapyrusStructFieldReference(mainProcessor.PapyrusAssembly, null)
                        {
                            StructSource = targetStructVariable,
                            StructVariable = mainProcessor.CreateVariableReferenceFromName(fieldRef.Name)
                        };

                        mainProcessor.EvaluationStack.Push(new EvaluationStackItem
                        {
                            Value = structRef,
                            TypeName = "$StructAccess$"
                        });

                        return outputInstructions;

                        //    // The target field is not inside the declared type.
                        //    // Most likely, this is a get field from struct.
                        //    if (fieldRef.FieldType.FullName.Contains("/"))
                        //    {
                        //        var location = fieldRef.FieldType.FullName.Split("/").LastOrDefault();

                        //        var targetStruct = mainInstructionProcessor.PapyrusType.NestedTypes.FirstOrDefault(n => n.Name.Value == location);
                        //        if (targetStruct != null)
                        //        {

                        //            targetField = mainInstructionProcessor.PapyrusType.Fields.FirstOrDefault(
                        //                f => f.Name.Value == "::" + fieldRef.Name);
                        //            // var stack = mainInstructionProcessor.EvaluationStack;
                        //            // TODO: Add support for getting values from Structs
                        //            // 
                        //            // CreatePapyrusInstruction(PapyrusOpCode.StructGet, ...)
                        //        }
                        //    }
                    }
                    //else
                    //{
                    //    targetField = mainInstructionProcessor.PapyrusType.Fields.FirstOrDefault(
                    //        f => f.Name.Value == "::" + fieldRef.Name.Replace('<', '_').Replace('>', '_'));
                    //}
                }
            }


            if (InstructionHelper.IsLoadElement(instruction.OpCode.Code))
            {
                // TODO: Load Element (Arrays, and what not)
                var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
                if (mainProcessor.EvaluationStack.Count >= popCount)
                {
                    var itemIndex = mainProcessor.EvaluationStack.Pop();
                    var itemArray = mainProcessor.EvaluationStack.Pop();

                    object targetItemIndex = null;

                    var targetItemArray = itemArray.Value;

                    if (itemIndex.Value != null)
                    {
                        targetItemIndex = itemIndex.Value;
                    }

                    // 128 is the array size limit for Skyrim
                    if (mainProcessor.PapyrusAssembly.VersionTarget == PapyrusVersionTargets.Skyrim)
                    {
                        if ((targetItemIndex as int?) > 128)
                            targetItemIndex = 128;
                    }

                    // We want to use the Array Element together with a Method Call?
                    var isBoxing = InstructionHelper.IsBoxing(instruction.Next.OpCode.Code);
                    if (isBoxing || InstructionHelper.IsCallMethod(instruction.Next.OpCode.Code))
                    {
                        if (isBoxing)
                        {
                            var sourceArray = targetItemArray as PapyrusVariableReference;
                            if (sourceArray != null)
                            {
                                // Since we apply our logic necessary for this "boxing" right here
                                // we can skip the next instruction to avoid unexpected behaviour.
                                mainProcessor.SkipNextInstruction = true;

                                // Create a new Temp Variable
                                // Assign our value to this temp variable and push it to the stack
                                // so that the next instruction can take care of it.
                                var tempVariableType = sourceArray.TypeName.Value.Replace("[]", "");

                                var destinationTempVar = mainProcessor.GetTargetVariable(instruction, null,
                                    out isStructAccess, tempVariableType, true);

                                var varRef =
                                    mainProcessor.PapyrusMethod.GetVariables()
                                        .FirstOrDefault(n => n.Name.Value == destinationTempVar);

                                mainProcessor.EvaluationStack.Push(new EvaluationStackItem
                                {
                                    Value = varRef ?? (object) destinationTempVar,
                                    // Should be the actual variable reference
                                    TypeName = tempVariableType
                                });

                                outputInstructions.Add(
                                    mainProcessor.CreatePapyrusInstruction(PapyrusOpCodes.ArrayGetElement,
                                        mainProcessor.CreateVariableReference(
                                            PapyrusPrimitiveType.Reference, destinationTempVar),
                                        targetItemArray,
                                        targetItemIndex));
                            }
                        }
                        else
                        {
                            // Get the method reference and then create a temp variable that
                            // we can use for assigning the value to.
                            var methodRef = instruction.Next.Operand as MethodReference;
                            if (methodRef != null && methodRef.HasParameters)
                            {
                                var sourceArray = targetItemArray as PapyrusVariableReference;
                                if (sourceArray != null)
                                {
                                    var tempVariableType = sourceArray.TypeName.Value.Replace("[]", "");
                                    var destinationTempVar = mainProcessor.GetTargetVariable(instruction,
                                        methodRef, out isStructAccess,
                                        tempVariableType);

                                    // "ArrayGetElement " + destinationTempVar + " " + targetItemArray + " " + targetItemIndex;
                                    outputInstructions.Add(
                                        mainProcessor.CreatePapyrusInstruction(
                                            PapyrusOpCodes.ArrayGetElement,
                                            mainProcessor.CreateVariableReference(
                                                PapyrusPrimitiveType.Reference, destinationTempVar),
                                            targetItemArray,
                                            targetItemIndex));
                                }
                            }
                        }
                    }
                    else
                    {
                        // Otherwise we just want to store it somewhere.
                        int destinationVariableIndex;
                        // Get the target variable by finding the next store instruction and returning the variable index.
                        mainProcessor.GetNextStoreLocalVariableInstruction(instruction,
                            out destinationVariableIndex);
                        var destinationVar =
                            mainProcessor.PapyrusMethod.GetVariables()[destinationVariableIndex];

                        // ArrayGetElement targetVariable targetItemArray targetItemIndex
                        outputInstructions.Add(
                            mainProcessor.CreatePapyrusInstruction(PapyrusOpCodes.ArrayGetElement,
                                destinationVar,
                                targetItemArray,
                                targetItemIndex)
                            );
                    }
                }
            }
            return outputInstructions;
        }
    }
}