using System.Collections.Generic;
using PapyrusDotNet.PapyrusAssembly.Extensions;

namespace PapyrusDotNet.PapyrusAssembly.Parser.Interfaces
{
    public interface IPapyrusAssemblyInstructionParser
    {
        /// <summary>
        /// Parses the string represented instruction and returns a <see cref="PapyrusAsmInstruction"/>.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <returns></returns>
        PapyrusAsmInstruction ParseInstruction(string instruction);

        /// <summary>
        /// Parses a string containing multiple instructions separated by a newline and returns collection of <see cref="PapyrusAsmInstruction"/>.
        /// </summary>
        /// <param name="inputInstructions">The input instructions.</param>
        /// <returns></returns>
        IList<PapyrusAsmInstruction> ParseInstructions(string inputInstructions);

        /// <summary>
        /// Parses an array of strings containing a string represented instruction per item and returns collection of <see cref="PapyrusAsmInstruction"/>.
        /// </summary>
        /// <param name="inputInstructions">The input instructions.</param>
        /// <returns></returns>
        IList<PapyrusAsmInstruction> ParseInstructions(IEnumerable<string> inputInstructions);
    }
}