namespace PapyrusDotNet.Converters.Papyrus2CSharp.FlowAnalyzer
{
    /// <summary>
    /// Type of the control flow node
    /// </summary>
    public enum PapyrusControlFlowNodeType
    {
        /// <summary>
        /// A normal node represents a basic block.
        /// </summary>
        Normal,
        /// <summary>
        /// The entry point of the method.
        /// </summary>
        EntryPoint,
        /// <summary>
        /// The exit point of the method (every ret instruction branches to this node)
        /// </summary>
        RegularExit,
    }
}