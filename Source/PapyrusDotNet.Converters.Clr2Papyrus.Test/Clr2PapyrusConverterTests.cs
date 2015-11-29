using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using PapyrusDotNet.Converters.Clr2Papyrus.Implementations;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Classes;
using PapyrusDotNet.PapyrusAssembly.Enums;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Test
{
    [TestClass]
    public class PapyrusMethodFlagsTests
    {
        [TestMethod]
        public void TestPapyrusMethodFlags()
        {
            var m = new PapyrusMethodDefinition(PapyrusAssemblyDefinition.CreateAssembly(PapyrusVersionTargets.Fallout4));
            m.SetFlags(PapyrusMethodFlags.Global);
            Assert.IsTrue(m.IsGlobal);
            Assert.IsFalse(m.IsNative);

            m.IsNative = true;
            Assert.IsTrue(m.IsGlobal);
            Assert.IsTrue(m.IsNative);


            m.IsGlobal = false;
            Assert.IsFalse(m.IsGlobal);
            Assert.IsTrue(m.IsNative);
        }
    }
    [TestClass]
    public class Clr2PapyrusConverterTests
    {
        [TestMethod]
        public void Clr2PapyrusConverter_Convert()
        {
            var papyrusCompiler = new Clr2PapyrusConverter(new Clr2PapyrusInstructionProcessor());
            var value = papyrusCompiler.Convert(new ClrAssemblyInput(
                AssemblyDefinition.ReadAssembly(
                    @"D:\Git\PapyrusDotNet\Examples\Fallout4Example\bin\Debug\Fallout4Example.dll"),
                PapyrusVersionTargets.Fallout4));

            var papyrusOutput = value as PapyrusAssemblyOutput;

            Assert.IsNotNull(papyrusOutput);
            var assemblies = papyrusOutput.Assemblies;
            Assert.IsNotNull(assemblies);
            Assert.IsTrue(assemblies.Length > 0);
        }
    }
}
