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
	
	Copyright 2015, Karl Patrik Johansson, zerratar@gmail.com
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PapyrusDotNet.Common;
using PapyrusDotNet.CoreBuilder.Implementation;

namespace PapyrusDotNet.CoreBuilder.Tests
{
    [TestClass]
    public class DictionaryPapyrusNameResolverTest
    {
        [TestMethod]
        public void NameResolver_PassCamelCase_ReturnPascalCase()
        {
            var statusCallBack = new DebugStatusCallbackService();

            var nameResolver = new DictionaryPapyrusNameResolver(statusCallBack);

            var targetName = nameResolver.Resolve("awesomeString");

            Assert.AreEqual("AwesomeString", targetName);
        }

        [TestMethod]
        public void NameResolver_PassLowerCase_ReturnTitleCase()
        {
            var statusCallBack = new DebugStatusCallbackService();

            var nameResolver = new DictionaryPapyrusNameResolver(statusCallBack, "wordlist.txt");

            var targetName = nameResolver.Resolve("awesomestring");

            Assert.AreEqual("Awesomestring", targetName);
        }

        [TestMethod]
        public void NameResolver_UsingDictionary_PassLowerCase_ReturnPascalCase()
        {
            var statusCallBack = new DebugStatusCallbackService();

            var nameResolver = new DictionaryPapyrusNameResolver(statusCallBack, "wordlist.txt");

            var targetName = nameResolver.Resolve("awesomestring");

            Assert.AreEqual("AwesomeString", targetName);
        }


        [TestMethod]
        public void NameResolver_PassNull_ReturnNull()
        {
            var statusCallBack = new DebugStatusCallbackService();

            var nameResolver = new DictionaryPapyrusNameResolver(statusCallBack);

            var targetName = nameResolver.Resolve(null);

            Assert.AreEqual(null, targetName);
        }
    }
}