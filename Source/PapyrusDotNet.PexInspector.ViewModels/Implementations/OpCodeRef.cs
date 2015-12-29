namespace PapyrusDotNet.PexInspector.ViewModels.Implementations
{
    public enum OpCodeRef
    {
        None,
        Type,
        Method,
        Instruction
    }

    public enum OpCodeConstraint
    {
        NoConstraints,
        None,
        Integer,
        Float,
        Boolean,
        String
    }
}