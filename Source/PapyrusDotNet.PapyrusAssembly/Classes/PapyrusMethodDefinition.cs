using System.Collections.Generic;

namespace PapyrusDotNet.PapyrusAssembly.Classes
{
    public class PapyrusMethodDefinition : PapyrusMethodReference
    {
        public PapyrusMethodDefinition()
        {
            Parameters = new List<PapyrusParameterDefinition>();
            Body = new PapyrusMethodBody(this);
        }
        public PapyrusMethodBody Body { get; set; }
        public bool HasBody => Body != null && !Body.IsEmpty;
        public string ReturnTypeName { get; set; }
        public string Documentation { get; set; }
        public int UserFlags { get; set; }
        public byte Flags { get; set; }
        public List<PapyrusParameterDefinition> Parameters { get; set; }
        public string Name { get; set; }
    }
}