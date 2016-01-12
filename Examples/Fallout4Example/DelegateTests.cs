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
//     Copyright 2016, Karl Patrik Johansson, zerratar@gmail.com

#region

using PapyrusDotNet.Core;

#endregion

namespace Fallout4Example
{
    public class DelegateTests
    {
        // Working
        public delegate void AnotherDelegate(string input);

        // Working
        public delegate void HelloThereDelegate();

        // Working
        public delegate void HorribleDelegate();

        // Working
        public delegate void SecondDelegate();

        public void UtilizeDelegate()
        {
            HelloThereDelegate awesome = () => { Debug.Trace("Awesome was used!", 0); };

            awesome();
        }

        // Working
        public void UtilizeDelegate1()
        {
            HelloThereDelegate awesome = () => { Debug.Trace("Awesome was used!", 0); };

            HelloThereDelegate secondAwesome = () => { Debug.Trace("Second awesome was used!", 0); };

            awesome();

            secondAwesome();
        }

        public void UtilizeDelegate3()
        {
            var horror = "test";

            AnotherDelegate awesome = s => { Debug.Trace("UtilizeDelegate3 was used!" + s, 0); };

            awesome(horror);
        }

        public void UtilizeDelegate2()
        {
            var whatHorrorLiesHere = "test123";

            SecondDelegate arrr = () => { Debug.Trace("UtilizeDelegate2 was used!" + whatHorrorLiesHere, 0); };

            arrr();
        }

        public void Delegate_In_Delegate()
        {
            var magic = "helloo";
            HorribleDelegate awesome = () =>
            {
                AnotherDelegate awe2 = s => { Debug.Trace("UtilizeDelegate4 was used!" + s, 0); };

                awe2(magic);
            };
            awesome();
        }
    }
}