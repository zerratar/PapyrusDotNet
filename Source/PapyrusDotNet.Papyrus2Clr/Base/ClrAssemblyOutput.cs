using System.Reflection.Emit;
using PapyrusDotNet.Common.Interfaces;

namespace PapyrusDotNet.Converters.Papyrus2Clr.Base
{
    public class ClrAssemblyOutput : IAssemblyOutput
    {
        private readonly AssemblyBuilder assembly;

        public ClrAssemblyOutput(AssemblyBuilder assembly)
        {
            this.assembly = assembly;
        }

        public void Save(string output)
        {
            assembly.Save(assembly.GetName().Name + ".dll");
        }
    }
}