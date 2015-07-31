namespace PapyrusDotNet.CoreBuilder
{
    using System.Collections.Generic;

    public class PapyrusStateFunction
    {
        public string Name;

        public bool IsAuto;

        public List<PapyrusFunction> Functions;

        public PapyrusStateFunction()
        {
            Functions = new List<PapyrusFunction>();
        }
    }
}