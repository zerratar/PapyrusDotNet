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

using System;
using System.Linq;
using PapyrusDotNet.PapyrusAssembly.Extensions;

#endregion

namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusPropertyDefinition : PapyrusPropertyReference
    {
        public readonly PapyrusAssemblyDefinition DeclaringAssembly;

        public PapyrusPropertyDefinition(PapyrusAssemblyDefinition declaringAssembly)
        {
            DeclaringAssembly = declaringAssembly;
        }

        public PapyrusPropertyDefinition(PapyrusAssemblyDefinition declaringAssembly, string name, string typeName)
            : base(new PapyrusStringRef(declaringAssembly, name), null, PapyrusPrimitiveType.None)
        {
            DeclaringAssembly = declaringAssembly;

            TypeName = typeName.Ref(declaringAssembly);

            DeclaringType = DeclaringAssembly.Types.FirstOrDefault();
        }

        public PapyrusTypeDefinition DeclaringType { get; }

        public PapyrusStringRef TypeName { get; set; }
        public PapyrusStringRef Documentation { get; set; }
        public int Userflags { get; set; }
        public byte Flags { get; set; }

        public bool IsAuto
        {
            get { return (Flags & (byte) PapyrusPropertyFlags.IsAuto) > 0; }
            set
            {
                if (value)
                {
                    SetFlags(GetFlags() | PapyrusPropertyFlags.IsAuto);
                }
                else
                {
                    SetFlags(GetFlags() & ~PapyrusPropertyFlags.IsAuto);
                }
            }
        }

        public bool HasGetter
        {
            get { return (Flags & (byte) PapyrusPropertyFlags.HasGetter) > 0; }
            set
            {
                if (value)
                {
                    SetFlags(GetFlags() | PapyrusPropertyFlags.HasGetter);
                }
                else
                {
                    SetFlags(GetFlags() & ~PapyrusPropertyFlags.HasGetter);
                }
            }
        }

        public bool HasSetter
        {
            get { return (Flags & (byte) PapyrusPropertyFlags.HasSetter) > 0; }
            set
            {
                if (value)
                {
                    SetFlags(GetFlags() | PapyrusPropertyFlags.HasSetter);
                }
                else
                {
                    SetFlags(GetFlags() & ~PapyrusPropertyFlags.HasSetter);
                }
            }
        }


        public string AutoName { get; set; }
        public PapyrusMethodDefinition GetMethod { get; set; }
        public PapyrusMethodDefinition SetMethod { get; set; }

        public void SetFlags(PapyrusPropertyFlags flags)
        {
            Flags = (byte) flags;
        }

        public PapyrusPropertyFlags GetFlags()
        {
            return (PapyrusPropertyFlags) Flags;
        }
    }

    [Flags]
    public enum PapyrusPropertyFlags : byte
    {
        IsAuto = 4,
        HasGetter = 1,
        HasSetter = 2
    }
}