using System.Collections.Generic;

namespace PapyrusDotNet.PapyrusAssembly.Implementations
{
    public class PapyrusStructDescription
    {
        public string ObjectName { get; set; }
        public string OrderName { get; set; }
        public List<string> FieldNames { get; set; }

        public PapyrusStructDescription()
        {
            FieldNames = new List<string>();
        }
    }
}