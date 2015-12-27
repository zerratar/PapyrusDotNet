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
using System.Collections.Generic;

#endregion

namespace PapyrusDotNet.PapyrusAssembly
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

        public bool IsEvent { get; set; }

        internal int DelegateInvokeCount { get; set; }

        public PapyrusStateDefinition DeclaringState { get; set; }

//=> (Flags & 4) > 0;

        public void SetFlags(PapyrusMethodFlags flags)
        {
            Flags = (byte)flags;
        }

        public PapyrusMethodFlags GetFlags()
        {
            return (PapyrusMethodFlags)Flags;
        }

        public List<PapyrusVariableReference> GetVariables()
        {
            var vars = Body.Variables;
            var tempVars = Body.TempVariables;
            var output = new List<PapyrusVariableReference>();
            output.AddRange(vars);
            output.AddRange(tempVars);

            return output; // We only want a readonly list
        }
    }

    [Flags]
    public enum PapyrusMethodFlags : byte
    {
        Global = 1,
        Native = 2
    }
}