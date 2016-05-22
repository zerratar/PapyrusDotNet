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
using Mono.Cecil;
using Mono.Cecil.Cil;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.Common.Utilities;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Extensions;

#endregion

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations.Processors
{
    public interface IReturnProcessor : ISubInstructionProcessor { }

    public class ReturnProcessor : IReturnProcessor
    {
        private readonly IValueTypeConverter valueTypeConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReturnProcessor" /> class.
        /// </summary>
        public ReturnProcessor(IValueTypeConverter valueTypeConverter)
        {
            this.valueTypeConverter = valueTypeConverter ?? new PapyrusValueTypeConverter();
        }

        /// <summary>
        /// Processes the specified instruction.
        /// </summary>
        /// <param name="mainProcessor">The main instruction processor.</param>
        /// <param name="asmCollection">The papyrus assembly collection.</param>
        /// <param name="instruction">The instruction.</param>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public IEnumerable<PapyrusInstruction> Process(
            IClrInstructionProcessor mainProcessor,
            IReadOnlyCollection<PapyrusAssemblyDefinition> asmCollection,
            Instruction instruction,
            MethodDefinition targetMethod, TypeDefinition type)
        {
            var output = new List<PapyrusInstruction>();
            if (Utility.IsVoid(targetMethod.ReturnType))
            {
                output.Add(PapyrusReturnNone());
                return output;
            }

            if (mainProcessor.EvaluationStack.Count >=
                Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
            {
                var topValue = mainProcessor.EvaluationStack.Pop();
                if (topValue.Value is PapyrusVariableReference)
                {
                    var variable = topValue.Value as PapyrusVariableReference;
                    // return "Return " + variable.Name;

                    output.Add(mainProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Return, variable));
                    // PapyrusReturnVariable(variable.Name)

                    return output;
                }
                if (topValue.Value is PapyrusFieldDefinition)
                {
                    var variable = topValue.Value as PapyrusFieldDefinition;
                    // return "Return " + variable.Name;
                    output.Add(mainProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Return, variable));
                    return output;
                }
                if (Utility.IsConstantValue(topValue.Value))
                {
                    var val = topValue.Value;

                    var typeName = topValue.TypeName;
                    var newValue = valueTypeConverter.Convert(typeName, val);
                    var papyrusVariableReference = new PapyrusVariableReference
                    {
                        TypeName = StringExtensions.Ref(typeName, mainProcessor.PapyrusAssembly),
                        Value = newValue,
                        Type = Utility.GetPapyrusPrimitiveType(typeName)
                    };
                    {
                        output.Add(mainProcessor.CreatePapyrusInstruction(PapyrusOpCodes.Return,
                            papyrusVariableReference));
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
            return new PapyrusInstruction
            {
                OpCode = PapyrusOpCodes.Return,
                Arguments = new List<PapyrusVariableReference>(new[]
                {
                    new PapyrusVariableReference
                    {
                        Type = PapyrusPrimitiveType.None
                    }
                })
            };
        }
    }
}