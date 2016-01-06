namespace PapyrusDotNet.PexInspector.ViewModels.Tools
{
    public interface IPapyrusReferenceFinder : IPapyrusItemFinder
    {
        /// <summary>
        /// Finds the type references.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        IFindResult FindTypeReference(string typeName);
    }
}