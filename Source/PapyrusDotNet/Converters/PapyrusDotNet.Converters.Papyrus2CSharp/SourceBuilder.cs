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
using System.Text;

#endregion

namespace PapyrusDotNet.Converters.Papyrus2CSharp
{
    public class SourceBuilder
    {
        private readonly Dictionary<object, TextRange> location = new Dictionary<object, TextRange>();
        private readonly StringBuilder sb = new StringBuilder();
        public int CurrentColumn;
        public int CurrentLine;
        private bool lastWasAppend;

        public int Size { get; private set; }

        public int Lines => CurrentLine;

        public int Column => CurrentColumn;

        public TextRange this[object key] => location[key];

        public override string ToString()
        {
            return sb.ToString();
        }

        public List<TextRange> GetTextRanges() => location.Select(v => v.Value).ToList();

        public void Append(string text, object key = null, int indent = 0)
        {
            text = Indent(text, indent);
            sb.Append(text);

            if (key != null)
            {
                location.Add(key, new TextRange(key, text, CurrentLine, CurrentColumn, text.Length));
            }

            Size += text.Length;
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

            Size += text.Length;
            CurrentLine++;
            CurrentColumn = 0;
            lastWasAppend = false;
        }

        public void AppendLine()
        {
            sb.AppendLine();
            Size += Environment.NewLine.Length;
            CurrentLine++;
            CurrentColumn = 0;
            lastWasAppend = false;
        }


        private string Indent(string text, int num)
        {
            var output = "";
            for (var i = 0; i < num; i++)
            {
                output += '\t';
            }
            return output + text.Trim('\t');
        }

        //}
        //    return output;
        //    }
        //        output += func(i);
        //    {
        //    for (var i = 0; i < num; i++)
        //    string output = "";
        //{

        //private string For(int num, Func<int, string> func)
    }
}