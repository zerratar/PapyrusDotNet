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

using System;
using System.Collections.Generic;
using System.Linq;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.Common.Papyrus;

#endregion

namespace PapyrusDotNet.Common.Utilities
{
    public class PapyrusAssemblyOptimizer : IPapyrusAssemblyOptimizer
    {
        private readonly IPapyrusCodeBlockParser codeblockParser;
        private string papyrus;

        public PapyrusAssemblyOptimizer(string papyrus, IPapyrusCodeBlockParser codeblockParser)
        {
            this.papyrus = papyrus;
            this.codeblockParser = codeblockParser;
        }

        public string OptimizeLabels()
        {
            papyrus = RemoveUnusedLabels();
            papyrus = RemoveUnnecessaryLabels();
            return papyrus;
        }

        public string RemoveUnusedLabels()
        {
            var rows = papyrus.Split(new[] {Environment.NewLine}, StringSplitOptions.None).ToList();

            var codeBlocks = codeblockParser.ParseCodeBlocks(rows);

            var labelsToRemove = new Dictionary<int, string>();

            foreach (var block in codeBlocks)
            {
                foreach (var lbl in block.Labels)
                {
                    var isCalled = false;
                    foreach (var ulbl in block.UsedLabels)
                    {
                        if (lbl.Name == ulbl.Name)
                        {
                            isCalled = true;
                        }
                    }
                    if (!isCalled)
                    {
                        labelsToRemove.Add(lbl.Row, lbl.Name);
                    }
                }
            }

            var ordered = labelsToRemove.OrderByDescending(i => i.Key).ToArray();

            foreach (var row in ordered)
            {
                rows.RemoveAt(row.Key);
            }

            return string.Join(Environment.NewLine, rows.ToArray());
        }

        public string RemoveUnnecessaryLabels()
        {
            var rows = papyrus.Split(new[] {Environment.NewLine}, StringSplitOptions.None).ToList();
            var labelReplacements =
                new List<ObjectReplacementHolder<ILabelDefinition, ILabelDefinition, ILabelReference>>();
            var codeBlocks = codeblockParser.ParseCodeBlocks(rows);
            var lastReplacement = new ObjectReplacementHolder<ILabelDefinition, ILabelDefinition, ILabelReference>();
            foreach (var codeBlock in codeBlocks)
            {
                for (var i = 0; i < codeBlock.Labels.Count; i++)
                {
                    var currentLabel = codeBlock.Labels[i];

                    var lastRowIndex = currentLabel.Row;
                    while (i + 1 < codeBlock.Labels.Count)
                    {
                        if (lastReplacement == null)
                        {
                            lastReplacement =
                                new ObjectReplacementHolder<ILabelDefinition, ILabelDefinition, ILabelReference>();
                        }

                        var label = codeBlock.GetLabelDefinition(lastRowIndex + 1);
                        if (label != null)
                        {
                            if (lastReplacement.Replacement == null) lastReplacement.Replacement = currentLabel;

                            lastReplacement.ToReplace.Add(label);

                            var usedAreas = codeBlock.UsedLabels.Where(b => b.Name == label.Name).ToArray();
                            if (usedAreas.Any())
                            {
                                lastReplacement.ToReplaceSecondary.AddRange(usedAreas);
                            }

                            lastRowIndex = label.Row;
                            // We have a previous label one row behind us.
                        }
                        else
                        {
                            break;
                        }
                        i++;
                    }
                    if (lastReplacement != null && lastReplacement.ToReplace.Count > 0)
                    {
                        labelReplacements.Add(lastReplacement);
                        lastReplacement = null;
                    }
                }
            }
            var rowsToRemove = new List<int>();
            foreach (var replacer in labelReplacements)
            {
                foreach (var old in replacer.ToReplace)
                {
                    rows[old.Row] = rows[old.Row].Replace(old.Name, replacer.Replacement.Name);
                    rowsToRemove.Add(old.Row);
                }
                foreach (var old in replacer.ToReplaceSecondary)
                {
                    rows[old.RowReference] = rows[old.RowReference].Replace(
                        old.Name.Remove(old.Name.Length - 1),
                        replacer.Replacement.Name.Remove(replacer.Replacement.Name.Length - 1));
                }
            }

            foreach (var r in rowsToRemove.OrderByDescending(v => v))
            {
                rows.RemoveAt(r);
            }

            return string.Join(Environment.NewLine, rows.ToArray());
        }
    }
}