using System.Collections.ObjectModel;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class PapyrusViewModel : ViewModelBase
    {
        private bool isSelected;
        private bool isExpanded;
        private string icon;
        private string text;
        private object item;

        public object Item
        {
            get { return item; }
            set
            {
                if (Set(ref item, value))
                {
                    SetIconFromItem(value);
                }
            }
        }

        private void SetIconFromItem(object value)
        {

            string packUri = "pack://application:,,,/PapyrusDotNet.PexInspector;component/";

            if (value == null || value.ToString() == "root")
            {
                Icon = packUri + "Assets/Icons/type.png";
            }
            else if (value != null && value.ToString() == "states")
            {
                Icon = packUri + "Assets/Icons/states.png";
            }
            else
            if (value is PapyrusStateDefinition)
            {
                Icon = packUri + "Assets/Icons/state.png";
            }
            else
            if (value is PapyrusPropertyDefinition)
            {
                Icon = packUri + "Assets/Icons/property.png";
            }
            else if (value is PapyrusFieldDefinition)
            {
                Icon = packUri + "Assets/Icons/field.png";
            }
            else if (value is PapyrusTypeDefinition)
            {
                var type = value as PapyrusTypeDefinition;
                if (type.IsStruct)
                {
                    Icon = packUri + "Assets/Icons/structure.png";
                }
                else
                {
                    Icon = packUri + "Assets/Icons/typedefinition.png";
                }
            }
            else if (value is PapyrusMethodDefinition)
            {
                var method = (value as PapyrusMethodDefinition);
                if (method.IsEvent)
                    Icon = packUri + "Assets/Icons/event.png";
                else
                {
                    Icon = packUri + "Assets/Icons/method.png";
                }
            }
        }

        public struct ss
        {

        }

        public PapyrusViewModel Parent { get; set; }

        public ObservableCollection<PapyrusViewModel> Children { get; set; }

        public PapyrusViewModel(PapyrusViewModel parent = null)
        {
            if (parent != null)
            {
                parent.Children.Add(this);
            }

            Children = new ObservableCollection<PapyrusViewModel>();

        }

        public string Text
        {
            get { return text; }
            set { Set(ref text, value); }
        }

        public string Icon
        {
            get { return icon; }
            set { Set(ref icon, value); }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set { Set(ref isSelected, value); }
        }

        public bool IsExpanded
        {
            get { return isExpanded; }
            set { Set(ref isExpanded, value); }
        }
    }
}
