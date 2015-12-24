using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations
{
    public class PropertyAnalyzerResult : IPropertyAnalyzerResult
    {
        public bool IsAutoVar { get; }
        public string AutoVarName { get; }

        public PropertyAnalyzerResult(bool isAutoVar, string autoVarName)
        {
            IsAutoVar = isAutoVar;
            AutoVarName = autoVarName;
        }
    }
}