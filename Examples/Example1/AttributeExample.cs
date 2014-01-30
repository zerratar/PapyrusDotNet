namespace Example1
{
	using PapyrusDotNet.Core;

	[Conditional]
	public class AttributeExample : Actor
	{
		[Property, Auto]
		public string MyPropertyString;

		[Property, Auto]
		public Weapon WeaponRef;

		[Property, Auto]
		public Actor PlayerRef;

		[InitialValue(0)]
		private int totalHoursElapsed = 0;

		public override void OnInit()
		{
			RegisterForSingleUpdateGameTime(1);
		}

		public override void OnUpdateGameTime()
		{
			totalHoursElapsed++;

			Debug.MessageBox(totalHoursElapsed + " hours spent ingame! And my name is " + PlayerRef.GetName());

			RegisterForSingleUpdateGameTime(1);
		}
	}
}
