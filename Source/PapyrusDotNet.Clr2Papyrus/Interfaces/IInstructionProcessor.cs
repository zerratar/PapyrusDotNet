using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Interfaces
{
    public interface IInstructionProcessor
    {
        IEnumerable<PapyrusInstruction> Process(Instruction instruction, MethodDefinition targetMethod,
            TypeDefinition type);
    }
}