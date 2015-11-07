using Mono.Cecil;

namespace PapyrusDotNet
{
    public struct MethodCallPair
    {
        public MethodDefinition CallerMethod;
        public MethodReference TargetMethod;

        public MethodCallPair(MethodDefinition cm, MethodReference tm)
        {
            CallerMethod = cm;
            TargetMethod = tm;
        }
    }
}