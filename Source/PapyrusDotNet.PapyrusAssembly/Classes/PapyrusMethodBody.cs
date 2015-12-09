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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace PapyrusDotNet.PapyrusAssembly.Classes
{
    public class PapyrusMethodBody
    {
        private readonly PapyrusMethodDefinition method;

        public PapyrusMethodBody(PapyrusMethodDefinition method)
        {
            this.method = method;
            Instructions = new InstructionCollection();
            Variables = new List<PapyrusVariableReference>();
            Fields = new List<PapyrusVariableReference>();
            TempVariables = new List<PapyrusVariableReference>();
        }

        public bool HasVariables => Variables.Any();
        public bool IsEmpty => !Instructions.Any();

        public List<PapyrusVariableReference> Variables { get; set; }

        public List<PapyrusVariableReference> Fields { get; set; }

        public List<PapyrusVariableReference> TempVariables { get; set; }

        public InstructionCollection Instructions { get; set; }

        public PapyrusMethodDefinition GetMethod() => method;
    }

    public class InstructionCollection : IEnumerable<PapyrusInstruction>
    {
        private readonly List<PapyrusInstruction> items = new List<PapyrusInstruction>();

        public int Count => items.Count;

        public IEnumerator<PapyrusInstruction> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public PapyrusInstruction this[int index] => items[index];

        public void Add(PapyrusInstruction item)
        {
            items.Add(item);
        }

        public void AddRange(IEnumerable<PapyrusInstruction> i)
        {
            items.AddRange(i);
        }

        public void Remove(PapyrusInstruction item)
        {
            items.Remove(item);
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        public void RecalculateOffsets()
        {
            // TODO: Call RecalculateOffset(); whenever the Method has been finalized.
            for (var offset = 0; offset < items.Count; offset++)
            {
                items[offset].Offset = offset;
            }
            // TODO: Update any instructions with operand of another instruction
            // now that the instructions have new offsets, the Parameters needs to be updated.
            // -- JUMP: First Parameter needs to be updated
            // -- JUMPF or JUMPT: Second Parameter needs to be updated
        }

        public void ForEach(Action<PapyrusInstruction> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }
    }
}