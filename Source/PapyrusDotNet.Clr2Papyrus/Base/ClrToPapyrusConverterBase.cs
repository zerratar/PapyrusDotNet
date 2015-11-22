using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.Converters.Clr2Papyrus.Implementations;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Base
{
    public abstract class ClrToPapyrusConverterBase : IPapyrusOutputConverter
    {
        protected abstract PapyrusAssemblyOutput ConvertAssembly(ClrAssemblyInput input);

        public IAssemblyOutput Convert(IAssemblyInput input)
        {
            return ConvertAssembly(input as ClrAssemblyInput);
        }
    }
}