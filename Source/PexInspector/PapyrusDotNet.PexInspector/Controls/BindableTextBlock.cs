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

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

#endregion

namespace PapyrusDotNet.PexInspector.Controls
{
    public class BindableTextBlock : TextBlock
    {
        public static readonly DependencyProperty InlineListProperty =
            DependencyProperty.Register("InlineList", typeof (ObservableCollection<Inline>), typeof (BindableTextBlock),
                new UIPropertyMetadata(null, OnPropertyChanged));

        public ObservableCollection<Inline> InlineList
        {
            get { return (ObservableCollection<Inline>) GetValue(InlineListProperty); }
            set { SetValue(InlineListProperty, value); }
        }

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = (BindableTextBlock) sender;
            textBlock.Inlines.Clear();
            textBlock.Inlines.AddRange((ObservableCollection<Inline>) e.NewValue);
        }
    }
}