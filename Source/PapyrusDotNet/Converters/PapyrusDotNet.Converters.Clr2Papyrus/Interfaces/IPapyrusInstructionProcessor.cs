using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PapyrusDotNet.PapyrusAssembly.Classes;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Interfaces
{
    public interface IPapyrusInstructionProcessor
    {
        IEnumerable<PapyrusInstruction> ParseInstruction(Instruction instruction, MethodDefinition targetMethod,
            TypeDefinition type);
    }
}