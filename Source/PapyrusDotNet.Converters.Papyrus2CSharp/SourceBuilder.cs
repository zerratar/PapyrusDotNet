using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PapyrusDotNet.Converters.Papyrus2CSharp
{
    public class SourceBuilder
    {
        private readonly StringBuilder sb = new StringBuilder();
        private readonly Dictionary<object, TextRange> location = new Dictionary<object, TextRange>();
        private int size;
        public int CurrentLine;
        public int CurrentColumn;
        private bool lastWasAppend;

        public int Size => size;

        public int Lines => CurrentLine;

        public int Column => CurrentColumn;

        public override string ToString()
        {
            return sb.ToString();
        }

        public TextRange this[object key] => location[key];

        public List<TextRange> GetTextRanges() => location.Select(v => v.Value).ToList();

        public void Append(string text, object key = null, int indent = 0)
        {
            text = Indent(text, indent);
            sb.Append(text);

            if (key != null)
            {
                location.Add(key, new TextRange(key, text, CurrentLine, CurrentColumn, text.Length));
            }

            size += text.Length;
            CurrentColumn += text.Length;
            lastWasAppend = true;
        }

        public void AppendLine(string text, object key = null, int indent = 0)
        {
            if (!lastWasAppend)
            {
                text = Indent(text, indent);
            }
            sb.AppendLine(text);

            if (key != null)
            {
                if (!location.ContainsKey(key)) // Only the first occurence will be added.
                {
                    location.Add(key, new TextRange(key, text, CurrentLine, 0, text.Length));
                }
            }

            size += text.Length;
            CurrentLine++;
            CurrentColumn = 0;
            lastWasAppend = false;
        }

        public void AppendLine()
        {
            sb.AppendLine();
            size += Environment.NewLine.Length;
            CurrentLine++;
            CurrentColumn = 0;
            lastWasAppend = false;
        }


        private string Indent(string text, int num)
        {
            string output = "";
            for (var i = 0; i < num; i++)
            {
                output += '\t';
            }
            return output + text.Trim('\t');
        }

        //private string For(int num, Func<int, string> func)
        //{
        //    string output = "";
        //    for (var i = 0; i < num; i++)
        //    {
        //        output += func(i);
        //    }
        //    return output;
        //}
    }
}