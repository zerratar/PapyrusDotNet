namespace PapyrusDotNet.Converters.Papyrus2CSharp.FlowAnalyzer
{
    public class PapyrusControlFlowEdge
    {
        public readonly PapyrusControlFlowNode Source;
        public readonly PapyrusControlFlowNode Target;

        public PapyrusControlFlowEdge(PapyrusControlFlowNode fromNode, PapyrusControlFlowNode toNode)
        {
            Source = fromNode;
            Target = toNode;
        }
    }
}