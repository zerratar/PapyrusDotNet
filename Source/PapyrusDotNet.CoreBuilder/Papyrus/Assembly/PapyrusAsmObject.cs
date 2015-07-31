namespace PapyrusDotNet.CoreBuilder
{
    using System.Collections.Generic;

    public class PapyrusAsmObject
    {
        public List<PapyrusAsmVariable> VariableTable { get; set; }
        public List<PapyrusAsmVariable> PropertyTable { get; set; }
        public List<PapyrusAsmState> States { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        // public PapyrusAsmObject Extends { get; set; }
        public PapyrusAsmObject()
        {
            VariableTable = new List<PapyrusAsmVariable>();
            PropertyTable = new List<PapyrusAsmVariable>();
            States = new List<PapyrusAsmState>();

        }

        public string ExtendsName { get; set; }
    }
}