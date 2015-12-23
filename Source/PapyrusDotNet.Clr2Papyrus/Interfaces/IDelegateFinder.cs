using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PapyrusDotNet.Converters.Clr2Papyrus.Implementations;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Interfaces
{
    public interface IDelegateFinder
    {
        string FindDelegateInvokeReference(IDelegatePairDefinition pairDefinitions,
            PapyrusMethodDefinition papyrusMethod);
        IDelegatePairDefinition FindDelegateTypes(ModuleDefinition mainModule);
        bool IsDelegateMethod(TypeDefinition type, MethodDefinition m);
    }
}