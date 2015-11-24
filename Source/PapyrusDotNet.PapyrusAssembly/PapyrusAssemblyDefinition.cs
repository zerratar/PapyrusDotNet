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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using PapyrusDotNet.PapyrusAssembly.Classes;
using PapyrusDotNet.PapyrusAssembly.Enums;
using PapyrusDotNet.PapyrusAssembly.Implementations;
using PapyrusDotNet.PapyrusAssembly.Interfaces;

#endregion

namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusAssemblyDefinition
    {

        private PapyrusVersionTargets versionTarget;

        internal PapyrusAssemblyDefinition()
        {
        }

        internal PapyrusAssemblyDefinition(PapyrusVersionTargets versionTarget) : this()
        {
            this.versionTarget = versionTarget;
        }

        /// <summary>
        /// Gets or sets the version target.
        /// </summary>
        /// <value>
        /// The version target.
        /// </value>
        public PapyrusVersionTargets VersionTarget { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is corrupted.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is corrupted; otherwise, <c>false</c>.
        /// </value>
        public bool IsCorrupted { get; set; }

        /// <summary>
        /// Gets the header.
        /// </summary>
        /// <value>
        /// The header.
        /// </value>
        public PapyrusHeader Header { get; internal set; } = new PapyrusHeader();

        /// <summary>
        /// Gets or sets the debug information.
        /// </summary>
        /// <value>
        /// The debug information.
        /// </value>
        public PapyrusTypeDebugInfo DebugInfo { get; set; } = new PapyrusTypeDebugInfo();

        /// <summary>
        /// Gets or sets the types.
        /// </summary>
        /// <value>
        /// The types.
        /// </value>
        public Collection<PapyrusTypeDefinition> Types { get; set; } = new Collection<PapyrusTypeDefinition>();

        /// <summary>
        /// Gets or sets the string table.
        /// </summary>
        /// <value>
        /// The string table.
        /// </value>
        public List<string> StringTable { get; set; } = new List<string>();

        /// <summary>
        /// Creates the assembly.
        /// </summary>
        /// <param name="versionTarget">The version target.</param>
        /// <returns></returns>
        public static PapyrusAssemblyDefinition CreateAssembly(PapyrusVersionTargets versionTarget)
        {
            return new PapyrusAssemblyDefinition(versionTarget);
        }

        /// <summary>
        /// Loads the assembly.
        /// </summary>
        /// <param name="pexFile">The pex file.</param>
        /// <param name="throwsException">Whether or not to throw exceptions.</param>
        /// <returns></returns>
        public static PapyrusAssemblyDefinition LoadAssembly(string pexFile, bool throwsException)
        {
            using (var reader = new PapyrusAssemblyReader(pexFile, throwsException))
            {
                var def = reader.Read();
                def.IsCorrupted = reader.IsCorrupted;
                return def;
            }
        }

        /// <summary>
        /// Loads the assembly.
        /// </summary>
        /// <param name="pexFile">The pex file.</param>
        /// <returns></returns>
        public static PapyrusAssemblyDefinition LoadAssembly(string pexFile)
        {
            using (var reader = new PapyrusAssemblyReader(pexFile, false))
            {
                var def = reader.Read();
                def.IsCorrupted = reader.IsCorrupted;
                return def;
            }
        }

        /// <summary>
        /// Writes the specified output file. Overwrites if already exists.
        /// </summary>
        /// <param name="outputFile">The output file.</param>
        public void Write(string outputFile)
        {
            using (var writer = new PapyrusAssemblyWriter(this))
            {
                writer.Write(outputFile);
            }
        }
    }
}