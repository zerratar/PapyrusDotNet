using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PexInspector.ViewModels.Implementations;

namespace PapyrusDotNet.PexInspector.ViewModels.Selectors
{
    public class PapyrusMethodSelectorViewModel : ViewModelBase
    {
        private readonly List<PapyrusAssemblyDefinition> loadedAssemblies;

        public PapyrusMethodSelectorViewModel(List<PapyrusAssemblyDefinition> loadedAssemblies, PapyrusTypeDefinition currentType, OpCodeArgumentDescription opCodeArgumentDescription)
        {
            this.loadedAssemblies = loadedAssemblies;

            if (currentType != null)
            {
                Methods =
                    new ObservableCollection<PapyrusViewModel>(currentType.States.SelectMany(s => s.Methods).Select(j => new PapyrusViewModel
                    {
                        Text = j.Name.Value + GetParameterString(j.Parameters) + " : " + j.ReturnTypeName.Value,
                        Item = j
                    }));
            }

            SelectedMethodCommand = new RelayCommand<PapyrusViewModel>(SelectMethod);

        }

        private void SelectMethod(PapyrusViewModel obj)
        {
            SelectedMethod = obj;
        }

        public RelayCommand<PapyrusViewModel> SelectedMethodCommand { get; set; }

        private string GetParameterString(List<PapyrusParameterDefinition> parameters, bool includeParameterNames = false)
        {
            var paramDefs = string.Join(", ", parameters.Select(p => p.TypeName.Value +
            (includeParameterNames ? (" " + p.Name.Value) : "")));

            return "(" + paramDefs + ")";
        }

        public PapyrusViewModel SelectedMethod
        {
            get { return selectedMethod; }
            set
            {
                if (Set(ref selectedMethod, value))
                {
                    if (value != null)
                    {
                        var method = value.Item as PapyrusMethodDefinition;
                        if (method != null)
                            SelectedMethodName = method.Name.Value;
                    }
                }
            }
        }

        public ObservableCollection<PapyrusViewModel> Methods
        {
            get { return methods; }
            set { Set(ref methods, value); }
        }

        public string SelectedMethodName
        {
            get { return selectedMethodName; }
            set { Set(ref selectedMethodName, value); }
        }

        public static PapyrusMethodSelectorViewModel DesignInstance = designInstance ??
                                                             (designInstance =
                                                                 new PapyrusMethodSelectorViewModel(null, null, null));

        private static PapyrusMethodSelectorViewModel designInstance;
        private PapyrusViewModel selectedMethod;
        private ObservableCollection<PapyrusViewModel> methods;
        private string selectedMethodName;
    }
}