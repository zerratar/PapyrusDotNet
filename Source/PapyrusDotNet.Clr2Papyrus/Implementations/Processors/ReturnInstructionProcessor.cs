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
using Mono.Cecil;
using Mono.Cecil.Cil;
using PapyrusDotNet.Common;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.Common.Utilities;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly;
using StringExtensions = PapyrusDotNet.PapyrusAssembly.Extensions.StringExtensions;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations.Processors
{
    public class ReturnInstructionProcessor : IInstructionProcessor
    {
        private readonly IValueTypeConverter valueTypeConverter;
        private readonly IClr2PapyrusInstructionProcessor mainInstructionProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReturnInstructionProcessor"/> class.
        /// </summary>
        /// <param name="clr2PapyrusInstructionProcessor">The CLR2 papyrus instruction processor.</param>
        public ReturnInstructionProcessor(IClr2PapyrusInstructionProcessor clr2PapyrusInstructionProcessor)
        {
            valueTypeConverter = new PapyrusValueTypeConverter();
            mainInstructionProcessor = clr2PapyrusInstructionProcessor;
        }

        /// <summary>
        /// Processes the specified instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public IEnumerable<PapyrusInstruction> Process(IReadOnlyCollection<PapyrusAssemblyDefinition> papyrusAssemblyCollection, Instruction instruction, MethodDefinition targetMethod, TypeDefinition type)
        {
            var output = new List<PapyrusInstruction>();
            if (Utility.IsVoid(targetMethod.ReturnType))
            {

                output.Add(PapyrusReturnNone());
                return output;
            }

            if (mainInstructionProcessor.EvaluationStack.Count >= Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
            {
                var topValue = mainInstructionProcessor.EvaluationStack.Pop();
                if (topValue.Value is PapyrusVariableReference)
                {
                    var variable = topValue.Value as PapyrusVariableReference;
                    // return "Return " + variable.Name;

                    output.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Return, variable));
                    // PapyrusReturnVariable(variable.Name)

                    return output;
                }
                if (topValue.Value is PapyrusFieldDefinition)
                {
                    var variable = topValue.Value as PapyrusFieldDefinition;
                    // return "Return " + variable.Name;
                    output.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Return, variable));
                    return output;
                }
                if (Utility.IsConstantValue(topValue.Value))
                {
                    var val = topValue.Value;

                    var typeName = topValue.TypeName;
                    var newValue = valueTypeConverter.Convert(typeName, val);
                    var papyrusVariableReference = new PapyrusVariableReference
                    {
                        TypeName = StringExtensions.Ref(typeName, mainInstructionProcessor.PapyrusAssembly),
                        Value = newValue,
                        ValueType = Utility.GetPapyrusPrimitiveType(typeName)
                    };
                    {
                        output.Add(mainInstructionProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Return, papyrusVariableReference));
                        return output;
                    }
                }
            }
            else
            {
                output.Add(PapyrusReturnNone());
                return output;
            }
            return output;
        }


        public PapyrusInstruction PapyrusReturnNone()
        {
            return new PapyrusInstruction()
            {
                OpCode = PapyrusOpCodes.Return,
                Arguments = new List<PapyrusVariableReference>(new[] { new PapyrusVariableReference()
                {
                    ValueType = PapyrusPrimitiveType.None
                }})
            };
        }
    }
}