using System.Collections.Generic;
using PapyrusDotNet.PapyrusAssembly.Enums;

namespace PapyrusDotNet.PapyrusAssembly.Classes
{
    public class PapyrusInstructionOpCodeDescription
    {
        public int ParamSize { get; }
        public bool HasVariableArguments { get; }

        private static readonly Dictionary<PapyrusInstructionOpCodes, PapyrusInstructionOpCodeDescription> Descriptions;

        public PapyrusInstructionOpCodeDescription(int paramSize, bool hasVariableArguments)
        {
            ParamSize = paramSize;
            HasVariableArguments = hasVariableArguments;
        }

        public static PapyrusInstructionOpCodeDescription FromOpCode(PapyrusInstructionOpCodes opcode)
        {
            if (Descriptions.ContainsKey(opcode))
                return Descriptions[opcode];
            return null;
        }

        static PapyrusInstructionOpCodeDescription()
        {
            Descriptions = new Dictionary<PapyrusInstructionOpCodes, PapyrusInstructionOpCodeDescription>();
            Descriptions.Add(PapyrusInstructionOpCodes.Nop, new PapyrusInstructionOpCodeDescription(0, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Iadd, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Fadd, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Isub, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Fsub, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Imul, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Fmul, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Idiv, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Fdiv, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Imod, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Not, new PapyrusInstructionOpCodeDescription(2, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Ineg, new PapyrusInstructionOpCodeDescription(2, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Fneg, new PapyrusInstructionOpCodeDescription(2, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Assign, new PapyrusInstructionOpCodeDescription(2, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Cast, new PapyrusInstructionOpCodeDescription(2, false));
            Descriptions.Add(PapyrusInstructionOpCodes.CmpEq, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.CmpLt, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.CmpLte, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.CmpGt, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.CmpGte, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Jmp, new PapyrusInstructionOpCodeDescription(1, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Jmpt, new PapyrusInstructionOpCodeDescription(2, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Jmpf, new PapyrusInstructionOpCodeDescription(2, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Callmethod, new PapyrusInstructionOpCodeDescription(3, true));
            Descriptions.Add(PapyrusInstructionOpCodes.Callparent, new PapyrusInstructionOpCodeDescription(2, true));
            Descriptions.Add(PapyrusInstructionOpCodes.Callstatic, new PapyrusInstructionOpCodeDescription(3, true));
            Descriptions.Add(PapyrusInstructionOpCodes.Return, new PapyrusInstructionOpCodeDescription(1, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Strcat, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Propget, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Propset, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.ArrayCreate, new PapyrusInstructionOpCodeDescription(2, false));
            Descriptions.Add(PapyrusInstructionOpCodes.ArrayLength, new PapyrusInstructionOpCodeDescription(2, false));
            Descriptions.Add(PapyrusInstructionOpCodes.ArrayGetelement, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.ArraySetelement, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.ArrayFindelement, new PapyrusInstructionOpCodeDescription(4, false));
            //Descriptions.Add(PapyrusInstructionOpCodes.ArrayFindelement, new PapyrusInstructionOpCodeDescription(4, false));
            Descriptions.Add(PapyrusInstructionOpCodes.Is, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.StructCreate, new PapyrusInstructionOpCodeDescription(1, false));
            Descriptions.Add(PapyrusInstructionOpCodes.StructGet, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.StructSet, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.ArrayFindstruct, new PapyrusInstructionOpCodeDescription(5, false));
            Descriptions.Add(PapyrusInstructionOpCodes.ArrayAddelements, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.ArrayInsertelement, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.ArrayRemovelastelement, new PapyrusInstructionOpCodeDescription(1, false));
            Descriptions.Add(PapyrusInstructionOpCodes.ArrayRemoveelements, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusInstructionOpCodes.ArrayClearelements, new PapyrusInstructionOpCodeDescription(1, false));
        }
    }
}