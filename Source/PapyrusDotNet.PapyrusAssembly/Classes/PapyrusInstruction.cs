using System.Collections.Generic;
using PapyrusDotNet.PapyrusAssembly.Enums;

namespace PapyrusDotNet.PapyrusAssembly.Classes
{
    public class PapyrusInstruction
    {
        public PapyrusInstruction()
        {
            Arguments = new List<PapyrusValueReference>();
            VariableArguments = new List<PapyrusValueReference>();
        }

        public int Offset { get; set; }
        public PapyrusOpCode OpCode { get; set; }
        public List<PapyrusValueReference> Arguments { get; set; }
        public PapyrusInstruction Previous { get; set; }
        public PapyrusInstruction Next { get; set; }
        public List<PapyrusValueReference> VariableArguments { get; set; }
    }
}