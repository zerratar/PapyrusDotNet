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
//     Copyright 2016, Karl Patrik Johansson, zerratar@gmail.com

#region



#endregion

namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusStringRef
    {
        private readonly PapyrusAssemblyDefinition assembly;
        private int index;
        private readonly PapyrusStringTableIndex stringTableIndex;
        private string value;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PapyrusStringRef" /> class.
        /// </summary>
        public PapyrusStringRef()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PapyrusStringRef" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        public PapyrusStringRef(string value, int index)
        {
            this.value = value;
            this.index = index;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PapyrusStringRef" /> class.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        public PapyrusStringRef(PapyrusAssemblyDefinition assembly, string value, int index)
        {
            this.assembly = assembly;
            Value = value;
            Index = index;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PapyrusStringRef" /> class.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="value">The value.</param>
        public PapyrusStringRef(PapyrusAssemblyDefinition assembly, string value) : this(assembly, value, -1)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PapyrusStringRef" /> class.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public PapyrusStringRef(PapyrusAssemblyDefinition assembly) : this(assembly, null, -1)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PapyrusStringRef" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public PapyrusStringRef(PapyrusStringTableIndex value)
        {
            stringTableIndex = value;
            this.value = value.Identifier;
            index = value.TableIndex;
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
                var asm = GetAssembly();
                if (asm != null && this.value == null && value >= 0)
                {
                    if (asm.StringTable.Size > value)
                    {
                        this.value = asm.StringTable[value].Identifier;
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
                var asm = GetAssembly();
                asm?.StringTable.Add(value);
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

        public PapyrusStringTableIndex AsTableIndex()
        {
            if (stringTableIndex != null) return stringTableIndex;

            var asm = GetAssembly();

            return asm.StringTable.Add(value);
        }

        public PapyrusStringTable GetStringTable() => GetAssembly()?.StringTable;

        public PapyrusAssemblyDefinition GetAssembly()
        {
            //var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            //var asm = PapyrusAssemblyDefinition.GetInternalInstance(threadId);
            return assembly;
        }

        public override string ToString()
        {
            return Value + " [" + Index + "]";
        }

        public static explicit operator string(PapyrusStringRef r)
        {
            return r?.Value;
        }
    }
}