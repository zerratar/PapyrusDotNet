using System;

namespace PapyrusDotNet.System.Linq
{
    public static class LinqExtensions
    {
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
            for(var i = source.Length; i > 0; i--)
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
