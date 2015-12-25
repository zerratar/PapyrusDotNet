using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations
{
    public class MethodBodyOptimizerResult : IMethodBodyOptimizerResult
    {
        public MethodBodyOptimizerResult(PapyrusMethodDefinition result, bool success, double ratio)
        {
            Result = result;
            Success = success;
            Ratio = ratio;
        }

        public bool Success { get; }
        public double Ratio { get; }
        public PapyrusMethodDefinition Result { get; }
    }
}