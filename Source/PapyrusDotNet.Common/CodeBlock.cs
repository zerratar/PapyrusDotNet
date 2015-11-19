#region License

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

#endregion

#region

using System.Collections.Generic;
using System.Linq;
using PapyrusDotNet.Common.Papyrus;

#endregion

namespace PapyrusDotNet.Common
{
    public class CodeBlock
    {
        public int StartRow { get; set; }

        public int EndRow { get; set; }

        public List<LabelReference> UsedLabels { get; set; } = new List<LabelReference>();

        public List<LabelDefinition> Labels { get; set; } = new List<LabelDefinition>();

        public LabelDefinition GetLabelDefinition(int row)
        {
            return Labels.FirstOrDefault(r => r.Row == row);
        }
    }
}