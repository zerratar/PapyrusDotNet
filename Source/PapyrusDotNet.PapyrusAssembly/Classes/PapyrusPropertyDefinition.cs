using PapyrusDotNet.PapyrusAssembly.Enums;

namespace PapyrusDotNet.PapyrusAssembly.Classes
{
    public class PapyrusPropertyDefinition : PapyrusPropertyReference
    {
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

        public PapyrusPropertyDefinition() { }

        public PapyrusPropertyDefinition(string name, string typeName) : base((PapyrusStringRef)name, null, PapyrusPrimitiveType.None)
        {
            TypeName = (PapyrusStringRef)typeName;
        }
    }
}