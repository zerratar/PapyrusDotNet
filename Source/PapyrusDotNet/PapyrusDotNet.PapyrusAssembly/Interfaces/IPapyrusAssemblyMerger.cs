namespace PapyrusDotNet.PapyrusAssembly.Interfaces
{
    public interface IPapyrusAssemblyMerger
    {
        /// <summary>
        /// Merges two instances of PapyrusAssemblyDefinitions to form a new one.
        /// </summary>
        /// <returns></returns>
        PapyrusAssemblyDefinition Merge(PapyrusMergerSettings settings);
    }
}