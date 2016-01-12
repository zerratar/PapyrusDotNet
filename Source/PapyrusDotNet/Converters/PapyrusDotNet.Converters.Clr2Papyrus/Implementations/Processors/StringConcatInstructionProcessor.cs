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
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations.Processors
{
    public class StringConcatInstructionProcessor : IInstructionProcessor
    {
        private readonly IClr2PapyrusInstructionProcessor mainInstructionProcessor;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StringConcatInstructionProcessor" /> class.
        /// </summary>
        /// <param name="clr2PapyrusInstructionProcessor">The CLR2 papyrus instruction processor.</param>
        public StringConcatInstructionProcessor(IClr2PapyrusInstructionProcessor clr2PapyrusInstructionProcessor)
        {
            mainInstructionProcessor = clr2PapyrusInstructionProcessor;
        }

        /// <summary>
        ///     Processes the specified instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<PapyrusInstruction> Process(
            IReadOnlyCollection<PapyrusAssemblyDefinition> papyrusAssemblyCollection, Instruction instruction,
            MethodDefinition targetMethod, TypeDefinition type)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Processes the specified instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="methodRef">The method reference.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public List<PapyrusInstruction> Process(Instruction instruction, MethodReference methodRef,
            List<object> parameters)
        {
            // BUG: We are always concating the string with itself, ex: temp0 = temp0 + "val"; - This works if the instruction isnt looped.
            var output = new List<PapyrusInstruction>();
            // Equiviliant Papyrus: StrCat <output_destination> <val1> <val2>
            // Make sure we have a temp variable if necessary
            bool isStructAccess;
            var destinationVariable = mainInstructionProcessor.GetTargetVariable(instruction, methodRef,
                out isStructAccess);

            for (var i = 0; i < parameters.Count; i++)
            {
                var stackItem = parameters[i] as EvaluationStackItem;
                if (stackItem != null)
                {
                    var fieldVar = stackItem.Value as PapyrusFieldDefinition;
                    var paramVar = stackItem.Value as PapyrusParameterDefinition;
                    var targetVar = stackItem.Value as PapyrusVariableReference;
                    if (targetVar != null)
                    {
                        if (!stackItem.TypeName.ToLower().Contains("string"))
                            output.Add(mainInstructionProcessor.CreatePapyrusCastInstruction(destinationVariable,
                                targetVar));

                        if (i == 0)
                            // Is First? Then we just want to assign the destinationVariable with the target value
                        {
                            output.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Assign,
                                mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference,
                                    destinationVariable), targetVar));
                        }
                        else
                            output.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Strcat,
                                mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference,
                                    destinationVariable),
                                mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference,
                                    destinationVariable),
                                targetVar));
                    }
                    else if (paramVar != null)
                    {
                        if (!stackItem.TypeName.ToLower().Contains("string"))
                            output.Add(mainInstructionProcessor.CreatePapyrusCastInstruction(destinationVariable,
                                mainInstructionProcessor.CreateVariableReferenceFromName(paramVar.Name.Value)));

                        if (i == 0)
                            // Is First? Then we just want to assign the destinationVariable with the target value
                        {
                            output.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Assign,
                                mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference,
                                    destinationVariable), paramVar));
                        }
                        else
                            output.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Strcat,
                                mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference,
                                    destinationVariable),
                                mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference,
                                    destinationVariable),
                                paramVar));
                    }
                    else if (fieldVar != null)
                    {
                        if (!stackItem.TypeName.ToLower().Contains("string"))
                            output.Add(mainInstructionProcessor.CreatePapyrusCastInstruction(destinationVariable,
                                mainInstructionProcessor.CreateVariableReferenceFromName(fieldVar.Name.Value)));

                        if (i == 0)
                            // Is First? Then we just want to assign the destinationVariable with the target value
                        {
                            output.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Assign,
                                mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference,
                                    destinationVariable), fieldVar));
                        }
                        else
                            output.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Strcat,
                                mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference,
                                    destinationVariable),
                                mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference,
                                    destinationVariable),
                                fieldVar));
                    }
                    else
                    {
                        var value = stackItem.Value;
                        var newTempVar = false;
                        if (!stackItem.TypeName.ToLower().Contains("string"))
                        {
                            // First, get a new temp variable of type string.
                            // This new temp variable will be used for casting the source object into a string.                            
                            value = mainInstructionProcessor.GetTargetVariable(instruction, methodRef,
                                out isStructAccess, "String", true);

                            // Create a new temp variable that we use to assign our source object to.
                            // this is so we avoid doing ex: cast ::temp0 55
                            // and instead we do: cast ::temp0 ::temp1
                            var valueToCastTemp = mainInstructionProcessor.GetTargetVariable(instruction, methodRef,
                                out isStructAccess, stackItem.TypeName, true);
                            var valueToCast =
                                mainInstructionProcessor.CreateVariableReference(
                                    Utility.GetPrimitiveTypeFromValue(stackItem.Value),
                                    stackItem.Value);

                            // Assign our newly created tempvalue with our object.
                            // ex: assign ::temp1 55
                            output.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Assign,
                                mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference,
                                    valueToCastTemp),
                                valueToCast));

                            // Cast the new ::temp1 to ::temp0 (equivilant to .ToString())
                            output.Add(mainInstructionProcessor.CreatePapyrusCastInstruction((string) value,
                                mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference,
                                    valueToCastTemp)));
                            newTempVar = true;

                            // Make sure that our newly ::temp1 is used when concating the string.
                            value = valueToCastTemp;
                        }

                        if (i == 0)
                            // Is First? Then we just want to assign the destinationVariable with the target value
                        {
                            output.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Assign,
                                mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference,
                                    destinationVariable),
                                mainInstructionProcessor.CreateVariableReference(newTempVar
                                    ? PapyrusPrimitiveType.Reference
                                    : PapyrusPrimitiveType.String, value)));
                        }
                        else
                            output.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Strcat,
                                mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference,
                                    destinationVariable),
                                mainInstructionProcessor.CreateVariableReference(PapyrusPrimitiveType.Reference,
                                    destinationVariable),
                                mainInstructionProcessor.CreateVariableReference(newTempVar
                                    ? PapyrusPrimitiveType.Reference
                                    : PapyrusPrimitiveType.String, value)));
                    }
                }
            }

            // TODO: In case we want to concat more strings together or call a method using this new value
            // we will have to use the targetVariable above and push it back into the stack. (Or do we...?)
            return output;
        }
    }
}