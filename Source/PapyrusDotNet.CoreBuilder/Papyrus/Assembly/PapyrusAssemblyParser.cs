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

using System.IO;
using PapyrusDotNet.CoreBuilder.Interfaces;

#endregion

namespace PapyrusDotNet.CoreBuilder.Papyrus.Assembly
{
    public class PapyrusAssemblyParser : IPapyrusAssemblyParser
    {
        private readonly IPapyrusNameResolver assemblyNameResolver;

        public PapyrusAssemblyParser(IPapyrusNameResolver nameResolver)
        {
            assemblyNameResolver = nameResolver;
        }

        public PapyrusAssemblyObject ParseAssembly(string file)
        {
            var inputScript = File.ReadAllLines(file);
            var obj = new PapyrusAssemblyObject();
            var inVariableTable = false;
            var inPropertyTable = false;
            var inStateTable = false;
            var inFunction = false;
            var inFunctionLocalTable = false;
            var inFunctionParamTable = false;

            PapyrusAssemblyState lastState = null;
            PapyrusAssemblyFunction lastFunction = null;

            foreach (var line in inputScript)
            {
                var tLine = line.Replace("\t", "").Trim();
                if (tLine.Contains(";"))
                    tLine = tLine.Split(';')[0].Trim();

                if (tLine.StartsWith(".variableTable"))
                    inVariableTable = true;
                if (tLine.StartsWith(".endVariableTable"))
                    inVariableTable = false;

                if (tLine.StartsWith(".propertyTable"))
                    inPropertyTable = true;
                if (tLine.StartsWith(".endPropertyTable"))
                    inPropertyTable = false;

                if (tLine.StartsWith(".stateTable"))
                    inStateTable = true;
                if (tLine.StartsWith(".endStateTable"))
                    inStateTable = false;

                if (tLine.StartsWith(".paramTable"))
                    inFunctionParamTable = true;
                if (tLine.StartsWith(".endParamTable"))
                    inFunctionParamTable = false;

                if (tLine.StartsWith(".localTable"))
                    inFunctionLocalTable = true;
                if (tLine.StartsWith(".endLocalTable"))
                    inFunctionLocalTable = false;

                if (tLine.StartsWith(".object "))
                {
                    //			obj.Name = tLine.Split(' ')[1];
                    obj.Name = Path.GetFileNameWithoutExtension(file);


                    if (obj.Name.Contains("."))
                    {
                        obj.Name = obj.Name.Split('.')[0];
                    }

                    //string before = obj.Name;

                    obj.Name = assemblyNameResolver.Resolve(obj.Name);

                    //var theBefore = before;
                    //var theAfter = obj.Name;


                    if (tLine.Split(' ').Length > 2)
                    {
                        obj.ExtendsName = tLine.Split(' ')[2];
                    }
                    if (tLine.Contains("extends"))
                    {
                        obj.ExtendsName = tLine.Split(' ')[3];
                        // Parse(@"C:\The Elder Scrolls V Skyrim\Papyrus Compiler\" + tLine.Split(' ')[3] + ".disassemble.pas");
                    }
                }

                if (inVariableTable)
                {
                }
                else if (inPropertyTable)
                {
                }
                else if (inStateTable)
                {
                    if (tLine.StartsWith(".state") || tLine.StartsWith(".endState"))
                    {
                        if (tLine.StartsWith(".state"))
                        {
                            lastState = new PapyrusAssemblyState();
                        }
                        if (tLine.StartsWith(".endState"))
                        {
                            obj.States.Add(lastState);
                        }
                        continue;
                    }
                    if (tLine.StartsWith(".function "))
                    {
                        inFunction = true;
                        lastFunction = new PapyrusAssemblyFunction();

                        if (tLine.Contains(" static")) lastFunction.IsStatic = true;
                        lastFunction.Name = tLine.Split(' ')[1];
                    }
                    if (tLine.StartsWith(".endFunction") && inFunction)
                    {
                        inFunction = false;
                        lastState.Functions.Add(lastFunction);
                    }
                    if (inFunctionLocalTable && lastFunction != null)
                    {
                        if (tLine.StartsWith(".local "))
                        {
                            lastFunction.LocalTable.Add(new PapyrusAssemblyVariable(tLine.Split(' ')[1],
                                tLine.Split(' ')[2]));
                        }
                    }
                    if (inFunctionParamTable && lastFunction != null)
                    {
                        if (tLine.StartsWith(".param "))
                        {
                            lastFunction.Params.Add(new PapyrusAssemblyVariable(tLine.Split(' ')[1], tLine.Split(' ')[2]));
                        }
                    }
                    if (tLine.StartsWith(".return ") && lastFunction != null)
                    {
                        lastFunction.ReturnType = tLine.Split(' ')[1];
                    }
                }
            }
            return obj;
        }
    }
}