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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PapyrusDotNet.PapyrusAssembly.Parser;

#endregion

namespace PapyrusDotNet.PapyrusAssembly.Tests
{
    [TestClass]
    public class PapyrusAssemblyDefinitionTests
    {
        [TestMethod]
        public void PapyrusParser_GetOpCodeFromAlias()
        {
            var desc =
            PapyrusInstructionOpCodeDescription.FromAlias("CALLMETHOD");
            Assert.IsNotNull(desc);
            Assert.AreEqual(PapyrusOpCodes.Callmethod, desc.Key);
        }

        [TestMethod]
        public void PapyrusParser_GetAsmValues()
        {
            var parser = new PapyrusAssemblyInstructionParser();
            var values = parser.GetAsmValues("CALLMETHOD OnEndState self ::nonevar 1 asNewState ;@line 8");
            Assert.AreEqual(6, values.Count);

            values = parser.GetAsmValues("CALLMETHOD OnEndState self ::nonevar 1 \"\" ;@line 8");
            Assert.AreEqual(6, values.Count);
            Assert.AreEqual(string.Empty, values[5].Value);
            values = parser.GetAsmValues("CALLMETHOD OnEndState self ::nonevar 1 \"hello there! asNewState\" ;@line 8");
            Assert.AreEqual(6, values.Count);
            Assert.AreEqual("hello there! asNewState", values[5].Value);
        }

        [TestMethod]
        public void PapyrusParser_ParseInstruction()
        {
            var parser = new PapyrusAssemblyInstructionParser();
            var inst = parser.ParseInstruction("CALLMETHOD OnEndState self ::nonevar 1 asNewState ;@line 8");
            Assert.AreEqual(PapyrusOpCodes.Callmethod, inst.GetOpCode());
            Assert.AreEqual(2, inst.GetOperandArguments().Count);
            Assert.AreEqual(3, inst.GetArguments().Count);
        }

        [TestMethod]
        public void PapyrusParser_ParseInstruction_AdditionalSpaces()
        {
            var parser = new PapyrusAssemblyInstructionParser();
            var inst = parser.ParseInstruction("CALLMETHOD      OnEndState self    ::nonevar 1   asNewState    ;@line 8");
            Assert.AreEqual(PapyrusOpCodes.Callmethod, inst.GetOpCode());
            Assert.AreEqual(2, inst.GetOperandArguments().Count);
            Assert.IsTrue(inst.GetOperandArguments().All(j => !j.Value.Contains(" ")));
            Assert.AreEqual(3, inst.GetArguments().Count);
            Assert.IsTrue(inst.GetArguments().All(j => !j.Value.Contains(" ")));
        }

        [TestMethod]
        public void PapyrusParser_ParseInstructions()
        {
            var parser = new PapyrusAssemblyInstructionParser();
            var inst = parser.ParseInstructions("CALLMETHOD OnEndState self ::nonevar 1 asNewState" + Environment.NewLine + "CALLMETHOD OnEndState self ::nonevar 1 asNewState");
            for (var i = 0; i < 2; i++)
            {
                Assert.AreEqual(PapyrusOpCodes.Callmethod, inst[i].GetOpCode());
                Assert.AreEqual(2, inst[i].GetOperandArguments().Count);
                Assert.IsTrue(!inst[i].GetOperandArguments().Last().Value.Contains("\r"));
                Assert.AreEqual(3, inst[i].GetArguments().Count);
            }
        }
        [TestMethod]
        public void PapyrusParser_ParseInstructions_WithComments()
        {
            var parser = new PapyrusAssemblyInstructionParser();
            var inst = parser.ParseInstructions("CALLMETHOD OnEndState self ::nonevar 1 asNewState ;@line 8" + Environment.NewLine + "CALLMETHOD OnEndState self ::nonevar 1 asNewState ;@line 8");
            for (var i = 0; i < 2; i++)
            {
                Assert.AreEqual(PapyrusOpCodes.Callmethod, inst[i].GetOpCode());
                Assert.AreEqual(2, inst[i].GetOperandArguments().Count);
                Assert.AreEqual(3, inst[i].GetArguments().Count);
            }
        }

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