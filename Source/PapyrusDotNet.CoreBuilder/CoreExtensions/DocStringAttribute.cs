using System;

namespace PapyrusDotNet.CoreBuilder.CoreExtensions
{
    public class DocStringAttribute : Attribute
    {
        public string Comment;
        public DocStringAttribute(string comment)
        {
            Comment = comment;
        }
    }
}
