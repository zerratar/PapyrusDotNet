namespace PapyrusDotNet.Tester
{
	using PapyrusDotNet.Core;
	using PapyrusDotNet.Core.Collections;

	public class Program
	{
		
		static void Main(string[] args)
		{
			
			var list = new List<Form>();
			var f1 = new Form();
			
			list.Add(f1);
			list.Add(f1);
			list.Add(f1);
			list.Add(f1);

			var f2 = list.Get(1);

			for (int j = 0; j < list.Size(); j++)
			{
				var item = list.Get(j);
			}
		}
	}
}
