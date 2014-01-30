namespace PapyrusDotNet.Common
{
	public class PapyrusLabelReference
	{
		public string Name { get; set; }
		public int RowReference { get; set; }
		public PapyrusLabelReference(string name, int row)
		{
			this.Name = name;
			this.RowReference = row;
		}
	}
}