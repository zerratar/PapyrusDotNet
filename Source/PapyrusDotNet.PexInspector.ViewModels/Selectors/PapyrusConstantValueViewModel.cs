using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using PapyrusDotNet.PexInspector.ViewModels.Implementations;

namespace PapyrusDotNet.PexInspector.ViewModels.Selectors
{
    public class PapyrusConstantValueViewModel : ViewModelBase
    {
        private readonly OpCodeArgumentDescription desc;

        public PapyrusConstantValueViewModel(OpCodeArgumentDescription desc)
        {
            this.desc = desc;
            ValueInputVisibility = Visibility.Visible;

            ComboBoxItems = new ObservableCollection<FrameworkElement>(CreateComboBoxItems());
            SelectedValueType = ComboBoxItems.First() as ComboBoxItem;
        }
        private ObservableCollection<FrameworkElement> comboBoxItems;
        public ObservableCollection<FrameworkElement> ComboBoxItems
        {
            get { return comboBoxItems; }
            set { Set(ref comboBoxItems, value); }
        }

        private List<FrameworkElement> CreateComboBoxItems()
        {
            var elements = new List<FrameworkElement>();

            if (desc != null && desc.Constraints.Length > 0)
            {
                if (desc.Constraints.Contains(OpCodeConstraint.None))
                    elements.Add(new ComboBoxItem { Content = "None" });
                if (desc.Constraints.Contains(OpCodeConstraint.Integer))
                    elements.Add(new ComboBoxItem { Content = "Integer" });
                if (desc.Constraints.Contains(OpCodeConstraint.Float))
                    elements.Add(new ComboBoxItem { Content = "Float" });
                if (desc.Constraints.Contains(OpCodeConstraint.Boolean))
                    elements.Add(new ComboBoxItem { Content = "Boolean" });
                if (desc.Constraints.Contains(OpCodeConstraint.String))
                    elements.Add(new ComboBoxItem { Content = "String" });
                return elements;
            }
            elements.Add(new ComboBoxItem { Content = "None" });
            elements.Add(new ComboBoxItem { Content = "Integer" });
            elements.Add(new ComboBoxItem { Content = "Float" });
            elements.Add(new ComboBoxItem { Content = "Boolean" });
            elements.Add(new ComboBoxItem { Content = "String" });
            return elements;
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

        private Visibility valueInputVisibility;
        public Visibility ValueInputVisibility
        {
            get { return valueInputVisibility; }
            set { Set(ref valueInputVisibility, value); }
        }

        public static PapyrusConstantValueViewModel DesignInstance = designInstance ??
                                                                     (designInstance =
                                                                         new PapyrusConstantValueViewModel(null));

        private static PapyrusConstantValueViewModel designInstance;
        private object selectedValue;
        private ComboBoxItem selectedValueType;

    }
}