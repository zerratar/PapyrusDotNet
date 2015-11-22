using Mono.Cecil;
using PapyrusDotNet.Common.Interfaces;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations
{
    public class ClrAssemblyInput : IAssemblyInput
    {
        public AssemblyDefinition Assembly { get; }
        public ClrAssemblyInput(AssemblyDefinition asm)
        {
            Assembly = asm;
        }
    }
}