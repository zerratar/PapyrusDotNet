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

using System;

#endregion

namespace PapyrusDotNet.System.Linq
{
    public static class LinqExtensions
    {
        // Not Supported Yet
        /* 
        public static T First<T>(this T[] source)
        {
            return source[0];
        }

        public static T FirstOrDefault<T>(this T[] source)
        {
            if (source.Length > 0)
                return source[0];

            return default(T);
        }
        public static T Last<T>(this T[] source)
        {
            return source[source.Length - 1];
        }

        public static T LastOrDefault<T>(this T[] source)
        {
            if (source.Length > 0)
                return source[source.Length - 1];
            
            return default(T);
        }
        */

        public static T FirstOrDefault<T>(this T[] source, Func<T, bool> predicate)
        {
            foreach (var obj in source)
            {
                if (predicate.Invoke(obj))
                {
                    return obj;
                }
            }
            return default(T);
        }

        public static T LastOrDefault<T>(this T[] source, Func<T, bool> predicate)
        {
            for (var i = source.Length; i > 0; i--)
            {
                if (predicate.Invoke(source[i]))
                {
                    return source[i];
                }
            }

            return default(T);
        }
    }
}