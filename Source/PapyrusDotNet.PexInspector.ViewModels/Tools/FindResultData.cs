using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.PexInspector.ViewModels.Tools
{
    public struct FindResultData
    {
        public PapyrusTypeDefinition Type;
        public PapyrusStateDefinition State;
        public PapyrusMethodDefinition Method;
        public PapyrusInstruction Instruction;
        public string Text;
        public string SearchText;
    }
}