using PapyrusDotNet.Common.Interfaces;

namespace PapyrusDotNet.Common.Utilities
{
    public interface IValueTypeConverter : IUtility
    {
        object Convert(string targetTypeName, object value);
    }
}