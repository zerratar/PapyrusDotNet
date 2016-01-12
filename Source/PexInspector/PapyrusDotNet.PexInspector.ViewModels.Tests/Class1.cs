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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PexInspector.ViewModels.Implementations;

#endregion

namespace PapyrusDotNet.PexInspector.ViewModels.Tests
{
    [TestClass]
    public class OpCodeDescriptionReaderTests
    {
        [TestMethod]
        public void Read_XmlFile_ReturnsOpCodeDefinition()
        {
            var reader = new OpCodeDescriptionReader();
            var def =
                reader.Read(@"D:\Git\PapyrusDotNet\Source\PapyrusDotNet.PexInspector\OpCodeDescriptions.xml");

            var nopDef = def.GetDesc(PapyrusOpCodes.Nop);

            Assert.IsNotNull(nopDef);

            Assert.IsNotNull(def);
        }
    }
}