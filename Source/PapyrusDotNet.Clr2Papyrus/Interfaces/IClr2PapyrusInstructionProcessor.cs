using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using PapyrusDotNet.PapyrusAssembly.Classes;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Interfaces
{
    public interface IClr2PapyrusInstructionProcessor
    {
        IEnumerable<PapyrusInstruction> ProcessInstructions(MethodDefinition method, MethodBody body, Collection<Instruction> instructions);
    }
}