using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.Converters.Papyrus2Clr.Implementations;

namespace PapyrusDotNet.Converters.Papyrus2CSharp
{
    public abstract class Papyrus2CSharpConverterBase : IPapyrusOutputConverter
    {
        protected Papyrus2CSharpConverterBase(INamespaceResolver namespaceResolver,
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

        protected abstract MultiCSharpOutput ConvertAssembly(PapyrusAssemblyInput input);
    }
}