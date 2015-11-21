using System.Collections.Generic;
using System.Linq;

namespace PapyrusDotNet.PapyrusAssembly.Implementations
{
    public class PapyrusMethodBody
    {
        public PapyrusMethodDefinition Method { get; }
        public bool HasVariables => Variables.Any();
        public bool IsEmpty => !Instructions.Any();
        public List<PapyrusVariableDefinition> Variables { get; set; }

        public PapyrusMethodBody(PapyrusMethodDefinition method)
        {
            Method = method;
            Instructions = new List<PapyrusInstruction>();
            Variables = new List<PapyrusVariableDefinition>();
        }

        public List<PapyrusInstruction> Instructions { get; set; }
    }
}