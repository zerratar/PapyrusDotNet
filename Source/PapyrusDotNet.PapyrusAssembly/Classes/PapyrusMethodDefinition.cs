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
        public PapyrusStringRef Name { get; set; }
        public PapyrusStringRef ReturnTypeName { get; set; }
        public PapyrusStringRef Documentation { get; set; }

        public PapyrusMethodBody Body { get; set; }

        public bool HasBody => Body != null && !Body.IsEmpty;

        public int UserFlags { get; set; }
        public byte Flags { get; set; }
        public List<PapyrusParameterDefinition> Parameters { get; set; }

        public bool IsGlobal => (Flags & 1) > 0;
        public bool IsNative => (Flags & 2) > 0;
        public bool IsEvent { get; set; }//=> (Flags & 4) > 0;
    }
}