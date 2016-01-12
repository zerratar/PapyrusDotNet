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
    public class PropertyGetSetTests
    {
        private MyTestStruct s;

        public int PropertyInt { get; set; }

        // Works
        public void PropertySet(int value)
        {
            var test = 9292;

            // Works
            PropertyInt = 1000;

            // Works
            PropertyInt = test;

            // Works
            PropertyInt = value;

            // Works
            for (PropertyInt = 0; PropertyInt < test; PropertyInt++)
            {
                Debug.MessageBox("Hello There!");
            }
        }

        // Works
        public int PropertyGet()
        {
            // Works
            var test = PropertyInt;

            // Works
            for (var i = 0; i < PropertyInt; i++)
            {
                Debug.MessageBox("Hello There: " + i);
            }

            return test;
        }

        public struct MyTestStruct
        {
            public int StructInteger;
        }
    }
}