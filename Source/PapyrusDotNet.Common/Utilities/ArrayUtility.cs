using PapyrusDotNet.Common.Interfaces;

namespace PapyrusDotNet.Common.Utilities
{
    public class ArrayUtility : IUtility
    {
        public static T[] ArrayOf<T>(params T[] items)
        {
            return items;
        }
    }
}