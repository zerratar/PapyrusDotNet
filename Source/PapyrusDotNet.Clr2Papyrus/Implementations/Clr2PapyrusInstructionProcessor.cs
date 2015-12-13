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
using PapyrusDotNet.Converters.Clr2Papyrus.Implementations.Processors;
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
        public readonly Stack<EvaluationStackItem> EvaluationStack = new Stack<EvaluationStackItem>();
        public PapyrusMethodDefinition PapyrusMethod { get; set; }
        public PapyrusAssemblyDefinition PapyrusAssembly { get; set; }
        public PapyrusTypeDefinition PapyrusType { get; set; }
        public PapyrusCompilerOptions PapyrusCompilerOptions { get; set; }
        public bool SkipNextInstruction { get; set; }
        public int SkipToOffset = -1;


        private readonly Dictionary<Instruction, List<PapyrusInstruction>> instructionReferences
            = new Dictionary<Instruction, List<PapyrusInstruction>>();

        public bool invertedBranch;
        private bool isInsideSwitch;
        private Instruction[] switchTargetInstructions = new Instruction[0];
        private int switchTargetIndex;
        private PapyrusVariableReference switchConditionalComparer;
        private readonly PapyrusLoadInstructionProcessor papyrusLoadInstructionProcessor;
        private readonly PapyrusStoreInstructionProcessor papyrusStoreInstructionProcessor;
        private readonly PapyrusBranchInstructionProcessor papyrusBranchInstructionProcessor;
        private readonly PapyrusCallInstructionProcessor papyrusCallInstructionProcessor;

        public Clr2PapyrusInstructionProcessor()
        {
            papyrusLoadInstructionProcessor = new PapyrusLoadInstructionProcessor(this);
            papyrusStoreInstructionProcessor = new PapyrusStoreInstructionProcessor(this);
            papyrusBranchInstructionProcessor = new PapyrusBranchInstructionProcessor(this);
            papyrusCallInstructionProcessor = new PapyrusCallInstructionProcessor(this);
        }


        /// <summary>
        /// Processes the instructions.
        /// </summary>
        /// <param name="targetPapyrusAssembly">The target papyrus assembly.</param>
        /// <param name="targetPapyrusType">Type of the target papyrus.</param>
        /// <param name="targetPapyrusMethod">The target papyrus method.</param>
        /// <param name="method">The method.</param>
        /// <param name="body">The body.</param>
        /// <param name="instructions">The instructions.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public IEnumerable<PapyrusInstruction> ProcessInstructions(PapyrusAssemblyDefinition targetPapyrusAssembly,
            PapyrusTypeDefinition targetPapyrusType,
            PapyrusMethodDefinition targetPapyrusMethod,
            MethodDefinition method,
            MethodBody body, Collection<Instruction> instructions,
            PapyrusCompilerOptions options = PapyrusCompilerOptions.Strict)
        {
            PapyrusAssembly = targetPapyrusAssembly;
            PapyrusType = targetPapyrusType;
            PapyrusMethod = targetPapyrusMethod;
            PapyrusCompilerOptions = options;

            ClrMethod = method;
            ClrMethodBody = body;

            Instruction currentInstruction = null;

            EvaluationStack.Clear();

            var outputInstructions = new List<PapyrusInstruction>();
            try
            {
                // 4 steps processing.
                // 1. Process all instructions as it is
                foreach (var i in instructions)
                {
                    currentInstruction = i;

                    instructionReferences.Add(i, new List<PapyrusInstruction>());

                    // LEAVE FOR NOW:
                    // Add a NOP if there is a NOP in the CIL
                    // to try and maintain the same debugging information
                    // this is not necessary though and can be skipped.
                    if (i.OpCode.Code == Code.Nop)
                    {
                        var nop = CreatePapyrusInstruction(PapyrusOpCode.Nop);
                        outputInstructions.Add(nop);
                        instructionReferences[i].Add(nop);
                    }

                    // If the next (this) instruction should be skipped,
                    // this usually happens when the next instruction is not necessary
                    // to generate the papyrus necessary
                    if (SkipNextInstruction)
                    {
                        SkipNextInstruction = false;
                        continue;
                    }

                    // Skip a range of instructions
                    // this happens when there are more than one instruction that are unecessary
                    if (SkipToOffset > 0)
                    {
                        if (i.Offset <= SkipToOffset)
                        {
                            continue;
                        }
                        SkipToOffset = -1;
                    }

                    var papyrusInstructions = new List<PapyrusInstruction>();

                    // If we are inside a switch, we will need to take care of the
                    // jump table in a decent manner, if the next (/this) instruction is a branching
                    // (after the switch) then we want to skip that one.
                    if (!ProcessSwitchStatement(i, ref papyrusInstructions))
                    {
                        papyrusInstructions.AddRange(ProcessInstruction(i).ToList());
                    }

                    outputInstructions.AddRange(papyrusInstructions);

                    instructionReferences[i] = papyrusInstructions;
                }

                // 2. Make sure we have set all our instruction's Previous and Next value
                AdjustInstructionChain(ref outputInstructions);

                // 3. Adjust all branch instructions to point at PapyrusInstructions instead of CIL instructions
                AdjustBranching(ref outputInstructions);

                // 4. Will automatically happen whenever the offsets are recalculated. 
                //    as the offset are being recalculated, so are the destinations offsets
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

        public MethodBody ClrMethodBody { get; set; }

        public MethodDefinition ClrMethod { get; set; }

        private bool ProcessSwitchStatement(Instruction instruction, ref List<PapyrusInstruction> papyrusInstructions)
        {
            bool skipThisInstruction = false;
            // if we are inside a switch, and this current instruction is a destination in the switch jump table
            // we will need to insert an equality check and conditional jumpf

            // the switch table will always be before any other if statements
            // so we need to insert the equality compare + jumps directly.
            // -------- FOREACH DESTINATION ------------
            // if true jump to case ------------(INSERT)
            // jump to end of switch -----------(INSERT-LAST)
            // ---- UNKNOWN INSTRUCTIONS IN BETWEEN ----
            // do case  ------------------------(EXISTS)
            // jump to end of switch -----------(EXISTS)
            if (isInsideSwitch) // && switchTargetInstructions.Contains(i))
            {
                // numeric to compare: switchConditionalComparer
                // final destination: switchEndInstruction
                // keep track on the destination index and used for comparing: switchTargetIndex
                //      EX: ::tempBool = switchConditionalComparer == switchTargetIndex
                //          Jumpf ::tempBool switchEndInstruction

                foreach (var dest in switchTargetInstructions)
                {
                    // Create a temp variable
                    var tmpBool = GetTargetVariable(instruction, null, "Bool");

                    // Push the values to the stack
                    EvaluationStack.Push(new EvaluationStackItem
                    {
                        TypeName = switchConditionalComparer.TypeName.Value,
                        Value = switchConditionalComparer
                    });
                    EvaluationStack.Push(new EvaluationStackItem { TypeName = "Int", Value = switchTargetIndex++ });

                    // Create the equality comparison
                    var conditional = GetConditional(instruction, Code.Ceq, tmpBool);
                    papyrusInstructions.AddRange(conditional);

                    // Create the jump to case if true
                    var jumpT = ConditionalJump(PapyrusOpCode.Jmpt, CreateVariableReferenceFromName(tmpBool), dest);
                    papyrusInstructions.Add(jumpT);
                }

                if (switchTargetIndex >= switchTargetInstructions.Length)
                {
                    //// create the jump to end
                    //var jump = CreatePapyrusInstruction(PapyrusOpCode.Jmp, switchEndInstruction);
                    //jump.Operand = switchEndInstruction;
                    //papyrusInstructions.Add(jump);

                    // Stop switch
                    switchConditionalComparer = null;
                    switchTargetIndex = 0;
                    switchTargetInstructions = new Instruction[0];
                    isInsideSwitch = false;
                    skipThisInstruction = InstructionHelper.IsBranch(instruction.OpCode.Code);
                    // skip the this instruction if it's branching
                }
            }
            return skipThisInstruction;
        }

        private void AdjustBranching(ref List<PapyrusInstruction> outputInstructions)
        {
            // TODO: 3. For the future: We can insert a NOP at each jump's destination and add a value to the operand
            //      used for verifying that the position is correct, and then use that NOP instruction as the new
            //      target, then we can make sure we always have the correct destination.
            //  --------------------------------------------
            //      ( Branching is found -> Take offset of that branch destination -> when the offset is reached
            //         add a NOP and set the operand to the offset -> when all instructions parsed, adjust the branching
            //          -> set the new operand of the jumps to the NOP instruction and remove the operand from the NOP )
            //  -------------------------------------------
            // 3. For now: Adjust all branch instructions to point at PapyrusInstructions instead of CIL Instruction
            //              by looking at the instructionReferences 
            //              ( What Papyrus Instructions was produced by the specific 
            //                  destination CIL Instruction and take the first )
            foreach (var papyrusInstruction in outputInstructions)
            {
                var inst = papyrusInstruction.Operand as Instruction;
                if (inst != null)
                {
                    // This is a jmp|jmpt|jmpf, so we need to adjust these ones.
                    var traversed = false;
                    var papyrusInstRefCollection = instructionReferences[inst];
                    var tempInst = inst;
                    // if a value an instruction has been skipped, then we want to take the
                    // previous one and find the target papyrus instruction
                    while (papyrusInstRefCollection.Count == 0)
                    {
                        // if we intend to load something and then use it in a method call, we
                        // have most likely added the actual method call as a reference since we do not
                        // add "loosly" hanging loads unless they are actually used as a variable.                       
                        if (InstructionHelper.IsLoad(tempInst.OpCode.Code) &&
                            InstructionHelper.IsCallMethod(tempInst.Next.OpCode.Code))
                        {
                            papyrusInstRefCollection = instructionReferences[tempInst.Next];
                            continue;
                        }
                        tempInst = tempInst.Previous;
                        if (tempInst == null) break;
                        papyrusInstRefCollection = instructionReferences[tempInst];
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
                            // TODO: we have a potential bug here, we may end up choosing the wrong instruction just because we do not know exactly which one to pick.
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

        private IEnumerable<PapyrusInstruction> ProcessInstruction(Instruction instruction)
        {
            var targetMethod = ClrMethod;
            var output = new List<PapyrusInstruction>();
            var type = targetMethod.DeclaringType;
            var code = instruction.OpCode.Code;
            if (InstructionHelper.IsLoad(code))
            {
                output.AddRange(papyrusLoadInstructionProcessor.Process(instruction, targetMethod, type));
            }

            if (InstructionHelper.IsStore(code))
            {
                output.AddRange(papyrusStoreInstructionProcessor.Process(instruction, targetMethod, type));

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
                if (EvaluationStack.Count >= Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
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

            if (InstructionHelper.IsBranch(instruction.OpCode.Code) || InstructionHelper.IsBranchConditional(instruction.OpCode.Code))
            {
                output.AddRange(papyrusBranchInstructionProcessor.Process(instruction, targetMethod, type));

                return output;
            }
            if (InstructionHelper.IsSwitch(instruction.OpCode.Code))
            {
                var switchTargets = instruction.Operand as Instruction[];
                // we need to convert the switch targets into if else statements,
                // 1. save these targets, we need to match any upcoming instructions against them 
                // 2. if a match, insert a equality compare <N == switch case index>
                //      and then a jmpf <condition> <end_of_swith>
                // -- that should be it! :-) we will have to figure out default cases later on.
                isInsideSwitch = true;
                switchTargetInstructions = switchTargets;
                switchTargetIndex = 0;

                // find the variable to compare with; used for the conditional bool temp var
                switchConditionalComparer = switchConditionalComparer ?? EvaluationStack.Peek().Value as PapyrusVariableReference;
            }
            if (InstructionHelper.IsNewArrayInstance(instruction.OpCode.Code))
            {
                output.Add(CreateNewPapyrusArrayInstance(instruction));
            }

            if (InstructionHelper.IsCallMethod(instruction.OpCode.Code))
            {
                output.AddRange(papyrusCallInstructionProcessor.Process(instruction, targetMethod, type));
            }
            if (instruction.OpCode.Code == Code.Ret)
            {
                IEnumerable<PapyrusInstruction> enumerable;
                if (ProcessReturnInstruction(targetMethod, instruction, out enumerable)) return enumerable;
            }
            return output;
        }

        private bool ProcessReturnInstruction(MethodDefinition targetMethod, Instruction instruction, out IEnumerable<PapyrusInstruction> enumerable)
        {
            enumerable = null;
            if (IsVoid(targetMethod.ReturnType))
            {
                enumerable = new List<PapyrusInstruction>(new[]
                {
                    PapyrusReturnNone()
                });
                return true;
            }

            if (EvaluationStack.Count >= Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
            {
                var topValue = EvaluationStack.Pop();
                if (topValue.Value is PapyrusVariableReference)
                {
                    var variable = topValue.Value as PapyrusVariableReference;
                    // return "Return " + variable.Name;
                    {
                        enumerable = new List<PapyrusInstruction>(new[]
                        {
                            CreatePapyrusInstruction(PapyrusOpCode.Return, variable) // PapyrusReturnVariable(variable.Name)
                        });
                        return true;
                    }
                }
                if (topValue.Value is PapyrusFieldDefinition)
                {
                    var variable = topValue.Value as PapyrusFieldDefinition;
                    // return "Return " + variable.Name;
                    {
                        enumerable = new List<PapyrusInstruction>(new[]
                        {
                            CreatePapyrusInstruction(PapyrusOpCode.Return, variable)
                        });
                        return true;
                    }
                }
                if (IsConstantValue(topValue.Value))
                {
                    var val = topValue.Value;

                    var typeName = topValue.TypeName;
                    var newValue = Utility.TypeValueConvert(typeName, val);
                    var papyrusVariableReference = new PapyrusVariableReference
                    {
                        TypeName = typeName.Ref(PapyrusAssembly),
                        Value = newValue,
                        ValueType = Utility.GetPapyrusValueType(typeName)
                    };
                    {
                        enumerable = new List<PapyrusInstruction>(new[]
                        {
                            CreatePapyrusInstruction(PapyrusOpCode.Return,
                                papyrusVariableReference
                                )
                        });
                        return true;
                    }
                }
            }
            else
            {
                {
                    enumerable = new List<PapyrusInstruction>(new[]
                    {
                        PapyrusReturnNone()
                    });
                    return true;
                }
            }
            return false;
        }


        public List<PapyrusInstruction> ProcessStringConcat(Instruction instruction, MethodReference methodRef, List<object> parameters)
        {
            List<PapyrusInstruction> output = new List<PapyrusInstruction>();
            // Equiviliant Papyrus: StrCat <output_destination> <val1> <val2>

            // Make sure we have a temp variable if necessary
            var destinationVariable = GetTargetVariable(instruction, methodRef);

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
                        var value = stackItem.Value;
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
                            var valueToCast = CreateVariableReference(Utility.GetPrimitiveTypeFromValue(stackItem.Value),
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


        public IEnumerable<PapyrusInstruction> GetConditional(Instruction instruction, Code overrideOpCode = Code.Nop, string tempVariable = null)
        {
            var output = new List<PapyrusInstruction>();

            //cast = null;

            var heapStack = EvaluationStack;

            // TODO: GetConditional only applies on Integers and must add support for Float further on.

            var papyrusOpCode = Utility.GetPapyrusMathOrEqualityOpCode(overrideOpCode != Code.Nop ? overrideOpCode : instruction.OpCode.Code, false);

            if (heapStack.Count >= 2) //Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
            {
                var numeratorObject = heapStack.Pop();
                var denumeratorObject = heapStack.Pop();
                var vars = PapyrusMethod.GetVariables();
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
                    numerator = CreateVariableReference(Utility.GetPrimitiveTypeFromValue(numeratorObject.Value), numeratorObject.Value);
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
                    denumerator = CreateVariableReference(Utility.GetPrimitiveTypeFromValue(numeratorObject.Value), denumeratorObject.Value);
                }

                if (!string.IsNullOrEmpty(tempVariable))
                {
                    output.Add(CreatePapyrusInstruction(papyrusOpCode, CreateVariableReferenceFromName(tempVariable), denumerator, numerator));
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
                        var newTempVariable = GetTargetVariable(instruction, null, "Int", true);
                        switchConditionalComparer = CreateVariableReferenceFromName(newTempVariable);
                        switchConditionalComparer.ValueType = PapyrusPrimitiveType.Reference;
                        switchConditionalComparer.TypeName = "Int".Ref(PapyrusAssembly);

                        output.Add(CreatePapyrusInstruction(papyrusOpCode, switchConditionalComparer, denumerator, numerator));
                        return output;
                    }

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

                        EvaluationStack.Push(new EvaluationStackItem { Value = tVar.Value, TypeName = tVar.TypeName.Value });

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

                    SkipToOffset = next.Offset;
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
                    // if the EvaluationStack.Count == 0
                    // The previous instruction might have been a call that returned a value
                    // Something we did not store...
                    var definedField =
                        PapyrusType.Fields.FirstOrDefault(
                            f => f.Name.Value == "::" + fref.Name.Replace('<', '_').Replace('>', '_'));
                    if (definedField != null)
                    {
                        return definedField;
                    }
                }
            }
            return null;
        }


        public string GetTargetVariable(Instruction instruction, MethodReference methodRef, string fallbackType = null, bool forceNew = false)
        {
            string targetVar = null;
            var whereToPlace = instruction.Next;
            var allVariables = PapyrusMethod.GetVariables();

            if (forceNew)
            {
                var tVar =
                  CreateTempVariable(
                      !string.IsNullOrEmpty(fallbackType) ? fallbackType : methodRef.ReturnType.FullName,
                      methodRef);
                targetVar = tVar.Name.Value;
                // EvaluationStack.Push(new EvaluationStackItem { Value = tVar, TypeName = tVar.TypeName.Value });
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
                SkipNextInstruction = true;

                // else 
                //EvaluationStack.Push(new EvaluationStackItem { IsMethodCall = true, Value = methodRef, TypeName = methodRef.ReturnType.FullName });
            }
            else if (whereToPlace != null &&
                     (InstructionHelper.IsLoad(whereToPlace.OpCode.Code) ||
                      InstructionHelper.IsCallMethod(whereToPlace.OpCode.Code) ||
                      InstructionHelper.IsBranchConditional(instruction.OpCode.Code)
                      || InstructionHelper.IsLoadLength(instruction.OpCode.Code)))
            {
                // Most likely this function call have a return value other than Void
                // and is used for an additional method call, witout being assigned to a variable first.

                // EvaluationStack.Push(new EvaluationStackItem { IsMethodCall = true, Value = methodRef, TypeName = methodRef.ReturnType.FullName });

                var tVar =
                    CreateTempVariable(
                        !string.IsNullOrEmpty(fallbackType) ? fallbackType : methodRef.ReturnType.FullName,
                        methodRef);
                targetVar = tVar.Name.Value;
                EvaluationStack.Push(new EvaluationStackItem { Value = tVar, TypeName = tVar.TypeName.Value });
                // LastSaughtTypeName = tVar.TypeName;
            }
            else
            {
                targetVar = "::NoneVar";
            }


            return targetVar;
        }


        public PapyrusOpCode TryInvertJump(PapyrusOpCode jmpt)
        {
            if (!invertedBranch) return jmpt;
            invertedBranch = false;
            if (jmpt == PapyrusOpCode.Jmpt)
                return PapyrusOpCode.Jmpf;
            return PapyrusOpCode.Jmpt;
        }

        public PapyrusInstruction ConditionalJump(PapyrusOpCode jumpType, PapyrusVariableReference conditionalVar,
            object destinationInstruction)
        {
            var jmpOp = TryInvertJump(jumpType);
            var jmp = CreatePapyrusInstruction(jmpOp, conditionalVar, destinationInstruction);
            jmp.Operand = destinationInstruction;
            return jmp;
        }

        private PapyrusInstruction CreateNewPapyrusArrayInstance(Instruction instruction)
        {
            var stack = EvaluationStack;
            // we will need:
            // 1. The Array We want to create a new instance of
            // 2. The Size of the array.

            var arraySize = stack.Pop();
            var variables = PapyrusMethod.GetVariables();

            int targetArrayIndex;
            GetNextStoreLocalVariableInstruction(instruction, out targetArrayIndex);
            var targetArrayItem = variables[targetArrayIndex];
            var targetArraySize = arraySize.Value;

            if (PapyrusAssembly.VersionTarget == PapyrusVersionTargets.Skyrim)
            {
                // Skyrim does not accept dynamic array sizes, so we 
                // forces the target array size to a integer if one has tried to be used.
                if (!(targetArraySize is int))
                {
                    if (PapyrusCompilerOptions == PapyrusCompilerOptions.Strict)
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

        private PapyrusInstruction CreatePapyrusCastInstruction(string destinationVariable, PapyrusVariableReference variableToCast)
        {
            return CreatePapyrusInstruction(PapyrusOpCode.Cast,
                CreateVariableReference(PapyrusPrimitiveType.Reference, destinationVariable),
                variableToCast);
        }

        public PapyrusInstruction CreatePapyrusCallInstruction(PapyrusOpCode callOpCode, MethodReference methodRef, string callerLocation, string destinationVariable, List<object> parameters)
        {
            var inst = new PapyrusInstruction { OpCode = callOpCode };
            if (callOpCode == PapyrusOpCode.Callstatic)
            {
                inst.Arguments.AddRange(ParsePapyrusParameters(new object[] {
                CreateVariableReference(PapyrusPrimitiveType.Reference, callerLocation),
                CreateVariableReference(PapyrusPrimitiveType.Reference, methodRef.Name),
                CreateVariableReference(PapyrusPrimitiveType.Reference, destinationVariable)}));
            }
            else
            {
                inst.Arguments.AddRange(ParsePapyrusParameters(new object[] {
                CreateVariableReference(PapyrusPrimitiveType.Reference, methodRef.Name),
                CreateVariableReference(PapyrusPrimitiveType.Reference, callerLocation),
                CreateVariableReference(PapyrusPrimitiveType.Reference, destinationVariable)}));
            }
            inst.OperandArguments.AddRange(EnsureParameterTypes(methodRef.Parameters, ParsePapyrusParameters(parameters.ToArray())));
            inst.Operand = methodRef;
            return inst;
        }

        private IEnumerable<PapyrusVariableReference> EnsureParameterTypes(Collection<ParameterDefinition> parameters, List<PapyrusVariableReference> papyrusParams)
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
                    papyrusParams[i].TypeName = papyrusReturnType.Ref(PapyrusAssembly);
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


        public MethodDefinition TryResolveMethodReference(MethodReference methodRef)
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

            var varname = "::temp" + PapyrusMethod.Body.TempVariables.Count;
            var type = Utility.GetPapyrusReturnType(name, @namespace);
            // var def = ".local " + varname + " " + type.Replace("<T>", "");
            var varnameRef = varname.Ref(PapyrusAssembly);
            var typenameRef = type.Ref(PapyrusAssembly);
            var varRef = new PapyrusVariableReference(varnameRef, typenameRef)
            {
                Value = varnameRef.Value,
                ValueType = PapyrusPrimitiveType.Reference
            };
            PapyrusMethod.Body.TempVariables.Add(varRef);
            return varRef;
        }

        public T[] ArrayOf<T>(params T[] items)
        {
            return items;
        }

        private bool IsConstantValue(object value)
        {
            return value is int || value is byte || value is short || value is long || value is double || value is float || value is string || value is bool;
        }

        public PapyrusInstruction CreatePapyrusInstruction(PapyrusOpCode papyrusOpCode, params object[] values)
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
                    var vars = PapyrusMethod.GetVariables();
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
                        var papyrusPrimitiveType = Utility.GetPrimitiveTypeFromValue(val);
                        args.Add(CreateVariableReference(papyrusPrimitiveType, val));
                    }
                }
            }
            return args;
        }


        public PapyrusVariableReference CreateVariableReference(PapyrusPrimitiveType papyrusPrimitiveType, object value)
        {
            return new PapyrusVariableReference
            {
                Value = value,
                ValueType = papyrusPrimitiveType
            };
        }

        private PapyrusVariableReference CreateVariableReferenceFromName(string varName)
        {
            var nameRef = varName.Ref(PapyrusAssembly);
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
                SkipToOffset = next.Offset;
            }
            return next;
        }


        private bool IsVoid(TypeReference typeReference)
        {
            return typeReference.FullName.ToLower().Equals("system.void")
                   || typeReference.Name.ToLower().Equals("void");
        }

        public object GetNumericValue(Instruction instruction)
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
                var allVars = PapyrusMethod.GetVariables();
                return Array.IndexOf(allVars.ToArray(),
                    allVars.FirstOrDefault(va => va.Name.Value == "V_" + variableReference.Index));
            }

            return instruction.Operand;
            // return IsNumeric(instruction.Operand) ? double.Parse(instruction.Operand.ToString()) : int.Parse(instruction.Operand.ToString());
        }
    }
}