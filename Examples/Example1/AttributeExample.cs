//     This file is part of PapyrusDotNet.
// 
//     PapyrusDotNet is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     PapyrusDotNet is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with PapyrusDotNet.  If not, see <http://www.gnu.org/licenses/>.
//  
//     Copyright 2015, Karl Patrik Johansson, zerratar@gmail.com

#region

using PapyrusDotNet.Core;

#endregion

namespace Example1
{
    [Conditional, Hidden]
    public class AttributeExample : Actor
    {
        [InitialValue("Hello world!")] private string dummy;

        [Property, Auto] public string MyPropertyString;

        [Property, Auto] public Actor PlayerRef;

        [InitialValue(0)] private int totalHoursElapsed;

        [Property, Auto] public Weapon WeaponRef;

        public override void OnInit()
        {
         //   RegisterForSingleUpdateGameTime(1);
        }

        //public override void OnUpdateGameTime()
        //{
        //    totalHoursElapsed++;

        //    Debug.MessageBox(totalHoursElapsed + " hours spent ingame! And my name is " + PlayerRef);


        //  RegisterForSingleUpdateGameTime(1);
        //}
    }
}