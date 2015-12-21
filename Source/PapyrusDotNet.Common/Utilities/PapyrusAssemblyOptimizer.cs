using System;
using System.Collections.Generic;
using System.Linq;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.Common.Papyrus;

namespace PapyrusDotNet.Common.Utilities
{
    public class PapyrusAssemblyOptimizer : IPapyrusAssemblyOptimizer
    {
        private string papyrus;
        private readonly IPapyrusCodeBlockParser codeblockParser;

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
            var rows = papyrus.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

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

            return String.Join(Environment.NewLine, rows.ToArray());
        }

        public string RemoveUnnecessaryLabels()
        {
            var rows = papyrus.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
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

            return String.Join(Environment.NewLine, rows.ToArray());
        }
    }
}