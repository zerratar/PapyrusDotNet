namespace PapyrusDotNet.CoreBuilder
{
    using System.Collections.Generic;

    public class PapyrusScriptObject
    {
        public string Name;

        public string Extends;

        public bool IsConditional;

        public bool IsHidden;

        public List<string> Imports;

        public List<PapyrusStateFunction> StateFunctions;

        public List<PapyrusVariable> Properties;

        public List<PapyrusVariable> InstanceVariables;

        public PapyrusScriptObject()
        {
            Imports = new List<string>();
            StateFunctions = new List<PapyrusStateFunction>();
            Properties = new List<PapyrusVariable>();
            InstanceVariables = new List<PapyrusVariable>();
        }
    }
}