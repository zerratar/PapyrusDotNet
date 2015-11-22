using System.Collections.ObjectModel;

namespace PapyrusDotNet.PapyrusAssembly.Classes
{
    public class PapyrusStateDefinition : PapyrusStateReference
    {
        public PapyrusStateDefinition()
        {
            Methods = new Collection<PapyrusMethodDefinition>();
        }

        public string Name { get; set; }
        public Collection<PapyrusMethodDefinition> Methods { get; set; }
    }
}