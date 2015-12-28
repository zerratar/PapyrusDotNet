using System.Windows;
using GalaSoft.MvvmLight;
using PapyrusDotNet.PexInspector.ViewModels;
using PapyrusDotNet.PexInspector.ViewModels.Interfaces;
using PapyrusDotNet.PexInspector.ViewModels.Selectors;
using PapyrusDotNet.PexInspector.Windows;

namespace PapyrusDotNet.PexInspector.Implementations
{
    public class DialogService : IDialogService
    {
        public DialogResult ShowDialog(ViewModelBase viewModel)
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
            // When selecting a Type
            if (viewModel is PapyrusTypeSelectorViewModel)
            {
                dialog = new PapyrusTypeSelectorWindow();
            }
            // When selecting a method
            if (viewModel is PapyrusMethodSelectorViewModel)
            {
                dialog = new PapyrusMethodSelectorWindow();
            }
            // When selecting a constant value (int, float, bool, string)
            if (viewModel is PapyrusConstantValueViewModel)
            {
                dialog = new PapyrusConstantValueEditorWindow();
            }
            // When selecting a reference value (variable, parameter or field)
            if (viewModel is PapyrusReferenceValueViewModel)
            {
                dialog = new PapyrusConstantValueEditorWindow();
            }
            // When selecting either a constant value or a reference
            if (viewModel is PapyrusReferenceAndConstantValueViewModel)
            {
                dialog = new PapyrusConstantValueEditorWindow();
            }
            // When selecting a instruction
            if (viewModel is PapyrusInstructionSelectorViewModel)
            {
                dialog = new PapyrusInstructionSelectorWindow();
            }
            if (dialog != null)
            {
                dialog.DataContext = viewModel;
                var res = dialog.ShowDialog();

                if (res == null)
                    return DialogResult.Undefined;
                if (res.GetValueOrDefault())
                    return DialogResult.OK;
                if (!res.GetValueOrDefault())
                    return DialogResult.Cancel;
            }
            return DialogResult.Cancel;
        }
    }
}
