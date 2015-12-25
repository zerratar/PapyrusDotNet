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
//     Copyright 2015, Karl Patrik Johansson, zerratar@gmail.com

#region

using System;
using Mono.Cecil.Cil;

#endregion

namespace PapyrusDotNet.Common
{
    public class InstructionHelper
    {
        public static bool IsConditional(Code code) => code == Code.Ceq || code == Code.Cgt || code == Code.Clt || code == Code.Cgt_Un || code == Code.Clt_Un;

        public static bool IsBranch(Code code) => code == Code.Br || code == Code.Br_S || code == Code.Brfalse_S || code == Code.Brtrue_S || code == Code.Brfalse || code == Code.Brtrue;

        public static bool IsBranchTrue(Code code) => code == Code.Brtrue || code == Code.Brtrue_S;

        public static bool IsBranchFalse(Code code) => code == Code.Brfalse || code == Code.Brfalse_S;

        public static bool IsBranchConditionalEq(Code code) => code == Code.Beq || code == Code.Beq_S;

        public static bool IsBranchConditionalLt(Code code) => code == Code.Blt || code == Code.Blt_S || code == Code.Blt_Un || code == Code.Blt_Un_S;

        public static bool IsBranchConditionalLe(Code code) => code == Code.Ble || code == Code.Ble_S || code == Code.Ble_Un || code == Code.Ble_Un_S;

        public static bool IsBranchConditionalGt(Code code) => code == Code.Bgt || code == Code.Bgt_S || code == Code.Bgt_Un || code == Code.Bgt_Un_S;

        public static bool IsBranchConditionalGe(Code code) => code == Code.Bge || code == Code.Bge_S || code == Code.Bge_Un || code == Code.Bge_Un_S;

        public static bool IsBranchConditional(Code code) => code == Code.Beq || code == Code.Beq_S || code == Code.Bgt || code == Code.Bgt_S || code == Code.Bgt_Un || code == Code.Bgt_Un_S || code == Code.Blt || code == Code.Blt_Un || code == Code.Blt_S || code == Code.Blt_Un_S || code == Code.Ble || code == Code.Ble_Un || code == Code.Ble_S || code == Code.Ble_Un_S || code == Code.Bge || code == Code.Bge_Un || code == Code.Bge_S || code == Code.Bge_Un_S;

        public static bool IsUnboxing(Code code) => code == Code.Unbox || code == Code.Unbox_Any;

        public static bool IsBoxing(Code code) => code == Code.Box;

        public static bool IsConverToNumber(Code code) => code == Code.Conv_I || code == Code.Conv_I1 || code == Code.Conv_I2 || code == Code.Conv_I4 || code == Code.Conv_I8 || code == Code.Conv_Ovf_I || code == Code.Conv_Ovf_I_Un || code == Code.Conv_Ovf_I1 || code == Code.Conv_Ovf_I1_Un || code == Code.Conv_Ovf_I2 || code == Code.Conv_Ovf_I2_Un || code == Code.Conv_Ovf_I4 || code == Code.Conv_Ovf_I4_Un || code == Code.Conv_Ovf_I8 || code == Code.Conv_Ovf_I8_Un || code == Code.Conv_R_Un || code == Code.Conv_R4 || code == Code.Conv_R8;

        public static bool IsLoadArgs(Code code) => code == Code.Ldarg || code == Code.Ldarg_0 || code == Code.Ldarg_1 || code == Code.Ldarg_2 || code == Code.Ldarg_3 || code == Code.Ldarg_S;

        public static bool IsCallMethod(Code code) => code == Code.Call || code == Code.Calli || code == Code.Callvirt;

        public static bool IsInstance(Code code) => code == Code.Isinst;

        public static bool IsLoadString(Code code) => code == Code.Ldstr;

        public static bool IsLoadInteger(Code code) => code == Code.Ldc_I4 || code == Code.Ldc_I4_0 || code == Code.Ldc_I4_1 || code == Code.Ldc_I4_2 || code == Code.Ldc_I4_3 || code == Code.Ldc_I4_4 || code == Code.Ldc_I4_5 || code == Code.Ldc_I4_6 || code == Code.Ldc_I4_7 || code == Code.Ldc_I4_8 || code == Code.Ldc_I4_S || code == Code.Ldc_I8 || code == Code.Ldc_R4 || code == Code.Ldc_R8;

        public static bool IsLoadFieldObject(Code code) => code == Code.Ldfld || code == Code.Ldflda;

        public static bool IsLoadMethodRef(Code code) => code == Code.Ldftn;

        public static bool IsLoadLocalVariable(Code code) => code == Code.Ldloc_0 || code == Code.Ldloc || code == Code.Ldloc_1 || code == Code.Ldloc_2 || code == Code.Ldloc_3 || code == Code.Ldloc_S;

        public static bool IsLoadLength(Code code) => code == Code.Ldlen;

        public static bool IsLoad(Code code) => IsLoadMethodRef(code) || IsLoadArgs(code) || IsLoadInteger(code) || IsLoadLocalVariable(code) || IsLoadString(code) || IsLoadFieldObject(code) || IsLoadFieldValue(code) || IsLoadElement(code) || IsLoadLength(code);

        public static bool IsStore(Code code) => IsStoreElement(code) || IsStoreFieldObject(code) || IsStoreLocalVariable(code) || IsStoreFieldValue(code);

        public static bool IsStoreFieldObject(Code code) => code == Code.Stfld;

        public static bool IsStoreLocalVariable(Code code) => code == Code.Stloc || code == Code.Stloc_0 || code == Code.Stloc_1 || code == Code.Stloc_2 || code == Code.Stloc_3 || code == Code.Stloc_S;

        public static bool IsGreaterThanOrEqual(Code code) => code == Code.Cgt || code == Code.Cgt_Un;

        public static bool IsGreaterThan(Code code) => code == Code.Cgt || code == Code.Cgt_Un;

        public static bool IsLessThan(Code code) => code == Code.Clt || code == Code.Clt_Un;

        public static bool IsEqualTo(Code code) => code == Code.Ceq;

        public static bool IsMath(Code code) => code == Code.Add || code == Code.Sub || code == Code.Div || code == Code.Mul;

        public static bool IsLoadFieldValue(Code code) => code == Code.Ldsfld || code == Code.Ldsflda;

        public static bool IsStoreFieldValue(Code code) => code == Code.Stsfld;

        public static bool IsLoadElement(Code code) => code == Code.Ldelem_Any || code == Code.Ldelem_I || code == Code.Ldelem_I1 || code == Code.Ldelem_I2 || code == Code.Ldelem_I4 || code == Code.Ldelem_I8 || code == Code.Ldelem_R4 || code == Code.Ldelem_R8 || code == Code.Ldelem_Ref || code == Code.Ldelem_U1 || code == Code.Ldelem_U2 || code == Code.Ldelem_U4 || code == Code.Ldelema;

        public static bool IsStoreElement(Code code) => code == Code.Stelem_Any || code == Code.Stelem_I || code == Code.Stelem_I1 || code == Code.Stelem_I2 || code == Code.Stelem_I4 || code == Code.Stelem_I8 || code == Code.Stelem_R4 || code == Code.Stelem_R8 || code == Code.Stelem_Ref;

        public static bool IsNewInstance(Code code) => IsNewArrayInstance(code) || IsNewObjectInstance(code);

        public static bool IsNewArrayInstance(Code code) => code == Code.Newarr;

        public static bool IsNewObjectInstance(Code code) => code == Code.Newobj;

        public static bool IsLoadNull(Code code) => code == Code.Ldnull;

        public static bool IsSwitch(Code code) => code == Code.Switch;

        public static int GetCodeIndex(Code code)
        {
            switch (code)
            {
                case Code.Ldarg_S:
                case Code.Ldarg:
                case Code.Stloc_S:
                case Code.Stloc:
                case Code.Ldc_I4:
                case Code.Ldloc:
                case Code.Ldloc_S:
                    return -1;
                case Code.Ldloc_0:
                case Code.Ldarg_0:
                case Code.Stloc_0:
                case Code.Ldc_I4_0:
                    return 0;
                case Code.Ldloc_1:
                case Code.Ldarg_1:
                case Code.Stloc_1:
                case Code.Ldc_I4_1:
                    return 1;
                case Code.Ldloc_2:
                case Code.Ldarg_2:
                case Code.Stloc_2:
                case Code.Ldc_I4_2:
                    return 2;
                case Code.Ldloc_3:
                case Code.Ldarg_3:
                case Code.Stloc_3:
                case Code.Ldc_I4_3:
                    return 3;
                case Code.Ldc_I4_4:
                    return 4;
                case Code.Ldc_I4_5:
                    return 5;
                case Code.Ldc_I4_6:
                    return 6;
                case Code.Ldc_I4_7:
                    return 7;
                case Code.Ldc_I4_8:
                    return 8;
            }
            return -1;
        }

        public static bool PreviousInstructionWas(Instruction instruction, Code targetOpCode)
        {
            return (instruction.Previous != null && instruction.Previous.OpCode.Code == targetOpCode) ||
                   (instruction.Previous != null && instruction.Previous.OpCode.Code == Code.Nop &&
                    instruction.Previous.Previous != null &&
                    instruction.Previous.Previous.OpCode.Code == targetOpCode);
        }

        public static bool NextInstructionIs(Instruction instruction, Code targetOpCode)
        {
            return (instruction.Next != null && instruction.Next.OpCode.Code == targetOpCode) ||
                   (instruction.Next != null && instruction.Next.OpCode.Code == Code.Nop &&
                    instruction.Next.Next != null &&
                    instruction.Next.Next.OpCode.Code == targetOpCode);
        }

        public static Instruction FindPreviousInstruction(Instruction instruction, Func<Code, bool> pred)
        {
            if (instruction == null) return null;
            var prev = instruction.Previous;
            while (prev != null && !pred(prev.OpCode.Code)) prev = prev.Previous;
            return prev;
        }


        public static bool NextInstructionIs(Instruction instruction, Func<Code, bool> targetOpCode)
        {
            return (instruction.Next != null && targetOpCode(instruction.Next.OpCode.Code)) ||
                   (instruction.Next != null && instruction.Next.OpCode.Code == Code.Nop &&
                    instruction.Next.Next != null &&
                    targetOpCode(instruction.Next.Next.OpCode.Code));
        }

        public static int NextInstructionIsOffset(Instruction instruction, Code targetOpCode)
        {
            var b1 = (instruction.Next != null && instruction.Next.OpCode.Code == targetOpCode);
            if (b1) return instruction.Next.Offset;

            var b2 = (instruction.Next != null && instruction.Next.OpCode.Code == Code.Nop &&
                     instruction.Next.Next != null &&
                     instruction.Next.Next.OpCode.Code == targetOpCode);
            if (b2) return instruction.Next.Next.Offset;
            return -1;
        }

        public static Instruction NextInstructionAtOffset(Instruction instruction, int offset)
        {
            var inst = instruction;
            while (inst.Offset < offset) inst = inst.Next;
            return inst;
        }
    }
}