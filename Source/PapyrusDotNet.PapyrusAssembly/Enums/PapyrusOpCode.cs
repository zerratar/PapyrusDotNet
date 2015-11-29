#region License

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

#endregion

namespace PapyrusDotNet.PapyrusAssembly.Enums
{
    //public enum Opcode : byte
    //{
    //    ArrayCreate = 30,
    //    ArrayFindelement = 34,
    //    ArrayGetelement = 32,
    //    ArrayLength = 31,
    //    ArrayRfindelement = 35,
    //    ArraySetelement = 33,
    //    Assign = 13,
    //    Callmethod = 23,
    //    Callparent = 24,
    //    Callstatic = 25,
    //    Cast = 14,
    //    CmpEq = 15,
    //    CmpGe = 19,
    //    CmpGt = 18,
    //    CmpLe = 17,
    //    CmpLt = 16,
    //    Fadd = 2,
    //    Fdiv = 8,
    //    Fmul = 6,
    //    Fneg = 12,
    //    Fsub = 4,
    //    Iadd = 1,
    //    Idiv = 7,
    //    Imod = 9,
    //    Imul = 5,
    //    Ineg = 11,
    //    Isub = 3,
    //    Jmp = 20,
    //    Jmpf = 22,
    //    Jmpt = 21,
    //    Nop = 0,
    //    Not = 10,
    //    OpReturn = 26,
    //    Propget = 28,
    //    Propset = 29,
    //    Strcat = 27
    //}

    public enum PapyrusOpCode : byte
    {
        Nop,
        Iadd,
        Fadd,
        Isub,
        Fsub,
        Imul,
        Fmul,
        Idiv,
        Fdiv,
        Imod,
        Not,
        Ineg,
        Fneg,
        Assign,
        Cast,
        CmpEq,
        CmpLt,
        CmpLte,
        CmpGt,
        CmpGte,
        Jmp,
        Jmpt,
        Jmpf,
        Callmethod,
        Callparent,
        Callstatic,
        Return,
        Strcat,
        Propget,
        Propset,
        ArrayCreate,
        ArrayLength,
        ArrayGetelement,
        ArraySetelement,
        ArrayFindelement,
        ArrayRfindelement,

        Is,
        StructCreate,
        StructGet,
        StructSet,
        ArrayFindstruct,
        ArrayRfindstruct,
        ArrayAddelements,
        ArrayInsertelement,
        ArrayRemovelastelement,
        ArrayRemoveelements,
        ArrayClearelements
    }
}