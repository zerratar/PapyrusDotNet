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
using System.Collections.ObjectModel;
using System.IO;
using PapyrusDotNet.PapyrusAssembly.Implementations;

#endregion

namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusAssemblyDefinition : IDisposable
    {
        private readonly bool disposed = false;
        private string filePath;
        private bool hasDebugInfo;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PapyrusAssemblyDefinition" /> class.
        /// </summary>
        internal PapyrusAssemblyDefinition()
        {
            Header = new PapyrusHeader(this);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PapyrusAssemblyDefinition" /> class.
        /// </summary>
        /// <param name="versionTarget">The version target.</param>
        internal PapyrusAssemblyDefinition(PapyrusVersionTargets versionTarget) : this()
        {
            VersionTarget = versionTarget;
        }

        /// <summary>
        ///     Gets or sets the version target.
        /// </summary>
        public PapyrusVersionTargets VersionTarget { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is corrupted.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is corrupted; otherwise, <c>false</c>.
        /// </value>
        public bool IsCorrupted { get; set; }

        /// <summary>
        ///     Gets the header.
        /// </summary>
        /// <value>
        ///     The header.
        /// </value>
        public PapyrusHeader Header { get; internal set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance has debug information.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has debug information; otherwise, <c>false</c>.
        /// </value>
        public bool HasDebugInfo
        {
            get { return hasDebugInfo || DebugInfo != null && DebugInfo.DebugTime > 0; }
            set { hasDebugInfo = value; }
        }

        /// <summary>
        ///     Gets or sets the debug information.
        /// </summary>
        /// <value>
        ///     The debug information.
        /// </value>
        public PapyrusTypeDebugInfo DebugInfo { get; set; } = new PapyrusTypeDebugInfo();

        /// <summary>
        ///     Gets or sets the types.
        /// </summary>
        /// <value>
        ///     The types.
        /// </value>
        public Collection<PapyrusTypeDefinition> Types { get; set; } = new Collection<PapyrusTypeDefinition>();

        /// <summary>
        /// Gets or sets the string table.
        /// </summary>
        public PapyrusStringTable StringTable { get; set; } = new PapyrusStringTable();

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        ///     Creates the string reference.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public PapyrusStringRef CreateStringRef(string value)
        {
            return new PapyrusStringRef(this, value);
        }

        /// <summary>
        ///     Creates the index of the string table.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public PapyrusStringTableIndex CreateStringTableIndex(string value)
        {
            return StringTable.Add(value);
        }

#if MERGE_IMPLEMENTED
        /// <summary>
        /// Merges the other assembly with this one to form a new one.
        /// </summary>
        /// <param name="otherAssembly">The other assembly.</param>
        /// <returns></returns>
        public PapyrusAssemblyDefinition Merge(PapyrusAssemblyDefinition otherAssembly)
        {
            return new PapyrusAssemblyMerger(this, otherAssembly).Merge();
        }
#endif

        /// <summary>
        ///     Creates the assembly.
        /// </summary>
        /// <param name="versionTarget">The version target.</param>
        /// <returns></returns>
        public static PapyrusAssemblyDefinition CreateAssembly(PapyrusVersionTargets versionTarget)
        {
            return new PapyrusAssemblyDefinition(versionTarget);
        }

        /// <summary>
        /// Reads the papyrus assembly.
        /// </summary>
        /// <param name="pexFile">The pex file.</param>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        public static PapyrusAssemblyDefinition ReadAssembly(string pexFile, PapyrusReaderSettings settings)
        {
            // bool throwsException
            using (var reader = new PapyrusAssemblyReader(new PapyrusAssemblyDefinition(), pexFile, settings))
            {
                var def = reader.Read();
                def.filePath = pexFile;
                def.IsCorrupted = reader.IsCorrupted;
                return def;
            }
        }

        /// <summary>
        ///     Reads the papyrus assembly.
        /// </summary>
        /// <param name="pexFile">The pex file.</param>
        /// <returns></returns>
        public static PapyrusAssemblyDefinition ReadAssembly(string pexFile)
        {
            var asm = new PapyrusAssemblyDefinition();
            asm.filePath = pexFile;
            using (var reader = new PapyrusAssemblyReader(asm, pexFile, PapyrusReaderSettings.Default))
            {
                var def = reader.Read();
                def.IsCorrupted = reader.IsCorrupted;
                return def;
            }
        }

        /// <summary>
        ///     Writes the specified output file. Overwrites if already exists.
        /// </summary>
        /// <param name="outputFile">The output file.</param>
        public void Write(string outputFile)
        {
            using (var writer = new PapyrusAssemblyWriter(this))
            {
                writer.Write(outputFile);
            }
        }

        /// <summary>
        ///     Overwrites the loaded papyrus binary with any modifications made.
        /// </summary>
        public void Write()
        {
            using (var writer = new PapyrusAssemblyWriter(this))
            {
                writer.Write(filePath);
            }
        }

        /// <summary>
        ///     Creates a backup of the original papyrus assembly. If this is a new instance, nothing will happen.
        /// </summary>
        /// <returns>True if a backup was made; otherwise false.</returns>
        public bool Backup()
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            if (!File.Exists(filePath)) return false;

            var destination = filePath + ".bak";
            var index = 0;
            while (File.Exists(destination)) destination = filePath + ".bak" + ++index;

            File.Copy(filePath, destination, true);
            return true;
        }

        /// <summary>
        ///     Reloads the papyrus assembly specified.
        /// </summary>
        /// <param name="definitionToReload">The definition to reload.</param>
        /// <returns></returns>
        public static PapyrusAssemblyDefinition ReloadAssembly(PapyrusAssemblyDefinition definitionToReload)
        {
            return ReadAssembly(definitionToReload.filePath, PapyrusReaderSettings.Default);
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
            }
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="PapyrusAssemblyDefinition" /> class.
        /// </summary>
        ~PapyrusAssemblyDefinition()
        {
            Dispose(false);
        }
    }
}