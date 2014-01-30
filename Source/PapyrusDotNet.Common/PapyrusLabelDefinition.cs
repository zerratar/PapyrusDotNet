namespace PapyrusDotNet.Common
{
	public class PapyrusLabelDefinition
	{
		public string Name { get; set; }
		public int Row { get; set; }
		public PapyrusLabelDefinition(int row, string name = null)
		{
			this.Row = row;
			this.Name = name;
			if (string.IsNullOrEmpty(this.Name))
			{
				this.Name = "_label" + this.Row;
			}
		}
	}
}