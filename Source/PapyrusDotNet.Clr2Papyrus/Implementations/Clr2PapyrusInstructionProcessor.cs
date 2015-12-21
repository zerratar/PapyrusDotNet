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
using PapyrusDotNet.Common.Utilities;
using PapyrusDotNet.Converters.Clr2Papyrus.Enums;
using PapyrusDotNet.Converters.Clr2Papyrus.Exceptions;
using PapyrusDotNet.Converters.Clr2Papyrus.Implementations.Processors;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Extensions;

#endregion

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations
{
    public class Clr2PapyrusInstructionProcessor : IClr2PapyrusInstructionProcessor
    {
        public Stack<EvaluationStackItem> EvaluationStack { get; set; } = new Stack<EvaluationStackItem>();
        public PapyrusMethodDefinition PapyrusMethod { get; set; }
        public PapyrusAssemblyDefinition PapyrusAssembly { get; set; }
        public PapyrusTypeDefinition PapyrusType { get; set; }
        public PapyrusCompilerOptions PapyrusCompilerOptions { get; set; }
        public bool SkipNextInstruction { get; set; }
        public int SkipToOffset { get; set; } = -1;
        public bool InvertedBranch { get; set; }
        public PapyrusVariableReference SwitchConditionalComparer { get; set; }
        private readonly Dictionary<Instruction, List<PapyrusInstruction>> instructionReferences
            = new Dictionary<Instruction, List<PapyrusInstruction>>();

        public MethodBody ClrMethodBody { get; set; }

        public MethodDefinition ClrMethod { get; set; }


        private bool isInsideSwitch;
        private Instruction[] switchTargetInstructions = new Instruction[0];
        private int switchTargetIndex;

        internal readonly LoadInstructionProcessor LoadInstructionProcessor;
        internal readonly StoreInstructionProcessor StoreInstructionProcessor;
        internal readonly BranchInstructionProcessor BranchInstructionProcessor;
        internal readonly CallInstructionProcessor CallInstructionProcessor;
        internal readonly ConditionalInstructionProcessor ConditionalInstructionProcessor;
        internal readonly ReturnInstructionProcessor ReturnInstructionProcessor;
        internal readonly StringConcatInstructionProcessor StringConcatInstructionProcessor;

        public Clr2PapyrusInstructionProcessor()
        {
            LoadInstructionProcessor = new LoadInstructionProcessor(this);
            StoreInstructionProcessor = new StoreInstructionProcessor(this);
            BranchInstructionProcessor = new BranchInstructionProcessor(this);
            CallInstructionProcessor = new CallInstructionProcessor(this);
            ConditionalInstructionProcessor = new ConditionalInstructionProcessor(this);
            ReturnInstructionProcessor = new ReturnInstructionProcessor(this);
            StringConcatInstructionProcessor = new StringConcatInstructionProcessor(this);
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
                        var nop = CreatePapyrusInstruction(PapyrusOpCodes.Nop);
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
                        TypeName = SwitchConditionalComparer.TypeName.Value,
                        Value = SwitchConditionalComparer
                    });
                    EvaluationStack.Push(new EvaluationStackItem { TypeName = "Int", Value = switchTargetIndex++ });

                    // Create the equality comparison
                    var conditional = ProcessConditionalInstruction(instruction, Code.Ceq, tmpBool);
                    papyrusInstructions.AddRange(conditional);

                    // Create the jump to case if true
                    var jumpT = ConditionalJump(PapyrusOpCodes.Jmpt, CreateVariableReferenceFromName(tmpBool), dest);
                    papyrusInstructions.Add(jumpT);
                }

                if (switchTargetIndex >= switchTargetInstructions.Length)
                {
                    //// create the jump to end
                    //var jump = CreatePapyrusInstruction(PapyrusOpCode.Jmp, switchEndInstruction);
                    //jump.Operand = switchEndInstruction;
                    //papyrusInstructions.Add(jump);

                    // Stop switch
                    SwitchConditionalComparer = null;
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
                output.AddRange(LoadInstructionProcessor.Process(instruction, targetMethod, type));
            }

            if (InstructionHelper.IsStore(code))
            {
                output.AddRange(StoreInstructionProcessor.Process(instruction, targetMethod, type));

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

                    output.AddRange(ProcessConditionalInstruction(instruction));
                }
            }

            if (InstructionHelper.IsConditional(instruction.OpCode.Code))
            {
                var conditionalInstructions = ProcessConditionalInstruction(instruction);

                output.AddRange(conditionalInstructions);
            }

            if (InstructionHelper.IsBranch(instruction.OpCode.Code) || InstructionHelper.IsBranchConditional(instruction.OpCode.Code))
            {
                output.AddRange(BranchInstructionProcessor.Process(instruction, targetMethod, type));

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
                SwitchConditionalComparer = SwitchConditionalComparer ?? EvaluationStack.Peek().Value as PapyrusVariableReference;
            }
            if (InstructionHelper.IsNewArrayInstance(instruction.OpCode.Code))
            {
                output.Add(CreateNewPapyrusArrayInstance(instruction));
            }

            if (InstructionHelper.IsCallMethod(instruction.OpCode.Code))
            {
                output.AddRange(CallInstructionProcessor.Process(instruction, targetMethod, type));
            }
            if (instruction.OpCode.Code == Code.Ret)
            {
                output.AddRange(ReturnInstructionProcessor.Process(instruction, targetMethod, type));
            }
            return output;
        }

        /// <summary>
        /// Processes the string concat instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="methodRef">The method reference.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public List<PapyrusInstruction> ProcessStringConcat(Instruction instruction, MethodReference methodRef, List<object> parameters)
        {
            return StringConcatInstructionProcessor.Process(instruction, methodRef, parameters);
        }

        /// <summary>
        /// Processes the conditional instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="overrideOpCode">The override op code.</param>
        /// <param name="tempVariable">The temporary variable.</param>
        /// <returns></returns>
        public IEnumerable<PapyrusInstruction> ProcessConditionalInstruction(Instruction instruction, Code overrideOpCode = Code.Nop, string tempVariable = null)
        {
            return ConditionalInstructionProcessor.Process(instruction, overrideOpCode, tempVariable);
        }

        /// <summary>
        /// Gets the field from STFLD.
        /// </summary>
        /// <param name="whereToPlace">The where to place.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the target variable.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="methodRef">The method reference.</param>
        /// <param name="fallbackType">Type of the fallback.</param>
        /// <param name="forceNew">if set to <c>true</c> [force new].</param>
        /// <returns></returns>
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


        public PapyrusOpCodes TryInvertJump(PapyrusOpCodes jmpt)
        {
            if (!InvertedBranch) return jmpt;
            InvertedBranch = false;
            if (jmpt == PapyrusOpCodes.Jmpt)
                return PapyrusOpCodes.Jmpf;
            return PapyrusOpCodes.Jmpt;
        }

        public PapyrusInstruction ConditionalJump(PapyrusOpCodes jumpType, PapyrusVariableReference conditionalVar,
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

            var arrayInstance = CreatePapyrusInstruction(PapyrusOpCodes.ArrayCreate,
                targetArrayItem,
                targetArraySize);
            return arrayInstance;
        }

        public PapyrusInstruction CreatePapyrusCastInstruction(string destinationVariable, PapyrusVariableReference variableToCast)
        {
            return CreatePapyrusInstruction(PapyrusOpCodes.Cast,
                CreateVariableReference(PapyrusPrimitiveType.Reference, destinationVariable),
                variableToCast);
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

        public PapyrusInstruction CreatePapyrusInstruction(PapyrusOpCodes papyrusOpCode, params object[] values)
        {
            var args = ParsePapyrusParameters(values);

            return new PapyrusInstruction()
            {
                OpCode = papyrusOpCode,
                Arguments = args
            };
        }

        public List<PapyrusVariableReference> ParsePapyrusParameters(object[] values)
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
                        args.Add(vars.Any(v => v.Name.Value == varName)
                            ? CreateVariableReferenceFromName(varName)
                            : CreateVariableReference(PapyrusPrimitiveType.String, varName.Trim('"')));
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

        public PapyrusVariableReference CreateVariableReferenceFromName(string varName)
        {
            var nameRef = varName.Ref(PapyrusAssembly);
            return new PapyrusVariableReference()
            {
                Name = nameRef,
                Value = nameRef.Value,
                ValueType = PapyrusPrimitiveType.Reference
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