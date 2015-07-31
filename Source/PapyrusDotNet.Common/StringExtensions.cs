namespace PapyrusDotNet.Common
{
    using System;
    using System.Linq;

    public static class StringExtensions
    {
        public static bool Contains(this string[] input, string val)
        {
            return input.Select(v => v.ToLower().Trim()).Any(b => b == val);
        }
        public static bool AnyContains(this string[] input, string val)
        {
            return input.Select(v => v.ToLower().Trim()).Any(b => b.Contains(val));
        }

        public static int IndexOf(this string[] input, string val)
        {
            return Array.IndexOf(input.Select(d => d.ToLower().Trim()).ToArray(), val);
        }

        public static string[] TrimSplit(this string input, string val)
        {
            return input.Split(new string[] { val }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}