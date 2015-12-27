using System.Collections.Generic;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PexInspector.ViewModels.Implementations;

namespace PapyrusDotNet.PexInspector.ViewModels.Interfaces
{
    public interface IOpCodeDescriptionDefinition
    {
        List<OpCodeDescription> Instructions { get; set; }
        OpCodeDescription GetDesc(PapyrusOpCodes code);
    }
}