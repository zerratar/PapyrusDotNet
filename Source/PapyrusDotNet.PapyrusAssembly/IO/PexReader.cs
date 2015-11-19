#region License

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

#endregion

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PapyrusDotNet.PapyrusAssembly.Enums;

#endregion

namespace PapyrusDotNet.PapyrusAssembly.IO
{
    public class PexReader : BinaryReader
    {
        public List<string> StringTable = null;
        private PapyrusVersionTargets papyrusVersionTarget;

        public PexReader(string inputPexFile) : base(new MemoryStream(File.ReadAllBytes(inputPexFile)))
        {
        }

        private MemoryStream BaseMemoryStream => BaseStream as MemoryStream;

        public long Position
        {
            get { return BaseMemoryStream.Position; }
            set { BaseMemoryStream.Position = value; }
        }

        public long Available => BaseMemoryStream.Length - Position;

        /// <summary>
        /// Gets or sets whether to use the String Table to read strings. If no string table is available, it will read from the current position of the stream.  
        /// </summary>
        public bool UseStringTable { get; set; } = true;

        public override short ReadInt16()
        {
            // return base.ReadInt16();
            var a = ReadByte();
            // if (size != 0)
            var b = ReadByte(); // Dummy
            return BitConverter.ToInt16(new byte[] { a, b }, 0);
        }

        public override string ReadString()
        {
            var outputString = "";
            if (StringTable != null)
            {
                int index = papyrusVersionTarget == PapyrusVersionTargets.Fallout4
                    ? ReadInt16()
                    : ReadByte();

                if (index > StringTable.Count) throw new IndexOutOfRangeException("The index read from the stream was not within the bound sof the string table.");
                return StringTable[index];
            }

            var size = papyrusVersionTarget == PapyrusVersionTargets.Fallout4
                ? ReadInt16()
                : ReadByte();

            for (var i = 0; i < size; i++)
            {
                outputString += ReadChar();
                if (papyrusVersionTarget == PapyrusVersionTargets.Skyrim)
                {
                    var next = PeekChar();
                    if (next == '\0')
                    {
                        ReadByte();
                        break;
                    }
                }
            }
            return outputString;
            // return base.ReadString();
        }

        public void SetStringTable(List<string> stringTable)
        {
            this.StringTable = stringTable;
        }

        public void SetVersionTarget(PapyrusVersionTargets papyrusVersionTarget)
        {
            this.papyrusVersionTarget = papyrusVersionTarget;
        }
    }
}