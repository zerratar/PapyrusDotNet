using System.Collections.Generic;

namespace PapyrusDotNet.PapyrusAssembly.Classes
{
    public class PapyrusStructDescription
    {
        public PapyrusStringRef ObjectName { get; set; }
        public PapyrusStringRef OrderName { get; set; }
        public List<PapyrusStringRef> FieldNames { get; set; }

        public PapyrusStructDescription()
        {
            FieldNames = new List<PapyrusStringRef>();
        }
    }
}