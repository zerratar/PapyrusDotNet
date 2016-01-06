namespace PapyrusDotNet.PexInspector.ViewModels.Tools
{
    public interface IPapyrusLocationFinder : IPapyrusItemFinder
    {
        /// <summary>
        /// Finds the method location.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        IFindResult FindMethodLocation(string methodName);

        /// <summary>
        /// Finds the property location.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IFindResult FindPropertyLocation(string propertyName);

        /// <summary>
        /// Finds the field location.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        IFindResult FindFieldLocation(string fieldName);
    }
}