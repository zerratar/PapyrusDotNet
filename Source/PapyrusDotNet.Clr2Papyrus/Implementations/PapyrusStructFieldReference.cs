using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations
{
    public class PapyrusStructFieldReference : PapyrusFieldDefinition
    {
        public object StructSource { get; set; }
        public PapyrusVariableReference StructVariable { get; set; }

        public PapyrusStructFieldReference(PapyrusAssemblyDefinition assembly) : base(assembly)
        {
        }

        public PapyrusStructFieldReference(PapyrusAssemblyDefinition assembly, string name, string typeName) : base(assembly, name, typeName)
        {
        }

        public override string ToString()
        {
            return "StructRef: " + StructSource + " -> " + StructVariable;
        }
    }
}