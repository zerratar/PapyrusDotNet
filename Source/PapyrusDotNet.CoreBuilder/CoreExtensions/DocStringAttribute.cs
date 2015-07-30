using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PapyrusDotNet.CoreBuilder.CoreExtensions
{
    public class DocStringAttribute : Attribute
    {
        public string Comment;
        public DocStringAttribute(string comment)
        {
            this.Comment = comment;
        }
    }
}
