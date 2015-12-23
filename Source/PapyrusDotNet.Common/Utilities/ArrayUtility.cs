using PapyrusDotNet.Common.Interfaces;

namespace PapyrusDotNet.Common.Utilities
{
    public class ArrayUtility : IUtility
    {
        /// <summary>
        /// Creates an array using the parameters passed to this method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static T[] ArrayOf<T>(params T[] items) => items;
    }
}