using System.Collections.Generic;
using PapyrusDotNet.Common.Papyrus;

namespace PapyrusDotNet.Common.Interfaces
{
    public interface IPapyrusCodeBlock
    {
        int EndRow { get; set; }
        int StartRow { get; set; }

        IList<ILabelDefinition> Labels { get; set; }

        IList<ILabelReference> UsedLabels { get; set; }

        ILabelDefinition GetLabelDefinition(int row);
    }
}