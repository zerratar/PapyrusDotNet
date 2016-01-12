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
using System.Collections;
using System.Collections.Generic;

#endregion

namespace PapyrusDotNet.PapyrusAssembly.Extensions
{
    public static class CollectionExtensions
    {
        public static void EnsureAdd<T>(this List<T> collection, T value)
        {
            if (collection == null) throw new NullReferenceException();
            if (!collection.Contains(value))
                collection.Add(value);
        }

        public static void EnsureAdd<T>(this IList<T> collection, T value)
        {
            if (collection == null) throw new NullReferenceException();
            if (!collection.Contains(value))
                collection.Add(value);
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> a)
        {
            if (a == null) return;
            foreach (var c in collection)
            {
                a(c);
            }
        }

        public static void ForEach(this IEnumerable collection, Action<object> a)
        {
            if (a == null) return;
            foreach (var c in collection)
            {
                a(c);
            }
        }
    }
}