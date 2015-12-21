namespace PapyrusDotNet.Common.Interfaces
{
    public interface IValueTypeConverter : IUtility
    {
        /// <summary>
        /// Converts the value object into the specified target type
        /// </summary>
        /// <param name="typeName">Name of the target type.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        object Convert(string typeName, object value);
    }
}