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

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace PapyrusDotNet.PapyrusAssembly.Tests
{
    [TestClass]
    public class PapyrusAssemblyDefinitionTests
    {
        [TestMethod]
        public void Fo4_ReadPex_SaveTheAssembly_ReadAgain_Compare_ReturnsEqual()
        {
            var sourceScript = "D:\\Spel\\Fallout 4 Scripts\\scripts\\Actor.pex";
            var destinationScript = "D:\\Spel\\Fallout 4 Scripts\\scripts\\Actor.pex_new";

            var src = PapyrusAssemblyDefinition.ReadAssembly(sourceScript);
            Assert.IsNotNull(src);
            Assert.IsNotNull(src.Header.SourceHeader.Source);

            src.Write(destinationScript);

            var dest = PapyrusAssemblyDefinition.ReadAssembly(destinationScript);
            Assert.IsNotNull(src);
            Assert.IsNotNull(dest.Header.SourceHeader.Source);

            Assert.AreEqual(src.Header.SourceHeader.Source, dest.Header.SourceHeader.Source);

            Assert.AreEqual(new FileInfo(sourceScript).Length, new FileInfo(destinationScript).Length);
        }

        [TestMethod]
        public void TestFallout4Papyrus()
        {
            var falloutScript = "D:\\Spel\\Fallout 4 Scripts\\scripts\\Actor.pex";
            var assembly = PapyrusAssemblyDefinition.ReadAssembly(falloutScript);
            Assert.IsNotNull(assembly.Header.SourceHeader.Source);
        }

        [TestMethod]
        public void TestSkyrimPapyrus()
        {
            var str = @"C:\CreationKit\Data\scripts\activemagiceffect.pex";
            var assembly = PapyrusAssemblyDefinition.ReadAssembly(str);
            Assert.IsNotNull(assembly.Header.SourceHeader.Source);
            Assert.AreNotEqual(0, assembly.Types.Count);
        }

        [TestMethod]
        public void TestManyFallout4Papyrus()
        {
            var scripts = Directory.GetFiles(@"D:\Spel\Fallout 4 Scripts\scripts\", "*.pex", SearchOption.AllDirectories);
            var success = 0;
            var items = scripts.Length;
            var corruptCount = 0;
            var failedScripts = "";
            foreach (var script in scripts)
            {
                var assembly = PapyrusAssemblyDefinition.ReadAssembly(script);
                if (assembly.IsCorrupted)
                {
                    corruptCount++;
                    failedScripts += script + Environment.NewLine;
                }
                Assert.IsNotNull(assembly.Header.SourceHeader.Source);
                Assert.AreNotEqual(0, assembly.Types.Count);
                success++;
            }
            Assert.AreEqual(0, corruptCount, failedScripts);
            Assert.AreEqual(items, success);
        }

        [TestMethod]
        public void TestManySkyrimPapyrus()
        {
            var scripts = Directory.GetFiles(@"C:\CreationKit\Data\scripts\", "*.pex", SearchOption.AllDirectories);
            var success = 0;
            foreach (var script in scripts)
            {
                var assembly = PapyrusAssemblyDefinition.ReadAssembly(script);
                Assert.IsNotNull(assembly.Header.SourceHeader.Source);
                Assert.AreNotEqual(0, assembly.Types.Count);
                success++;
            }
            Assert.AreNotEqual(0, success);
        }
    }
}