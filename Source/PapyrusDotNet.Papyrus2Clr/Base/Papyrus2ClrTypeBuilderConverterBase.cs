using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.Converters.Papyrus2Clr.Implementations;

namespace PapyrusDotNet.Converters.Papyrus2Clr.Base
{
    public abstract class Papyrus2ClrTypeBuilderConverterBase : IPapyrusOutputConverter
    {
        protected Papyrus2ClrTypeBuilderConverterBase(INamespaceResolver namespaceResolver,
            ITypeReferenceResolver typeReferenceResolver)
        {
            NamespaceResolver = namespaceResolver;
            TypeReferenceResolver = typeReferenceResolver;
        }

        protected INamespaceResolver NamespaceResolver { get; }
        protected ITypeReferenceResolver TypeReferenceResolver { get; }

        public IAssemblyOutput Convert(IAssemblyInput input)
        {
            return ConvertAssembly(input as PapyrusAssemblyInput);
        }
        protected abstract ClrAssemblyOutput ConvertAssembly(PapyrusAssemblyInput input);
    }
}