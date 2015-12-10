using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Exceptions
{
    public class StackUnderflowException : Exception
    {
        public MethodDefinition Method { get; }
        public Instruction Instruction { get; }
        public StackUnderflowException() { }
        public StackUnderflowException(MethodDefinition method, Instruction instruction)
        {
            this.Method = method;
            this.Instruction = instruction;
        }
    }
}