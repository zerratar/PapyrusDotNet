namespace PapyrusDotNet.CoreBuilder
{
    using System.Collections.Generic;

    public class PapyrusFunction
    {
        public string Name;

        public string StateName;

        public PapyrusVariable ReturnType;

        public List<PapyrusVariable> Parameters;

        public bool IsGlobal;

        public bool IsNative;

        public bool IsEvent;

        public PapyrusFunction()
        {
            this.Parameters = new List<PapyrusVariable>();
        }

        public PapyrusFunction(string name, string stateName)
        {
            this.Name = name;
            this.StateName = stateName;
            this.Parameters = new List<PapyrusVariable>();
        }
    }
}