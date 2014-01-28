/*
    This file is part of PapyrusDotNet.

    PapyrusDotNet is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    PapyrusDotNet is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with PapyrusDotNet.  If not, see <http://www.gnu.org/licenses/>.
	
	Copyright 2014, Karl Patrik Johansson, zerratar@gmail.com
 */

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