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
using PapyrusDotNet.Common.Interfaces;

#endregion

namespace PapyrusDotNet.Common.Utilities
{
    public class StringUtility : IUtility
    {
        public static string Indent(int indents, string line, bool newLine = true)
        {
            var output = "";
            for (var j = 0; j < indents; j++) output += '\t';
            output += line;

            if (newLine) output += Environment.NewLine;

            return output;
        }

        public static string AsString(object p)
        {
            if (p is string) return (string) p;
            return "";
        }
    }
}