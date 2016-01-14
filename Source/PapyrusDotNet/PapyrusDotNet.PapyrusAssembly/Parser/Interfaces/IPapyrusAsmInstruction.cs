using System.Collections.Generic;

namespace PapyrusDotNet.PapyrusAssembly.Parser.Interfaces
{
    public interface IPapyrusAsmInstruction
    {
        /// <summary>
        /// Converts the Parsed Instruction into a <see cref="PapyrusInstruction"/>.
        /// </summary>
        /// <returns></returns>
        PapyrusInstruction ToPapyrusInstruction();

        /// <summary>
        /// Sets the op code.
        /// </summary>
        /// <param name="opcode">The opcode.</param>
        void SetOpCode(PapyrusOpCodes opcode);

        /// <summary>
        /// Gets the op code.
        /// </summary>
        /// <returns></returns>
        PapyrusOpCodes GetOpCode();

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <returns></returns>
        List<PapyrusAsmValue> GetArguments();

        /// <summary>
        /// Gets the operand arguments.
        /// </summary>
        /// <returns></returns>
        List<PapyrusAsmValue> GetOperandArguments();

        /// <summary>
        /// Sets an argument.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        void SetArgument(int index, PapyrusAsmValue value);

        /// <summary>
        /// Sets an operand argument.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        void SetOperandArgument(int index, PapyrusAsmValue value);
    }
}