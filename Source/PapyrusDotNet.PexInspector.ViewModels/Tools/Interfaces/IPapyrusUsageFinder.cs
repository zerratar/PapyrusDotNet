using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PapyrusDotNet.PexInspector.ViewModels.Tools
{
    public interface IPapyrusUsageFinder : IPapyrusItemFinder
    {
        /// <summary>
        /// Finds the method usage.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        IFindResult FindMethodUsage(string methodName);

        /// <summary>
        /// Finds the property usage.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IFindResult FindPropertyUsage(string propertyName);

        /// <summary>
        /// Finds the field usage.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        IFindResult FindFieldUsage(string fieldName);

        ///// <summary>
        ///// Finds the type usage.
        ///// </summary>
        ///// <param name="typeName">Name of the type.</param>
        ///// <returns></returns>
        //IFindUsageResult FindTypeUsage(string typeName);
    }
}
