using System;
using System.Collections.Generic;

namespace PapyrusDotNet.PapyrusAssembly.Classes
{
    public class PapyrusMethodDefinition : PapyrusMethodReference
    {
        private readonly PapyrusAssemblyDefinition assembly;

        public PapyrusMethodDefinition(PapyrusAssemblyDefinition assembly)
        {
            this.assembly = assembly;
            Parameters = new List<PapyrusParameterDefinition>();
            Body = new PapyrusMethodBody(this);
        }

        public PapyrusStringRef Name { get; set; }
        public PapyrusStringRef ReturnTypeName { get; set; }
        public PapyrusStringRef Documentation { get; set; }

        public PapyrusMethodBody Body { get; set; }

        public bool HasBody => Body != null && !Body.IsEmpty;

        public int UserFlags { get; set; }
        public byte Flags { get; set; }
        public List<PapyrusParameterDefinition> Parameters { get; set; }

        public void SetFlags(PapyrusMethodFlags flags)
        {
            Flags = (byte)flags;
        }

        public PapyrusMethodFlags GetFlags()
        {
            return (PapyrusMethodFlags)Flags;
        }
        public bool IsGlobal
        {
            get { return (Flags & (byte)PapyrusMethodFlags.Global) > 0; }
            set
            {
                if (value)
                {
                    SetFlags(GetFlags() | PapyrusMethodFlags.Global);
                }
                else
                {
                    SetFlags(GetFlags() & ~PapyrusMethodFlags.Global);
                }
            }
        }

        public bool IsNative
        {
            get { return (Flags & (byte)PapyrusMethodFlags.Native) > 0; }
            set
            {
                if (value)
                {
                    SetFlags(GetFlags() | PapyrusMethodFlags.Native);
                }
                else
                {
                    SetFlags(GetFlags() & ~PapyrusMethodFlags.Native);
                }
            }
        }

        public bool IsEvent { get; set; }//=> (Flags & 4) > 0;
    }

    [Flags]
    public enum PapyrusMethodFlags : byte
    {
        Global = 1,
        Native = 2
    }
}