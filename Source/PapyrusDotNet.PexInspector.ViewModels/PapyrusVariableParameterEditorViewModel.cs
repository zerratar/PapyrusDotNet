using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class PapyrusVariableParameterEditorViewModel : ViewModelBase
    {
        private string name;
        private ObservableCollection<object> typeReferences;
        private object selectedType;

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

        private bool isConstantValueType;
        private bool isArray;

        public bool IsArray
        {
            get { return isArray; }
            set
            {
                if (Set(ref isArray, value))
                {
                    if (value == true)
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

        public PapyrusVariableParameterEditorViewModel(IEnumerable<string> types)
        {
            var src = new List<object>(new[] { "None", "Int", "Float", "Bool", "String" });

            src.Add(new Separator());

            src.AddRange(types);

            TypeReferences = new ObservableCollection<object>(src);

            SelectedType = "None";
        }

        public string SelectedTypeName { get; set; }

        private static Lazy<PapyrusVariableParameterEditorViewModel> lazyDesignInstance = new Lazy<PapyrusVariableParameterEditorViewModel>(CreateDesignViewModel);

        private static PapyrusVariableParameterEditorViewModel CreateDesignViewModel() => new PapyrusVariableParameterEditorViewModel(new List<string>())
        {
            Name = "::MyVariable"
        };

        public static PapyrusVariableParameterEditorViewModel DesignInstance = lazyDesignInstance.Value;



    }
}