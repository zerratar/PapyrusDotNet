using PapyrusDotNet.PapyrusAssembly.Extensions;

namespace PapyrusDotNet.PapyrusAssembly.Classes
{
    public class PapyrusStringRef
    {
        private string value;
        private int index;

        public int Index
        {
            get { return index; }
            set
            {
                index = value;
                var asm = GetAssemblyDefinition();
                if (asm != null && this.value == null)
                {
                    if (asm.StringTable.Count > value)
                    {
                        this.value = asm.StringTable[value];
                    }
                }
            }
        }

        public string Value
        {
            get { return value; }
            set
            {
                this.value = value;
                var asm = GetAssemblyDefinition();
                asm?.StringTable.EnsureAdd(value);
            }
        }

        private static PapyrusAssemblyDefinition GetAssemblyDefinition()
        {
            var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            var asm = PapyrusAssemblyDefinition.GetInternalInstance(threadId);
            return asm;
        }

        public PapyrusStringRef(string value, int index)
        {
            Value = value;
            Index = index;
        }

        public PapyrusStringRef(string value) : this(value, -1)
        {
        }

        public PapyrusStringRef() : this(null, -1)
        {
        }

        public static explicit operator string (PapyrusStringRef r)
        {
            return r.Value;
        }

        public static explicit operator PapyrusStringRef(string s)
        {
            return new PapyrusStringRef(s);
        }
    }
}
