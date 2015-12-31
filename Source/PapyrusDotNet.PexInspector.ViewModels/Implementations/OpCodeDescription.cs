using System.Collections.Generic;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.PexInspector.ViewModels.Implementations
{
    public class OpCodeDescription
    {
        public PapyrusOpCodes OpCode { get; set; }
        public List<OpCodeArgumentDescription> Arguments { get; set; } = new List<OpCodeArgumentDescription>();
        public List<OpCodeArgumentDescription> OperandArguments { get; set; } = new List<OpCodeArgumentDescription>();
        public OpCodeDescriptionDefinition Definition { get; set; }
    }
}