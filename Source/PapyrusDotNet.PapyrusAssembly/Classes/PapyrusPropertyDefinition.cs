namespace PapyrusDotNet.PapyrusAssembly.Implementations
{
    public class PapyrusPropertyDefinition : PapyrusPropertyReference
    {
        public string TypeName { get; set; }
        public string Documentation { get; set; }
        public int Userflags { get; set; }
        public byte Flags { get; set; }
        public bool IsAuto => (Flags & 1) > 0;
        public bool HasGetter => (Flags & 2) > 0;
        public bool HasSetter => (Flags & 4) > 0;
        public string AutoName { get; set; }
        public PapyrusMethodDefinition GetMethod { get; set; }
        public PapyrusMethodDefinition SetMethod { get; set; }
    }
}