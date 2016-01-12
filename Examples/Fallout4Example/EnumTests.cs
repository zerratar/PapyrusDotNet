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
    public class EnumTests : ObjectReference
    {
        // Works
        public enum TestEnum
        {
            A = 1,
            B = 999,
            C = 12
        }

        // Works
        public void EnumVal()
        {
            var test = TestEnum.A;

            if (test == TestEnum.C)
            {
                Debug.MessageBox("Hehehehe");
            }
        }

        // Works
        public TestEnum EnumVal2()
        {
            var test = TestEnum.A;

            if (test == TestEnum.C)
            {
                Debug.MessageBox("Hehehehe");
            }

            return test;
        }

        // Works
        public TestEnum EnumVal3(TestEnum asd)
        {
            var test = TestEnum.A;

            if (test == TestEnum.C)
            {
                Debug.MessageBox("Hehehehe");
            }

            return asd;
        }
    }
}