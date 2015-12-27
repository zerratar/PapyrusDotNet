using System.Windows;
using GalaSoft.MvvmLight;
using PapyrusDotNet.PexInspector.ViewModels;
using PapyrusDotNet.PexInspector.ViewModels.Interfaces;
using PapyrusDotNet.PexInspector.Windows;

namespace PapyrusDotNet.PexInspector.Implementations
{
    public class DialogService : IDialogService
    {
        public IDialogResult ShowDialog(ViewModelBase viewModel)
        {
            Window dialog = null;
            if (viewModel is PapyrusInstructionEditorViewModel)
            {
                dialog = new PapyrusInstructionEditorWindow();
            }
            if (viewModel is PapyrusParameterEditorViewModel || viewModel is PapyrusVariableEditorViewModel)
            {
                dialog = new PapyrusParameterAndVariableEditorWindow();
            }
            if (dialog != null)
            {
                dialog.DataContext = viewModel;
                return new DialogResult(dialog.ShowDialog());
            }
            return new DialogResult(null);
        }
    }
}
