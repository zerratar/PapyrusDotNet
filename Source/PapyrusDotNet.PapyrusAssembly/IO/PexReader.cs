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
using System.Linq;
using System.Text;
using PapyrusDotNet.PapyrusAssembly.Enums;

#endregion

namespace PapyrusDotNet.PapyrusAssembly.IO
{
    public class PexReader : BinaryReader
    {
        /// <summary>
        /// Gets or sets the string table.
        /// </summary>
        /// <value>
        /// The string table.
        /// </value>
        public List<string> StringTable { get; set; }

        private PapyrusVersionTargets papyrusVersionTarget;
        private readonly bool throwsExceptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="PexReader"/> class.
        /// </summary>
        /// <param name="inputPexFile">The input pex file.</param>
        /// <param name="throwsExceptions">Whether or not to throw unhandled exceptions.</param>
        public PexReader(string inputPexFile, bool throwsExceptions = false) : base(new MemoryStream(File.ReadAllBytes(inputPexFile)))
        {
            this.throwsExceptions = throwsExceptions;
        }

        private MemoryStream BaseMemoryStream => BaseStream as MemoryStream;

        /// <summary>
        /// Gets or sets the position of the stream.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
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

        /// <summary>
        /// Reads a 2-byte signed integer from the current stream and advances the current position of the stream by two bytes.
        /// </summary>
        /// <returns>
        /// A 2-byte signed integer read from the current stream.
        /// </returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception><exception cref="T:System.ObjectDisposedException">The stream is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><filterpriority>2</filterpriority>
        public override short ReadInt16()
        {
            if (papyrusVersionTarget == PapyrusVersionTargets.Fallout4)
                return BitConverter.ToInt16(new[] { ReadByte(), ReadByte() }, 0);
            return BitConverter.ToInt16(ReadBytesReversed(2), 0);
            // return base.ReadInt16();
        }

        public override int ReadInt32()
        {
            if (papyrusVersionTarget == PapyrusVersionTargets.Fallout4)
                return base.ReadInt32();
            return BitConverter.ToInt32(ReadBytesReversed(4), 0);
        }

        public override long ReadInt64()
        {
            if (papyrusVersionTarget == PapyrusVersionTargets.Fallout4)
                return base.ReadInt64();
            return BitConverter.ToInt64(ReadBytesReversed(8), 0);
        }

        private byte[] ReadBytesReversed(int count)
        {
            var bytes = ReadBytes(count).ToList();
            bytes.Reverse();
            return bytes.ToArray();
        }

        public bool DEBUGGING;
        /// <summary>
        /// Reads a string from the current stream. The string is prefixed with the length, encoded as an integer seven bits at a time.
        /// ... Add more specific comments later regarding what is actually happening ...
        /// </summary>
        /// <returns>
        /// The string being read.
        /// </returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception><exception cref="T:System.ObjectDisposedException">The stream is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><filterpriority>2</filterpriority>
        public override string ReadString()
        {
            var outputString = "";

            if (DEBUGGING)
            {
                // var manyDummies = ReadChars(100);
                //var dummy = ReadBytesReversed(2);
                //var dummy2 = BitConverter.ToInt16(dummy, 0);
            }

            if (StringTable != null && UseStringTable)
            {
                //int index = papyrusVersionTarget == PapyrusVersionTargets.Fallout4
                //    ? ReadInt16()
                //    : BitConverter.ToInt16(ReadBytes(2), 0);




                var index = ReadInt16();

                if (index > StringTable.Count || index < 0)
                {
                    if (throwsExceptions)
                        throw new IndexOutOfRangeException("The index read from the stream was not within the bounds of the string table.");

                    index = (short)(StringTable.Count - 1);
                    IsCorrupted = true;
                }
                return StringTable[index];
            }

            var size = ReadInt16();
            //papyrusVersionTarget == PapyrusVersionTargets.Fallout4
            //? ReadInt16()
            //: ReadByte();

            for (var i = 0; i < size; i++)
            {
                outputString += ReadChar();
                //if (papyrusVersionTarget == PapyrusVersionTargets.Skyrim)
                //{
                //    var next = PeekChar();
                //    if (next == '\0')
                //    {
                //        ReadByte();
                //        break;
                //    }
                //}
            }
            return outputString;
            // return base.ReadString();
        }

        public bool IsCorrupted { get; set; }

        /// <summary>
        /// Sets the string table.
        /// </summary>
        /// <param name="stringTable">The string table.</param>
        public void SetStringTable(List<string> stringTable)
        {
            this.StringTable = stringTable;
        }

        /// <summary>
        /// Sets the version target.
        /// </summary>
        /// <param name="papyrusVersionTarget">The papyrus version target.</param>
        public void SetVersionTarget(PapyrusVersionTargets papyrusVersionTarget)
        {
            this.papyrusVersionTarget = papyrusVersionTarget;
        }
    }
}