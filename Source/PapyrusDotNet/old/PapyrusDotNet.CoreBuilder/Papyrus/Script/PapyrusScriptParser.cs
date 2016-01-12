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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using PapyrusDotNet.Common;
using PapyrusDotNet.CoreBuilder.Interfaces;
using PapyrusDotNet.CoreBuilder.Papyrus.Assembly;

#endregion

namespace PapyrusDotNet.CoreBuilder.Papyrus.Script
{
    public class PapyrusScriptParser : IPapyrusScriptParser
    {
        private readonly IPapyrusNameResolver functionNameResolver;

        public PapyrusScriptParser(IPapyrusNameResolver nameResolver)
        {
            functionNameResolver = nameResolver;
        }

        public PapyrusAssemblyObject ParseScript(string file)
        {
            var obj = new PapyrusAssemblyObject();

            var scriptObject = Parse(file);

            var objName = scriptObject.Name;

            obj.Name = objName;

            foreach (var s in scriptObject.StateFunctions)
            {
                var state = new PapyrusAssemblyState();
                state.Name = s.Name;
                foreach (var f in s.Functions)
                {
                    var sourceFunction = f;

                    if (!string.IsNullOrEmpty(sourceFunction.StateName) && string.IsNullOrEmpty(state.Name))
                        state.Name = sourceFunction.StateName;

                    var function = new PapyrusAssemblyFunction();
                    function.Name = functionNameResolver.Resolve(sourceFunction.Name);
                    function.ReturnType = sourceFunction.ReturnType.VarType;
                    function.ReturnArray = sourceFunction.ReturnType.IsArray;
                    var inputNameIndex = 0;
                    foreach (var p in sourceFunction.Parameters)
                    {
                        function.Params.Add(new PapyrusAssemblyVariable(p.Name, p.VarType));

                        inputNameIndex++;
                    }

                    function.IsStatic = sourceFunction.IsGlobal;
                    function.IsNative = sourceFunction.IsNative;
                    function.IsEvent = sourceFunction.IsEvent;
                    // Since we are only generating "dummy" classes,
                    // the function scope is not necessary at this point.
                    // var fs = sourceFunction.FunctionScope;

                    state.Functions.Add(function);
                }

                obj.States.Add(state);
            }

            // Ignore Non Overridable Functions for now.
            // We will need to handle these later to make sure that a user
            // does not override them. GetState, SetState are both common.
            //var nof = scriptObject.kNonOverridableFunctions;
            //var instanceVars = scriptObject.kInstanceVariables;
            //var propertyVars = scriptObject.kProperties;

            // If the script is extended by another script
            // The script will have a parent.
            if (scriptObject.Extends != null)
            {
                obj.ExtendsName = scriptObject.Extends;
            }

            foreach (var prop in scriptObject.Properties)
            {
                obj.PropertyTable.Add(new PapyrusAssemblyVariable(prop.Name, prop.VarType));
            }

            foreach (var field in scriptObject.InstanceVariables)
            {
                // Most likely an auto- property
                // And we have already added them above.
                if (field.Name.StartsWith("::")) continue;
            }

            //      throw new NotImplementedException("Parsing from .psc/ Skyrim Papyrus Scripts are not yet supported.");

            //	TypeDefinition def = new TypeDefinition("","", TypeAttributes.);


            return obj;
        }

        public PapyrusScriptObject Parse(string file)
        {
            var output = new PapyrusScriptObject();
            output.Name = Path.GetFileNameWithoutExtension(file);

            var source = File.ReadAllText(file);


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

        public void ParseScript(ref PapyrusScriptObject output, string[] sourceLines)
        {
            var insideFunction = false;
            var insideState = false;

            PapyrusScriptFunction lastFunction = null;

            var lastState = new PapyrusScriptStateFunction();

            output.StateFunctions.Add(lastState);

            var lastParametersFinished = true;

            for (var i = 0; i < sourceLines.Length; i++)
            {
                sourceLines[i] = sourceLines[i].Trim();
                if (sourceLines[i].StartsWith(";") || string.IsNullOrEmpty(sourceLines[i]) || sourceLines[i].Length < 3)
                    continue;

                if (sourceLines[i].Contains(";"))
                {
                    sourceLines[i] = sourceLines[i].Remove(sourceLines[i].IndexOf(';'));
                }

                var l = sourceLines[i].ToLower();
                var u = sourceLines[i];
                var dataSplits = l.TrimSplit(" ");
                var dataSplitsNormal = u.TrimSplit(" ");

                // We won't handle EndProperty just yet.
                // Since we are not exposing any properties to the DLL atm
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
                    lastState = new PapyrusScriptStateFunction();
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

                if (l.StartsWith("endstate") && insideState)
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

                if (l.StartsWith("event ") || l.Contains(" event ") || l.StartsWith("function ") ||
                    l.Contains(" function ") && l.Contains("(") && l.Contains(")"))
                {
                    lastFunction = new PapyrusScriptFunction();
                    lastFunction.ReturnType = new PapyrusScriptVariable
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
                            lastFunction.ReturnType = new PapyrusScriptVariable
                            {
                                VarType = val,
                                IsArray = val.Contains("[]")
                            };
                        }
                    }
                    else if (l.Contains("event"))
                    {
                        lastFunction.IsEvent = true;
                        lastFunction.Name =
                            dataSplitsNormal[dataSplitsNormal.IndexOf("event") + 1].Split('(').FirstOrDefault();
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
                    var prop = new PapyrusScriptVariable();
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


        public IEnumerable<PapyrusScriptVariable> ParseParameterList(string input, out bool wasFinished)
        {
            wasFinished = true;
            var p = new List<PapyrusScriptVariable>();
            var vars = new[] {input};
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
                var param = new PapyrusScriptVariable();
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

        public IEnumerable<PapyrusScriptVariable> GetParameters(string input, out bool wasFinished)
        {
            wasFinished = true;

            if (input.Contains("()") || input.Contains("( )"))
                return new List<PapyrusScriptVariable>();

            var varData = input.Split('(')[1].Split(')')[0];

            if (string.IsNullOrEmpty(varData.Trim()))
                return new List<PapyrusScriptVariable>();

            return ParseParameterList(varData, out wasFinished);
        }
    }
}