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
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.Common.Utilities;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations.Processors
{
    public class StoreInstructionProcessor : IInstructionProcessor
    {
        private readonly IClr2PapyrusInstructionProcessor mainInstructionProcessor;
        private readonly IValueTypeConverter valueTypeConverter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StoreInstructionProcessor" /> class.
        /// </summary>
        /// <param name="clr2PapyrusInstructionProcessor">The CLR2 papyrus instruction processor.</param>
        public StoreInstructionProcessor(IClr2PapyrusInstructionProcessor clr2PapyrusInstructionProcessor)
        {
            valueTypeConverter = new PapyrusValueTypeConverter();
            mainInstructionProcessor = clr2PapyrusInstructionProcessor;
        }

        /// <summary>
        ///     Parses the instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public IEnumerable<PapyrusInstruction> Process(
            IReadOnlyCollection<PapyrusAssemblyDefinition> papyrusAssemblyCollection, Instruction instruction,
            MethodDefinition targetMethod,
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
                        targetItemIndex = itemIndex.Value as PapyrusVariableReference;
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
                        targetItemArray = itemArray.Value as PapyrusVariableReference;
                    }

                    if (newValue.Value is PapyrusVariableReference)
                    {
                        targetItemValue = newValue.Value as PapyrusVariableReference;
                    }
                    else if (newValue.Value != null)
                    {
                        targetItemValue = newValue.Value;
                    }


                    return
                        ArrayUtility.ArrayOf(
                            mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.ArraySetElement,
                                targetItemArray, targetItemIndex,
                                targetItemValue));
                    //return "ArraySetElement " + tar + " " + oidx + " " + val;
                }
            }

            if (InstructionHelper.IsStoreLocalVariable(instruction.OpCode.Code) ||
                InstructionHelper.IsStoreFieldObject(instruction.OpCode.Code))
            {
                if (instruction.Operand is FieldReference)
                {
                    var fref = instruction.Operand as FieldReference;
                    // if the EvaluationStack.Count == 0
                    // The previous instruction might have been a call that returned a value
                    // Something we did not store...

                    if (mainInstructionProcessor.EvaluationStack.Count == 0 &&
                        InstructionHelper.IsCallMethod(instruction.Previous.OpCode.Code))
                    {
                        // If previous was a call, then we should have the evaluation stack with at least one item.
                        // But it seem like we don't... Inject tempvar?
                    }

                    if (mainInstructionProcessor.EvaluationStack.Count > 0)
                    {
                        var obj = mainInstructionProcessor.EvaluationStack.Pop();

                        var definedField = mainInstructionProcessor.PapyrusType.Fields.FirstOrDefault(
                            f => f.Name.Value == "::" + fref.Name.Replace('<', '_').Replace('>', '_'));

                        if (definedField == null)
                            definedField = mainInstructionProcessor.GetDelegateField(fref);

                        if (mainInstructionProcessor.EvaluationStack.Count > 0)
                        {
                            var nextObj = mainInstructionProcessor.EvaluationStack.Peek();

                            if (nextObj != null && nextObj.TypeName != null && nextObj.TypeName.Contains("#"))
                            {
                                // Store into Struct field.
                                definedField = nextObj.Value as PapyrusFieldDefinition;
                                var structPropName = fref.Name;
                                mainInstructionProcessor.EvaluationStack.Pop();
                                    // Just pop it so it does not interfere with any other instructions
                                return
                                    ArrayUtility.ArrayOf(
                                        mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.StructSet,
                                            definedField, // location
                                            mainInstructionProcessor.CreateVariableReferenceFromName(structPropName),
                                            // Struct Property/Field Name
                                            obj.Value // value
                                            ));
                            }
                        }


                        if (definedField != null)
                        {
                            var structRef = obj.Value as PapyrusStructFieldReference;
                            if (structRef != null)
                            {
                                // StructGet -> TempVar
                                // Var <- TempVar

                                var structSource = structRef.StructSource as PapyrusFieldDefinition;
                                var structField = structRef.StructVariable;

                                var fieldType = GetStructFieldType(papyrusAssemblyCollection, structSource, structField);

                                // 1. Create Temp Var
                                bool isStructAccess;
                                var tempVar = mainInstructionProcessor.GetTargetVariable(instruction, null,
                                    out isStructAccess,
                                    fieldType, true);

                                // 2. StructGet -> tempVar
                                // 3. Assign var <- tempVar
                                return ArrayUtility.ArrayOf(
                                    mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.StructGet,
                                        mainInstructionProcessor.CreateVariableReferenceFromName(tempVar), structSource,
                                        structField),
                                    mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Assign,
                                        definedField, mainInstructionProcessor.CreateVariableReferenceFromName(tempVar)));
                            }
                            if (obj.Value is PapyrusParameterDefinition)
                            {
                                var varRef = obj.Value as PapyrusParameterDefinition;
                                // definedField.FieldVariable = varRef.;

                                // CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField.Name.Value, varRef.Name.Value)
                                return
                                    ArrayUtility.ArrayOf(
                                        mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Assign,
                                            definedField, varRef));
                            }
                            if (obj.Value is PapyrusVariableReference)
                            {
                                var varRef = obj.Value as PapyrusVariableReference;
                                // definedField.Value = varRef.Value;
                                definedField.DefaultValue = varRef;
                                definedField.DefaultValue.Type = PapyrusPrimitiveType.Reference;
                                // CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField.Name.Value, varRef.Name.Value)                                
                                if (varRef.IsDelegateReference)
                                {
                                    return new PapyrusInstruction[0];
                                }
                                return
                                    ArrayUtility.ArrayOf(
                                        mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Assign,
                                            definedField, varRef));
                            }
                            //definedField.FieldVariable.Value =
                            //    Utility.TypeValueConvert(definedField.FieldVariable.TypeName.Value, obj.Value);
                            var targetValue = valueTypeConverter.Convert(definedField.DefaultValue.TypeName.Value,
                                obj.Value);

                            return
                                ArrayUtility.ArrayOf(
                                    mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Assign,
                                        definedField, targetValue));
                            // definedField.FieldVariable.Value
                            // "Assign " + definedField.Name + " " + definedField.Value;
                        }
                    }
                }
                var index = (int) mainInstructionProcessor.GetNumericValue(instruction);
                object outVal = null;
                if (index < allVariables.Count)
                {
                    if (mainInstructionProcessor.EvaluationStack.Count > 0)
                    {
                        var heapObj = mainInstructionProcessor.EvaluationStack.Pop();

                        var structRef = heapObj.Value as PapyrusStructFieldReference;
                        if (structRef != null)
                        {
                            // Grabbing value from struct

                            var structSource = structRef.StructSource as PapyrusFieldDefinition;
                            var structField = structRef.StructVariable;

                            var fieldType = GetStructFieldType(papyrusAssemblyCollection, structSource, structField);

                            // 1. Create Temp Var
                            bool isStructAccess;
                            var tempVar = mainInstructionProcessor.GetTargetVariable(instruction, null,
                                out isStructAccess,
                                fieldType, true);

                            // 2. StructGet -> tempVar
                            // 3. Assign var <- tempVar
                            return ArrayUtility.ArrayOf(
                                mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.StructGet,
                                    mainInstructionProcessor.CreateVariableReferenceFromName(tempVar), structSource,
                                    structField),
                                mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Assign,
                                    allVariables[index],
                                    mainInstructionProcessor.CreateVariableReferenceFromName(tempVar)));
                        }

                        if (heapObj.Value is PapyrusFieldDefinition)
                        {
                            heapObj.Value = (heapObj.Value as PapyrusFieldDefinition).DefaultValue;
                        }
                        if (heapObj.Value is PapyrusVariableReference)
                        {
                            var varRef = heapObj.Value as PapyrusVariableReference;
                            allVariables[index].Value = allVariables[index].Name.Value;
                            //varRef.Name.Value;
                            // "Assign " + allVariables[(int)index].Name.Value + " " + varRef.Name.Value;                         
                            return
                                ArrayUtility.ArrayOf(
                                    mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Assign,
                                        allVariables[index], varRef));
                        }
                        // allVariables[index].Value
                        outVal = valueTypeConverter.Convert(allVariables[index].TypeName.Value, heapObj.Value);
                    }
                    var valout = outVal; //allVariables[index].Value;


                    //if (valout is string)
                    //{
                    //    stringValue = valout.ToString();
                    //}

                    if (valout is PapyrusFieldDefinition)
                    {
                        return
                            ArrayUtility.ArrayOf(mainInstructionProcessor.CreatePapyrusInstruction(
                                PapyrusOpCodes.Assign, allVariables[index], valout as PapyrusFieldDefinition));
                    }

                    if (valout is PapyrusVariableReference)
                    {
                        return
                            ArrayUtility.ArrayOf(mainInstructionProcessor.CreatePapyrusInstruction(
                                PapyrusOpCodes.Assign, allVariables[index], valout as PapyrusVariableReference));
                    }

                    if (valout is PapyrusParameterDefinition)
                    {
                        return
                            ArrayUtility.ArrayOf(mainInstructionProcessor.CreatePapyrusInstruction(
                                PapyrusOpCodes.Assign, allVariables[index], valout as PapyrusParameterDefinition));
                    }


                    // "Assign " + allVariables[(int)index].Name.Value + " " + valoutStr;

                    if (valout == null)
                    {
                        valout = "None";
                    }

                    if (allVariables[index].IsDelegateReference)
                    {
                        // If this is a delegate reference, then we do not want to assign this value to anything.
                        return new PapyrusInstruction[0];
                    }

                    return
                        ArrayUtility.ArrayOf(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Assign,
                            allVariables[index], valout));
                }
            }
            return new PapyrusInstruction[0];
        }

        private string GetStructFieldType(IReadOnlyCollection<PapyrusAssemblyDefinition> papyrusAssemblyCollection,
            PapyrusFieldDefinition structSource, PapyrusVariableReference structField)
        {
            foreach (var a in papyrusAssemblyCollection)
            {
                foreach (var t in a.Types)
                {
                    foreach (var s in t.NestedTypes)
                    {
                        var name = structSource.TypeName.Split('#').LastOrDefault();

                        if (s.Name.Value.ToLower() == name.ToLower())
                        {
                            var targetField = s.Fields.FirstOrDefault(f => f.Name.Value == "::" + structField.Name.Value);
                            if (targetField != null)
                            {
                                return targetField.TypeName;
                            }
                        }
                    }
                }
            }
            return "none";
        }
    }
}