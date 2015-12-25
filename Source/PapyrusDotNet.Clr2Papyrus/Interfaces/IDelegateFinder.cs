using Mono.Cecil;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Interfaces
{
    public interface IDelegateFinder
    {
        string FindDelegateInvokeReference(IDelegatePairDefinition pairDefinitions,
            PapyrusMethodDefinition papyrusMethod);
        IDelegatePairDefinition FindDelegateTypes(TypeDefinition type);
        bool IsDelegateMethod(TypeDefinition type, MethodDefinition m);
    }
}