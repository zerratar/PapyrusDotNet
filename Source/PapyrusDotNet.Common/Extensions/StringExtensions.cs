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
using System.Linq;

#endregion

namespace PapyrusDotNet.Common
{
    public static class StringExtensions
    {
        public static bool Contains(this string[] input, string val)
            => input.Select(v => v.ToLower().Trim()).Any(b => b == val);

        public static bool AnyContains(this string[] input, string val)
            => input.Select(v => v.ToLower().Trim()).Any(b => b.Contains(val));

        public static int IndexOf(this string[] input, string val)
            => Array.IndexOf(input.Select(d => d.ToLower().Trim()).ToArray(), val);

        public static string[] TrimSplit(this string input, string val)
            => input.Split(new[] {val}, StringSplitOptions.RemoveEmptyEntries);
    }
}