using PapyrusDotNet.PapyrusAssembly.Extensions;

namespace PapyrusDotNet.PapyrusAssembly.Classes
{
    public class PapyrusStringRef
    {
        private readonly PapyrusAssemblyDefinition assembly;

        public PapyrusStringRef(PapyrusAssemblyDefinition assembly, string value, int index)
        {
            this.assembly = assembly;
            Value = value;
            Index = index;
        }

        public PapyrusStringRef(PapyrusAssemblyDefinition assembly, string value) : this(assembly, value, -1)
        {
        }

        public PapyrusStringRef(PapyrusAssemblyDefinition assembly) : this(assembly, null, -1)
        {
        }

        public PapyrusStringRef Ref(string value)
        {
            return new PapyrusStringRef(assembly, value);
        }

        private string value;
        private int index;

        public int Index
        {
            get { return index; }
            set
            {
                // If the value isnt null, we do not want to
                // set the index from here. Otherwise we might overwrite the index
                // whenever we set a new string value.
                if (this.value != null) return;

                index = value;
                var asm = GetAssemblyDefinition();
                if (asm != null && this.value == null && value >= 0)
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
                if (value == null)
                    value = string.Empty;

                this.value = value;
                var asm = GetAssemblyDefinition();
                asm?.StringTable.EnsureAdd(value);
                if (asm != null)
                {
                    index = asm.StringTable.IndexOf(value);
                }
            }
        }

        private PapyrusAssemblyDefinition GetAssemblyDefinition()
        {
            //var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            //var asm = PapyrusAssemblyDefinition.GetInternalInstance(threadId);
            return assembly;
        }

        public override string ToString()
        {
            return Value + " [" + Index + "]";
        }

        public static explicit operator string (PapyrusStringRef r)
        {
            return r.Value;
        } 
    }
}
