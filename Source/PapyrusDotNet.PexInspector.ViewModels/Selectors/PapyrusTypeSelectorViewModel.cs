using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PexInspector.ViewModels.Implementations;

namespace PapyrusDotNet.PexInspector.ViewModels.Selectors
{
    public class PapyrusTypeSelectorViewModel : ViewModelBase
    {
        private readonly List<PapyrusAssemblyDefinition> loadedAssemblies;
        private readonly OpCodeArgumentDescription opCodeArgumentDescription;

        public PapyrusTypeSelectorViewModel(List<PapyrusAssemblyDefinition> loadedAssemblies, OpCodeArgumentDescription opCodeArgumentDescription)
        {
            this.loadedAssemblies = loadedAssemblies;
            this.opCodeArgumentDescription = opCodeArgumentDescription;

            SelectedTypeCommand = new RelayCommand<PapyrusViewModel>(SelectType);

            if (loadedAssemblies != null)
            {
                var defs = loadedAssemblies.SelectMany(t => t.Types).ToList();
                Types = new ObservableCollection<PapyrusViewModel>(
                        defs.Select(i => new PapyrusViewModel
                        {
                            Text = i.Name.Value + (!string.IsNullOrEmpty(i.BaseTypeName.Value) ? " : " + i.BaseTypeName.Value : ""),
                            Item = i
                        })
                    );
            }
        }

        private void SelectType(PapyrusViewModel obj)
        {
            SelectedType = obj.Item as PapyrusTypeDefinition;
        }

        public PapyrusTypeDefinition SelectedType
        {
            get { return selectedType; }
            set { Set(ref selectedType, value); }
        }

        public ObservableCollection<PapyrusViewModel> Types
        {
            get { return types; }
            set { Set(ref types, value); }
        }

        public RelayCommand<PapyrusViewModel> SelectedTypeCommand { get; set; }

        public static PapyrusTypeSelectorViewModel DesignInstance = designInstance ??
                                                       (designInstance =
                                                           new PapyrusTypeSelectorViewModel(null, null));

        private static PapyrusTypeSelectorViewModel designInstance;
        private PapyrusTypeDefinition selectedType;
        private ObservableCollection<PapyrusViewModel> types;
    }
}