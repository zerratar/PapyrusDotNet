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
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.PexInspector.ViewModels.Tools
{
    public class PapyrusLocationFinder : IPapyrusLocationFinder
    {
        private readonly IList<PapyrusAssemblyDefinition> haystack;

        public PapyrusLocationFinder(IList<PapyrusAssemblyDefinition> haystack)
        {
            this.haystack = haystack;
        }

        public IFindResult FindMethodLocation(string methodName)
        {
            var result = new FindResult();
            result.SearchText = methodName;
            var mn = methodName.ToLower();
            foreach (var asm in haystack)
            {
                foreach (var t in asm.Types)
                {
                    foreach (var s in t.States)
                    {
                        foreach (var m in s.Methods)
                        {
                            if (m.Name != null && m.Name.Value != null && m.Name.Value.ToLower() == mn)
                            {
                                result.AddResult(
                                    t, s, m, null, methodName,
                                    t.Name + "->" + s.Name + "->" + m.Name.Value + "(" +
                                    string.Join(", ", m.Parameters.Select(j => j.TypeName.Value + " " + j.Name.Value)) +
                                    ")");
                            }
                        }
                    }
                }
            }
            return result;
        }

        public IFindResult FindPropertyLocation(string propertyName)
        {
            var result = new FindResult();
            result.SearchText = propertyName;
            var mn = propertyName.ToLower();
            foreach (var asm in haystack)
            {
                foreach (var t in asm.Types)
                {
                    foreach (var s in t.States)
                    {
                        foreach (var m in s.Methods)
                        {
                            if (m.Name != null && m.Name.Value != null && m.Name.Value.ToLower() == mn)
                            {
                                result.AddResult(
                                    t, s, m, null, propertyName,
                                    t.Name + "->" + s.Name + "->" + m.Name.Value + "(" +
                                    string.Join(", ", m.Parameters.Select(j => j.TypeName.Value + " " + j.Name.Value)) +
                                    ")");
                            }
                        }
                    }
                }
            }
            return result;
        }

        public IFindResult FindFieldLocation(string fieldName)
        {
            throw new NotImplementedException();
        }
    }
}