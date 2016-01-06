using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using PapyrusDotNet.PexInspector.ViewModels;
using PapyrusDotNet.PexInspector.ViewModels.Interfaces;
using PapyrusDotNet.PexInspector.ViewModels.Selectors;
using PapyrusDotNet.PexInspector.Windows;

namespace PapyrusDotNet.PexInspector.Implementations
{
    public class DialogService : IDialogService
    {
        private static Window GetActiveWindow()
        {
            var active = User32.GetActiveWindow();
            Window activeWindow = null;

            if (Application.Current != null)
            {
                if (Thread.CurrentThread == Dispatcher.CurrentDispatcher.Thread)
                {
                    var windows = Application.Current.Windows.OfType<Window>();
                    activeWindow = windows
                        .SingleOrDefault(window => new WindowInteropHelper(window).Handle == active);
                }
            }
            return activeWindow;
        }

        private static Window GetWindow(Window owner)
        {
            var windows = Application.Current.Windows.OfType<Window>();
            return windows
                .SingleOrDefault(window => window.Owner == owner);
        }

        private static Window GetWindow(IntPtr windowHandle)
        {
            Window targetWindow = null;
            if (Application.Current != null)
            {
                if (Thread.CurrentThread == Dispatcher.CurrentDispatcher.Thread)
                {
                    var windows = Application.Current.Windows.OfType<Window>();
                    targetWindow = windows
                        .SingleOrDefault(window => new WindowInteropHelper(window).Handle == windowHandle);
                }
            }
            return targetWindow;
        }

        public DialogResult ShowDialog(ViewModelBase viewModel)
        {
            Window dialog = null;
            if (viewModel is AboutViewModel)
            {
                dialog = new AboutWindow();
            }
            if (viewModel is PapyrusFieldEditorViewModel)
            {
                dialog = new PapyrusFieldEditorWindow();
            }
            if (viewModel is PapyrusStateEditorViewModel)
            {
                dialog = new PapyrusStateEditorWindow();
            }
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
                dialog = new PapyrusReferenceValueEditorWindow();
            }
            // When selecting either a constant value or a reference
            if (viewModel is PapyrusReferenceAndConstantValueViewModel)
            {
                dialog = new PapyrusReferenceAndConstantValueEditorWindow();
            }
            // When selecting a instruction
            if (viewModel is PapyrusInstructionSelectorViewModel)
            {
                dialog = new PapyrusInstructionSelectorWindow();
            }
            if (dialog != null)
            {
                dialog.Owner = GetActiveWindow();
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
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
