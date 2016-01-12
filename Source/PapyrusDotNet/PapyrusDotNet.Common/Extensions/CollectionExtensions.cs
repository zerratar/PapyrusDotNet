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
using Mono.Collections.Generic;

#endregion

namespace PapyrusDotNet.Common.Extensions
{
    public static class CollectionExtensions
    {
        public static int IndexOf<T>(this List<T> collection, Func<T, bool> predicate)
        {
            var item =
                collection.FirstOrDefault(predicate);
            if (item != null)
            {
                return collection.IndexOf(item);
            }
            return -1;
        }

        public static bool Contains<T>(this Collection<T> collection, Func<T, bool> predicate)
        {
            return collection.Any(predicate);
        }

        public static bool Contains<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            return collection.Any(predicate);
        }

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

        //    {
        //    foreach (var c in collection)
        //    if (a == null) return;
        //{

        //public static void ForEach<T>(this IEnumerable<T> collection, Action<T> a)
        //        a(c);
        //    }
        //}

        //public static void ForEach(this IEnumerable collection, Action<object> a)
        //{
        //    if (a == null) return;
        //    foreach (var c in collection)
        //    {
        //        a(c);
        //    }
        //}
    }
}