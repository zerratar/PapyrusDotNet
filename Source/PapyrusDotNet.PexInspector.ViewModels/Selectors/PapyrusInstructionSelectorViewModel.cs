using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PexInspector.ViewModels.Implementations;

namespace PapyrusDotNet.PexInspector.ViewModels.Selectors
{
    public class PapyrusInstructionSelectorViewModel : ViewModelBase
    {
        public PapyrusInstructionSelectorViewModel(IEnumerable<PapyrusInstruction> instructions, OpCodeArgumentDescription opCodeArgumentDescription)
        {
            if (instructions != null)
            {
                Instructions = new ObservableCollection<PapyrusInstruction>(instructions);
            }
        }

        public ObservableCollection<PapyrusInstruction> Instructions
        {
            get { return instructions; }
            set { Set(ref instructions, value); }
        }

        public PapyrusInstruction SelectedInstruction
        {
            get { return selectedInstruction; }
            set { Set(ref selectedInstruction, value); }
        }

        public static PapyrusInstructionSelectorViewModel DesignInstance = designInstance ??
                                                             (designInstance =
                                                                 new PapyrusInstructionSelectorViewModel(null, null));

        private static PapyrusInstructionSelectorViewModel designInstance;
        private ObservableCollection<PapyrusInstruction> instructions;
        private PapyrusInstruction selectedInstruction;
    }
}