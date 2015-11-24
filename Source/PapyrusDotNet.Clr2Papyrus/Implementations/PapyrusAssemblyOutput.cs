using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations
{
    public class PapyrusAssemblyOutput : IAssemblyOutput
    {
        private PapyrusAssemblyDefinition[] papyrusAssemblyDefinition;

        public PapyrusAssemblyOutput(PapyrusAssemblyDefinition[] papyrusAssemblyDefinition)
        {
            this.papyrusAssemblyDefinition = papyrusAssemblyDefinition;
        }
    }
}