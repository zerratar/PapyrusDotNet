using System;
using PapyrusDotNet.Converters.Clr2Papyrus.Base;
using PapyrusDotNet.Converters.Clr2Papyrus.Implementations;

namespace PapyrusDotNet.Converters.Clr2Papyrus
{
    public class ClrToPapyrusConverter : ClrToPapyrusConverterBase
    {
        protected override PapyrusAssemblyOutput ConvertAssembly(ClrAssemblyInput input)
        {
            var clr = input.Assembly;
            var mainModule = clr.MainModule;

            foreach (var type in mainModule.Types)
            {

            }

            return new PapyrusAssemblyOutput();
        }
    }
}
