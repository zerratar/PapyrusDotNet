using System.Collections.Generic;
using PapyrusDotNet.PapyrusAssembly.Extensions;

namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusHeaderUserflagCollection : Dictionary<PapyrusStringRef, byte>
    {
        private readonly PapyrusAssemblyDefinition asm;

        public PapyrusHeaderUserflagCollection(PapyrusAssemblyDefinition asm)
        {
            this.asm = asm;
        }

        public void Add(string key, byte value)
        {
            Add(key.Ref(asm), value);
        }

        public new void Add(PapyrusStringRef key, byte value)
        {
            if (asm.StringTable == null)
            {
                asm.StringTable = new List<string>();
            }

            if (!asm.StringTable.Contains(key.Value))
            {
                asm.StringTable.Add(key.Value);
            }

            base.Add(key, value);
        }

        public new void Remove(PapyrusStringRef key)
        {
            base.Remove(key);
        }
    }
}