using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PapyrusDotNet.Models
{
    using PapyrusDotNet.Common;

    public class PapyrusFunction
    {
        public List<PapyrusVariableReference> Parameters;

        public List<PapyrusVariableReference> Variables;

        public List<PapyrusVariableReference> TempVariables;

        public List<PapyrusVariableReference> Fields;

        public List<string> CodeInstructions;

        public StringBuilder Source;
        public bool IsGlobal;
        public string ReturnType;
        public bool IsNative;
        public int UserFlags = 0;

        public PapyrusFunction()
        {
            this.Parameters = new List<PapyrusVariableReference>();
            this.Variables = new List<PapyrusVariableReference>();
            this.TempVariables = new List<PapyrusVariableReference>();
            this.CodeInstructions = new List<string>();
        }

        public void InsertCodeInstruction(int index, string instruction)
        {
            var sourcecode = this.Source.ToString();

            instruction = instruction.Replace("\t", "");
            instruction = "\t\t\t\t\t\t" + instruction;

            var lines = sourcecode.Split('\n').ToList();
            var targetLine = lines.FirstOrDefault(l => l.Contains(".code"));
            var startIndex = Array.IndexOf(lines.ToArray(), targetLine) + 1;

            lines.Insert(index + startIndex, instruction);

            CodeInstructions.Insert(index, instruction);

            Source = new StringBuilder(string.Join("\n", lines));
        }

        public List<PapyrusVariableReference> AllVariables
        {
            get
            {
                var var1 = new List<PapyrusVariableReference>();
                if (Variables != null)
                    var1.AddRange(Variables);
                if (TempVariables != null)
                    var1.AddRange(TempVariables);
                if (Fields != null)
                    var1.AddRange(Fields);
                return var1;
            }
        }

        public PapyrusVariableReference CreateTempVariable(string p)
        {
            string Namespace = "";
            string Name = "";
            if (p.Contains("."))
            {
                Namespace = p.Remove(p.LastIndexOf('.'));
                Name = p.Split('.').LastOrDefault();
            }
            else
            {
                Name = p;
            }
            var varname = "::temp" + TempVariables.Count;
            var type = Utility.GetPapyrusReturnType(Name, Namespace);
            var def = ".local " + varname + " " + type.Replace("<T>", "");
            var varRef = new PapyrusVariableReference(varname, type, def);
            TempVariables.Add(varRef);
            return varRef;
        }

        public override string ToString()
        {
            return Source.ToString();
        }
    }
}
