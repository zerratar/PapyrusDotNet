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


        private Dictionary<Instruction, List<PapyrusInstruction>> InstructionReferences = new Dictionary<Instruction, List<PapyrusInstruction>>();


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
                // 4 steps processing.
                // 1. Process all instructions as it is
                foreach (var i in instructions)
                {
                    currentInstruction = i;

                    InstructionReferences.Add(i, new List<PapyrusInstruction>());

                    if (i.OpCode.Code == Code.Nop)
                    {
                        var nop = CreatePapyrusInstruction(PapyrusOpCode.Nop);
                        outputInstructions.Add(nop);
                        InstructionReferences[i].Add(nop);
                    }

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

                    var papyrusInstructions = ProcessInstruction(method, i).ToList();

                    outputInstructions.AddRange(papyrusInstructions);

                    InstructionReferences[i] = papyrusInstructions;
                }

                // 2. Make sure we have set all our instruction's Previous and Next value
                AdjustInstructionChain(ref outputInstructions);

                // 3. Adjust all branch instructions to point at PapyrusInstructions instead of CIL Instruction
                foreach (var papyrusInstruction in outputInstructions)
                {
                    var inst = papyrusInstruction.Operand as Instruction;
                    if (inst != null)
                    {
                        // This is a jmp|jmpt|jmpf, so we need to adjust these ones.
                        var traversed = false;
                        var papyrusInstRefCollection = InstructionReferences[inst];
                        var tempInst = inst;
                        // if a value an instruction has been skipped, then we want to take the
                        // previous one and find the target papyrus instruction
                        while (papyrusInstRefCollection.Count == 0)
                        {
                            tempInst = tempInst.Previous;
                            if (tempInst == null) break;
                            papyrusInstRefCollection = InstructionReferences[tempInst];
                            traversed = true;
                        }
                        if (tempInst == null)
                        {
                            papyrusInstruction.Operand = null;
                            papyrusInstruction.Arguments.RemoveAt(papyrusInstruction.Arguments.Count - 1);
                        }
                        else
                        {
                            var target = papyrusInstRefCollection.First();
                            if (traversed && target.Next != null)
                            {
#warning we have a potential bug here, we may end up choosing the wrong instruction just because we do not know exactly which one to pick.
                                // All case scenarios so far seem to point that
                                // whenever we need to search backwards to find our destination
                                // we want to do <found instruction>.offset + 1
                                target = target.Next;
                            }
                            papyrusInstruction.Operand = target;
                            papyrusInstruction.Arguments[papyrusInstruction.Arguments.Count - 1].Value = papyrusInstruction.Operand;
                        }
                    }
                }

                // 3. Will automatically happen whenever the offsets are recalculated. At that point it will adjust any instructions to point to the correct offset
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

        private void AdjustInstructionChain(ref List<PapyrusInstruction> outputInstructions)
        {
            for (var i = 0; i < outputInstructions.Count; i++)
            {
                var instruction = outputInstructions[i];
                if (i > 0)
                {
                    var previous = outputInstructions[i - 1];
                    previous.Next = instruction;
                    instruction.Previous = previous;
                }
                if (i < outputInstructions.Count - 1)
                {
                    var next = outputInstructions[i + 1];
                    instruction.Next = next;
                    next.Previous = instruction;
                }
            }
        }

        private IEnumerable<PapyrusInstruction> ProcessInstruction(MethodDefinition targetMethod, Instruction instruction)
        {
            var output = new List<PapyrusInstruction>();
            var type = targetMethod.DeclaringType;
            var code = instruction.OpCode.Code;
            if (InstructionHelper.IsLoad(code))
            {
                ParseLoadInstruction(targetMethod, instruction, type, ref output);
            }

            if (InstructionHelper.IsStore(code))
            {
                output.AddRange(ParseStoreInstruction(instruction));

                return output;
            }


            if (InstructionHelper.IsUnboxing(code))
            {
                // Unboxing: From object to value type
                // Ex:  object o = 1; 
                //      int i = (int)o;
                // -----------------------------------------
                // the Type object does not exist in papyrus, which means we should never
                // have a object -> value type conversion.
                // ... Hopefully? Leave this as it is for now.
            }

            if (InstructionHelper.IsBoxing(code))
            {
                // Boxing: From value type to object
                // Ex:  int i = 1; 
                //      object o = i;
                // -----------------------------------------
                // When it comes to papyrus, the most similar action is casting an object to another type.
                // since there are no "Object" in Papyrus, their boxing and unboxing is direct cast between Type A and Type B
                // ------------------
                // Most of the time we can just ignore this instruction
                // as it implies on converting Type A into object before being used
                // as a parameter in a method, ex. string string.concat(object, object).
                // And since objects are not supported in papyrus, this kind of cast is completely unecessary.

                if (InstructionHelper.IsCallMethod(instruction.Next.OpCode.Code))
                {
                    return output;
                }

                // TODO: However, if it is not before calling a method, we might want to cast the source variable into a target type used for whatever?

                // var typeBeforeBoxing = instruction.Operand as Type;
            }

            if (InstructionHelper.IsMath(instruction.OpCode.Code))
            {
                // Must be 2.
                if (evaluationStack.Count >= Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
                {
                    // Make sure we have a temp variable if necessary
                    GetTargetVariable(instruction, null, "Int");

                    // Equiviliant Papyrus: <MathOp> <sumOutput> <denumerator> <numerator>

                    output.AddRange(GetConditional(instruction));
                }
            }

            if (InstructionHelper.IsConditional(instruction.OpCode.Code))
            {
                var conditionalInstructions = GetConditional(instruction);

                output.AddRange(conditionalInstructions);
            }


            if (InstructionHelper.IsBranchConditional(instruction.OpCode.Code))
            {
                var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
                if (evaluationStack.Count >= popCount)
                {
                    var obj1 = evaluationStack.Pop();
                    var obj2 = evaluationStack.Pop();
                    // gets or create a temp boolean variable we can use to store the conditional check on.
                    var temp = GetTargetVariable(instruction, null, "Bool");

                    var allVars = papyrusMethod.GetVariables();

                    var tempVar = allVars.FirstOrDefault(v => v.Name.Value == temp);

                    var destinationInstruction = instruction.Operand;

                    if (InstructionHelper.IsBranchConditionalEq(instruction.OpCode.Code))
                        output.Add(CreatePapyrusInstruction(PapyrusOpCode.CmpEq, tempVar, obj1, obj2));
                    else if (InstructionHelper.IsBranchConditionalLt(instruction.OpCode.Code))
                        output.Add(CreatePapyrusInstruction(PapyrusOpCode.CmpLt, tempVar, obj1, obj2));
                    else if (InstructionHelper.IsBranchConditionalGt(instruction.OpCode.Code))
                        output.Add(CreatePapyrusInstruction(PapyrusOpCode.CmpGt, tempVar, obj1, obj2));
                    else if (InstructionHelper.IsBranchConditionalGe(instruction.OpCode.Code))
                        output.Add(CreatePapyrusInstruction(PapyrusOpCode.CmpGte, tempVar, obj1, obj2));
                    else if (InstructionHelper.IsBranchConditionalGe(instruction.OpCode.Code))
                        output.Add(CreatePapyrusInstruction(PapyrusOpCode.CmpLte, tempVar, obj1, obj2));

                    if (InstructionHelper.IsBranchTrue(instruction.OpCode.Code))
                    {
                        var jmp = CreatePapyrusInstruction(PapyrusOpCode.Jmpt, tempVar, destinationInstruction);
                        jmp.Operand = destinationInstruction;
                        output.Add(jmp);
                        return output;
                    }
                    if (InstructionHelper.IsBranchFalse(instruction.OpCode.Code))
                    {
                        var jmp = CreatePapyrusInstruction(PapyrusOpCode.Jmpf, tempVar, destinationInstruction);
                        jmp.Operand = destinationInstruction;
                        output.Add(jmp);
                        return output;
                    }
                }
            }
            else if (InstructionHelper.IsBranch(instruction.OpCode.Code))
            {
                var stack = evaluationStack;
                var targetInstruction = instruction.Operand;

                if (stack.Count > 0)
                {
                    var conditionalVariable = stack.Pop();

                    if (InstructionHelper.IsBranchTrue(instruction.OpCode.Code))
                    {
                        var jmp = CreatePapyrusInstruction(PapyrusOpCode.Jmpt, conditionalVariable, targetInstruction);
                        jmp.Operand = targetInstruction;
                        output.Add(jmp);
                        return output;
                    }
                    if (InstructionHelper.IsBranchFalse(instruction.OpCode.Code))
                    {
                        var jmp = CreatePapyrusInstruction(PapyrusOpCode.Jmpf, conditionalVariable, targetInstruction);
                        jmp.Operand = targetInstruction;
                        output.Add(jmp);
                        return output;
                    }
                }

                var jmpInst = CreatePapyrusInstruction(PapyrusOpCode.Jmp, targetInstruction);
                jmpInst.Operand = targetInstruction;
                output.Add(jmpInst);
            }

            if (InstructionHelper.IsNewArrayInstance(instruction.OpCode.Code))
            {
                output.Add(CreateNewPapyrusArrayInstance(instruction));
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
                        output.AddRange(ProcessStringConcat(instruction, methodRef, parameters));
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
                    if (topValue.Value is PapyrusVariableReference)
                    {
                        var variable = topValue.Value as PapyrusVariableReference;
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

        private PapyrusInstruction CreateNewPapyrusArrayInstance(Instruction instruction)
        {
            var stack = evaluationStack;
            // we will need:
            // 1. The Array We want to create a new instance of
            // 2. The Size of the array.

            var arraySize = stack.Pop();
            var variables = papyrusMethod.GetVariables();

            int targetArrayIndex;
            GetNextStoreLocalVariableInstruction(instruction, out targetArrayIndex);
            var targetArrayItem = variables[targetArrayIndex];
            var targetArraySize = arraySize.Value;

            if (papyrusAssembly.VersionTarget == PapyrusVersionTargets.Skyrim)
            {
                // Skyrim does not accept dynamic array sizes, so we 
                // forces the target array size to a integer if one has tried to be used.
                if (!(targetArraySize is int))
                {
                    if (papyrusCompilerOptions == PapyrusCompilerOptions.Strict)
                    {
                        throw new ProhibitedCodingBehaviourException();
                    }
                    targetArraySize = 128;
                }

                if ((targetArraySize as int?) > 128)
                    targetArraySize = 128;
            }

            var arrayInstance = CreatePapyrusInstruction(PapyrusOpCode.ArrayCreate,
                targetArrayItem,
                targetArraySize);
            return arrayInstance;
        }

        private List<PapyrusInstruction> ProcessStringConcat(Instruction instruction, MethodReference methodRef, List<object> parameters)
        {
            List<PapyrusInstruction> output = new List<PapyrusInstruction>();
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
                if (stackItem != null)
                {
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
                            var valueToCast = CreateVariableReference(Utility.GetPrimitiveType(stackItem.Value),
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
            }

            // TODO: In case we want to concat more strings together or call a method using this new value
            // we will have to use the targetVariable above and push it back into the stack. (Or do we...?)
            return output;
        }

        private PapyrusInstruction CreatePapyrusCastInstruction(string destinationVariable, PapyrusVariableReference variableToCast)
        {
            return CreatePapyrusInstruction(PapyrusOpCode.Cast,
                CreateVariableReference(PapyrusPrimitiveType.Reference, destinationVariable),
                variableToCast);
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

            var papyrusOpCode = Utility.GetPapyrusMathOrEqualityOpCode(instruction.OpCode.Code, false);

            if (heapStack.Count >= 2) //Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
            {
                var numeratorObject = heapStack.Pop();
                var denumeratorObject = heapStack.Pop();
                var vars = papyrusMethod.GetVariables();
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
                    numerator = CreateVariableReference(Utility.GetPrimitiveType(numeratorObject.Value), numeratorObject.Value);
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
                    denumerator = CreateVariableReference(Utility.GetPrimitiveType(numeratorObject.Value), denumeratorObject.Value);
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



        private string GetTargetVariable(Instruction instruction, MethodReference methodRef, string fallbackType = null, bool forceNew = false)
        {
            string targetVar = null;
            var whereToPlace = instruction.Next;
            var allVariables = papyrusMethod.GetVariables();

            if (forceNew)
            {
                var tVar =
                  CreateTempVariable(
                      !string.IsNullOrEmpty(fallbackType) ? fallbackType : methodRef.ReturnType.FullName,
                      methodRef);
                targetVar = tVar.Name.Value;
                // evaluationStack.Push(new EvaluationStackItem { Value = tVar, TypeName = tVar.TypeName.Value });
                return targetVar;
            }

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
                      || InstructionHelper.IsLoadLength(instruction.OpCode.Code)))
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
            string name;
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
            var varnameRef = varname.Ref(papyrusAssembly);
            var typenameRef = type.Ref(papyrusAssembly);
            var varRef = new PapyrusVariableReference(varnameRef, typenameRef)
            {
                Value = varnameRef.Value,
                ValueType = PapyrusPrimitiveType.Reference
            };
            papyrusMethod.Body.TempVariables.Add(varRef);
            return varRef;
        }

        private IEnumerable<PapyrusInstruction> ParseStoreInstruction(Instruction instruction)
        {
            var allVariables = papyrusMethod.GetVariables();

            if (InstructionHelper.IsStoreElement(instruction.OpCode.Code))
            {
                var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
                if (evaluationStack.Count >= popCount)
                {
                    var newValue = evaluationStack.Pop();
                    var itemIndex = evaluationStack.Pop();
                    var itemArray = evaluationStack.Pop();

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

                    if (papyrusAssembly.VersionTarget == PapyrusVersionTargets.Skyrim)
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


                    return ArrayOf(CreatePapyrusInstruction(PapyrusOpCode.ArraySetElement, targetItemArray, targetItemIndex,
                            targetItemValue));
                    //return "ArraySetElement " + tar + " " + oidx + " " + val;
                }
            }

            if (InstructionHelper.IsStoreLocalVariable(instruction.OpCode.Code) || InstructionHelper.IsStoreField(instruction.OpCode.Code))
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

                                // CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField.Name.Value, varRef.Name.Value)
                                return ArrayOf(CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField, varRef));
                            }
                            if (obj.Value is PapyrusVariableReference)
                            {
                                var varRef = obj.Value as PapyrusVariableReference;
                                // definedField.Value = varRef.Value;
                                definedField.FieldVariable = varRef;
                                definedField.FieldVariable.ValueType = PapyrusPrimitiveType.Reference;
                                // CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField.Name.Value, varRef.Name.Value)
                                return ArrayOf(CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField, varRef));
                            }
                            //definedField.FieldVariable.Value =
                            //    Utility.TypeValueConvert(definedField.FieldVariable.TypeName.Value, obj.Value);
                            var targetValue = Utility.TypeValueConvert(definedField.FieldVariable.TypeName.Value,
                                obj.Value);

                            return ArrayOf(CreatePapyrusInstruction(PapyrusOpCode.Assign, definedField, targetValue));
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
                            return ArrayOf(CreatePapyrusInstruction(PapyrusOpCode.Assign, allVariables[index], varRef));
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
                        return ArrayOf(CreatePapyrusInstruction(PapyrusOpCode.Assign, allVariables[index], valout as PapyrusFieldDefinition));
                    }

                    if (valout is PapyrusVariableReference)
                    {
                        return ArrayOf(CreatePapyrusInstruction(PapyrusOpCode.Assign, allVariables[index], valout as PapyrusVariableReference));
                    }

                    if (valout is PapyrusParameterDefinition)
                    {
                        return ArrayOf(CreatePapyrusInstruction(PapyrusOpCode.Assign, allVariables[index], valout as PapyrusParameterDefinition));
                    }


                    // "Assign " + allVariables[(int)index].Name.Value + " " + valoutStr;

                    if (valout == null)
                    {
                        valout = "None";
                    }

                    return ArrayOf(CreatePapyrusInstruction(PapyrusOpCode.Assign, allVariables[index], valout));
                }
            }
            return new PapyrusInstruction[0];
        }

        private T[] ArrayOf<T>(params T[] items)
        {
            return items;
        }

        private bool IsConstantValue(object value)
        {
            return value is int || value is byte || value is short || value is long || value is double || value is float || value is string || value is bool;
        }

        private PapyrusInstruction CreatePapyrusInstruction(PapyrusOpCode papyrusOpCode, params object[] values)
        {
            var args = ParsePapyrusParameters(values);

            return new PapyrusInstruction()
            {
                OpCode = papyrusOpCode,
                Arguments = args
            };
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
                            args.Add(CreateVariableReference(PapyrusPrimitiveType.String, varName.Trim('"')));
                        }
                    }
                    else
                    {
                        var papyrusPrimitiveType = Utility.GetPrimitiveType(val);
                        args.Add(CreateVariableReference(papyrusPrimitiveType, val));
                    }
                }
            }
            return args;
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

        private void ParseLoadInstruction(MethodDefinition targetMethod, Instruction instruction, TypeDefinition type, ref List<PapyrusInstruction> outputInstructions)
        {
            if (InstructionHelper.IsLoadLength(instruction.OpCode.Code))
            {
                var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
                if (evaluationStack.Count >= popCount)
                {
                    var val = evaluationStack.Pop();
                    if (val.TypeName.EndsWith("[]"))
                    {
                        if (val.Value is PapyrusVariableReference)
                        {
                            int variableIndex;

                            var storeInstruction = GetNextStoreLocalVariableInstruction(instruction, out variableIndex);

                            if (storeInstruction != null ||
                                InstructionHelper.IsConverToNumber(instruction.Next.OpCode.Code))
                            {
                                if (InstructionHelper.IsConverToNumber(instruction.Next.OpCode.Code))
                                {
                                    skipNextInstruction = false;
                                    skipToOffset = 0;

                                    var targetVariableName = GetTargetVariable(instruction, null, "Int", true);

                                    var allVars = papyrusMethod.GetVariables();
                                    var targetVariable = allVars.FirstOrDefault(v => v.Name.Value == targetVariableName);

                                    if (targetVariable == null && papyrusCompilerOptions == PapyrusCompilerOptions.Strict)
                                    {
                                        throw new MissingVariableException(targetVariableName);
                                    }
                                    if (targetVariable != null)
                                    {
                                        evaluationStack.Push(new EvaluationStackItem
                                        {
                                            TypeName = targetVariable.TypeName.Value,
                                            Value = targetVariable
                                        });

                                        outputInstructions.Add(
                                            CreatePapyrusInstruction(PapyrusOpCode.ArrayLength,
                                                CreateVariableReference(PapyrusPrimitiveType.Reference, targetVariableName), val.Value));
                                    }
                                }
                                else
                                {
                                    var allVars = papyrusMethod.GetVariables();

                                    evaluationStack.Push(new EvaluationStackItem
                                    {
                                        TypeName = allVars[variableIndex].TypeName.Value,
                                        Value = allVars[variableIndex]
                                    });

                                    outputInstructions.Add(CreatePapyrusInstruction(PapyrusOpCode.ArrayLength, allVars[variableIndex], val.Value));

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
                var index = (int)GetNumericValue(instruction);
                if (targetMethod.IsStatic && index == 0 && targetMethod.Parameters.Count == 0)
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
                            Value = papyrusMethod.Parameters[index],
                            TypeName = papyrusMethod.Parameters[index].TypeName.Value
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
                        Value = allVariables[index],
                        TypeName = allVariables[index].TypeName.Value
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
                    var fieldRef = instruction.Operand as FieldReference;


                    PapyrusFieldDefinition targetField = null;

                    if (fieldRef.DeclaringType.FullName != type.FullName)
                    {
                        // The target field is not inside the declared type.
                        // Most likely, this is a get field from struct.
                        if (fieldRef.DeclaringType.FullName.Contains("/"))
                        {
                            var location = fieldRef.DeclaringType.FullName.Split("/").LastOrDefault();

                            var targetStruct = papyrusType.NestedTypes.FirstOrDefault(n => n.Name.Value == location);
                            if (targetStruct != null)
                            {


                                // TODO: Add support for getting values from Structs
                                // 
                                // CreatePapyrusInstruction(PapyrusOpCode.StructGet, ...)
                            }
                        }
                    }
                    else
                    {
                        targetField = papyrusType.Fields.FirstOrDefault(
                            f => f.Name.Value == "::" + fieldRef.Name.Replace('<', '_').Replace('>', '_'));
                    }

                    if (targetField != null)
                    {
                        evaluationStack.Push(new EvaluationStackItem
                        {
                            Value = targetField,
                            TypeName = targetField.TypeName
                        });
                    }
                }
            }


            if (InstructionHelper.IsLoadElement(instruction.OpCode.Code))
            {
                // TODO: Load Element (Arrays, and what not)
                var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
                if (evaluationStack.Count >= popCount)
                {
                    var itemIndex = evaluationStack.Pop();
                    var itemArray = evaluationStack.Pop();

                    object targetItemIndex = null;

                    var targetItemArray = itemArray.Value;

                    if (itemIndex.Value != null)
                    {
                        targetItemIndex = itemIndex.Value;
                    }

                    // 128 is the array size limit for Skyrim
                    if (papyrusAssembly.VersionTarget == PapyrusVersionTargets.Skyrim)
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
                                skipNextInstruction = true;

                                // Create a new Temp Variable
                                // Assign our value to this temp variable and push it to the stack
                                // so that the next instruction can take care of it.
                                var tempVariableType = sourceArray.TypeName.Value.Replace("[]", "");
                                var destinationTempVar = GetTargetVariable(instruction, null, tempVariableType, true);

                                var varRef =
                                    papyrusMethod.GetVariables().FirstOrDefault(n => n.Name.Value == destinationTempVar);

                                evaluationStack.Push(new EvaluationStackItem
                                {
                                    Value = varRef ?? (object)destinationTempVar, // Should be the actual variable reference
                                    TypeName = tempVariableType
                                });

                                outputInstructions.Add(
                                    CreatePapyrusInstruction(PapyrusOpCode.ArrayGetElement,
                                        CreateVariableReference(PapyrusPrimitiveType.Reference, destinationTempVar),
                                        targetItemArray,
                                        targetItemIndex
                                    )
                                );
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
                                    var destinationTempVar = GetTargetVariable(instruction, methodRef,
                                        tempVariableType);

                                    // "ArrayGetElement " + destinationTempVar + " " + targetItemArray + " " + targetItemIndex;
                                    outputInstructions.Add(
                                            CreatePapyrusInstruction(PapyrusOpCode.ArrayGetElement,
                                                CreateVariableReference(PapyrusPrimitiveType.Reference, destinationTempVar),
                                                targetItemArray,
                                                targetItemIndex
                                            )
                                        );
                                }
                            }
                        }
                    }
                    else
                    {
                        // Otherwise we just want to store it somewhere.
                        int destinationVariableIndex;
                        // Get the target variable by finding the next store instruction and returning the variable index.
                        GetNextStoreLocalVariableInstruction(instruction, out destinationVariableIndex);
                        var destinationVar = papyrusMethod.GetVariables()[destinationVariableIndex];

                        // ArrayGetElement targetVariable targetItemArray targetItemIndex
                        outputInstructions.Add(
                            CreatePapyrusInstruction(PapyrusOpCode.ArrayGetElement,
                                destinationVar,
                                targetItemArray,
                                targetItemIndex
                            )
                        );
                    }
                }
            }
        }

        public Instruction GetNextStoreLocalVariableInstruction(Instruction input, out int varIndex)
        {
            varIndex = -1;
            var next = input.Next;
            while (next != null && !InstructionHelper.IsStore(next.OpCode.Code))
            {
                next = next.Next;
            }

            if (next != null)
            {
                varIndex = (int)GetNumericValue(next);
                skipToOffset = next.Offset;
            }
            return next;
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
    }
}