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
    public class PapyrusReferenceFinder : IPapyrusReferenceFinder
    {
        private readonly IEnumerable<PapyrusAssemblyDefinition> haystack;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PapyrusReferenceFinder" /> class.
        /// </summary>
        /// <param name="haystack">The haystack.</param>
        public PapyrusReferenceFinder(IEnumerable<PapyrusAssemblyDefinition> haystack)
        {
            this.haystack = haystack;
        }

        /// <summary>
        ///     Finds the type references.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        public IFindResult FindTypeReference(string typeName)
        {
            var result = new FindResult();
            result.SearchText = typeName;
            foreach (var asm in haystack)
            {
                foreach (var t in asm.Types)
                {
                    if (t.BaseTypeName != null
                        && !string.IsNullOrEmpty(t.BaseTypeName.Value)
                        && t.BaseTypeName.Value.ToLower() == typeName.ToLower())
                    {
                        // result.AddResult();
                        result.AddResult(t, null, null, null, typeName, null);
                    }
                }
            }
            return result;
        }
    }
}