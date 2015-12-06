using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Exceptions
{
    public class ProhibitedCodingBehaviourException : Exception
    {
        public MethodDefinition Method { get; }
        public int? Offset { get; }
        public OpCode? OpCode { get; }
        public ProhibitedCodingBehaviourException() { }
        public ProhibitedCodingBehaviourException(MethodDefinition method, OpCode? opCode, int? offset)
        {
            this.Method = method;
            this.OpCode = opCode;
            this.Offset = offset;
        }
    }
}