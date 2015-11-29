using System.Collections.Generic;

namespace PapyrusDotNet.PapyrusAssembly.Classes
{
    public class PapyrusStructDescription
    {
        public PapyrusStringRef DeclaringTypeName { get; set; }
        public PapyrusStringRef Name { get; set; }
        public List<PapyrusStringRef> FieldNames { get; set; }

        public PapyrusStructDescription()
        {
            FieldNames = new List<PapyrusStringRef>();
        }
    }
}