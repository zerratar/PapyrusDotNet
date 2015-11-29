using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using PapyrusDotNet.Common;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly.Classes;
using PapyrusDotNet.PapyrusAssembly.Enums;

namespace PapyrusDotNet.Converters.Clr2Papyrus
{
    public class Clr2PapyrusInstructionProcessor : IClr2PapyrusInstructionProcessor
    {
        public IEnumerable<PapyrusInstruction> ProcessInstructions(MethodDefinition method, MethodBody body, Collection<Instruction> instructions)
        {
            var outputInstructions = new List<PapyrusInstruction>();

            foreach (var i in instructions)
            {
                var pi = new PapyrusInstruction();

                pi.OpCode = TranslateOpCode(i.OpCode.Code);

                outputInstructions.Add(pi);
            }
            return outputInstructions;
        }

        public PapyrusOpCode TranslateOpCode(Code code)
        {
            /* Just going to simplify it as much as possible for now. */
            if (InstructionHelper.IsCallMethod(code))
            {
                return PapyrusOpCode.Callmethod;
            }

            if (InstructionHelper.IsNewArrayInstance(code))
            {
                return PapyrusOpCode.ArrayCreate;
            }


            if (code == Code.Ret)
                return PapyrusOpCode.Return;

            return PapyrusOpCode.Nop;
        }
    }
}