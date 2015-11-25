using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using PapyrusDotNet.Converters.Clr2Papyrus.Implementations;
using PapyrusDotNet.PapyrusAssembly.Enums;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Test
{
    [TestClass]
    public class Clr2PapyrusConverterTests
    {
        [TestMethod]
        public void Clr2PapyrusConverter_Convert()
        {
            var papyrusCompiler = new Clr2PapyrusConverter();
            var value = papyrusCompiler.Convert(new ClrAssemblyInput(
                AssemblyDefinition.ReadAssembly(
                    @"D:\Git\PapyrusDotNet\Examples\Fallout4Example\bin\Debug\Fallout4Example.dll"),
                PapyrusVersionTargets.Fallout4));

            var papyrusOutput = value as PapyrusAssemblyOutput;
            var assemblies = papyrusOutput.Assemblies;
        }
    }
}
