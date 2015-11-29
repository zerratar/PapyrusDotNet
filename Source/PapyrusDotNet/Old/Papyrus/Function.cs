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
//     Copyright 2015, Karl Patrik Johansson, zerratar@gmail.com

#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using PapyrusDotNet.Common;
using PapyrusDotNet.Common.Papyrus;

#endregion

namespace PapyrusDotNet.Old.Papyrus
{
    public class Function
    {
        public List<string> CodeInstructions;
        public int DelegateInvokeCount;
        public int ExtensionInvokeCount;

        public List<VariableReference> Fields;
        public bool IsGlobal;
        public bool IsNative;

        public MethodDefinition MethodDefinition;
        public string Name;
        public List<VariableReference> Parameters;
        public string ReturnType;

        public StringBuilder Source;

        public List<VariableReference> TempVariables;
        public int UserFlags = 0;

        public List<VariableReference> Variables;

        public Function()
        {
            Parameters = new List<VariableReference>();
            Variables = new List<VariableReference>();
            TempVariables = new List<VariableReference>();
            CodeInstructions = new List<string>();
        }

        public Assembly Assembly { get; set; }

        public List<VariableReference> AllVariables
        {
            get
            {
                var var1 = new List<VariableReference>();
                if (Variables != null)
                    var1.AddRange(Variables);
                if (TempVariables != null)
                    var1.AddRange(TempVariables);
                if (Fields != null)
                    var1.AddRange(Fields);
                return var1;
            }
        }

        public string InstanceCaller { get; set; }

        public void InsertCodeInstruction(int index, string instruction)
        {
            var sourcecode = Source.ToString();

            instruction = instruction.Replace("\t", "");
            instruction = "\t\t\t\t\t\t" + instruction;

            var lines = sourcecode.Split('\n').ToList();
            var targetLine = lines.FirstOrDefault(l => l.Contains(".code"));
            var startIndex = Array.IndexOf(lines.ToArray(), targetLine) + 1;

            lines.Insert(index + startIndex, instruction);

            CodeInstructions.Insert(index, instruction);

            Source = new StringBuilder(string.Join("\n", lines));
        }

        public VariableReference CreateTempVariable(string p, MethodReference methodRef = null)
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
                    catch
                    {
                        p = originalTarget;
                    }
                }
            }

            var @namespace = "";
            var name = "";
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
            var varRef = new VariableReference(varname, type, def);
            TempVariables.Add(varRef);
            return varRef;
        }

        public override string ToString()
        {
            return Source.ToString();
        }

        public void RemoveStaticKeyword()
        {
            IsGlobal = false;

            var sourcecode = Source.ToString();

            var lines = sourcecode.Split('\n').ToList();

            lines[0] = lines[0].Replace(" static", "");

            Source = new StringBuilder(string.Join("\n", lines));
        }

        internal void ReplaceGenericTypesWith(string LastSaughtTypeName)
        {
            var sourcecode = Source.ToString();

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
    }
}