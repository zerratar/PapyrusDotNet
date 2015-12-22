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
using PapyrusDotNet.Converters.Clr2Papyrus.Enums;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Converters.Clr2Papyrus.Interfaces
{
    public interface IClr2PapyrusInstructionProcessor
    {
        /// <summary>
        /// Gets or sets the evaluation stack.
        /// </summary>
        Stack<EvaluationStackItem> EvaluationStack { get; set; }
        /// <summary>
        /// Gets or sets the papyrus method.
        /// </summary>
        PapyrusMethodDefinition PapyrusMethod { get; set; }
        /// <summary>
        /// Gets or sets the papyrus assembly.
        /// </summary>
        PapyrusAssemblyDefinition PapyrusAssembly { get; set; }
        /// <summary>
        /// Gets or sets the papyrus object type.
        /// </summary>
        PapyrusTypeDefinition PapyrusType { get; set; }
        /// <summary>
        /// Gets or sets the papyrus compiler options.
        /// </summary>
        PapyrusCompilerOptions PapyrusCompilerOptions { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the next instruction should be skipped or not.
        /// </summary>
        bool SkipNextInstruction { get; set; }
        /// <summary>
        /// Gets or sets the offset to skip to.
        /// </summary>
        int SkipToOffset { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the next conditional branching should be inverted or not..
        /// </summary>
        bool InvertedBranch { get; set; }

        /// <summary>
        /// Gets or sets the switch conditional comparer.
        /// </summary>
        PapyrusVariableReference SwitchConditionalComparer { get; set; }

        /// <summary>
        ///     Processes the instructions.
        /// </summary>
        /// <param name="papyrusAssemblyCollection"></param>
        /// <param name="targetPapyrusAssembly"></param>
        /// <param name="targetPapyrusType"></param>
        /// <param name="targetPapyrusMethod"></param>
        /// <param name="method">The method.</param>
        /// <param name="body">The body.</param>
        /// <param name="instructions">The instructions.</param>
        /// <param name="options"></param>
        /// <returns></returns>
        IEnumerable<PapyrusInstruction> ProcessInstructions(IEnumerable<PapyrusAssemblyDefinition> papyrusAssemblyCollection, PapyrusAssemblyDefinition targetPapyrusAssembly, PapyrusTypeDefinition targetPapyrusType, PapyrusMethodDefinition targetPapyrusMethod, MethodDefinition method, MethodBody body,
            Collection<Instruction> instructions, PapyrusCompilerOptions options);

        /// <summary>
        /// Gets the target variable needed for this instruction.
        /// If none exists, then a temporarily variable will be created and returned.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="methodRef">The method reference.</param>
        /// <param name="isStructAccess"></param>
        /// <param name="fallbackType">Type of the fallback.</param>
        /// <param name="forceNew">if set to <c>true</c> [force new].</param>
        /// <returns></returns>
        string GetTargetVariable(Instruction instruction, MethodReference methodRef, out bool isStructAccess, string fallbackType = null,
            bool forceNew = false);

        /// <summary>
        /// Creates a papyrus instruction.
        /// </summary>
        /// <param name="papyrusOpCode">The papyrus op code.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        PapyrusInstruction CreatePapyrusInstruction(PapyrusOpCodes papyrusOpCode, params object[] values);

        /// <summary>
        /// Creates a conditional jump instruction.
        /// </summary>
        /// <param name="jumpType">Type of the jump.</param>
        /// <param name="conditionalVar">The conditional variable.</param>
        /// <param name="destinationInstruction">The destination instruction.</param>
        /// <returns></returns>
        PapyrusInstruction ConditionalJump(PapyrusOpCodes jumpType, PapyrusVariableReference conditionalVar,
            object destinationInstruction);

        /// <summary>
        /// Tries to invert the jump instruction depending on the InvertedBranch value.
        /// </summary>
        /// <param name="jmpt">The JMPT.</param>
        /// <returns></returns>
        PapyrusOpCodes TryInvertJump(PapyrusOpCodes jmpt);

        /// <summary>
        /// Tries to resolve the resolve method reference.        
        /// </summary>
        /// <param name="methodRef">The method reference.</param>
        /// <returns>A method definition if success, null if failed.</returns>
        MethodDefinition TryResolveMethodReference(MethodReference methodRef);

        /// <summary>
        /// Processes the string concat.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="methodRef">The method reference.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        List<PapyrusInstruction> ProcessStringConcat(Instruction instruction, MethodReference methodRef,
            List<object> parameters);

        /// <summary>
        /// Parses the conditional instruction and returns a collection of instructions converted into papyrus.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="overrideOpCode">The override op code.</param>
        /// <param name="tempVariable">The temporary variable.</param>
        /// <returns></returns>
        IEnumerable<PapyrusInstruction> ProcessConditionalInstruction(Instruction instruction, Code overrideOpCode = Code.Nop,
            string tempVariable = null);


        /// <summary>
        /// Creates a variable reference.
        /// </summary>
        /// <param name="papyrusPrimitiveType">Type of the papyrus primitive.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        PapyrusVariableReference CreateVariableReference(PapyrusPrimitiveType papyrusPrimitiveType, object value);

        /// <summary>
        /// Creates a variable reference using the name of the variable.
        /// </summary>
        /// <param name="varName">Name of the variable.</param>
        /// <returns></returns>
        PapyrusVariableReference CreateVariableReferenceFromName(string varName);

        /// <summary>
        /// Gets the next store local variable instruction.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="varIndex">Index of the variable.</param>
        /// <returns></returns>
        Instruction GetNextStoreLocalVariableInstruction(Instruction input, out int varIndex);

        /// <summary>
        /// Gets a numeric value from the instruction.
        /// Either a variable index or by a instruction load numeric value.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <returns></returns>
        object GetNumericValue(Instruction instruction);

        /// <summary>
        /// Creates the papyrus cast instruction.
        /// </summary>
        /// <param name="destinationVariable">The destination variable.</param>
        /// <param name="variableToCast">The variable to cast.</param>
        /// <returns></returns>
        PapyrusInstruction CreatePapyrusCastInstruction(string destinationVariable,
            PapyrusVariableReference variableToCast);

        /// <summary>
        /// Gets the field from STFLD.
        /// </summary>
        /// <param name="whereToPlace">The where to place.</param>
        /// <returns></returns>
        PapyrusFieldDefinition GetFieldFromStfld(Instruction whereToPlace);

        /// <summary>
        /// Creates a temporary variable.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="methodRef">The method reference.</param>
        /// <returns></returns>
        PapyrusVariableReference CreateTempVariable(string variableName, MethodReference methodRef = null);

        /// <summary>
        /// Parses the papyrus parameters.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        List<PapyrusVariableReference> ParsePapyrusParameters(object[] values);
    }
}