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

using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using PapyrusDotNet.Common;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly.Classes;
using PapyrusDotNet.PapyrusAssembly.Enums;

#endregion

namespace PapyrusDotNet.Converters.Clr2Papyrus
{
    public class Clr2PapyrusInstructionProcessor : IClr2PapyrusInstructionProcessor
    {
        /// <summary>
        ///     Processes the instructions.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="body">The body.</param>
        /// <param name="instructions">The instructions.</param>
        /// <returns></returns>
        public IEnumerable<PapyrusInstruction> ProcessInstructions(MethodDefinition method, MethodBody body,
            Collection<Instruction> instructions)
        {
            var outputInstructions = new List<PapyrusInstruction>();

            foreach (var i in instructions)
            {
                var pi = new PapyrusInstruction();

                pi.OpCode = TranslateOpCode(i.OpCode.Code);

                outputInstructions.Add(pi);
            }
            return outputInstructions;
        }

        /// <summary>
        ///     Translates the op code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public PapyrusOpCode TranslateOpCode(Code code)
        {
            /* Just going to simplify it as much as possible for now. */
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