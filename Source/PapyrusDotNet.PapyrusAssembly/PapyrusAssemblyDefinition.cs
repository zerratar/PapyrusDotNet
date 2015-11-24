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
        private readonly IPapyrusAssemblyWriter writer;

        private PapyrusVersionTargets versionTarget;

        internal PapyrusAssemblyDefinition()
        {
            writer = new PapyrusAssemblyWriter();
        }

        internal PapyrusAssemblyDefinition(PapyrusVersionTargets versionTarget) : this()
        {
            this.versionTarget = versionTarget;
        }

        /// <summary>
        /// Gets the header.
        /// </summary>
        /// <value>
        /// The header.
        /// </value>
        public PapyrusHeader Header { get; internal set; }

        /// <summary>
        /// Gets or sets the debug information.
        /// </summary>
        /// <value>
        /// The debug information.
        /// </value>
        public PapyrusTypeDebugInfo DebugInfo { get; set; }

        /// <summary>
        /// Gets or sets the types.
        /// </summary>
        /// <value>
        /// The types.
        /// </value>
        public Collection<PapyrusTypeDefinition> Types { get; set; }

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

        public bool IsCorrupted { get; set; }

        /// <summary>
        /// Writes the specified output file.
        /// </summary>
        /// <param name="outputFile">The output file.</param>
        public void Write(string outputFile)
        {
            writer.Write(outputFile);
        }
    }
}