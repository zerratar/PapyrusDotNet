//     This file is part of PapyrusDotNet.
// 
//     PapyrusDotNet is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     PapyrusDotNet is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with PapyrusDotNet.  If not, see <http://www.gnu.org/licenses/>.
//  
//     Copyright 2016, Karl Patrik Johansson, zerratar@gmail.com

#region

using System.Collections.Generic;
using System.Linq;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.Common.Papyrus;

#endregion

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