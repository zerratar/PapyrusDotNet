using System.Collections.Generic;
using System.Linq;
using PapyrusDotNet.PapyrusAssembly.Parser.Interfaces;

namespace PapyrusDotNet.PapyrusAssembly.Parser
{
    public class PapyrusAssemblyInstructionParser : IPapyrusAssemblyInstructionParser
    {
        /// <summary>
        /// Parses the string represented instruction and returns a <see cref="PapyrusAsmInstruction"/>.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <returns></returns>
        public PapyrusAsmInstruction ParseInstruction(string instruction)
        {
            if (string.IsNullOrEmpty(instruction)) return null;
            var data = GetAsmValues(instruction.Trim().Trim(' ', '\t'));
            var desc = PapyrusInstructionOpCodeDescription.FromAlias(data[0].Value);
            var ins = new PapyrusAsmInstruction(desc.Key, desc.Value.ArgumentCount, desc.Value.HasOperandArguments, desc.Value.Aliases);

            for (var i = 0; i < ins.ArgumentCount; i++)
                ins.SetArgument(i, data[i + 1]);

            var opargs = data.Count - ins.ArgumentCount - 1;

            for (var i = 0; i < opargs; i++)
                ins.SetOperandArgument(i, data[i + ins.ArgumentCount + 1]);

            return ins;
        }

        public List<PapyrusAsmValue> GetAsmValues(string input)
        {
            var res = new List<PapyrusAsmValue>();
            PapyrusAsmValue activeVal = null;
            var insideString = false;
            var specialToken = false;
            foreach (var c in input.TakeWhile(c => c != ';'))
            {
                if (activeVal == null) activeVal = new PapyrusAsmValue();
                switch (c)
                {
                    case '\\':
                        specialToken = true;
                        continue;
                    case '"':
                        if (specialToken)
                        {
                            specialToken = false;
                            activeVal.Value += "\\\""; // \"
                            continue;
                        }
                        insideString = !insideString;
                        if (insideString) continue;
                        if (activeVal.Value == null) activeVal.Value = string.Empty; // Value should just be empty, not null.
                        res.Add(activeVal);
                        activeVal = null;
                        break;
                    default:
                        if ((c == ' ' || c == '\t') && !insideString)
                        {
                            if (!string.IsNullOrEmpty(activeVal.Value)) res.Add(activeVal);
                            activeVal = null;
                        }
                        else
                            activeVal.Value += c;
                        break;
                }
            }
            if (!string.IsNullOrEmpty(activeVal?.Value)) res.Add(activeVal);
            return res;
        }

        /// <summary>
        /// Parses a string containing multiple instructions separated by a newline and returns collection of <see cref="PapyrusAsmInstruction"/>.
        /// </summary>
        /// <param name="inputInstructions">The input instructions.</param>
        /// <returns></returns>
        public IList<PapyrusAsmInstruction> ParseInstructions(string inputInstructions)
        {
            return ParseInstructions(inputInstructions.Split('\n'));
        }

        /// <summary>
        /// Parses an array of strings containing a string represented instruction per item and returns collection of <see cref="PapyrusAsmInstruction"/>.
        /// </summary>
        /// <param name="inputInstructions">The input instructions.</param>
        /// <returns></returns>
        public IList<PapyrusAsmInstruction> ParseInstructions(string[] inputInstructions)
        {
            return new List<PapyrusAsmInstruction>(inputInstructions.Select(ParseInstruction));
        }
    }
}