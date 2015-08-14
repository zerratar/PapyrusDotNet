using System;

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
