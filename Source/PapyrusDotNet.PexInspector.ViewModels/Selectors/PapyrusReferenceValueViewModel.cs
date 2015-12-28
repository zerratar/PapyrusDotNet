using System.Collections.Generic;
using GalaSoft.MvvmLight;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PexInspector.ViewModels.Implementations;

namespace PapyrusDotNet.PexInspector.ViewModels.Selectors
{
    public class PapyrusReferenceValueViewModel : ViewModelBase
    {
        private readonly List<PapyrusAssemblyDefinition> loadedAssemblies;
        private readonly PapyrusTypeDefinition currentType;
        private readonly PapyrusMethodDefinition currentMethod;
        private readonly OpCodeArgumentDescription opCodeArgumentDescription;

        public PapyrusReferenceValueViewModel(List<PapyrusAssemblyDefinition> loadedAssemblies, PapyrusTypeDefinition currentType, PapyrusMethodDefinition currentMethod, OpCodeArgumentDescription opCodeArgumentDescription)
        {
            this.loadedAssemblies = loadedAssemblies;
            this.currentType = currentType;
            this.currentMethod = currentMethod;
            this.opCodeArgumentDescription = opCodeArgumentDescription;
        }

        public PapyrusVariableReference SelectedReference
        {
            get { return selectedReference; }
            set { Set(ref selectedReference, value); }
        }

        public static PapyrusReferenceValueViewModel DesignInstance = designInstance ??
                                                         (designInstance =
                                                             new PapyrusReferenceValueViewModel(null, null, null, null));

        private static PapyrusReferenceValueViewModel designInstance;
        private PapyrusVariableReference selectedReference;
    }
}