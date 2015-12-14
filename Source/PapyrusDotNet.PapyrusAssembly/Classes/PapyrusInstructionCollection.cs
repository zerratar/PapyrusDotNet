using System;
using System.Collections;
using System.Collections.Generic;

namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusInstructionCollection : IEnumerable<PapyrusInstruction>
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

            for (var offset = 0; offset < items.Count; offset++)
            {
                var instruction = items[offset].Operand as PapyrusInstruction;
                if (instruction != null)
                {
                    if (items[offset].OpCode == PapyrusOpCode.Jmp)
                    {
                        items[offset].Arguments[0].Value = instruction.Offset - offset;
                        items[offset].Arguments[0].ValueType = PapyrusPrimitiveType.Integer;
                    }
                    else if (items[offset].OpCode == PapyrusOpCode.Jmpt || items[offset].OpCode == PapyrusOpCode.Jmpf)
                    {
                        items[offset].Arguments[1].Value = instruction.Offset - offset;
                        items[offset].Arguments[1].ValueType = PapyrusPrimitiveType.Integer;
                    }
                }
            }

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