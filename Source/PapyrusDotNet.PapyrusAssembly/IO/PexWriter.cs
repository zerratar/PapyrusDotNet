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
using System.IO;
using System.Linq;

#endregion

namespace PapyrusDotNet.PapyrusAssembly.IO
{
    internal class PexWriter : BinaryWriter
    {
        private readonly PapyrusAssemblyDefinition assembly;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PexWriter" /> class.
        /// </summary>
        /// <param name="asm">The asm.</param>
        /// <param name="output">The output.</param>
        public PexWriter(PapyrusAssemblyDefinition asm, Stream output) : base(output)
        {
            assembly = asm;
        }

        public bool UseStringTable { get; set; }

        public void WriteReversedBytes(byte[] bytes, int offset, int count)
        {
            var b = bytes.ToList();
            b.Reverse();
            Write(b.ToArray(), offset, count);
        }

        public void WriteReversedBytes(byte[] bytes)
        {
            var b = bytes.ToList();
            b.Reverse();
            Write(b.ToArray(), 0, b.Count);
        }

        public override void Write(bool s)
        {
            base.Write(s ? (byte) 1 : (byte) 0);
        }

        public void Write(PapyrusStringRef value)
        {
            if (assembly.StringTable == null)
            {
                assembly.StringTable = new List<string>();
            }
            if (!assembly.StringTable.Contains(value.Value))
            {
                assembly.StringTable.Add(value.Value);
            }
            Write(value.Value);
        }

        public override void Write(string value)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (value == null)
                value = string.Empty;

            if (UseStringTable)
            {
                if (assembly.StringTable == null) throw new NullReferenceException(nameof(assembly.StringTable));
                Write((short) assembly.StringTable.IndexOf(value));
                return;
            }

            if (assembly.VersionTarget == PapyrusVersionTargets.Fallout4)
            {
                base.Write((short) value.Length);
                base.Write(value.ToCharArray());
            }
            else
            {
                WriteReversedBytes(BitConverter.GetBytes((short) value.Length));
                base.Write(value.ToCharArray());
            }
        }

        public override void Write(short s)
        {
            if (assembly.VersionTarget == PapyrusVersionTargets.Fallout4)
                base.Write(s);
            else
                WriteReversedBytes(BitConverter.GetBytes(s));
        }

        public override void Write(int s)
        {
            if (assembly.VersionTarget == PapyrusVersionTargets.Fallout4)
            {
                var buffer = BitConverter.GetBytes(s);
                base.Write(buffer);
            }
            else
                WriteReversedBytes(BitConverter.GetBytes(s));
        }

        public override void Write(uint s)
        {
            if (assembly.VersionTarget == PapyrusVersionTargets.Fallout4)
                base.Write(s);
            else
                WriteReversedBytes(BitConverter.GetBytes(s));
        }

        public override void Write(long s)
        {
            if (assembly.VersionTarget == PapyrusVersionTargets.Fallout4)
            {
                var buffer = BitConverter.GetBytes(s);
                base.Write(buffer);
            }
            else
                WriteReversedBytes(BitConverter.GetBytes(s));
        }

        public override void Write(float s)
        {
            if (assembly.VersionTarget == PapyrusVersionTargets.Fallout4)
                base.Write(s);
            else
                WriteReversedBytes(BitConverter.GetBytes(s));
        }

        public override void Write(double s)
        {
            if (assembly.VersionTarget == PapyrusVersionTargets.Fallout4)
                base.Write(s);
            else
                WriteReversedBytes(BitConverter.GetBytes(s));
        }
    }
}