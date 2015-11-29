using PapyrusDotNet.PapyrusAssembly.Classes;
using PapyrusDotNet.PapyrusAssembly.Enums;

namespace PapyrusDotNet.PapyrusAssembly.Extensions
{
    public static class PapyrusInstructionOpCodesExtensions
    {
        /// <summary>
        /// Gets the size of the instruction parameter.
        /// </summary>
        /// <param name="opcode">The opcode.</param>
        /// <returns></returns>
        public static int GetInstructionParamSize(this PapyrusOpCode opcode)
        {
            return PapyrusInstructionOpCodeDescription.FromOpCode(opcode).ParamSize;
        }
    }
}