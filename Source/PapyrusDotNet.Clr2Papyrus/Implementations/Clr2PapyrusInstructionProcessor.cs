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
        private PapyrusCompilerOptions papyrusCompilerOptions;
        private bool skipNextInstruction;
        private int skipToOffset = -1;

        /// <summary>
        ///     Processes the instructions.
        /// </summary>
        /// <param name="targetPapyrusAssembly"></param>
        /// <param name="targetPapyrusType"></param>
        /// <param name="targetPapyrusMethod"></param>
        /// <param name="method">The method.</param>
        /// <param name="body">The body.</param>
        /// <param name="instructions">The instructions.</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public IEnumerable<PapyrusInstruction> ProcessInstructions(PapyrusAssemblyDefinition targetPapyrusAssembly,
            PapyrusTypeDefinition targetPapyrusType,
            PapyrusMethodDefinition targetPapyrusMethod,
            MethodDefinition method,
            MethodBody body, Collection<Instruction> instructions,
            PapyrusCompilerOptions options = PapyrusCompilerOptions.Strict)
        {
            papyrusAssembly = targetPapyrusAssembly;
            papyrusType = targetPapyrusType;
            papyrusMethod = targetPapyrusMethod;
            papyrusCompilerOptions = options;

            Instruction currentInstruction = null;

            evaluationStack.Clear();

            var outputInstructions = new List<PapyrusInstruction>();
            try
            {
                foreach (var i in instructions)
                {
                    currentInstruction = i;

                    if (skipNextInstruction)
                    {
                        skipNextInstruction = false;
                        continue;
                    }

                    if (skipToOffset > 0)
                    {
                        if (i.Offset <= skipToOffset)
                        {
                            continue;
                        }
                        skipToOffset = -1;
                    }

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

            if (InstructionHelper.IsMath(instruction.OpCode.Code))
            {
                if (evaluationStack.Count >= Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
                {
                    // should be 2.
                    // Make sure we have a temp variable if necessary
                    var concatTargetVar = GetTargetVariable(instruction, null, "Int");

                    // Equiviliant Papyrus: StrCat <output> <val1> <val2>

                    //var castVal = "";
                    // string cast;
                    var instructions = GetConditional(instruction);
                    output.AddRange(instructions);


                    /* var retVal = Utility.GetPapyrusMathOp(instruction.OpCode.Code) + " " + value;
                    if (!string.IsNullOrEmpty(cast))
                        castVal = cast + Environment.NewLine + retVal; */
                    // return retVal;
                }
            }

            if (InstructionHelper.IsCallMethod(instruction.OpCode.Code))
            {
                var stack = evaluationStack;
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
                        if (papyrusCompilerOptions == PapyrusCompilerOptions.Strict)
                        {
                            throw new StackUnderflowException(targetMethod, instruction);
                        }
                        return output;
                    }

                    // Check if the current invoked Method is inside the same instance of this type
                    // by checking if it is using "this.Method(params);" << "this."...
                    var isThisCall = false;
                    var isStaticCall = false;
                    var callerLocation = "";
                    var parameters = new List<object>();
                    for (var paramIndex = 0; paramIndex < itemsToPop; paramIndex++)
                    {
                        var parameter = stack.Pop();
                        if (parameter.IsThis && evaluationStack.Count > methodRef.Parameters.Count
                            || methodRef.CallingConvention == MethodCallingConvention.ThisCall)
                        {
                            isThisCall = true;
                            callerLocation = "self"; // Location: 'self' is the same as 'this'
                        }
                        parameters.Insert(0, parameter);
                    }

                    var methodDefinition = TryResolveMethodReference(methodRef);
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
                        // Equiviliant Papyrus: StrCat <output_destination> <val1> <val2>

                        // 1. Make sure we have a temp variable if necessary
                        var destinationVariable = GetTargetVariable(instruction, methodRef);
                        //var targetVariable = evaluationStack.Peek();
                        //if (targetVariable.Value is PapyrusVariableReference)
                        //{
                        //    targetVariable = evaluationStack.Pop();
                        //}
                        //else
                        //{
                        //    targetVariable = null;
                        //}

                        for (var i = 0; i < parameters.Count; i++)
                        {
                            var stackItem = parameters[i] as EvaluationStackItem;
                            var targetVar = stackItem.Value as PapyrusVariableReference;
                            if (targetVar != null)
                            {
                                if (!stackItem.TypeName.ToLower().Contains("string"))
                                {
                                    // Not a string? Not a problem!
                                    // Since we already have a variable reference, we do not need to create an additional
                                    // temp variable before casting.
                                    // So we can go directly and do: cast ::temp0 ::awesomeVariable
                                    output.Add(CreatePapyrusCastInstruction(destinationVariable, targetVar));
                                }
                                output.Add(CreatePapyrusInstruction(PapyrusOpCode.Strcat,
                                    CreateVariableReference(PapyrusPrimitiveType.Reference, destinationVariable),
                                    CreateVariableReference(PapyrusPrimitiveType.Reference, destinationVariable),
                                    targetVar));
                            }
                            else
                            {
                                object value = stackItem.Value;
                                var newTempVar = false;
                                if (!stackItem.TypeName.ToLower().Contains("string"))
                                {
                                    // output.Add("Cast " + destinationVariable + " " + targetVar.Name.Value);
                                    // First, get a new temp variable of type string.
                                    // This new temp variable will be used for casting the source object into a string.
                                    value = GetTargetVariable(instruction, methodRef, "String", true);

                                    // Create a new temp variable that we use to assign our source object to.
                                    // this is so we avoid doing ex: cast ::temp0 55
                                    // and instead we do: cast ::temp0 ::temp1
                                    var valueToCastTemp = GetTargetVariable(instruction, methodRef, stackItem.TypeName, true);
                                    var valueToCast = CreateVariableReference(GetPrimitiveType(stackItem.Value),
                                        stackItem.Value);

                                    // Assign our newly created tempvalue with our object.
                                    // ex: assign ::temp1 55
                                    output.Add(CreatePapyrusInstruction(PapyrusOpCode.Assign,
                                        CreateVariableReference(PapyrusPrimitiveType.Reference, valueToCastTemp),
                                        valueToCast));

                                    // Cast the new ::temp1 to ::temp0 (equivilant to .ToString())
                                    output.Add(CreatePapyrusCastInstruction((string)value,
                                    CreateVariableReference(PapyrusPrimitiveType.Reference, valueToCastTemp)));
                                    newTempVar = true;

                                    // Make sure that our newly ::temp1 is used when concating the string.
                                    value = valueToCastTemp;
                                }
                                output.Add(CreatePapyrusInstruction(PapyrusOpCode.Strcat,
                                    CreateVariableReference(PapyrusPrimitiveType.Reference, destinationVariable),
                                    CreateVariableReference(PapyrusPrimitiveType.Reference, destinationVariable),
                                    CreateVariableReference(newTempVar
                                        ? PapyrusPrimitiveType.Reference
                                        : PapyrusPrimitiveType.String, value)));
                            }
                        }

                        // TODO: In case we want to concat more strings together or call a method using this new value
                        // we will have to use the targetVariable above and push it back into the stack. (Or do we...?)

                    }
                    else if (methodRef.Name.ToLower().Contains("op_equal") ||
                             methodRef.Name.ToLower().Contains("op_inequal"))
                    {
                        // TODO: Add Equality comparison
                    }
                    else
                    {
                        if (isStaticCall)
                        {
                            var destinationVariable = GetTargetVariable(instruction, methodRef);
                            return new List<PapyrusInstruction>(new[]
                            {
                                CreatePapyrusCallInstruction(PapyrusOpCode.Callstatic, methodRef, callerLocation,
                                    destinationVariable, parameters)
                            });
                        }
                        if (isThisCall)
                        {
                            var destinationVariable = GetTargetVariable(instruction, methodRef);
                            return new List<PapyrusInstruction>(new[]
                                {
                                    CreatePapyrusCallInstruction(PapyrusOpCode.Callmethod, methodRef, callerLocation,
                                        destinationVariable, parameters)
                                });
                        }
                    }
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
                    if (topValue.Value is PapyrusFieldDefinition)
                    {
                        var variable = topValue.Value as PapyrusFieldDefinition;
                        // return "Return " + variable.Name;
                        return new List<PapyrusInstruction>(new[] {
                            CreatePapyrusInstruction(PapyrusOpCode.Return, variable)
                        });
                    }
                    if (IsConstantValue(topValue.Value))
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

        private PapyrusInstruction CreatePapyrusCastInstruction(string destinationVariable, PapyrusVariableReference targetVar)
        {
            return CreatePapyrusInstruction(PapyrusOpCode.Cast,
                CreateVariableReference(PapyrusPrimitiveType.Reference, destinationVariable),
                targetVar);
        }

        private PapyrusInstruction CreatePapyrusCallInstruction(PapyrusOpCode callOpCode, MethodReference methodRef, string callerLocation, string destinationVariable, List<object> parameters)
        {
            var inst = new PapyrusInstruction();
            inst.OpCode = callOpCode;
            inst.Arguments.AddRange(ParsePapyrusParameters(new object[] {
                CreateVariableReference(PapyrusPrimitiveType.Reference, callerLocation),
                CreateVariableReference(PapyrusPrimitiveType.Reference, methodRef.Name),
                CreateVariableReference(PapyrusPrimitiveType.Reference, destinationVariable)}));
            inst.OperandArguments.AddRange(ParsePapyrusParameters(parameters.ToArray()));
            inst.Operand = methodRef;
            return inst;
        }

        public IEnumerable<PapyrusInstruction> GetConditional(Instruction instruction)
        {
            var output = new List<PapyrusInstruction>();

            //cast = null;

            var heapStack = evaluationStack;

#warning GetConditional only applies on Integers and must add support for Float further on.

            var papyrusOpCode = GetPapyrusMathOrEqualityOpCode(instruction.OpCode.Code, false);

            if (heapStack.Count >= 2) //Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
            {
                var numeratorObject = heapStack.Pop();
                var denumeratorObject = heapStack.Pop();
                var vars = papyrusMethod.GetVariables();
                var varIndex = 0;

                object numerator = null;
                object denumerator = null;

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
                    var refTypeName = varRef.TypeName.Value;

                    numerator = varRef;

                    // if not int or string, we need to cast it.
                    if (!refTypeName.ToLower().Equals("int") && !refTypeName.ToLower().Equals("system.int32") &&
                        !refTypeName.ToLower().Equals("system.string") && !refTypeName.ToLower().Equals("string"))
                    {
                        var typeVariable = GetTargetVariable(instruction, null, "Int");
                        output.Add(CreatePapyrusCastInstruction(typeVariable, varRef));
                        // cast = "Cast " + typeVariable + " " + value1;
                    }
                }
                else
                {
                    numerator = CreateVariableReference(GetPrimitiveType(numeratorObject.Value), numeratorObject.Value);
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

                        var typeVariable = GetTargetVariable(instruction, null, "Int");
                        output.Add(CreatePapyrusCastInstruction(typeVariable, varRef));

                    }
                }
                else
                {
                    denumerator = CreateVariableReference(GetPrimitiveType(numeratorObject.Value), denumeratorObject.Value);
                }

                // if (Utility.IsGreaterThan(code) || Utility.IsLessThan(code))
                {
                    var next = instruction.Next;
                    while (next != null &&
                           !InstructionHelper.IsStoreLocalVariable(next.OpCode.Code) &&
                           !InstructionHelper.IsStoreField(next.OpCode.Code) &&
                           !InstructionHelper.IsCallMethod(next.OpCode.Code))
                    {
                        next = next.Next;
                    }

                    if (instruction.Operand is MethodReference)
                    {
                        // if none found, create a temp one.
                        var methodRef = instruction.Operand as MethodReference;
                        var tVar = CreateTempVariable(methodRef.ReturnType.FullName);
                        var targetVar = tVar;

                        evaluationStack.Push(new EvaluationStackItem { Value = tVar.Value, TypeName = tVar.TypeName.Value });

                        output.Add(CreatePapyrusInstruction(papyrusOpCode, targetVar, denumerator, numerator));
                        return output;
                    }


                    if (next == null)
                    {
                        // No intentions to store this value into a variable, 
                        // Its to be used in a function call.
                        //return "NULLPTR " + denumerator + " " + numerator;
                        return output;
                    }

                    skipToOffset = next.Offset;
                    if (next.Operand is FieldReference)
                    {
                        var field = GetFieldFromStfld(next);
                        if (field != null)
                        {
                            output.Add(CreatePapyrusInstruction(papyrusOpCode, field, denumerator, numerator));
                            return output;
                            //return field.Name + " " + denumerator + " " + numerator;
                        }
                    }


                    varIndex = (int)GetNumericValue(next);
                }

                output.Add(CreatePapyrusInstruction(papyrusOpCode, vars[varIndex], denumerator, numerator));
                //return vars[varIndex].Name + " " + denumerator + " " + numerator;
            }
            return output;
        }

        public PapyrusFieldDefinition GetFieldFromStfld(Instruction whereToPlace)
        {
            if (InstructionHelper.IsStoreField(whereToPlace.OpCode.Code))
            {
                if (whereToPlace.Operand is FieldReference)
                {
                    var fref = whereToPlace.Operand as FieldReference;
                    // if the evaluationStack.Count == 0
                    // The previous instruction might have been a call that returned a value
                    // Something we did not store...
                    var definedField =
                        papyrusType.Fields.FirstOrDefault(
                            f => f.Name.Value == "::" + fref.Name.Replace('<', '_').Replace('>', '_'));
                    if (definedField != null)
                    {
                        return definedField;
                    }
                }
            }
            return null;
        }


        public static PapyrusOpCode GetPapyrusMathOrEqualityOpCode(Code code, bool isFloat)
        {
            if (InstructionHelper.IsLessThan(code))
            {
                return PapyrusOpCode.CmpLt;
            }
            if (InstructionHelper.IsEqualTo(code))
            {
                return PapyrusOpCode.CmpEq;
            }
            if (InstructionHelper.IsGreaterThan(code))
            {
                return PapyrusOpCode.CmpGt;
            }
            switch (code)
            {
                case Code.Add_Ovf:
                case Code.Add_Ovf_Un:
                case Code.Add:
                    return isFloat ? PapyrusOpCode.Fadd : PapyrusOpCode.Iadd;
                case Code.Sub:
                case Code.Sub_Ovf:
                case Code.Sub_Ovf_Un:
                    return isFloat ? PapyrusOpCode.Fsub : PapyrusOpCode.Isub;
                case Code.Div_Un:
                case Code.Div:
                    return isFloat ? PapyrusOpCode.Fdiv : PapyrusOpCode.Idiv;
                case Code.Mul:
                case Code.Mul_Ovf:
                case Code.Mul_Ovf_Un:
                    return isFloat ? PapyrusOpCode.Fmul : PapyrusOpCode.Imul;
                default:
                    return isFloat ? PapyrusOpCode.Fadd : PapyrusOpCode.Iadd;
            }
        }


        private string GetTargetVariable(Instruction instruction, MethodReference methodRef, string fallbackType = null,
        bool forceNew = false)
        {
            string targetVar = null;
            var whereToPlace = instruction.Next;
            var allVariables = papyrusMethod.GetVariables();
            if (whereToPlace != null &&
                (InstructionHelper.IsStoreLocalVariable(whereToPlace.OpCode.Code) ||
                 InstructionHelper.IsStoreField(whereToPlace.OpCode.Code)))
            {
                if (InstructionHelper.IsStoreField(whereToPlace.OpCode.Code))
                {
                    var fieldData = GetFieldFromStfld(whereToPlace);
                    if (fieldData != null)
                    {
                        targetVar = fieldData.Name.Value;
                        // LastSaughtTypeName = fieldData.TypeName;
                    }
                }
                else
                {
                    var index = (int)GetNumericValue(whereToPlace);

                    if (index < allVariables.Count)
                    {
                        targetVar = allVariables[index].Name.Value;
                        // LastSaughtTypeName = function.AllVariables[(int)index].TypeName;
                    }
                }
                skipNextInstruction = true;

                // else 
                //evaluationStack.Push(new EvaluationStackItem { IsMethodCall = true, Value = methodRef, TypeName = methodRef.ReturnType.FullName });
            }
            else if (whereToPlace != null &&
                     (InstructionHelper.IsLoad(whereToPlace.OpCode.Code) ||
                      InstructionHelper.IsCallMethod(whereToPlace.OpCode.Code) ||
                      InstructionHelper.IsBranchConditional(instruction.OpCode.Code)
                      || Utility.IsLoadLength(instruction.OpCode.Code)))
            {
                // Most likely this function call have a return value other than Void
                // and is used for an additional method call, witout being assigned to a variable first.

                // evaluationStack.Push(new EvaluationStackItem { IsMethodCall = true, Value = methodRef, TypeName = methodRef.ReturnType.FullName });

                var tVar =
                    CreateTempVariable(
                        !string.IsNullOrEmpty(fallbackType) ? fallbackType : methodRef.ReturnType.FullName,
                        methodRef);
                targetVar = tVar.Name.Value;
                evaluationStack.Push(new EvaluationStackItem { Value = tVar, TypeName = tVar.TypeName.Value });
                // LastSaughtTypeName = tVar.TypeName;
            }
            else
            {
                targetVar = "::NoneVar";
            }

            if (forceNew)
            {
                var tVar =
                  CreateTempVariable(
                      !string.IsNullOrEmpty(fallbackType) ? fallbackType : methodRef.ReturnType.FullName,
                      methodRef);
                targetVar = tVar.Name.Value;
                evaluationStack.Push(new EvaluationStackItem { Value = tVar, TypeName = tVar.TypeName.Value });
            }

            return targetVar;
        }


        private MethodDefinition TryResolveMethodReference(MethodReference methodRef)
        {
            try { return methodRef.Resolve(); }
            catch
            {
                // ignored
            }
            return null;
        }

        public PapyrusVariableReference CreateTempVariable(string variableName, MethodReference methodRef = null)
        {
            var originalTarget = variableName;
            if (variableName.StartsWith("!"))
            {
                // Get argument variable at index 1
                if (methodRef != null && methodRef.FullName.Contains("<") && methodRef.FullName.Contains(","))
                {
                    try
                    {
                        var pm = methodRef.FullName.TrimSplit("<")[1].TrimSplit(">")[0];
                        var vars = pm.TrimSplit(",");
                        var argIndex = int.Parse(variableName.Substring(1));
                        variableName = vars[argIndex];
                    }
                    catch
                    {
                        variableName = originalTarget;
                    }
                }
            }

            var @namespace = "";
            var name = "";
            if (variableName.Contains("."))
            {
                @namespace = variableName.Remove(variableName.LastIndexOf('.'));
                name = variableName.Split('.').LastOrDefault();
            }
            else
            {
                name = variableName;
            }

            var varname = "::temp" + papyrusMethod.Body.TempVariables.Count;
            var type = Utility.GetPapyrusReturnType(name, @namespace);
            // var def = ".local " + varname + " " + type.Replace("<T>", "");
            var varRef = new PapyrusVariableReference(varname.Ref(papyrusAssembly), type.Ref(papyrusAssembly))
            {
                ValueType = PapyrusPrimitiveType.Reference
            };
            papyrusMethod.Body.TempVariables.Add(varRef);
            return varRef;
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

                                return new[]
                                {
                                    // CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField.Name.Value, varRef.Name.Value)
                                    CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField, varRef)
                                };
                            }
                            if (obj.Value is PapyrusVariableReference)
                            {
                                var varRef = obj.Value as PapyrusVariableReference;
                                // definedField.Value = varRef.Value;
                                definedField.FieldVariable = varRef;
                                definedField.FieldVariable.ValueType = PapyrusPrimitiveType.Reference;
                                return new[]
                                {
                                    // CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField.Name.Value, varRef.Name.Value)
                                    CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField, varRef)
                                };
                            }
                            //definedField.FieldVariable.Value =
                            //    Utility.TypeValueConvert(definedField.FieldVariable.TypeName.Value, obj.Value);
                            var targetValue = Utility.TypeValueConvert(definedField.FieldVariable.TypeName.Value,
                                obj.Value);
                            return new[]
                            {
                                CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField,
                                    targetValue)
                            };
                            // definedField.FieldVariable.Value
                            // "Assign " + definedField.Name + " " + definedField.Value;
                        }
                    }
                }
                var index = (int)GetNumericValue(instruction);
                object outVal = null;
                if (index < allVariables.Count)
                {
                    if (evaluationStack.Count > 0)
                    {
                        var heapObj = evaluationStack.Pop();
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
                            return new[]
                            {
                                    CreatePapyrusInstruction(PapyrusOpCode.Assign, allVariables[index], varRef)
                                };
                        }

                        // allVariables[index].Value
                        outVal =
                            Utility.TypeValueConvert(allVariables[index].TypeName.Value, heapObj.Value);
                    }
                    var valout = outVal;//allVariables[index].Value;

                    string stringValue = "";

                    if (valout is string)
                    {
                        stringValue = valout.ToString();
                    }

                    if (valout is PapyrusFieldDefinition)
                    {
                        return new[] {
                            CreatePapyrusInstruction(PapyrusOpCode.Assign, allVariables[index], (valout as PapyrusFieldDefinition))
                        };
                    }

                    if (valout is PapyrusVariableReference)
                    {
                        return new[] {
                            CreatePapyrusInstruction(PapyrusOpCode.Assign, allVariables[index], (valout as PapyrusVariableReference))
                        };
                    }

                    if (valout is PapyrusParameterDefinition)
                    {
                        return new[] {
                            CreatePapyrusInstruction(PapyrusOpCode.Assign, allVariables[index], (valout as PapyrusParameterDefinition))
                        };
                    }

                    if (string.IsNullOrEmpty(stringValue))
                        stringValue = "None";

                    // "Assign " + allVariables[(int)index].Name.Value + " " + valoutStr;

                    return new[]
                    {
                        CreatePapyrusInstruction(PapyrusOpCode.Assign, allVariables[index], stringValue)
                    };

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

            if (papyrusOpCode == PapyrusOpCode.Assign)
            {

            }

            var args = ParsePapyrusParameters(values);

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

        private List<PapyrusVariableReference> ParsePapyrusParameters(object[] values)
        {
            var args = new List<PapyrusVariableReference>();
            foreach (var sourceVal in values)
            {
                var val = sourceVal;
                if (val is EvaluationStackItem)
                {
                    val = ((EvaluationStackItem)val).Value;
                }
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
                        var papyrusPrimitiveType = GetPrimitiveType(val);
                        args.Add(CreateVariableReference(papyrusPrimitiveType, val));
                        // throw new NotImplementedException();

                        //args.Add(new PapyrusVariableReference()
                        //{
                        //    Name = GetPossibleVariableName???  // varName.Ref(papyrusAssembly),
                        //    ValueType = GetPrimitiveType(val); // PapyrusPrimitiveType.Reference 
                        //});
                    }
                }
            }
            return args;
        }

        private PapyrusPrimitiveType GetPrimitiveType(object val)
        {
            var type = val.GetType();

            var typeName = Utility.GetPapyrusReturnType(type.FullName);

            return Utility.GetPapyrusValueType(typeName);
        }

        private PapyrusVariableReference CreateVariableReference(PapyrusPrimitiveType papyrusPrimitiveType, object value)
        {
            if (value + "" == "100")
            {
                var asd = GetPrimitiveType(value);
            }
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
                var index = (int)GetNumericValue(instruction);
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
                var index = (int)GetNumericValue(instruction);
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

                evaluationStack.Push(new EvaluationStackItem { Value = value, TypeName = "String" });
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

        private object GetNumericValue(Instruction instruction)
        {
            int index = InstructionHelper.GetCodeIndex(instruction.OpCode.Code);

            if (index != -1)
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
            var variableReference = instruction.Operand as VariableReference;
            if (variableReference != null)
            {
                var allVars = papyrusMethod.GetVariables();
                return Array.IndexOf(allVars.ToArray(),
                    allVars.FirstOrDefault(va => va.Name.Value == "V_" + variableReference.Index));
            }

            return instruction.Operand;
            // return IsNumeric(instruction.Operand) ? double.Parse(instruction.Operand.ToString()) : int.Parse(instruction.Operand.ToString());
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