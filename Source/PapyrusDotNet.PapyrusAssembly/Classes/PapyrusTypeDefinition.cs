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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

#endregion

namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusTypeDefinition : PapyrusVariableReference
    {
        public readonly PapyrusAssemblyDefinition Assembly;
        
        public PapyrusTypeDefinition(PapyrusAssemblyDefinition assembly, bool isStruct = false)
        {
            Assembly = assembly;
            Fields = new Collection<PapyrusFieldDefinition>();
            NestedTypes = new Collection<PapyrusTypeDefinition>();
            Properties = new Collection<PapyrusPropertyDefinition>();
            States = new Collection<PapyrusStateDefinition>();

            if (!isStruct)
            {
                Assembly.Types.Add(this);
            }
        }

        public int Size { get; set; }
        public PapyrusStringRef BaseTypeName { get; set; }
        public byte Flags { get; set; }
        public PapyrusStringRef Documentation { get; set; }
        public int UserFlags { get; set; }
        public PapyrusStringRef AutoStateName { get; set; }
        public bool IsClass { get; set; }
        public bool IsStruct { get; set; }
        public Collection<PapyrusFieldDefinition> Fields { get; set; }
        public Collection<PapyrusTypeDefinition> NestedTypes { get; set; }
        public Collection<PapyrusPropertyDefinition> Properties { get; set; }
        public Collection<PapyrusStateDefinition> States { get; set; }

        public override string ToString()
        {
            return (IsClass ? "class " : "struct ") + Name + (BaseTypeName != null ? " : " + BaseTypeName : "");
        }

        public void UpdateOperand(PapyrusInstruction instruction, PapyrusInstructionCollection instructions)
        {
            var allmethods = States.SelectMany(m => m.Methods);
            var papyrusMethodDefinitions = allmethods.ToList();
            var i = instruction;
            if (i.OpCode == PapyrusOpCodes.Jmpt || i.OpCode == PapyrusOpCodes.Jmpf)
            {
                i.Operand = instructions.FirstOrDefault(i2 => i2.Offset == i.Offset + int.Parse(i.GetArg(1)));
                if (i.Operand == null)
                    i.Operand = instructions.FirstOrDefault(i2 => i2.Offset == i.Offset + (int.Parse(i.GetArg(1)) - 1));
            }
            else if (i.OpCode == PapyrusOpCodes.Jmp)
            {
                i.Operand = instructions.FirstOrDefault(i2 => i2.Offset == i.Offset + int.Parse(i.GetArg(0)));
                if (i.Operand == null)
                    i.Operand = instructions.FirstOrDefault(i2 => i2.Offset == i.Offset + (int.Parse(i.GetArg(0)) - 1));
            }

            else if (i.OpCode == PapyrusOpCodes.Callparent)
            {
                var arg = i.GetArg(0);
                i.Operand = papyrusMethodDefinitions
                            .FirstOrDefault(m => m.Name.Value.ToLower() == arg.ToLower());
                if (i.Operand == null)
                {
                    i.Operand = "<Method Reference Not Loaded>";
                }
            }
            else if (i.OpCode == PapyrusOpCodes.Callmethod)
            {
                var arg = i.GetArg(0);
                i.Operand = papyrusMethodDefinitions
                            .FirstOrDefault(m => m.Name.Value.ToLower() == arg.ToLower());
                if (i.Operand == null)
                {
                    i.Operand = "<Method Reference Not Loaded>";
                }
            }
            else if (i.OpCode == PapyrusOpCodes.Callstatic)
            {
                var arg = i.GetArg(0);
                i.Operand = papyrusMethodDefinitions
                            .FirstOrDefault(m => m.Name.Value.ToLower() == arg.ToLower());
                if (i.Operand == null)
                {
                    i.Operand = "<Method Reference Not Loaded>";
                }
            }

            else
            {
                i.Operand = null;
            }
        }

    }
}