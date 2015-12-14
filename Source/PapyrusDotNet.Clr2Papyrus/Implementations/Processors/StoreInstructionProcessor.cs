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
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations.Processors
{
    public class StoreInstructionProcessor : IInstructionProcessor
    {
        private readonly IClr2PapyrusInstructionProcessor mainInstructionProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreInstructionProcessor"/> class.
        /// </summary>
        /// <param name="clr2PapyrusInstructionProcessor">The CLR2 papyrus instruction processor.</param>
        public StoreInstructionProcessor(IClr2PapyrusInstructionProcessor clr2PapyrusInstructionProcessor)
        {
            mainInstructionProcessor = clr2PapyrusInstructionProcessor;
        }

        /// <summary>
        /// Parses the instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public IEnumerable<PapyrusInstruction> Process(Instruction instruction, MethodDefinition targetMethod,
            TypeDefinition type)
        {
            var allVariables = mainInstructionProcessor.PapyrusMethod.GetVariables();

            if (InstructionHelper.IsStoreElement(instruction.OpCode.Code))
            {
                var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
                if (mainInstructionProcessor.EvaluationStack.Count >= popCount)
                {
                    var newValue = mainInstructionProcessor.EvaluationStack.Pop();
                    var itemIndex = mainInstructionProcessor.EvaluationStack.Pop();
                    var itemArray = mainInstructionProcessor.EvaluationStack.Pop();

                    object targetItemIndex = null;
                    object targetItemArray = null;
                    object targetItemValue = null;

                    if (itemIndex.Value is PapyrusVariableReference)
                    {
                        targetItemIndex = (itemIndex.Value as PapyrusVariableReference);
                    }
                    else if (itemIndex.Value != null)
                    {
                        targetItemIndex = itemIndex.Value;
                    }

                    if (mainInstructionProcessor.PapyrusAssembly.VersionTarget == PapyrusVersionTargets.Skyrim)
                    {
                        if ((targetItemIndex as int?) > 128)
                            targetItemIndex = 128;
                    }

                    if (itemArray.Value is PapyrusVariableReference)
                    {
                        targetItemArray = (itemArray.Value as PapyrusVariableReference);
                    }

                    if (newValue.Value is PapyrusVariableReference)
                    {
                        targetItemValue = (newValue.Value as PapyrusVariableReference);
                    }
                    else if (newValue.Value != null)
                    {
                        targetItemValue = newValue.Value;
                    }


                    return Utility.ArrayOf(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCode.ArraySetElement, targetItemArray, targetItemIndex,
                        targetItemValue));
                    //return "ArraySetElement " + tar + " " + oidx + " " + val;
                }
            }

            if (InstructionHelper.IsStoreLocalVariable(instruction.OpCode.Code) || InstructionHelper.IsStoreField(instruction.OpCode.Code))
            {
                if (instruction.Operand is FieldReference)
                {
                    var fref = instruction.Operand as FieldReference;
                    // if the EvaluationStack.Count == 0
                    // The previous instruction might have been a call that returned a value
                    // Something we did not store...
                    if (mainInstructionProcessor.EvaluationStack.Count > 0)
                    {
                        var obj = mainInstructionProcessor.EvaluationStack.Pop();

                        var definedField = mainInstructionProcessor.PapyrusType.Fields.FirstOrDefault(
                            f => f.Name.Value == "::" + fref.Name.Replace('<', '_').Replace('>', '_'));
                        if (definedField != null)
                        {
                            if (obj.Value is PapyrusParameterDefinition)
                            {
                                var varRef = obj.Value as PapyrusParameterDefinition;
                                // definedField.FieldVariable = varRef.;

                                // CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField.Name.Value, varRef.Name.Value)
                                return Utility.ArrayOf(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField, varRef));
                            }
                            if (obj.Value is PapyrusVariableReference)
                            {
                                var varRef = obj.Value as PapyrusVariableReference;
                                // definedField.Value = varRef.Value;
                                definedField.FieldVariable = varRef;
                                definedField.FieldVariable.ValueType = PapyrusPrimitiveType.Reference;
                                // CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField.Name.Value, varRef.Name.Value)
                                return Utility.ArrayOf(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField, varRef));
                            }
                            //definedField.FieldVariable.Value =
                            //    Utility.TypeValueConvert(definedField.FieldVariable.TypeName.Value, obj.Value);
                            var targetValue = Utility.TypeValueConvert(definedField.FieldVariable.TypeName.Value,
                                obj.Value);

                            return Utility.ArrayOf(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField, targetValue));
                            // definedField.FieldVariable.Value
                            // "Assign " + definedField.Name + " " + definedField.Value;
                        }
                    }
                }
                var index = (int)mainInstructionProcessor.GetNumericValue(instruction);
                object outVal = null;
                if (index < allVariables.Count)
                {
                    if (mainInstructionProcessor.EvaluationStack.Count > 0)
                    {
                        var heapObj = mainInstructionProcessor.EvaluationStack.Pop();
                        if (heapObj.Value is PapyrusFieldDefinition)
                        {
                            heapObj.Value = (heapObj.Value as PapyrusFieldDefinition).FieldVariable;
                        }
                        if (heapObj.Value is PapyrusVariableReference)
                        {
                            var varRef = heapObj.Value as PapyrusVariableReference;
                            allVariables[index].Value = allVariables[index].Name.Value;
                            //varRef.Name.Value;
                            // "Assign " + allVariables[(int)index].Name.Value + " " + varRef.Name.Value;                         
                            return Utility.ArrayOf(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCode.Assign, allVariables[index], varRef));
                        }
                        // allVariables[index].Value
                        outVal = Utility.TypeValueConvert(allVariables[index].TypeName.Value, heapObj.Value);
                    }
                    var valout = outVal;//allVariables[index].Value;


                    //if (valout is string)
                    //{
                    //    stringValue = valout.ToString();
                    //}

                    if (valout is PapyrusFieldDefinition)
                    {
                        return Utility.ArrayOf(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCode.Assign, allVariables[index], valout as PapyrusFieldDefinition));
                    }

                    if (valout is PapyrusVariableReference)
                    {
                        return Utility.ArrayOf(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCode.Assign, allVariables[index], valout as PapyrusVariableReference));
                    }

                    if (valout is PapyrusParameterDefinition)
                    {
                        return Utility.ArrayOf(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCode.Assign, allVariables[index], valout as PapyrusParameterDefinition));
                    }


                    // "Assign " + allVariables[(int)index].Name.Value + " " + valoutStr;

                    if (valout == null)
                    {
                        valout = "None";
                    }

                    return Utility.ArrayOf(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCode.Assign, allVariables[index], valout));
                }
            }
            return new PapyrusInstruction[0];
        }
    }
}