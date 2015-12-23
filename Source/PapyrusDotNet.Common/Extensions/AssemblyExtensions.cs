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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion

namespace PapyrusDotNet.Common.Extensions
{
    public enum FindFilterOptions
    {
        NameEndsWith,
        NameStartsWith,
        NameContains
    }

    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> FindTypes(this Assembly asm, string search,
            FindFilterOptions options = FindFilterOptions.NameEndsWith)
        {
            switch (options)
            {
                case FindFilterOptions.NameEndsWith:
                    return from t in asm.GetTypes()
                        where t.Name.ToLower().EndsWith(search)
                        select t;
                case FindFilterOptions.NameContains:
                    return from t in asm.GetTypes()
                        where t.Name.ToLower().Contains(search)
                        select t;
                case FindFilterOptions.NameStartsWith:
                    return from t in asm.GetTypes()
                        where t.Name.ToLower().StartsWith(search)
                        select t;
            }
            return from t in asm.GetTypes()
                where t.Name.ToLower().EndsWith(search)
                select t;
        }
    }
}