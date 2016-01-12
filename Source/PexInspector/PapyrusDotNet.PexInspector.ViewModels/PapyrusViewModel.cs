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
using GalaSoft.MvvmLight;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class PapyrusViewModel : ViewModelBase
    {
        private string icon;
        private bool isDirty;
        private bool isExpanded;
        private bool isSelected;
        private object item;
        private string text;

        public PapyrusViewModel(PapyrusViewModel parent = null)
        {
            Parent = parent;
            if (parent != null)
            {
                parent.Children.Add(this);
            }

            Children = new ObservableCollection<PapyrusViewModel>();
        }

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

        public bool IsDirty
        {
            get { return isDirty; }
            set { Set(ref isDirty, value); }
        }

        public PapyrusViewModel Parent { get; set; }

        public ObservableCollection<PapyrusViewModel> Children { get; set; }

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

        public bool IsHierarchyDirty
        {
            get
            {
                if (IsDirty) return true;
                if (Parent != null)
                    return Parent.IsHierarchyDirty;

                return false;
            }
        }

        public void SetDirty(bool dirty)
        {
            IsDirty = dirty;
            if (dirty)
            {
                Parent?.SetDirty(true);
            }
        }

        private void SetIconFromItem(object value)
        {
            var packUri = "pack://application:,,,/PapyrusDotNet.PexInspector;component/";

            if (value == null || value.ToString() == "root" || value is PapyrusAssemblyDefinition)
            {
                Icon = packUri + "Assets/Icons/type.png";
            }
            else if (value != null && value.ToString() == "states")
            {
                Icon = packUri + "Assets/Icons/states.png";
            }
            else if (value is PapyrusStateDefinition)
            {
                Icon = packUri + "Assets/Icons/state.png";
            }
            else if (value is PapyrusPropertyDefinition)
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
                var method = value as PapyrusMethodDefinition;
                if (method.IsEvent)
                    Icon = packUri + "Assets/Icons/event.png";
                else
                {
                    Icon = packUri + "Assets/Icons/method.png";
                }
            }
        }

        public PapyrusViewModel GetTopParent()
        {
            if (Parent == null) return this;
            return Parent.GetTopParent();
        }

        public struct ss
        {
        }
    }
}