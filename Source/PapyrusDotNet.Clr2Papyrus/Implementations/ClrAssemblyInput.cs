using Mono.Cecil;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.PapyrusAssembly.Enums;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations
{
    public class ClrAssemblyInput : IAssemblyInput
    {
        public AssemblyDefinition Assembly { get; }
        public PapyrusVersionTargets TargetPapyrusVersion { get; }
        public ClrAssemblyInput(AssemblyDefinition asm, PapyrusVersionTargets targetPapyrusVersion)
        {
            Assembly = asm;
            TargetPapyrusVersion = targetPapyrusVersion;
        }
    }
}