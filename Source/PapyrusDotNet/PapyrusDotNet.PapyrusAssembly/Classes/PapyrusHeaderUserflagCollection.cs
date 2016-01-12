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

using System.Collections.Generic;
using PapyrusDotNet.PapyrusAssembly.Extensions;

#endregion

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
                //asm.StringTable = new List<string>();
                asm.StringTable = new PapyrusStringTable();
            }

            asm.StringTable.Add(key.Value);

            //if (!asm.StringTable.Contains(key.Value))
            //{
            //    asm.StringTable.Add(key.Value);
            //}

            base.Add(key, value);
        }

        public new void Remove(PapyrusStringRef key)
        {
            base.Remove(key);
        }
    }
}