using System.Collections.ObjectModel;

namespace PapyrusDotNet.PapyrusAssembly.Classes
{
    public class PapyrusStateDefinition : PapyrusStateReference
    {
        private PapyrusTypeDefinition type;

        public PapyrusStateDefinition()
        {
            Methods = new Collection<PapyrusMethodDefinition>();
        }

        public PapyrusStateDefinition(PapyrusTypeDefinition type) : this()
        {
            this.type = type;
            this.type.States.Add(this);
        }

        public PapyrusStringRef Name { get; set; }
        public Collection<PapyrusMethodDefinition> Methods { get; set; }
    }
}