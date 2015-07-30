using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PapyrusDotNet.CoreBuilder
{
    using System.Globalization;
    using System.Text.RegularExpressions;

    using Mono.Cecil;

    public class PscParser
    {
        public static PapyrusScriptObject Parse(string file)
        {

            //   file = @"C:\CreationKit\Data\Scripts\Source\MS05BardLutePlay.psc";

            PapyrusScriptObject output = new PapyrusScriptObject();
            output.Name = System.IO.Path.GetFileNameWithoutExtension(file);

            var source = System.IO.File.ReadAllText(file);


            while (source.Contains(";/") && source.Contains("/;"))
            {
                var start = source.IndexOf(";/");
                var end = source.IndexOf("/;");
                source = source.Remove(start, 1 + end - start);
            }

            while (source.Contains('{') && source.Contains('}'))
            {
                var start = source.IndexOf('{');
                var end = source.IndexOf('}');
                source = source.Remove(start, 1 + end - start);
            }

            source = source.Replace('\t', ' ').Trim();

            var sourceLines = source.Split('\n');

            ParseScript(ref output, sourceLines);

            return output;
        }

        private static void ParseScript(ref PapyrusScriptObject output, string[] sourceLines)
        {
            bool insideFunction = false;
            bool insideState = false;

            PapyrusFunction lastFunction = null;

            PapyrusStateFunction lastState = new PapyrusStateFunction();

            output.StateFunctions.Add(lastState);

            bool lastParametersFinished = true;

            for (var i = 0; i < sourceLines.Length; i++)
            {
                sourceLines[i] = sourceLines[i].Trim();
                if (sourceLines[i].StartsWith(";") || string.IsNullOrEmpty(sourceLines[i]) || sourceLines[i].Length < 3) continue;

                if (sourceLines[i].Contains(";"))
                {
                    sourceLines[i] = sourceLines[i].Remove(sourceLines[i].IndexOf(';'));
                }

                var l = sourceLines[i].ToLower();
                var u = sourceLines[i];
                var dataSplits = l.TrimSplit(" ");
                var dataSplitsNormal = u.TrimSplit(" ");

                if (l.StartsWith("endproperty")) continue;

                if (dataSplits.Contains("import"))
                {
                    output.Imports.Add(dataSplits[1]);
                    continue;
                }

                if (l.StartsWith("scriptname "))
                {
                    var data = sourceLines[i].TrimSplit(" ");

                    if (data.Contains("extends"))
                    {
                        var e = data.IndexOf("extends");
                        output.Extends = data[e + 1];
                    }

                    output.Name = data[data.IndexOf("scriptname") + 1];
                    output.IsConditional = data.Contains("conditional");
                    output.IsHidden = data.Contains("hidden");
                    continue;
                }

                if ((l.StartsWith("state ") || l.StartsWith("auto state ")) && !insideFunction)
                {
                    var values = l.TrimSplit(" ");
                    lastState = new PapyrusStateFunction();
                    if (values.Contains("auto"))
                    {
                        lastState.IsAuto = true;
                        lastState.Name = values[2].Trim();
                    }
                    else
                    {
                        lastState.Name = values[1];
                    }
                    output.StateFunctions.Add(lastState);

                    insideState = true;
                    continue;
                }

                if (l.StartsWith("endstate"))
                {
                    insideState = false;
                    continue;
                }

                if (!lastParametersFinished && insideFunction)
                {
                    var param = ParseParameterList(u, out lastParametersFinished);

                    lastFunction.Parameters.AddRange(param);

                    if (dataSplits.Contains("global"))
                    {
                        lastFunction.IsGlobal = true;
                    }

                    if (dataSplits.Contains("native"))
                    {
                        lastState.Functions.Add(lastFunction);
                        insideFunction = false;
                    }

                    continue;
                }

                if (l.StartsWith("event ") || l.Contains(" event ") || l.StartsWith("function ") || l.Contains(" function ") && l.Contains("(") && l.Contains(")"))
                {
                    lastFunction = new PapyrusFunction();
                    lastFunction.ReturnType = new PapyrusVariable()
                                              {
                                                  VarType = "void"
                                              };

                    var parameters = GetParameters(sourceLines[i], out lastParametersFinished);

                    if (l.Contains("function"))
                    {
                        var fi = dataSplitsNormal.IndexOf("function");
                        lastFunction.Name = dataSplitsNormal[fi + 1].Split('(').FirstOrDefault();
                        if (fi > 0)
                        {
                            var val = dataSplitsNormal.FirstOrDefault();
                            lastFunction.ReturnType = new PapyrusVariable()
                          {
                              VarType = val,
                              IsArray = val.Contains("[]")
                          };
                        }
                    }
                    else if (l.Contains("event"))
                    {
                        lastFunction.IsEvent = true;
                        lastFunction.Name = dataSplitsNormal[dataSplitsNormal.IndexOf("event") + 1].Split('(').FirstOrDefault();
                    }

                    if (dataSplits.Contains("global"))
                    {
                        lastFunction.IsGlobal = true;
                    }

                    lastFunction.Parameters.AddRange(parameters);

                    insideFunction = true;
                    if (dataSplits.Contains("native"))
                    {
                        lastFunction.IsNative = true;

                        lastState.Functions.Add(lastFunction);
                        insideFunction = false;
                    }
                    continue;
                }

                if (insideFunction && (l.StartsWith("endfunction") || l.StartsWith("endevent")))
                {
                    lastState.Functions.Add(lastFunction);
                    insideFunction = false;
                    continue;
                }


                if (!insideFunction)
                {
                    var prop = new PapyrusVariable();
                    prop.VarType = dataSplits[0];
                    prop.IsArray = prop.VarType.Contains("[]");
                    prop.IsConditional = dataSplits.Contains("conditional");
                    prop.IsProperty = dataSplits.Contains("property");
                    prop.IsAuto = dataSplits.Contains("auto");
                    prop.Name = prop.IsProperty ? dataSplitsNormal[2] : dataSplitsNormal[1];

                    if (dataSplits.Contains("="))
                    {
                        prop.InitialValue = dataSplitsNormal.LastOrDefault().Trim();
                        prop.HasInitialValue = true;
                    }

                    if (prop.Name.Contains("="))
                    {
                        var val = prop.Name.TrimSplit("=");
                        prop.Name = val.FirstOrDefault();
                        prop.InitialValue = val.LastOrDefault();
                        prop.HasInitialValue = true;
                    }

                    output.Properties.Add(prop);
                }

            }
        }


        public static List<PapyrusVariable> ParseParameterList(string input, out bool wasFinished)
        {
            wasFinished = true;
            var p = new List<PapyrusVariable>();
            var vars = new[] { input };
            if (input.Contains(','))
            {
                vars = input.Split(',');
            }
            foreach (var v in vars)
            {
                if (!v.Contains(" ") || v.TrimSplit(" ").Length == 1)
                {
                    wasFinished = false;
                    continue;
                }
                var param = new PapyrusVariable();
                var d = v.TrimSplit(" ");
                param.VarType = d[0];
                param.Name = d[1];
                param.Name = param.Name.Replace(")", "");
                if (d.Length > 2 && v.Contains("="))
                {
                    // Default values
                    param.InitialValue = v.TrimSplit("=").LastOrDefault();
                    param.InitialValue = param.InitialValue.Replace(")", "");
                    param.HasInitialValue = true;
                }
                p.Add(param);
            }
            return p;
        }

        private static List<PapyrusVariable> GetParameters(string input, out bool wasFinished)
        {
            wasFinished = true;

            if (input.Contains("()") || input.Contains("( )"))
                return new List<PapyrusVariable>();

            var varData = input.Split('(')[1].Split(')')[0];

            if (string.IsNullOrEmpty(varData.Trim()))
                return new List<PapyrusVariable>();

            return ParseParameterList(varData, out wasFinished);

        }
    }
    public static class StringExtensions
    {
        public static bool Contains(this string[] input, string val)
        {
            return input.Select(v => v.ToLower().Trim()).Any(b => b == val);
        }
        public static bool AnyContains(this string[] input, string val)
        {
            return input.Select(v => v.ToLower().Trim()).Any(b => b.Contains(val));
        }

        public static int IndexOf(this string[] input, string val)
        {
            return Array.IndexOf(input.Select(d => d.ToLower().Trim()).ToArray(), val);
        }

        public static string[] TrimSplit(this string input, string val)
        {
            return input.Split(new string[] { val }, StringSplitOptions.RemoveEmptyEntries);
        }
    }

    public class PapyrusScriptObject
    {
        public string Name;

        public string Extends;

        public bool IsConditional;

        public bool IsHidden;

        public List<string> Imports;

        public List<PapyrusStateFunction> StateFunctions;

        public List<PapyrusVariable> Properties;

        public List<PapyrusVariable> InstanceVariables;

        public PapyrusScriptObject()
        {
            Imports = new List<string>();
            StateFunctions = new List<PapyrusStateFunction>();
            Properties = new List<PapyrusVariable>();
            InstanceVariables = new List<PapyrusVariable>();
        }
    }

    public class PapyrusStateFunction
    {
        public string Name;

        public bool IsAuto;

        public List<PapyrusFunction> Functions;

        public PapyrusStateFunction()
        {
            Functions = new List<PapyrusFunction>();
        }
    }

    public class PapyrusFunction
    {
        public string Name;

        public string StateName;

        public PapyrusVariable ReturnType;

        public List<PapyrusVariable> Parameters;

        public bool IsGlobal;

        public bool IsNative;

        public bool IsEvent;

        public PapyrusFunction()
        {
            this.Parameters = new List<PapyrusVariable>();
        }

        public PapyrusFunction(string name, string stateName)
        {
            this.Name = name;
            this.StateName = stateName;
            this.Parameters = new List<PapyrusVariable>();
        }
    }

    public class PapyrusVariable
    {
        public string Name;

        public string VarType;

        public bool HasInitialValue;

        public string InitialValue;

        public bool IsGlobal;

        public bool IsAuto;

        public bool IsConditional;

        public bool IsProperty;

        public bool IsHidden;

        public bool IsArray;
    }
}
