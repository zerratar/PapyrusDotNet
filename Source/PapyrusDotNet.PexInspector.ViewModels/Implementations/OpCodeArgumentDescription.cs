namespace PapyrusDotNet.PexInspector.ViewModels.Implementations
{
    public class OpCodeArgumentDescription
    {
        public int Index { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public OpCodeValueTypes ValueType { get; set; }
        public OpCodeRef Ref { get; set; }
        public OpCodeConstraint[] Constraints { get; set; }
    }
}