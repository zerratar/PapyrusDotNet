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

using Mono.Cecil.Cil;

#endregion

namespace PapyrusDotNet.Common
{
    public class InstructionHelper
    {
        public static bool IsBranchConditionalEq(Code code)
        {
            return code == Code.Beq || code == Code.Beq_S;
        }

        public static bool IsBranchConditionalLt(Code code)
        {
            return code == Code.Blt || code == Code.Blt_S || code == Code.Blt_Un || code == Code.Blt_Un_S;
        }

        public static bool IsBranchConditionalLe(Code code)
        {
            return code == Code.Ble || code == Code.Ble_S || code == Code.Ble_Un || code == Code.Ble_Un_S;
        }

        public static bool IsBranchConditionalGt(Code code)
        {
            return code == Code.Bgt || code == Code.Bgt_S || code == Code.Bgt_Un || code == Code.Bgt_Un_S;
        }

        public static bool IsBranchConditionalGe(Code code)
        {
            return code == Code.Bge || code == Code.Bge_S || code == Code.Bge_Un || code == Code.Bge_Un_S;
        }


        public static bool IsBranchConditional(Code code)
        {
            return code == Code.Beq || code == Code.Beq_S || code == Code.Bgt || code == Code.Bgt_S ||
                   code == Code.Bgt_Un
                   || code == Code.Bgt_Un_S || code == Code.Blt || code == Code.Blt_Un || code == Code.Blt_S ||
                   code == Code.Blt_Un_S
                   || code == Code.Ble || code == Code.Ble_Un || code == Code.Ble_S || code == Code.Ble_Un_S ||
                   code == Code.Bge
                   || code == Code.Bge_Un || code == Code.Bge_S || code == Code.Bge_Un_S;
        }

        public static bool IsBoxing(Code code)
        {
            return code == Code.Box;
        }

        public static bool IsConverToNumber(Code code)
        {
            return code == Code.Conv_I || code == Code.Conv_I1 || code == Code.Conv_I2 || code == Code.Conv_I4
                   || code == Code.Conv_I8 || code == Code.Conv_Ovf_I || code == Code.Conv_Ovf_I_Un ||
                   code == Code.Conv_Ovf_I1
                   || code == Code.Conv_Ovf_I1_Un || code == Code.Conv_Ovf_I2 || code == Code.Conv_Ovf_I2_Un
                   || code == Code.Conv_Ovf_I4 || code == Code.Conv_Ovf_I4_Un || code == Code.Conv_Ovf_I8
                   || code == Code.Conv_Ovf_I8_Un || code == Code.Conv_R_Un || code == Code.Conv_R4 ||
                   code == Code.Conv_R8;
        }

        public static bool IsLoadArgs(Code code)
        {
            return code == Code.Ldarg || code == Code.Ldarg_0 || code == Code.Ldarg_1 || code == Code.Ldarg_2
                   || code == Code.Ldarg_3 || code == Code.Ldarg_S;
        }

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


        public static bool IsCallMethod(Code code)
        {
            return code == Code.Call || code == Code.Calli || code == Code.Callvirt;
        }


        public static bool IsLoadString(Code code)
        {
            return code == Code.Ldstr;
        }


        public static bool IsLoadInteger(Code code)
        {
            return code == Code.Ldc_I4 || code == Code.Ldc_I4_0 || code == Code.Ldc_I4_1 || code == Code.Ldc_I4_2
                   || code == Code.Ldc_I4_3 || code == Code.Ldc_I4_4 || code == Code.Ldc_I4_5 || code == Code.Ldc_I4_6
                   || code == Code.Ldc_I4_7 || code == Code.Ldc_I4_8 || code == Code.Ldc_I4_S || code == Code.Ldc_I8
                   || code == Code.Ldc_R4 || code == Code.Ldc_R8;
        }

        public static bool IsLoadField(Code code)
        {
            return code == Code.Ldfld || code == Code.Ldflda;
        }

        public static bool IsLoadLocalVariable(Code code)
        {
            return code == Code.Ldloc_0 || code == Code.Ldloc || code == Code.Ldloc_1 || code == Code.Ldloc_2
                   || code == Code.Ldloc_3 || code == Code.Ldloc_S;
        }

        public static bool IsLoad(Code code)
        {
            return IsLoadArgs(code) || IsLoadInteger(code) || IsLoadLocalVariable(code) || IsLoadString(code);
        }

        public static bool IsStore(Code code)
        {
            return IsStoreElement(code) || IsStoreField(code) || IsStoreLocalVariable(code) || IsStoreStaticField(code);
        }

        public static bool IsStoreField(Code code)
        {
            return code == Code.Stfld;
        }

        public static bool IsStoreLocalVariable(Code code)
        {
            return code == Code.Stloc || code == Code.Stloc_0 || code == Code.Stloc_1 || code == Code.Stloc_2
                   || code == Code.Stloc_3 || code == Code.Stloc_S;
        }

        public static bool IsGreaterThan(Code code)
        {
            return code == Code.Cgt || code == Code.Cgt_Un;
        }

        public static bool IsLessThan(Code code)
        {
            return code == Code.Clt || code == Code.Clt_Un;
        }

        public static bool IsEqualTo(Code code)
        {
            return code == Code.Ceq;
        }

        public static bool IsMath(Code code)
        {
            return code == Code.Add || code == Code.Sub || code == Code.Div || code == Code.Mul;
        }

        public static bool IsLoadStaticField(Code code)
        {
            return code == Code.Ldsfld || code == Code.Ldsflda;
        }

        public static bool IsStoreStaticField(Code code)
        {
            return code == Code.Stsfld;
        }


        public static bool IsLoadElement(Code code)
        {
            return code == Code.Ldelem_Any || code == Code.Ldelem_I || code == Code.Ldelem_I1 || code == Code.Ldelem_I2
                   || code == Code.Ldelem_I4 || code == Code.Ldelem_I8 || code == Code.Ldelem_R4 ||
                   code == Code.Ldelem_R8
                   || code == Code.Ldelem_Ref || code == Code.Ldelem_U1 || code == Code.Ldelem_U2 ||
                   code == Code.Ldelem_U4
                   || code == Code.Ldelema;
        }

        public static bool IsStoreElement(Code code)
        {
            return code == Code.Stelem_Any || code == Code.Stelem_I || code == Code.Stelem_I1 || code == Code.Stelem_I2
                   || code == Code.Stelem_I4 || code == Code.Stelem_I8 || code == Code.Stelem_R4 ||
                   code == Code.Stelem_R8
                   || code == Code.Stelem_Ref;
        }

        public static bool IsNewInstance(Code code)
        {
            return IsNewArrayInstance(code) || IsNewObjectInstance(code);
        }

        public static bool IsNewArrayInstance(Code code)
        {
            return code == Code.Newarr;
        }

        public static bool IsNewObjectInstance(Code code)
        {
            return code == Code.Newobj;
        }

        public static bool IsLoadNull(Code code)
        {
            return code == Code.Ldnull;
        }
    }
}