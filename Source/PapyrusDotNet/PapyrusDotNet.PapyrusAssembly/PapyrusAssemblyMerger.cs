using System;
using PapyrusDotNet.PapyrusAssembly.Interfaces;

namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusAssemblyMerger : IPapyrusAssemblyMerger
    {
        private readonly PapyrusAssemblyDefinition source;
        private readonly PapyrusAssemblyDefinition otherAssembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="PapyrusAssemblyMerger" /> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="otherAssembly">The other assembly.</param>
        public PapyrusAssemblyMerger(PapyrusAssemblyDefinition source, PapyrusAssemblyDefinition otherAssembly)
        {
            this.source = source;
            this.otherAssembly = otherAssembly;
        }

        /// <summary>
        /// Merges two instances of PapyrusAssemblyDefinitions to form a new one.
        /// </summary>
        /// <returns></returns>
        public PapyrusAssemblyDefinition Merge()
        {
            return Merge(PapyrusMergerSettings.Default);
        }

        /// <summary>
        /// Merges two instances of PapyrusAssemblyDefinitions to form a new one.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        public PapyrusAssemblyDefinition Merge(PapyrusMergerSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}