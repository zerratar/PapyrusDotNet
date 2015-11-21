using PapyrusDotNet.PapyrusAssembly.Enums;

namespace PapyrusDotNet.PapyrusAssembly.Extensions
{
    public static class PapyrusInstructionOpCodesExtensions
    {
        public static int GetInstructionParamSize(this PapyrusInstructionOpCodes opcode)
        {
            return PapyrusInstructionOpCodeDescription.FromOpCode(opcode).ParamSize;
        }
    }
}