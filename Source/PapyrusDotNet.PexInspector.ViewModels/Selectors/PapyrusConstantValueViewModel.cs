using GalaSoft.MvvmLight;
using PapyrusDotNet.PexInspector.ViewModels.Implementations;

namespace PapyrusDotNet.PexInspector.ViewModels.Selectors
{
    public class PapyrusConstantValueViewModel : ViewModelBase
    {
        public PapyrusConstantValueViewModel(OpCodeArgumentDescription opCodeArgumentDescription)
        {

        }

        public object SelectedValue
        {
            get { return selectedValue; }
            set { Set(ref selectedValue, value); }
        }

        public object SelectedValueType { get; set; }

        public static PapyrusConstantValueViewModel DesignInstance = designInstance ??
                                                                     (designInstance =
                                                                         new PapyrusConstantValueViewModel(null));

        private static PapyrusConstantValueViewModel designInstance;
        private object selectedValue;
    }
}