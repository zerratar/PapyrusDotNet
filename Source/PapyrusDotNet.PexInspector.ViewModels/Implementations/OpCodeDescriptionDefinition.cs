using System.Collections.Generic;
using System.Linq;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PexInspector.ViewModels.Interfaces;

namespace PapyrusDotNet.PexInspector.ViewModels.Implementations
{
    public class OpCodeDescriptionDefinition : IOpCodeDescriptionDefinition
    {
        private object descLock = new object();
        public List<OpCodeDescription> Instructions { get; set; } = new List<OpCodeDescription>();
        public OpCodeDescription GetDesc(PapyrusOpCodes code)
        {
            lock (descLock)
            {
                return Instructions.FirstOrDefault(i => i.OpCode == code);
            }
        }

        public OpCodeDescriptionDefinition()
        {

        }
    }
}