using System.Collections.Generic;

namespace PapyrusDotNet.PapyrusAssembly.Classes
{
    public class PapyrusPropertyDescriptions
    {
        public string ObjectName { get; set; }
        public string GroupName { get; set; }
        public string GroupDocumentation { get; set; }
        public int Userflags { get; set; }
        public List<string> PropertyNames { get; set; }

        public PapyrusPropertyDescriptions()
        {
            PropertyNames = new List<string>();
        }
    }
}