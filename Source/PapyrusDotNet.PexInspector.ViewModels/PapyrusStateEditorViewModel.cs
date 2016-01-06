using GalaSoft.MvvmLight;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class PapyrusStateEditorViewModel : ViewModelBase
    {
        public PapyrusStateEditorViewModel(PapyrusStateDefinition stateToEdit = null)
        {
            if (stateToEdit != null)
            {
                Name = stateToEdit.Name.Value;
            }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }
    }
}