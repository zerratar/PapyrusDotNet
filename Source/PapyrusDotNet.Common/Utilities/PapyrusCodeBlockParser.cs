using System.Collections.Generic;
using System.Linq;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.Common.Papyrus;

namespace PapyrusDotNet.Common.Utilities
{
    public class PapyrusCodeBlockParser : IPapyrusCodeBlockParser
    {
        public IPapyrusCodeBlock ParseCodeBlock(string codeBlock)
        {
            var rows = codeBlock.Split('\n');
            return ParseCodeBlocks(rows.ToList()).FirstOrDefault();
        }

        public IEnumerable<IPapyrusCodeBlock> ParseCodeBlocks(IEnumerable<string> rows)
        {
            var codeBlocks = new List<IPapyrusCodeBlock>();
            IPapyrusCodeBlock latestPapyrusCodeBlock = null;
            var rowI = 0;

            foreach (var row in rows)
            {
                if (row.Replace("\t", "").Trim().StartsWith(".code"))
                {
                    latestPapyrusCodeBlock = new PapyrusCodeBlock();
                    latestPapyrusCodeBlock.StartRow = rowI;
                }
                else if (row.Replace("\t", "").Trim().StartsWith(".endCode"))
                {
                    if (latestPapyrusCodeBlock != null)
                    {
                        latestPapyrusCodeBlock.EndRow = rowI;
                        codeBlocks.Add(latestPapyrusCodeBlock);
                    }
                }
                else if (latestPapyrusCodeBlock != null)
                {
                    if (row.Replace("\t", "").StartsWith("_") && row.Trim().EndsWith(":"))
                    {
                        latestPapyrusCodeBlock.Labels.Add(new LabelDefinition(rowI, row.Replace("\t", "").Trim()));
                    }
                    else if (row.Replace("\t", "").Contains("_label") /* && !row.Contains(":")*/&&
                             row.ToLower().Contains("jump"))
                    {
                        latestPapyrusCodeBlock.UsedLabels.Add(
                            new LabelReference(row.Substring(row.IndexOf("_label")).Split(' ')[0] + ":", rowI));
                    }
                }
                rowI++;
            }
            return codeBlocks;
        }

    }
}