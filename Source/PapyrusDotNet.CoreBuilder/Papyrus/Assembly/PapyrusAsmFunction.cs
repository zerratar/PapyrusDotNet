namespace PapyrusDotNet.CoreBuilder
{
    using System.Collections.Generic;

    public class PapyrusAsmFunction
    {
        public string Name { get; set; }
        public string DocString { get; set; }
        public bool IsStatic { get; set; }
        public bool IsNative { get; set; }
        public bool IsEvent { get; set; }
        public bool ReturnArray { get; set; }
        public string ReturnType { get; set; }

        public List<PapyrusAsmVariable> Params { get; set; }
        public List<PapyrusAsmVariable> LocalTable { get; set; }

        public PapyrusAsmFunction()
        {
            Params = new List<PapyrusAsmVariable>();
            LocalTable = new List<PapyrusAsmVariable>();
        }
    }
}