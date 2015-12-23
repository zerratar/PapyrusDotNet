namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusSequencePoint
    {
        public string Document { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
    }
}
