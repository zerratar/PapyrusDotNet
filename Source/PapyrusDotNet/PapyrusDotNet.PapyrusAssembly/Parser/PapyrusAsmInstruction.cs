using System.Collections.Generic;
using PapyrusDotNet.PapyrusAssembly.Extensions;
using PapyrusDotNet.PapyrusAssembly.Parser.Interfaces;

namespace PapyrusDotNet.PapyrusAssembly.Parser
{
    public class PapyrusAsmInstruction : IPapyrusAsmInstruction
    {
        /// <summary>
        /// Gets the argument count.
        /// </summary>
        public int ArgumentCount { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has operand arguments.
        /// </summary>
        public bool HasOperandArguments { get; }

        private readonly List<PapyrusAsmValue> args = new List<PapyrusAsmValue>();
        private readonly List<PapyrusAsmValue> opargs = new List<PapyrusAsmValue>();
        private PapyrusOpCodes opcode;
        private readonly string[] aliases;

        /// <summary>
        /// Initializes a new instance of the <see cref="PapyrusAsmInstruction" /> class.
        /// </summary>
        /// <param name="opcode">The opcode.</param>
        /// <param name="argumentCount">The argument count.</param>
        /// <param name="hasOperandArguments">if set to <c>true</c> [has operand arguments].</param>
        /// <param name="aliases">The aliases.</param>
        public PapyrusAsmInstruction(PapyrusOpCodes opcode, int argumentCount, bool hasOperandArguments, params string[] aliases)
        {
            this.opcode = opcode;
            this.aliases = aliases;
            ArgumentCount = argumentCount;
            HasOperandArguments = hasOperandArguments;
            For.Do(argumentCount, () => args.Add(new PapyrusAsmValue()));
            //For.Do(argumentCount, () => opargs.Add(new PapyrusAsmValue()));
        }

        /// <summary>
        /// Converts the Parsed Instruction into a <see cref="PapyrusInstruction"/>.
        /// </summary>
        /// <returns></returns>
        public PapyrusInstruction ToPapyrusInstruction()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Sets the op code.
        /// </summary>
        /// <param name="opcode">The opcode.</param>
        public void SetOpCode(PapyrusOpCodes opcode) => this.opcode = opcode;

        /// <summary>
        /// Gets the op code.
        /// </summary>
        /// <returns></returns>
        public PapyrusOpCodes GetOpCode() => opcode;

        /// <summary>
        /// Sets an argument.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public void SetArgument(int index, PapyrusAsmValue value) => args[index] = value;

        /// <summary>
        /// Sets a operand argument.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public void SetOperandArgument(int index, PapyrusAsmValue value)
        {
            if (index >= opargs.Count)
                FillOpArgs(1+index - opargs.Count);
            opargs[index] = value;
        }

        private void FillOpArgs(int count)
        {
            For.Do(count, () => opargs.Add(new PapyrusAsmValue()));
        }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <returns></returns>
        public List<PapyrusAsmValue> GetArguments() => args;

        /// <summary>
        /// Gets the operand arguments.
        /// </summary>
        /// <returns></returns>
        public List<PapyrusAsmValue> GetOperandArguments() => opargs;
    }
}