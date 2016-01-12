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

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PapyrusDotNet.Decompiler.HelperClasses;

#endregion

namespace PapyrusDotNet.Decompiler.Tests
{
    [TestClass]
    public class MapTests
    {
        [TestMethod]
        public void Map_Test_Assignment_And_Find()
        {
            var map = new Map<int, int>();
            map[99] = 25;
            map[21] = 66;
            map[5] = 99999;

            Assert.AreEqual(3, map.Size);
            Assert.AreEqual(25, map[99]);
            Assert.AreEqual(66, map[21]);
            Assert.AreEqual(99999, map[5]);
            Assert.AreEqual(0, map[8228]);
                // Non existing map should return the default value of the T2. In this case, 0


            Assert.AreEqual(66, map.Find(21));
            Assert.AreEqual(0, map.Find(99292929));

            Assert.AreEqual(99, map.FindKey(25));
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public void Map_FindKey_UsingBadValue_ThrowsException()
        {
            var map = new Map<int, int>();
            map.FindKey(29125);
        }


        [TestMethod]
        public void Map_TryFindKey_UsingBadValue_Returns_0()
        {
            var map = new Map<int, int>();
            int key;
            var result = map.TryFindKey(29922, out key);
            Assert.AreEqual(false, result);
            Assert.AreEqual(0, key);
        }
    }
}