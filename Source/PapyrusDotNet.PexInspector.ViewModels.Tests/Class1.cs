using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PexInspector.ViewModels.Implementations;

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
