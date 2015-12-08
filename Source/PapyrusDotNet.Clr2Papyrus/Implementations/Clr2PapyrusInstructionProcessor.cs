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

#region

using System;
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
using PapyrusDotNet.PapyrusAssembly.Classes;
using PapyrusDotNet.PapyrusAssembly.Enums;
using PapyrusDotNet.PapyrusAssembly.Extensions;

#endregion

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations
{
    public class Clr2PapyrusInstructionProcessor : IClr2PapyrusInstructionProcessor
    {
        private readonly Stack<EvaluationStackItem> evaluationStack = new Stack<EvaluationStackItem>();
        private PapyrusMethodDefinition papyrusMethod;
        private PapyrusAssemblyDefinition papyrusAssembly;
        private PapyrusTypeDefinition papyrusType;

        /// <summary>
        ///     Processes the instructions.
        /// </summary>
        /// <param name="papyrusAssembly"></param>
        /// <param name="papyrusMethod"></param>
        /// <param name="method">The method.</param>
        /// <param name="body">The body.</param>
        /// <param name="instructions">The instructions.</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public IEnumerable<PapyrusInstruction> ProcessInstructions(PapyrusAssemblyDefinition papyrusAssembly,
            PapyrusTypeDefinition papyrusType,
            PapyrusMethodDefinition papyrusMethod,
            MethodDefinition method,
            MethodBody body, Collection<Instruction> instructions,
            PapyrusCompilerOptions options = PapyrusCompilerOptions.Strict)
        {
            this.papyrusAssembly = papyrusAssembly;
            this.papyrusType = papyrusType;
            this.papyrusMethod = papyrusMethod;

            Instruction currentInstruction = null;

            evaluationStack.Clear();

            var outputInstructions = new List<PapyrusInstruction>();
            try
            {
                foreach (var i in instructions)
                {
                    currentInstruction = i;

                    var papyrusInstructions = ProcessInstruction(method, i);

                    outputInstructions.AddRange(papyrusInstructions);

                    //    var pi = new PapyrusInstruction();

                    //    pi.OpCode = TranslateOpCode(i.OpCode.Code, options);

                    //    outputInstructions.Add(pi);
                }
            }
            catch (ProhibitedCodingBehaviourException)
            {
                throw new ProhibitedCodingBehaviourException(
                    method,
                    currentInstruction?.OpCode,
                    currentInstruction?.Offset);
            }
            return outputInstructions;
        }

        private IEnumerable<PapyrusInstruction> ProcessInstruction(MethodDefinition targetMethod, Instruction instruction)
        {
            var output = new List<PapyrusInstruction>();
            var type = targetMethod.DeclaringType;
            var code = instruction.OpCode.Code;
            if (InstructionHelper.IsLoad(code))
            {
                ParseLoadInstruction(targetMethod, instruction, type);
            }

            if (InstructionHelper.IsStore(code))
            {
                var instructions = ParseStoreInstruction(instruction);
                output.AddRange(instructions);
                return output;
            }

            if (InstructionHelper.IsCallMethod(instruction.OpCode.Code))
            {
                var methodRef = instruction.Operand as MethodReference;
                if (methodRef != null)
                {

                }
                else
                {
                    
                }
            }
            if (instruction.OpCode.Code == Code.Ret)
            {
                if (IsVoid(targetMethod.ReturnType))
                {
                    return new List<PapyrusInstruction>(new[] {
                        PapyrusReturnNone()
                    });
                }

                if (evaluationStack.Count >= Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
                {
                    var topValue = evaluationStack.Pop();
                    if (topValue.Value is VariableReference)
                    {
                        var variable = topValue.Value as VariableReference;
                        // return "Return " + variable.Name;
                        return new List<PapyrusInstruction>(new[] {
                            CreatePapyrusInstruction(PapyrusOpCode.Return, variable) // PapyrusReturnVariable(variable.Name)
                        });
                    }
                    else if (topValue.Value is PapyrusFieldDefinition)
                    {
                        var variable = topValue.Value as PapyrusFieldDefinition;
                        // return "Return " + variable.Name;
                        return new List<PapyrusInstruction>(new[] {
                            CreatePapyrusInstruction(PapyrusOpCode.Return, variable)
                        });
                    }
                    else if (IsConstantValue(topValue.Value))
                    {
                        var val = topValue.Value;

                        var typeName = topValue.TypeName;
                        var newValue = Utility.TypeValueConvert(typeName, val);
                        var papyrusVariableReference = new PapyrusVariableReference
                        {
                            TypeName = typeName.Ref(papyrusAssembly),
                            Value = newValue,
                            ValueType = Utility.GetPapyrusValueType(typeName)
                        };
                        return new List<PapyrusInstruction>(new[] {
                            CreatePapyrusInstruction(PapyrusOpCode.Return,
                            papyrusVariableReference
                            )
                        });
                    }
                }
                else
                {
                    return new List<PapyrusInstruction>(new[] {
                        PapyrusReturnNone()
                    });
                }
            }
            return output;
        }

        private IEnumerable<PapyrusInstruction> ParseStoreInstruction(Instruction instruction)
        {
            var allVariables = papyrusMethod.GetVariables();
            var isStoreLocalVariable = InstructionHelper.IsStoreLocalVariable(instruction.OpCode.Code);
            var isStoreField = InstructionHelper.IsStoreField(instruction.OpCode.Code);
            if (isStoreLocalVariable || isStoreField)
            {
                if (instruction.Operand is FieldReference)
                {
                    var fref = instruction.Operand as FieldReference;
                    // if the evaluationStack.Count == 0
                    // The previous instruction might have been a call that returned a value
                    // Something we did not store...
                    if (evaluationStack.Count > 0)
                    {
                        var obj = evaluationStack.Pop();

                        var definedField =
                            papyrusType.Fields.FirstOrDefault(
                                f => f.Name.Value == "::" + fref.Name.Replace('<', '_').Replace('>', '_'));
                        if (definedField != null)
                        {
                            if (obj.Value is PapyrusParameterDefinition)
                            {
                                var varRef = obj.Value as PapyrusParameterDefinition;
                                // definedField.FieldVariable = varRef.;
                                {
                                    return new[]
                                    {
                                        // CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField.Name.Value, varRef.Name.Value)
                                        CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField, varRef)
                                    };
                                }
                            }
                            if (obj.Value is PapyrusVariableReference)
                            {
                                var varRef = obj.Value as PapyrusVariableReference;
                                // definedField.Value = varRef.Value;
                                definedField.FieldVariable = varRef;
                                {
                                    return new[]
                                    {
                                        // CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField.Name.Value, varRef.Name.Value)
                                        CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField, varRef)
                                    };
                                }
                            }
                            definedField.FieldVariable.Value =
                                Utility.TypeValueConvert(definedField.FieldVariable.TypeName.Value, obj.Value);
                            {
                                return new[]
                                {
                                    CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField,
                                        definedField.FieldVariable.Value)
                                };
                            }

                            // "Assign " + definedField.Name + " " + definedField.Value;
                        }
                    }
                }
                var index = GetNumericValue(instruction);
                if (index < allVariables.Count)
                {
                    if (evaluationStack.Count > 0)
                    {
                        var heapObj = evaluationStack.Pop();
                        if (heapObj.Value is PapyrusVariableReference)
                        {
                            var varRef = heapObj.Value as PapyrusVariableReference;
                            allVariables[(int)index].Value = varRef.Value;
                            // "Assign " + allVariables[(int)index].Name.Value + " " + varRef.Name.Value;
                            {
                                return new[]
                                {
                                    CreatePapyrusInstruction(PapyrusOpCode.Assign, allVariables[(int) index].Name.Value,
                                        varRef.Name.Value)
                                };
                            }
                        }

                        allVariables[(int)index].Value =
                            Utility.TypeValueConvert(allVariables[(int)index].TypeName.Value, heapObj.Value);
                    }
                    var valout = allVariables[(int)index].Value;
                    var valoutStr = valout + "";
                    if (string.IsNullOrEmpty(valoutStr)) valoutStr = "None";
                    // "Assign " + allVariables[(int)index].Name.Value + " " + valoutStr;
                    {
                        return new[]
                        {
                            CreatePapyrusInstruction(PapyrusOpCode.Assign, allVariables[(int) index], valoutStr)
                        };
                    }
                }
            }
            return new PapyrusInstruction[0];
        }

        private bool IsConstantValue(object value)
        {
            return value is int || value is byte || value is short || value is long || value is double || value is float || value is string || value is bool;
        }

        private PapyrusInstruction CreatePapyrusInstruction(PapyrusOpCode papyrusOpCode, params object[] values)
        {
            var args = new List<PapyrusVariableReference>();
            foreach (var val in values)
            {
                var varRef = val as PapyrusVariableReference;
                var fieldDef = val as PapyrusFieldDefinition;
                var paramDef = val as PapyrusParameterDefinition;
                if (varRef != null)
                {
                    args.Add(varRef);
                }
                else if (fieldDef != null)
                {
                    args.Add(fieldDef.FieldVariable);
                }
                else if (paramDef != null)
                {
                    args.Add(CreateVariableReferenceFromName(paramDef.Name.Value));
                }
                else
                {
                    var vars = papyrusMethod.GetVariables();
                    var varName = val as string;
                    if (varName != null)
                    {
                        if (vars.Any(v => v.Name.Value == varName))
                        {
                            args.Add(CreateVariableReferenceFromName(varName));
                        }
                        else
                        {
                            args.Add(CreateVariableReference(PapyrusPrimitiveType.String, varName.Trim(
                                '"')));
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();

                        //args.Add(new PapyrusVariableReference()
                        //{
                        //    Name = GetPossibleVariableName???  // varName.Ref(papyrusAssembly),
                        //    ValueType = GetPrimitiveType(val); // PapyrusPrimitiveType.Reference 
                        //});
                    }
                }
            }

            return new PapyrusInstruction()
            {
                OpCode = papyrusOpCode,
                Arguments = args
            };

            /*new List<PapyrusVariableReference>(new[] { new PapyrusVariableReference()
                {
                    Name = name.Ref(papyrusAssembly),
                    ValueType = PapyrusPrimitiveType.Reference
                }})*/
        }

        private PapyrusVariableReference CreateVariableReference(PapyrusPrimitiveType papyrusPrimitiveType, object value)
        {
            return new PapyrusVariableReference
            {
                Value = value,
                ValueType = papyrusPrimitiveType
            };
        }

        private PapyrusVariableReference CreateVariableReferenceFromName(string varName)
        {
            var nameRef = varName.Ref(papyrusAssembly);
            return new PapyrusVariableReference()
            {
                Name = nameRef,
                Value = nameRef.Value,
                ValueType = PapyrusPrimitiveType.Reference
            };
        }

        private PapyrusInstruction PapyrusReturnVariable(string name)
        {
            return new PapyrusInstruction()
            {
                OpCode = PapyrusOpCode.Return,
                Arguments = new List<PapyrusVariableReference>(new[] { new PapyrusVariableReference()
                {
                    Name = name.Ref(papyrusAssembly),
                    ValueType = PapyrusPrimitiveType.Reference
                }})
            };
        }

        private static PapyrusInstruction PapyrusReturnNone()
        {
            return new PapyrusInstruction()
            {
                OpCode = PapyrusOpCode.Return,
                Arguments = new List<PapyrusVariableReference>(new[] { new PapyrusVariableReference()
                {
                    ValueType = PapyrusPrimitiveType.None
                }})
            };
        }

        private void ParseLoadInstruction(MethodDefinition targetMethod, Instruction instruction, TypeDefinition type)
        {
            if (InstructionHelper.IsLoadArgs(instruction.OpCode.Code))
            {
                var index = GetNumericValue(instruction);
                if (targetMethod.IsStatic && (int)index == 0 && targetMethod.Parameters.Count == 0)
                {
                    evaluationStack.Push(new EvaluationStackItem { IsThis = true, Value = type, TypeName = type.FullName });
                }
                else
                {
                    if (!targetMethod.IsStatic && index > 0) index--;
                    if (index < papyrusMethod.Parameters.Count)
                    {
                        evaluationStack.Push(new EvaluationStackItem
                        {
                            Value = papyrusMethod.Parameters[(int)index],
                            TypeName = papyrusMethod.Parameters[(int)index].TypeName.Value
                        });
                    }
                }
            }
            if (InstructionHelper.IsLoadInteger(instruction.OpCode.Code))
            {
                var index = GetNumericValue(instruction);
                evaluationStack.Push(new EvaluationStackItem { Value = index, TypeName = "Int" });
            }

            if (InstructionHelper.IsLoadNull(instruction.OpCode.Code))
            {
                evaluationStack.Push(new EvaluationStackItem { Value = "None", TypeName = "None" });
            }

            if (InstructionHelper.IsLoadLocalVariable(instruction.OpCode.Code))
            {
                var index = GetNumericValue(instruction);
                var allVariables = papyrusMethod.GetVariables();
                if (index < allVariables.Count)
                {
                    evaluationStack.Push(new EvaluationStackItem
                    {
                        Value = allVariables[(int)index],
                        TypeName = allVariables[(int)index].TypeName.Value
                    });
                }
            }

            if (InstructionHelper.IsLoadString(instruction.OpCode.Code))
            {
                var value = Utility.GetString(instruction.Operand);

                evaluationStack.Push(new EvaluationStackItem { Value = "\"" + value + "\"", TypeName = "String" });
            }

            if (InstructionHelper.IsLoadField(instruction.OpCode.Code))
            {
                if (instruction.Operand is FieldReference)
                {
                    var fref = instruction.Operand as FieldReference;

                    var definedField =
                        papyrusType.Fields.FirstOrDefault(
                            f => f.Name.Value == "::" + fref.Name.Replace('<', '_').Replace('>', '_'));
                    if (definedField != null)
                    {
                        evaluationStack.Push(new EvaluationStackItem
                        {
                            Value = definedField,
                            TypeName = definedField.TypeName
                        });
                    }
                }
            }
        }

        private bool IsVoid(TypeReference typeReference)
        {
            return typeReference.FullName.ToLower().Equals("system.void")
                   || typeReference.Name.ToLower().Equals("void");
        }

        private double GetNumericValue(Instruction instruction)
        {
            double index = InstructionHelper.GetCodeIndex(instruction.OpCode.Code);

            if ((int)index != -1)
            {
                return index;
            }

            if (instruction.Operand is ParameterReference)
            {
                // not yet implemented
                return 0;
            }
            if (instruction.Operand is FieldReference)
            {
                // not yet implemented
                return 0;
            }
            var variableReference = instruction.Operand as Mono.Cecil.Cil.VariableReference;
            if (variableReference != null)
            {
                var allVars = papyrusMethod.GetVariables();
                return Array.IndexOf(allVars.ToArray(),
                    allVars.FirstOrDefault(va => va.Name.Value == "V_" + variableReference.Index));
            }

            return IsNumeric(instruction.Operand)
                ? double.Parse(instruction.Operand.ToString())
                : int.Parse(instruction.Operand.ToString());
        }

        private bool IsNumeric(object obj)
        {
            return obj is double || obj is long || obj is float ||
                   obj is decimal;
        }
        /// <summary>
        ///     Translates the op code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public PapyrusOpCode TranslateOpCode(Code code, PapyrusCompilerOptions options)
        {
            /* Just going to simplify it as much as possible for now. */
            if (InstructionHelper.IsNewObjectInstance(code))
            {
                if (options == PapyrusCompilerOptions.Strict)
                    throw new ProhibitedCodingBehaviourException();
            }
            if (InstructionHelper.IsCallMethod(code))
            {
                return PapyrusOpCode.Callmethod;
            }

            if (InstructionHelper.IsNewArrayInstance(code))
            {
                return PapyrusOpCode.ArrayCreate;
            }

            if (code == Code.Ret)
                return PapyrusOpCode.Return;

            return PapyrusOpCode.Nop;
        }
    }
}