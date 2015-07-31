namespace PapyrusDotNet.CoreBuilder
{
    using System.Collections.Generic;

    public class PapyrusAsmState
    {
        public string Name { get; set; }

        public List<PapyrusAsmFunction> Functions { get; set; }


        public PapyrusAsmState()
        {
            Functions = new List<PapyrusAsmFunction>();
        }
    }
}