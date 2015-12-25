using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Interfaces
{
    public interface IMethodBodyOptimizer
    {
        IMethodBodyOptimizerResult Optimize(PapyrusMethodDefinition method);
    }
}