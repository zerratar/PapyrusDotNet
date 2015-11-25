using System.IO;
using System.Linq;
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

        public PapyrusAssemblyDefinition[] Assemblies => papyrusAssemblyDefinition;


        public void Save(string output)
        {
            //foreach (var asm in papyrusAssemblyDefinition)
            //{
            //    asm.Write(Path.Combine(output, asm.Types.FirstOrDefault().Name + ".pex"));
            //}
        }
    }
}