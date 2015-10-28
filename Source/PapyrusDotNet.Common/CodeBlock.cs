using System.Collections.Generic;
using System.Linq;
using PapyrusDotNet.Common.Interfaces;

namespace PapyrusDotNet.Common
{
    public class CodeBlock : ICodeBlock
    {
        public int StartRow { get; set; }

        public int EndRow { get; set; }

        public List<PapyrusLabelReference> UsedLabels { get; set; } = new List<PapyrusLabelReference>();

        public List<PapyrusLabelDefinition> Labels { get; set; } = new List<PapyrusLabelDefinition>();

        public PapyrusLabelDefinition GetLabelDefinition(int row)
        {
            return Labels.FirstOrDefault(r => r.Row == row);
        }
    }
}