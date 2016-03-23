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
    public class PapyrusHeader
    {
        private readonly PapyrusAssemblyDefinition assembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="PapyrusHeader"/> class.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public PapyrusHeader(PapyrusAssemblyDefinition assembly)
        {
            this.assembly = assembly;
            UserflagReferenceHeader = new PapyrusHeaderUserflagCollection(assembly);
            SourceHeader = new PapyrusSourceHeader();
        }

        /// <summary>
        /// The fallout4 papyrus header identifier
        /// </summary>
        public const uint Fallout4PapyrusHeaderIdentifier = 0xFA57C0DE;

        /// <summary>
        /// The skyrim papyrus header identifier
        /// </summary>
        public const uint SkyrimPapyrusHeaderIdentifier = 0xDEC057FA;

        /// <summary>
        /// The skyrim papyrus version
        /// </summary>
        public static readonly Version SkyrimPapyrusVersion = new Version(3, 2);

        /// <summary>
        /// The fallout4 papyrus version
        /// </summary>
        public static readonly Version Fallout4PapyrusVersion = new Version(3, 9);

        /// <summary>
        /// Gets the source header.
        /// </summary>
        public PapyrusSourceHeader SourceHeader { get; internal set; }

        /// <summary>
        /// Gets the userflag reference header.
        /// </summary>
        public PapyrusHeaderUserflagCollection UserflagReferenceHeader { get; }

        /// <summary>
        /// Gets or sets the header identifier.
        /// </summary>
        public uint HeaderIdentifier { get; set; }
    }
}