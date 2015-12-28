using System.Collections.Generic;
using GalaSoft.MvvmLight;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PexInspector.ViewModels.Implementations;

namespace PapyrusDotNet.PexInspector.ViewModels.Selectors
{
    public class PapyrusReferenceAndConstantValueViewModel : ViewModelBase
    {
        private readonly List<PapyrusAssemblyDefinition> loadedAssemblies;
        private readonly PapyrusTypeDefinition currentType;
        private readonly PapyrusMethodDefinition currentMethod;
        private readonly OpCodeArgumentDescription opCodeArgumentDescription;

        public PapyrusReferenceAndConstantValueViewModel(List<PapyrusAssemblyDefinition> loadedAssemblies, PapyrusTypeDefinition currentType, 
            PapyrusMethodDefinition currentMethod, OpCodeArgumentDescription opCodeArgumentDescription)
        {
            this.loadedAssemblies = loadedAssemblies;
            this.currentType = currentType;
            this.currentMethod = currentMethod;
            this.opCodeArgumentDescription = opCodeArgumentDescription;
        }

        public object SelectedItem
        {
            get { return selectedItem; }
            set { Set(ref selectedItem, value); }
        }

        public static PapyrusReferenceAndConstantValueViewModel DesignInstance = designInstance ??
                                                         (designInstance =
                                                             new PapyrusReferenceAndConstantValueViewModel(null, null, null, null));

        private static PapyrusReferenceAndConstantValueViewModel designInstance;
        private object selectedItem;
    }
}