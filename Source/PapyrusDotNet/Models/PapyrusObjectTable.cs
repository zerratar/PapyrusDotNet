namespace PapyrusDotNet.Models
{
    using System.Collections.Generic;

    using PapyrusDotNet.Common;

    public class PapyrusObjectTable
    {
        public string Name { get; set; }

        public string BaseType { get; set; }

        public PapyrusFieldProperties Info { get; set; }

        public string AutoState { get; set; }

        public List<PapyrusVariableReference> VariableTable { get; set; }

        public List<PapyrusVariableReference> PropertyTable { get; set; }

        public List<PapyrusObjectState> StateTable { get; set; }

        public PapyrusObjectTable()
        {
            Info = new PapyrusFieldProperties();
            VariableTable = new List<PapyrusVariableReference>();
            PropertyTable = new List<PapyrusVariableReference>();
            StateTable = new List<PapyrusObjectState>();
        }
    }
}