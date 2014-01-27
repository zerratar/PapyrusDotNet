namespace TestDll
{
	using PapyrusDotNet.Core;

	public class GodMode : Actor
	{
		public override void OnInit()
		{

			object hej = 123;
			int hej2 = (int)hej;

			GodMode[] objs = new GodMode[120];


			var objlen = objs.Length;



			var at1 = objs[28];
			objs[28] = null;
			float[] floatArray = new float[128];
			float at = floatArray[28];
		}

		public void ActivateGodMode(Actor player)
		{
			var equippedWeapon = player.GetEquippedWeapon(false);

			equippedWeapon.SetBaseDamage(9999);

			player.SetActorValue("Health", 999999);

			player.SetActorValue("Magicka", 999999);

			player.SetActorValue("Stamina", 999999);

			Debug.MessageBox("God Mode activated!");
		}
	}
}