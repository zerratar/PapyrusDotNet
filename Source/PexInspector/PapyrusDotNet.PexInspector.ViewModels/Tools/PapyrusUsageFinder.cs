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
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.PexInspector.ViewModels.Tools
{
    public class PapyrusUsageFinder : IPapyrusUsageFinder
    {
        private readonly IEnumerable<PapyrusAssemblyDefinition> haystack;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PapyrusUsageFinder" /> class.
        /// </summary>
        /// <param name="haystack">The haystack.</param>
        public PapyrusUsageFinder(IEnumerable<PapyrusAssemblyDefinition> haystack)
        {
            this.haystack = haystack;
        }

        /// <summary>
        ///     Finds the method usage.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        public IFindResult FindMethodUsage(string methodName)
        {
            var result = new FindResult();
            result.SearchText = methodName;
            foreach (var asm in haystack)
            {
                foreach (var t in asm.Types)
                {
                    foreach (var s in t.States)
                    {
                        foreach (var m in s.Methods)
                        {
                            foreach (var i in m.Body.Instructions)
                            {
                                if (i.OpCode != PapyrusOpCodes.Callmethod && i.OpCode != PapyrusOpCodes.Callparent &&
                                    i.OpCode != PapyrusOpCodes.Callstatic)
                                    continue;

                                var m2 = methodName.ToLower();
                                var a = i.GetArg(0).ToLower();
                                var b = i.GetArg(1).ToLower();
                                var c = "";
                                if (i.Arguments.Count > 2)
                                    c = i.GetArg(2).ToLower();

                                if (c == m2 || a == m2 || b == m2)
                                {
                                    result.AddResult(
                                        t, s, m, i, methodName,
                                        t.Name + "->" + s.Name + "->" + m.Name.Value + "-> L_" + i.Offset + ": " +
                                        i.OpCode + " - " + methodName + "(" +
                                        string.Join(", ", i.OperandArguments.Select(j => j.Value)) + ")");
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        ///     Finds the property usage.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public IFindResult FindPropertyUsage(string propertyName)
        {
            var result = new FindResult();
            result.SearchText = propertyName;
            foreach (var asm in haystack)
            {
                foreach (var t in asm.Types)
                {
                    foreach (var s in t.States)
                    {
                        foreach (var m in s.Methods)
                        {
                            foreach (var i in m.Body.Instructions)
                            {
                                if (i.OpCode != PapyrusOpCodes.PropGet && i.OpCode != PapyrusOpCodes.PropSet)
                                    continue;

                                var m2 = propertyName.ToLower();
                                var a = i.GetArg(0).ToLower();
                                var b = i.GetArg(1).ToLower();
                                var c = "";
                                if (i.Arguments.Count > 2)
                                    c = i.GetArg(2).ToLower();

                                if (c == m2 || a == m2 || b == m2)
                                {
                                    result.AddResult(
                                        t, s, m, i, propertyName,
                                        t.Name + "->" + s.Name + "->" + m.Name.Value + "-> L_" + i.Offset + ": " +
                                        i.OpCode + " - " + propertyName);
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        ///     Finds the field usage.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        public IFindResult FindFieldUsage(string fieldName)
        {
            var result = new FindResult();
            result.SearchText = fieldName;
            foreach (var asm in haystack)
            {
                foreach (var t in asm.Types)
                {
                    foreach (var s in t.States)
                    {
                        foreach (var m in s.Methods)
                        {
                            foreach (var i in m.Body.Instructions)
                            {
                                if (i.OpCode == PapyrusOpCodes.Nop)
                                    continue;

                                var m2 = fieldName.ToLower();

                                if (i.OperandArguments.Any(a => a.GetStringRepresentation().ToLower() == m2) ||
                                    i.Arguments.Any(a => a.GetStringRepresentation().ToLower() == m2))
                                {
                                    result.AddResult(
                                        t, s, m, i, fieldName,
                                        t.Name + "->" + s.Name + "->" + m.Name.Value + "-> L_" + i.Offset + ": " +
                                        i.OpCode + " - " + fieldName);
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}