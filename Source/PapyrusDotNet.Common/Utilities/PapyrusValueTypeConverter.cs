using System;
using PapyrusDotNet.Common.Interfaces;

namespace PapyrusDotNet.Common.Utilities
{
    public class PapyrusValueTypeConverter : IValueTypeConverter
    {
        /// <summary>
        /// Converts the value object into a papyrus friendly type
        /// </summary>
        /// <param name="typeName">Name of the target type.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public object Convert(string typeName, object value)
        {
            if (typeName.ToLower().StartsWith("bool") || typeName.ToLower().StartsWith("system.bool"))
            {
                if (value is int || value is float || value is short || value is double || value is long || value is byte)
                    return (int)Double.Parse(value.ToString()) == 1;
                if (value is bool) return (bool)value;
                if (value is string) return (string)value == "1" || value.ToString().ToLower() == "true";
            }
            if (typeName.ToLower().StartsWith("string") || typeName.ToLower().StartsWith("system.string"))
            {
                if (!value.ToString().Contains("\"")) return "\"" + value + "\"";
            }
            else if (value is float || value is decimal || value is double)
            {
                if (value.ToString().Contains(","))
                {
                    return value.ToString().Replace(',', '.');
                }
            }

            if (typeName.ToLower().StartsWith("int"))
            {
                if (value is int || value is float || value is short || value is double || value is long || value is byte)
                {
                    return Int32.Parse(value.ToString());
                }
            }

            return value;
        }
    }
}