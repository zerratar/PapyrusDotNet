using System.Collections.Generic;
using PapyrusDotNet.PapyrusAssembly.Enums;

namespace PapyrusDotNet.PapyrusAssembly.Implementations
{
    public class PapyrusInstruction
    {
        public PapyrusInstruction()
        {
            VariableArguments = new List<PapyrusTypeReference>();
        }

        public int Offset { get; set; }
        public PapyrusInstructionOpCodes OpCode { get; set; }
        public object Operand { get; set; }
        public PapyrusInstruction Previous { get; set; }
        public PapyrusInstruction Next { get; set; }
        public List<PapyrusTypeReference> VariableArguments { get; set; }

    }
}