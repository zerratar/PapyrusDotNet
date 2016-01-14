//     This file is part of PapyrusDotNet.
// 
//     PapyrusDotNet is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     PapyrusDotNet is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with PapyrusDotNet.  If not, see <http://www.gnu.org/licenses/>.
//  
//     Copyright 2016, Karl Patrik Johansson, zerratar@gmail.com

#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusInstructionOpCodeDescription
    {
        private static readonly Dictionary<PapyrusOpCodes, PapyrusInstructionOpCodeDescription> Descriptions;

        static PapyrusInstructionOpCodeDescription()
        {
            Descriptions = new Dictionary<PapyrusOpCodes, PapyrusInstructionOpCodeDescription>();
            Descriptions.Add(PapyrusOpCodes.Nop, new PapyrusInstructionOpCodeDescription(0, false));
            Descriptions.Add(PapyrusOpCodes.Iadd, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusOpCodes.Fadd, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusOpCodes.Isub, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusOpCodes.Fsub, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusOpCodes.Imul, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusOpCodes.Fmul, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusOpCodes.Idiv, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusOpCodes.Fdiv, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusOpCodes.Imod, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusOpCodes.Not, new PapyrusInstructionOpCodeDescription(2, false));
            Descriptions.Add(PapyrusOpCodes.Ineg, new PapyrusInstructionOpCodeDescription(2, false));
            Descriptions.Add(PapyrusOpCodes.Fneg, new PapyrusInstructionOpCodeDescription(2, false));
            Descriptions.Add(PapyrusOpCodes.Assign, new PapyrusInstructionOpCodeDescription(2, false));
            Descriptions.Add(PapyrusOpCodes.Cast, new PapyrusInstructionOpCodeDescription(2, false));
            Descriptions.Add(PapyrusOpCodes.CmpEq, new PapyrusInstructionOpCodeDescription(3, false, "COMPAREEQ"));
            Descriptions.Add(PapyrusOpCodes.CmpLt, new PapyrusInstructionOpCodeDescription(3, false, "COMPARELT"));
            Descriptions.Add(PapyrusOpCodes.CmpLte, new PapyrusInstructionOpCodeDescription(3, false, "COMPARELTE"));
            Descriptions.Add(PapyrusOpCodes.CmpGt, new PapyrusInstructionOpCodeDescription(3, false, "COMPAREGT"));
            Descriptions.Add(PapyrusOpCodes.CmpGte, new PapyrusInstructionOpCodeDescription(3, false, "COMPAREGTE"));
            Descriptions.Add(PapyrusOpCodes.Jmp, new PapyrusInstructionOpCodeDescription(1, false));
            Descriptions.Add(PapyrusOpCodes.Jmpt, new PapyrusInstructionOpCodeDescription(2, false));
            Descriptions.Add(PapyrusOpCodes.Jmpf, new PapyrusInstructionOpCodeDescription(2, false));
            Descriptions.Add(PapyrusOpCodes.Callmethod, new PapyrusInstructionOpCodeDescription(3, true));
            Descriptions.Add(PapyrusOpCodes.Callparent, new PapyrusInstructionOpCodeDescription(2, true));
            Descriptions.Add(PapyrusOpCodes.Callstatic, new PapyrusInstructionOpCodeDescription(3, true));
            Descriptions.Add(PapyrusOpCodes.Return, new PapyrusInstructionOpCodeDescription(1, false));
            Descriptions.Add(PapyrusOpCodes.Strcat, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusOpCodes.PropGet, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusOpCodes.PropSet, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusOpCodes.ArrayCreate, new PapyrusInstructionOpCodeDescription(2, false));
            Descriptions.Add(PapyrusOpCodes.ArrayLength, new PapyrusInstructionOpCodeDescription(2, false));
            Descriptions.Add(PapyrusOpCodes.ArrayGetElement, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusOpCodes.ArraySetElement, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusOpCodes.ArrayFindElement, new PapyrusInstructionOpCodeDescription(4, false));
            Descriptions.Add(PapyrusOpCodes.ArrayFindLastElement, new PapyrusInstructionOpCodeDescription(4, false, "ARRAYRFINDELEMENT"));

            Descriptions.Add(PapyrusOpCodes.Is, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusOpCodes.StructCreate, new PapyrusInstructionOpCodeDescription(1, false));
            Descriptions.Add(PapyrusOpCodes.StructGet, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusOpCodes.StructSet, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusOpCodes.ArrayFindStruct, new PapyrusInstructionOpCodeDescription(5, false));
            Descriptions.Add(PapyrusOpCodes.ArrayFindLastStruct, new PapyrusInstructionOpCodeDescription(5, false, "ARRAYRFINDSTRUCT"));
            Descriptions.Add(PapyrusOpCodes.ArrayAddElements, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusOpCodes.ArrayInsertElement, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusOpCodes.ArrayRemoveLastElement, new PapyrusInstructionOpCodeDescription(1, false));
            Descriptions.Add(PapyrusOpCodes.ArrayRemoveElements, new PapyrusInstructionOpCodeDescription(3, false));
            Descriptions.Add(PapyrusOpCodes.ArrayClearElements, new PapyrusInstructionOpCodeDescription(1, false));
        }

        public string[] Aliases { get; }
        public int ArgumentCount { get; }
        public bool HasOperandArguments { get; }

        public PapyrusInstructionOpCodeDescription(int argumentCount, bool hasOperandArguments, params string[] aliases)
        {
            Aliases = aliases;
            ArgumentCount = argumentCount;
            HasOperandArguments = hasOperandArguments;
        }

        public static KeyValuePair<PapyrusOpCodes, PapyrusInstructionOpCodeDescription> FromAlias(string alias)
        {
            return Descriptions.FirstOrDefault(k =>
            {
                var a = k.Value.Aliases;
                if (a != null && a.Any(i => i.ToLower() == alias.ToLower())) return true;
                return k.Key.ToString().ToLower() == alias.ToLower();
            });
        }

        public static PapyrusInstructionOpCodeDescription FromOpCode(PapyrusOpCodes opcode)
        {
            if (Descriptions.ContainsKey(opcode))
                return Descriptions[opcode];
            return null;
        }
    }
}