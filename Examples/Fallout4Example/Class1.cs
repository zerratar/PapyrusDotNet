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

namespace Fallout4Example
{
    public class MyObjectReference : ObjectReference
    {

        public struct MyTestStruct
        {
            public string StructString;
            public int StructInteger;
        }

        public string HelloWorld;

        public bool HelloThere { get; set; }

        //public int Return0() => 0;

        //public int Return1() => 1;

        //public int Return10() => 10;

        //public int Return100() => 100;

        public void LoopTest()
        {
            for (var i = 0; i < 1000; i++)
            {
                Debug.MessageBox("This is message # " + i);
            }
        }


        public int testInteger1 = 0;
        public int asda = 0;
        public override void OnInit()
        {
            //MyTestStruct meStruct;

            //meStruct.StructInteger = 252;

            asda = 0;
            testInteger1++;

            int[] intArray = new int[9999];

            intArray[0] = 9212;
            intArray[22] = 10; // + meStruct.StructInteger;

            Debug.MessageBox("Hello" + intArray[22]);

            var stringMessage = "asd" + intArray.Length;

            Debug.MessageBox(stringMessage);

            var asd2 = stringMessage + "25" + 58;

            Debug.MessageBox(asd2);

            testInteger1 += 100;

            if (HelloThere)
            {
                Debug.MessageBox("It is!");
            }
        }
    }
}