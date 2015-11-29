using System.Collections.Generic;
using System.Linq;

namespace PapyrusDotNet.PapyrusAssembly.Classes
{
    public class PapyrusMethodBody
    {
        private readonly PapyrusMethodDefinition method;
        public bool HasVariables => Variables.Any();
        public bool IsEmpty => !Instructions.Any();
        public List<PapyrusVariableDefinition> Variables { get; set; }

        public PapyrusMethodBody(PapyrusMethodDefinition method)
        {
            this.method = method;
            Instructions = new List<PapyrusInstruction>();
            Variables = new List<PapyrusVariableDefinition>();
        }

        public List<PapyrusInstruction> Instructions { get; set; }

        public PapyrusMethodDefinition GetMethod() => method;
    }
}