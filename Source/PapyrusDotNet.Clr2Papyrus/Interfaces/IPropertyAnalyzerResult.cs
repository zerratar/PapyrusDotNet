namespace PapyrusDotNet.Converters.Clr2Papyrus.Interfaces
{
    public interface IPropertyAnalyzerResult
    {
        bool IsAutoVar { get; }
        string AutoVarName { get; }
    }
}