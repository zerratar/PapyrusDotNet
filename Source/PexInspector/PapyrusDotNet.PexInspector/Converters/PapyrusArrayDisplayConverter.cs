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

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using PapyrusDotNet.PapyrusAssembly;

#endregion

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
                            inlines.Add(new Run("None"));
                        else if (variable.Type == PapyrusPrimitiveType.String)
                            inlines.Add(new Run("\"" + variable.Value + "\"") {Foreground = StringColor});
                        else if (variable.Type == PapyrusPrimitiveType.Reference)
                            inlines.Add(new Run(variable.Value.ToString()));
                        else
                            inlines.Add(new Run(variable.Value.ToString().Replace(',', '.')));
                    }
                    else
                        inlines.Add(new Run(obj.ToString()));
                }
                else
                    inlines.Add(new Run("None"));
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