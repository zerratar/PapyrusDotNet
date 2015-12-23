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

using PapyrusDotNet.PapyrusAssembly.Extensions;

#endregion

namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusStringRef
    {
        private readonly PapyrusAssemblyDefinition assembly;
        private int index;

        private string value;

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

        public PapyrusStringRef Ref(string value)
        {
            return new PapyrusStringRef(assembly, value);
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
            return r?.Value;
        }
    }
}