using PapyrusDotNet.PapyrusAssembly.Enums;
using PapyrusDotNet.PapyrusAssembly.Extensions;

namespace PapyrusDotNet.PapyrusAssembly.Classes
{
    public class PapyrusPropertyDefinition : PapyrusPropertyReference
    {
        private readonly PapyrusAssemblyDefinition assembly;
        public PapyrusStringRef TypeName { get; set; }
        public PapyrusStringRef Documentation { get; set; }
        public int Userflags { get; set; }
        public byte Flags { get; set; }

        public bool IsAuto => (Flags & 4) > 0;
        public bool HasGetter => (Flags & 1) > 0;
        public bool HasSetter => (Flags & 2) > 0;


        public string AutoName { get; set; }
        public PapyrusMethodDefinition GetMethod { get; set; }
        public PapyrusMethodDefinition SetMethod { get; set; }

        public PapyrusPropertyDefinition(PapyrusAssemblyDefinition assembly)
        {
            this.assembly = assembly;
        }

        public PapyrusPropertyDefinition(PapyrusAssemblyDefinition assembly, string name, string typeName) : base(new PapyrusStringRef(assembly, name), null, PapyrusPrimitiveType.None)
        {
            this.assembly = assembly;
            TypeName = typeName.Ref(assembly);
        }
    }
}