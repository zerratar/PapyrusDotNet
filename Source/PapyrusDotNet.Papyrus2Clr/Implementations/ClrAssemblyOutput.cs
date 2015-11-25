using Mono.Cecil;
using PapyrusDotNet.Common.Interfaces;

namespace PapyrusDotNet.Converters.Papyrus2Clr.Implementations
{
    public class ClrAssemblyOutput : IAssemblyOutput
    {
        public ClrAssemblyOutput(AssemblyDefinition clrAssembly)
        {
            OutputAssembly = clrAssembly;
        }

        public AssemblyDefinition OutputAssembly { get; }

        public void Save(string output)
        {
        }
    }
}