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
using PapyrusDotNet.Common;
using PapyrusDotNet.Common.Utilities;
using PapyrusDotNet.Converters.Clr2Papyrus.Enums;
using PapyrusDotNet.Converters.Clr2Papyrus.Exceptions;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations.Processors
{
    public class LoadInstructionProcessor : IInstructionProcessor
    {
        private readonly IClr2PapyrusInstructionProcessor mainInstructionProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadInstructionProcessor"/> class.
        /// </summary>
        /// <param name="mainInstructionProcessor">The main instruction processor.</param>
        public LoadInstructionProcessor(IClr2PapyrusInstructionProcessor mainInstructionProcessor)
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
        /// <exception cref="MissingVariableException"></exception>
        public IEnumerable<PapyrusInstruction> Process(IReadOnlyCollection<PapyrusAssemblyDefinition> papyrusAssemblyCollection, Instruction instruction, MethodDefinition targetMethod, TypeDefinition type)
        {
            bool isStructAccess;
            var outputInstructions = new List<PapyrusInstruction>();

            if (InstructionHelper.NextInstructionIs(instruction, Code.Ldnull) &&
                InstructionHelper.NextInstructionIs(instruction.Next, Code.Cgt_Un))
            {
                var stack = mainInstructionProcessor.EvaluationStack;
                //var itemToCheck = stack.Pop().Value;
                var itemToCheck = instruction.Operand as FieldReference;

                if (itemToCheck != null)
                {
                    mainInstructionProcessor.EvaluationStack.Push(
                            new EvaluationStackItem()
                            {
                                Value = itemToCheck,
                                TypeName = ""
                            }
                        );
                    mainInstructionProcessor.EvaluationStack.Push(
                            new EvaluationStackItem()
                            {
                                Value = null,
                                TypeName = "none"
                            }
                        );

                    mainInstructionProcessor.SkipToOffset =
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
                if (mainInstructionProcessor.EvaluationStack.Count >= popCount)
                {
                    var val = mainInstructionProcessor.EvaluationStack.Pop();
                    if (val.TypeName.EndsWith("[]"))
                    {
                        if (val.Value is PapyrusVariableReference)
                        {
                            int variableIndex;

                            var storeInstruction = mainInstructionProcessor.GetNextStoreLocalVariableInstruction(instruction, out variableIndex);

                            if (storeInstruction != null ||
                                InstructionHelper.IsConverToNumber(instruction.Next.OpCode.Code))
                            {
                                if (InstructionHelper.IsConverToNumber(instruction.Next.OpCode.Code))
                                {
                                    mainInstructionProcessor.SkipNextInstruction = false;
                                    mainInstructionProcessor.SkipToOffset = 0;

                                    var targetVariableName = mainInstructionProcessor.GetTargetVariable(instruction, null, out isStructAccess, "Int", true);

                                    var allVars = mainInstructionProcessor.PapyrusMethod.GetVariables();
                                    var targetVariable = allVars.FirstOrDefault(v => v.Name.Value == targetVariableName);

                                    if (targetVariable == null && mainInstructionProcessor.PapyrusCompilerOptions == PapyrusCompilerOptions.Strict)
                                    {
                                        throw new MissingVariableException(targetVariableName);
                                    }
                                    if (targetVariable != null)
                                    {
                                        mainInstructionProcessor.EvaluationStack.Push(new EvaluationStackItem
                                        {
                                            TypeName = targetVariable.TypeName.Value,
                                            Value = targetVariable
                                        });

                                        outputInstructions.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.ArrayLength, mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference, targetVariableName), val.Value));
                                    }
                                }
                                else
                                {
                                    var allVars = mainInstructionProcessor.PapyrusMethod.GetVariables();

                                    mainInstructionProcessor.EvaluationStack.Push(new EvaluationStackItem
                                    {
                                        TypeName = allVars[variableIndex].TypeName.Value,
                                        Value = allVars[variableIndex]
                                    });

                                    outputInstructions.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.ArrayLength, allVars[variableIndex], val.Value));

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
                var index = (int)mainInstructionProcessor.GetNumericValue(instruction);
                if (targetMethod.IsStatic && index == 0 && targetMethod.Parameters.Count == 0)
                {
                    mainInstructionProcessor.EvaluationStack.Push(new EvaluationStackItem { IsThis = true, Value = type, TypeName = type.FullName });
                }
                else
                {
                    if (targetMethod.HasThis && index == 0)
                    {
                        return outputInstructions;
                    }

                    if (!targetMethod.IsStatic && index > 0) index--;
                    if (index < mainInstructionProcessor.PapyrusMethod.Parameters.Count)
                    {
                        mainInstructionProcessor.EvaluationStack.Push(new EvaluationStackItem
                        {
                            Value = mainInstructionProcessor.PapyrusMethod.Parameters[index],
                            TypeName = mainInstructionProcessor.PapyrusMethod.Parameters[index].TypeName.Value
                        });
                    }
                }
            }
            if (InstructionHelper.IsLoadInteger(instruction.OpCode.Code))
            {
                var index = mainInstructionProcessor.GetNumericValue(instruction);
                mainInstructionProcessor.EvaluationStack.Push(new EvaluationStackItem { Value = index, TypeName = "Int" });
            }

            if (InstructionHelper.IsLoadNull(instruction.OpCode.Code))
            {
                mainInstructionProcessor.EvaluationStack.Push(new EvaluationStackItem { Value = "None", TypeName = "None" });
            }

            if (InstructionHelper.IsLoadLocalVariable(instruction.OpCode.Code))
            {
                var index = (int)mainInstructionProcessor.GetNumericValue(instruction);
                var allVariables = mainInstructionProcessor.PapyrusMethod.GetVariables();
                if (index < allVariables.Count)
                {
                    mainInstructionProcessor.EvaluationStack.Push(new EvaluationStackItem
                    {
                        Value = allVariables[index],
                        TypeName = allVariables[index].TypeName.Value
                    });
                }
            }

            if (InstructionHelper.IsLoadString(instruction.OpCode.Code))
            {
                var value = StringUtility.AsString(instruction.Operand);

                mainInstructionProcessor.EvaluationStack.Push(new EvaluationStackItem { Value = value, TypeName = "String" });
            }

            if (InstructionHelper.IsLoadFieldObject(instruction.OpCode.Code))
            {
                if (instruction.Operand is FieldReference)
                {
                    var fieldRef = instruction.Operand as FieldReference;


                    PapyrusFieldDefinition targetField = null;

                    targetField = mainInstructionProcessor.PapyrusType.Fields.FirstOrDefault(
                       f => f.Name.Value == "::" + fieldRef.Name.Replace('<', '_').Replace('>', '_'));

                    if (targetField == null)
                        targetField = mainInstructionProcessor.GetDelegateField(fieldRef);


                    if (targetField != null)
                    {
                        mainInstructionProcessor.EvaluationStack.Push(new EvaluationStackItem
                        {
                            Value = targetField,
                            TypeName = targetField.TypeName
                        });
                    }

                    if (InstructionHelper.PreviousInstructionWas(instruction, Code.Ldflda) && fieldRef.FullName.Contains("/"))
                    {


                        var targetStructVariable = mainInstructionProcessor.EvaluationStack.Pop().Value;

                        var structRef = new PapyrusStructFieldReference(mainInstructionProcessor.PapyrusAssembly, null)
                        {
                            StructSource = targetStructVariable,
                            StructVariable = mainInstructionProcessor.CreateVariableReferenceFromName(fieldRef.Name)
                        };

                        mainInstructionProcessor.EvaluationStack.Push(new EvaluationStackItem
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
                if (mainInstructionProcessor.EvaluationStack.Count >= popCount)
                {
                    var itemIndex = mainInstructionProcessor.EvaluationStack.Pop();
                    var itemArray = mainInstructionProcessor.EvaluationStack.Pop();

                    object targetItemIndex = null;

                    var targetItemArray = itemArray.Value;

                    if (itemIndex.Value != null)
                    {
                        targetItemIndex = itemIndex.Value;
                    }

                    // 128 is the array size limit for Skyrim
                    if (mainInstructionProcessor.PapyrusAssembly.VersionTarget == PapyrusVersionTargets.Skyrim)
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
                                mainInstructionProcessor.SkipNextInstruction = true;

                                // Create a new Temp Variable
                                // Assign our value to this temp variable and push it to the stack
                                // so that the next instruction can take care of it.
                                var tempVariableType = sourceArray.TypeName.Value.Replace("[]", "");

                                var destinationTempVar = mainInstructionProcessor.GetTargetVariable(instruction, null, out isStructAccess, tempVariableType, true);

                                var varRef = mainInstructionProcessor.PapyrusMethod.GetVariables().FirstOrDefault(n => n.Name.Value == destinationTempVar);

                                mainInstructionProcessor.EvaluationStack.Push(new EvaluationStackItem
                                {
                                    Value = varRef ?? (object)destinationTempVar, // Should be the actual variable reference
                                    TypeName = tempVariableType
                                });

                                outputInstructions.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.ArrayGetElement, mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference, destinationTempVar),
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
                                    var destinationTempVar = mainInstructionProcessor.GetTargetVariable(instruction, methodRef, out isStructAccess,
                                        tempVariableType);

                                    // "ArrayGetElement " + destinationTempVar + " " + targetItemArray + " " + targetItemIndex;
                                    outputInstructions.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.ArrayGetElement, mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference, destinationTempVar),
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
                        mainInstructionProcessor.GetNextStoreLocalVariableInstruction(instruction, out destinationVariableIndex);
                        var destinationVar = mainInstructionProcessor.PapyrusMethod.GetVariables()[destinationVariableIndex];

                        // ArrayGetElement targetVariable targetItemArray targetItemIndex
                        outputInstructions.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.ArrayGetElement,
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