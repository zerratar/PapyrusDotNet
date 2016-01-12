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
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.PexInspector.ViewModels.Tools
{
    public class FindResult : IFindResult
    {
        private readonly List<FindResultData> usageRepresentaitons;

        public FindResult()
        {
            usageRepresentaitons = new List<FindResultData>();
        }

        public IEnumerable<FindResultData> Results => usageRepresentaitons;
        public bool HasResults => usageRepresentaitons.Count > 0;
        public string SearchText { get; set; }

        public void AddResult(PapyrusTypeDefinition type, PapyrusStateDefinition state, PapyrusMethodDefinition method,
            PapyrusInstruction instruction, string search, string resultRepresentation)
        {
            SearchText = search;
            usageRepresentaitons.Add(new FindResultData
            {
                Type = type,
                State = state,
                Method = method,
                Instruction = instruction,
                Text = resultRepresentation,
                SearchText = search
            });
        }
    }
}