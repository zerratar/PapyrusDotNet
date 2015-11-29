using System.Collections.Generic;

namespace PapyrusDotNet.PapyrusAssembly.Classes
{
    public class PapyrusStatePropertyDescriptions
    {
        public PapyrusStringRef ObjectName { get; set; }
        public PapyrusStringRef GroupName { get; set; }
        public PapyrusStringRef GroupDocumentation { get; set; }
        public int Userflags { get; set; }
        public List<PapyrusStringRef> PropertyNames { get; set; }

        public PapyrusStatePropertyDescriptions()
        {
            PropertyNames = new List<PapyrusStringRef>();
        }
    }
}