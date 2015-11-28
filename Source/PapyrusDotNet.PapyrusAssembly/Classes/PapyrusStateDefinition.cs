using System.Collections.ObjectModel;

namespace PapyrusDotNet.PapyrusAssembly.Classes
{
    public class PapyrusStateDefinition : PapyrusStateReference
    {
        public PapyrusStateDefinition()
        {
            Methods = new Collection<PapyrusMethodDefinition>();
        }

        public PapyrusStringRef Name { get; set; }
        public Collection<PapyrusMethodDefinition> Methods { get; set; }
    }
}