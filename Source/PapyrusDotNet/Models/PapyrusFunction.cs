using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

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

        public MethodDefinition MethodDefinition;

        public PapyrusAssembly PapyrusAssembly { get; set; }

        public StringBuilder Source;
        public bool IsGlobal;
        public string ReturnType;
        public bool IsNative;
        public int UserFlags = 0;
        public string Name;
        public int DelegateInvokeCount;
        public int ExtensionInvokeCount;
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

        public PapyrusVariableReference CreateTempVariable(string p, MethodReference methodRef = null)
        {
            var originalTarget = p;
            if (p.StartsWith("!"))
            {
                // Get argument variable at index 1
                if (methodRef != null && methodRef.FullName.Contains("<") && methodRef.FullName.Contains(","))
                {
                    try
                    {
                        var pm = methodRef.FullName.TrimSplit("<")[1].TrimSplit(">")[0];
                        var vars = pm.TrimSplit(",");
                        var argIndex = int.Parse(p.Substring(1));
                        p = vars[argIndex];
                    }
                    catch { p = originalTarget; }
                }
            }

            string @namespace = "";
            string name = "";
            if (p.Contains("."))
            {
                @namespace = p.Remove(p.LastIndexOf('.'));
                name = p.Split('.').LastOrDefault();
            }
            else
            {
                name = p;
            }


            var varname = "::temp" + TempVariables.Count;
            var type = Utility.GetPapyrusReturnType(name, @namespace);
            var def = ".local " + varname + " " + type.Replace("<T>", "");
            var varRef = new PapyrusVariableReference(varname, type, def);
            TempVariables.Add(varRef);
            return varRef;
        }

        public override string ToString()
        {
            return Source.ToString();
        }

        public void RemoveStaticKeyword()
        {
            this.IsGlobal = false;

            var sourcecode = this.Source.ToString();

            var lines = sourcecode.Split('\n').ToList();

            lines[0] = lines[0].Replace(" static", "");

            Source = new StringBuilder(string.Join("\n", lines));
        }

        internal void ReplaceGenericTypesWith(string LastSaughtTypeName)
        {
            var sourcecode = this.Source.ToString();

            var lines = sourcecode.Split('\n').ToList();

            for (var i = 0; i < lines.Count; i++)
            {

                var trimmedLine = lines[i].Replace("\t", "").Trim();
                if (trimmedLine.Equals(".return T") || trimmedLine.Equals(".return T[]"))
                {
                    lines[i] = lines[i].Replace(".return T", ".return " + LastSaughtTypeName);
                }
                if (trimmedLine.EndsWith(" T") || trimmedLine.EndsWith(" T[]"))
                {
                    lines[i] = lines[i].Replace(" T", " " + LastSaughtTypeName);
                }
            }

            Source = new StringBuilder(string.Join("\n", lines));
        }

        public string InstanceCaller { get; set; }
    }
}
