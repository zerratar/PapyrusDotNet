using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.PexInspector.Converters
{
    public class PapyrusArrayDisplayConverter : IValueConverter
    {
        public static SolidColorBrush StringColor = new SolidColorBrush(Color.FromRgb(174, 107, 82));
        public static SolidColorBrush ConstantColor = new SolidColorBrush(Color.FromRgb(83, 87, 55));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var inlines = new ObservableCollection<Inline>();
            var array = value as IEnumerable;
            if (array == null) return inlines;
            var items = array.Cast<object>().ToArray();
            var i = 0;
            foreach (var obj in items)
            {
                if (obj != null)
                {
                    var variable =
                        obj as PapyrusVariableReference;
                    if (variable != null)
                    {
                        if (variable.Value == null)
                            inlines.Add(new Run("NONE"));
                        else if (variable.ValueType == PapyrusPrimitiveType.String)
                            inlines.Add(new Run("\"" + variable.Value + "\"") { Foreground = StringColor });
                        else if (variable.ValueType == PapyrusPrimitiveType.Reference)
                            inlines.Add(new Run(variable.Value.ToString()));
                        else
                            inlines.Add(new Run(variable.Value.ToString().Replace(',', '.'))
                            {
                                //Foreground = ConstantColor
                            });
                    }
                    else
                        inlines.Add(new Run(obj.ToString()));
                }
                else
                    inlines.Add(new Run("NONE"));
                i++;
                if (i < items.Length)
                    inlines.Add(new Run(", "));
            }
            return inlines;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}