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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using GalaSoft.MvvmLight;

#endregion

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class PapyrusVariableParameterEditorViewModel : ViewModelBase
    {
        private static readonly Lazy<PapyrusVariableParameterEditorViewModel> lazyDesignInstance =
            new Lazy<PapyrusVariableParameterEditorViewModel>(CreateDesignViewModel);

        public static PapyrusVariableParameterEditorViewModel DesignInstance = lazyDesignInstance.Value;
        private bool isArray;

        private bool isConstantValueType;
        private string name;
        private object selectedType;
        private ObservableCollection<object> typeReferences;

        public PapyrusVariableParameterEditorViewModel(IEnumerable<string> types)
        {
            var src = new List<object>(new[] {"None", "Int", "Float", "Bool", "String"});

            src.Add(new Separator());

            src.AddRange(types);

            TypeReferences = new ObservableCollection<object>(src);

            SelectedType = "None";
        }

        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }

        public ObservableCollection<object> TypeReferences
        {
            get { return typeReferences; }
            set { Set(ref typeReferences, value); }
        }

        public object SelectedType
        {
            get { return selectedType; }
            set
            {
                if (Set(ref selectedType, value))
                {
                    if (value == null)
                        SelectedTypeName = "None";
                    else
                        SelectedTypeName = value.ToString();

                    var i =
                        TypeReferences.IndexOf(value);

                    IsConstantValueType = i > 0 && i < 5;

                    if (IsArray)
                    {
                        SelectedTypeName = SelectedTypeName.Replace("[]", "") + "[]";
                        IsConstantValueType = false;
                    }
                }
            }
        }

        public bool IsArray
        {
            get { return isArray; }
            set
            {
                if (Set(ref isArray, value))
                {
                    if (value)
                        IsConstantValueType = false;
                    else
                    {
                        var i =
                            TypeReferences.IndexOf(SelectedTypeName.Replace("[]", ""));

                        IsConstantValueType = i > 0 && i < 5;
                    }
                }
            }
        }

        public bool IsConstantValueType
        {
            get { return isConstantValueType; }
            set { Set(ref isConstantValueType, value); }
        }

        public string SelectedTypeName { get; set; }

        private static PapyrusVariableParameterEditorViewModel CreateDesignViewModel()
            => new PapyrusVariableParameterEditorViewModel(new List<string>())
            {
                Name = "::MyVariable"
            };
    }
}