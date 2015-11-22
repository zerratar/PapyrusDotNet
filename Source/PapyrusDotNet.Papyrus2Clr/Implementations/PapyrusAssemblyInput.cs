using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.Converters.Papyrus2Clr.Implementations
{
    public class PapyrusAssemblyInput : IAssemblyInput
    {
        public PapyrusAssemblyDefinition[] Assemblies { get; }
        public PapyrusAssemblyInput(params PapyrusAssemblyDefinition[] asm)
        {
            Assemblies = asm;
        }
    }
}