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

#endregion

namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusSourceHeader
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PapyrusSourceHeader" /> class.
        /// </summary>
        public PapyrusSourceHeader()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PapyrusSourceHeader" /> struct.
        /// </summary>
        /// <param name="majorVersion">The major version.</param>
        /// <param name="minorVersion">The minor version.</param>
        /// <param name="gameId">The game identifier.</param>
        /// <param name="compileTime">The compile time.</param>
        /// <param name="source">The source.</param>
        /// <param name="user">The user.</param>
        /// <param name="computer">The computer.</param>
        public PapyrusSourceHeader(byte majorVersion, byte minorVersion, short gameId, long compileTime, string source,
            string user, string computer)
        {
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            GameId = gameId;
            CompileTime = compileTime;
            Source = source;
            User = user;
            Computer = computer;
        }

        /// <summary>
        ///     The source
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     The modify time
        /// </summary>
        public long ModifyTime { get; set; }

        /// <summary>
        ///     The compile time
        /// </summary>
        public long CompileTime { get; set; }

        /// <summary>
        ///     The user
        /// </summary>
        public string User { get; set; }

        /// <summary>
        ///     The computer
        /// </summary>
        public string Computer { get; set; }

        /// <summary>
        ///     The major version
        /// </summary>
        public byte MajorVersion { get; set; }

        /// <summary>
        ///     The minor version
        /// </summary>
        public byte MinorVersion { get; set; }

        /// <summary>
        ///     Gets or sets the version.
        /// </summary>
        /// <value>
        ///     The version.
        /// </value>
        public Version Version
        {
            get { return new Version(MajorVersion, MinorVersion); }
            set
            {
                if (value == null) return;
                MajorVersion = (byte) value.Major;
                MinorVersion = (byte) value.Minor;
            }
        }

        /// <summary>
        ///     The game identifier
        /// </summary>
        public short GameId { get; set; }
    }
}