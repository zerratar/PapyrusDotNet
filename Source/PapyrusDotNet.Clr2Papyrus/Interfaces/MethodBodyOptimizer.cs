using PapyrusDotNet.Converters.Clr2Papyrus.Implementations;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Interfaces
{
    public class MethodBodyOptimizer : IMethodBodyOptimizer
    {
        public IMethodBodyOptimizerResult Optimize(PapyrusMethodDefinition method)
        {
            var methodBody = method.Body;
            var variables = method.Body.Variables;
            var success = false;
            var optimizationRatio = 0d;

            // 1. find all assignment on these variables
            // 2. find all usage on the variables
            // - If a variable is never assigned or never used, it is safe to be removed
            // - If a variable is assigned but never used, it could still be needed.
            //     (Consider this: You have a property getter method that does some logic more than just returning the value) 
            //      -- So, depending on what kind of assigning; it should find out if its safe to be removed or not.



            return new MethodBodyOptimizerResult(method, success, optimizationRatio);
        }
    }
}