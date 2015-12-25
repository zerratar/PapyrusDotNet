using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Interfaces
{
    public interface IMethodBodyOptimizerResult
    {
        bool Success { get; }
        double Ratio { get; }
        PapyrusMethodDefinition Result { get; }
    }
}