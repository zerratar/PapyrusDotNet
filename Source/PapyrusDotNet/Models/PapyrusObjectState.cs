namespace PapyrusDotNet.Models
{
    using System.Collections.Generic;

    public class PapyrusObjectState
    {
        public string Name { get; set; }

        public bool IsAuto { get; set; }

        public List<PapyrusFunction> Functions { get; set; }

        public PapyrusObjectState()
        {
            Functions = new List<PapyrusFunction>();
        }
    }
}