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
    public class LoopTests
    {
        private int[] array;

        // Works
        public void ForEachTest()
        {
            foreach (var i in array)
            {
                Debug.MessageBox("i = " + i);
            }
        }

        // Works
        public void ForTest()
        {
            for (var index = 0; index < array.Length; index++)
            {
                var i = array[index];
                Debug.MessageBox("i = " + i);
            }
        }

        // Works
        public void WhileTest()
        {
            var index = 0;
            while (index < array.Length)
            {
                var i = array[index];
                Debug.MessageBox("i = " + i);
                index++;
            }
        }

        // Works
        public void DoWhileTest()
        {
            var index = 0;
            do
            {
                var i = array[index];
                Debug.MessageBox("i = " + i);
            } while (index++ < array.Length);
        }
    }
}