namespace PapyrusDotNet.Common
{
	public class EvaluationStackItem
	{
		public bool IsMethodCall { get; set; }
		public bool IsThis { get; set; }
		public object Value { get; set; }
		public string TypeName { get; set; }
	}
}