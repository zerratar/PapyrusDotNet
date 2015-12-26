using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.PexInspector.Converters
{
    public class PapyrusArrayDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var array = value as IEnumerable;
            if (array != null)
            {
                var items =
                array.Cast<object>();

                var display = new List<string>();

                foreach (var obj in items)
                {
                    var variable =
                    obj as PapyrusVariableReference;
                    if (variable != null)
                    {
                        if (variable.Value == null)
                            display.Add("NONE");
                        else
                            display.Add(variable.Value.ToString());
                    }
                    else
                    {
                        display.Add(obj.ToString());
                    }
                }

                return string.Join(", ", display.ToArray());
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}