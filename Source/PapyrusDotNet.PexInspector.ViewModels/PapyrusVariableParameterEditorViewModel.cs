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
            set { Set(ref selectedType, value); }
        }

        public PapyrusVariableParameterEditorViewModel(IEnumerable<string> types)
        {
            var src = new List<object>(new[] { "None", "Int", "Float", "Bool", "String" });

            src.Add(new Separator());

            src.AddRange(types);

            TypeReferences = new ObservableCollection<object>(src);

            SelectedType = "None";
        }

        private static Lazy<PapyrusVariableParameterEditorViewModel> lazyDesignInstance = new Lazy<PapyrusVariableParameterEditorViewModel>(CreateDesignViewModel);

        private static PapyrusVariableParameterEditorViewModel CreateDesignViewModel() => new PapyrusVariableParameterEditorViewModel(new List<string>())
        {
            Name = "::MyVariable"
        };

        public static PapyrusVariableParameterEditorViewModel DesignInstance = lazyDesignInstance.Value;



    }
}