﻿//     This file is part of PapyrusDotNet.
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PapyrusDotNet.CoreBuilder.Implementation;

#endregion

namespace PapyrusDotNet.CoreBuilder.Tests
{
    [TestClass]
    public class CoreAssemblyNameResolverTest
    {
        [TestMethod]
        public void Resolve_PassNothing_ReturnAssemblyNameDefinition()
        {
            var nameResolver = new CoreAssemblyNameResolver();
            var assemblyNameDef = nameResolver.Resolve(null);

            Assert.IsNotNull(assemblyNameDef);
            Assert.AreEqual("PapyrusDotNet.Core", assemblyNameDef.Name);
            Assert.AreEqual("PapyrusDotNet.Core", nameResolver.BaseNamespace);
            Assert.AreEqual("PapyrusDotNet.Core.dll", nameResolver.OutputLibraryFilename);
        }
    }
}