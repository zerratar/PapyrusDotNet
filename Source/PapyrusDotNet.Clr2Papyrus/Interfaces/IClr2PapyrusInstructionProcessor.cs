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
using PapyrusDotNet.Converters.Clr2Papyrus.Enums;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Classes;

#endregion

namespace PapyrusDotNet.Converters.Clr2Papyrus.Interfaces
{
    public interface IClr2PapyrusInstructionProcessor
    {
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
        IEnumerable<PapyrusInstruction> ProcessInstructions(PapyrusAssemblyDefinition targetPapyrusAssembly, PapyrusTypeDefinition targetPapyrusType, PapyrusMethodDefinition targetPapyrusMethod, MethodDefinition method, MethodBody body,
            Collection<Instruction> instructions, PapyrusCompilerOptions options);
    }
}