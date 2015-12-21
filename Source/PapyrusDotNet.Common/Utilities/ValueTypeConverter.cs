using System;

namespace PapyrusDotNet.Common.Utilities
{
    public class ValueTypeConverter : IValueTypeConverter
    {
        private static ValueTypeConverter instance;

        public static ValueTypeConverter Instance => instance ?? new ValueTypeConverter();

        public object Convert(string targetTypeName, object value)
        {
            if (targetTypeName.ToLower().StartsWith("bool") || targetTypeName.ToLower().StartsWith("system.bool"))
            {
                if (value is int || value is float || value is short || value is double || value is long || value is byte)
                    return (int)Double.Parse(value.ToString()) == 1;
                if (value is bool) return (bool)value;
                if (value is string) return (string)value == "1" || value.ToString().ToLower() == "true";
            }
            if (targetTypeName.ToLower().StartsWith("string") || targetTypeName.ToLower().StartsWith("system.string"))
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

            if (targetTypeName.ToLower().StartsWith("int"))
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