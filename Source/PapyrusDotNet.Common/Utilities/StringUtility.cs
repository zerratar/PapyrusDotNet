using System;
using PapyrusDotNet.Common.Interfaces;

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
            if (p is string) return (string)p;
            return "";
        }
    }
}