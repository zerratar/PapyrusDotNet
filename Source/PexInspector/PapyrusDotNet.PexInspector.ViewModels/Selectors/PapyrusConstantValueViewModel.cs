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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using PapyrusDotNet.PexInspector.ViewModels.Implementations;

#endregion

namespace PapyrusDotNet.PexInspector.ViewModels.Selectors
{
    public class PapyrusConstantValueViewModel : ViewModelBase
    {
        public static PapyrusConstantValueViewModel DesignInstance = designInstance ??
                                                                     (designInstance =
                                                                         new PapyrusConstantValueViewModel(null));

        private static PapyrusConstantValueViewModel designInstance;
        private readonly OpCodeArgumentDescription desc;
        private ObservableCollection<FrameworkElement> comboBoxItems;
        private object selectedValue;
        private ComboBoxItem selectedValueType;

        private Visibility valueInputVisibility;

        public PapyrusConstantValueViewModel(OpCodeArgumentDescription desc)
        {
            this.desc = desc;
            ValueInputVisibility = Visibility.Visible;

            ComboBoxItems = new ObservableCollection<FrameworkElement>(CreateComboBoxItems());
            SelectedValueType = ComboBoxItems.First() as ComboBoxItem;
        }

        public ObservableCollection<FrameworkElement> ComboBoxItems
        {
            get { return comboBoxItems; }
            set { Set(ref comboBoxItems, value); }
        }

        public object SelectedValue
        {
            get { return selectedValue; }
            set { Set(ref selectedValue, value); }
        }


        public ComboBoxItem SelectedValueType
        {
            get { return selectedValueType; }
            set
            {
                if (Set(ref selectedValueType, value))
                {
                    if (value.Content.ToString().ToLower() == "none")
                    {
                        ValueInputVisibility = Visibility.Collapsed;
                        SelectedValue = null;
                    }
                    else
                    {
                        ValueInputVisibility = Visibility.Visible;
                    }
                }
            }
        }

        public Visibility ValueInputVisibility
        {
            get { return valueInputVisibility; }
            set { Set(ref valueInputVisibility, value); }
        }

        private List<FrameworkElement> CreateComboBoxItems()
        {
            var elements = new List<FrameworkElement>();

            if (desc != null && desc.Constraints.Length > 0)
            {
                if (desc.Constraints.Contains(OpCodeConstraint.None))
                    elements.Add(new ComboBoxItem {Content = "None"});
                if (desc.Constraints.Contains(OpCodeConstraint.Integer))
                    elements.Add(new ComboBoxItem {Content = "Integer"});
                if (desc.Constraints.Contains(OpCodeConstraint.Float))
                    elements.Add(new ComboBoxItem {Content = "Float"});
                if (desc.Constraints.Contains(OpCodeConstraint.Boolean))
                    elements.Add(new ComboBoxItem {Content = "Boolean"});
                if (desc.Constraints.Contains(OpCodeConstraint.String))
                    elements.Add(new ComboBoxItem {Content = "String"});
                return elements;
            }
            elements.Add(new ComboBoxItem {Content = "None"});
            elements.Add(new ComboBoxItem {Content = "Integer"});
            elements.Add(new ComboBoxItem {Content = "Float"});
            elements.Add(new ComboBoxItem {Content = "Boolean"});
            elements.Add(new ComboBoxItem {Content = "String"});
            return elements;
        }
    }
}