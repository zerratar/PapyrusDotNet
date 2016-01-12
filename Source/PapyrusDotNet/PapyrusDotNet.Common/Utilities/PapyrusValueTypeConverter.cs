//     This file is part of PapyrusDotNet.
// 
//     PapyrusDotNet is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     PapyrusDotNet is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with PapyrusDotNet.  If not, see <http://www.gnu.org/licenses/>.
//  
//     Copyright 2016, Karl Patrik Johansson, zerratar@gmail.com

#region

using PapyrusDotNet.Common.Interfaces;

#endregion

namespace PapyrusDotNet.Common.Utilities
{
    public class PapyrusValueTypeConverter : IValueTypeConverter
    {
        /// <summary>
        ///     Converts the value object into a papyrus friendly type
        /// </summary>
        /// <param name="typeName">Name of the target type.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public object Convert(string typeName, object value)
        {
            if (typeName.ToLower().StartsWith("bool") || typeName.ToLower().StartsWith("system.bool"))
            {
                if (value is int || value is float || value is short || value is double || value is long ||
                    value is byte)
                    return (int) double.Parse(value.ToString()) == 1;
                if (value is bool) return (bool) value;
                if (value is string) return (string) value == "1" || value.ToString().ToLower() == "true";
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
                if (value is int || value is float || value is short || value is double || value is long ||
                    value is byte)
                {
                    return int.Parse(value.ToString());
                }
            }

            return value;
        }
    }
}