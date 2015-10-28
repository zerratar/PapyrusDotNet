using System.Collections.Generic;

namespace PapyrusDotNet.Common.Interfaces
{
    public interface ICodeBlock
    {
        int EndRow { get; set; }

        int StartRow { get; set; }

        List<PapyrusLabelDefinition> Labels { get; set; }      

        List<PapyrusLabelReference> UsedLabels { get; set; }

        PapyrusLabelDefinition GetLabelDefinition(int row);
    }
}