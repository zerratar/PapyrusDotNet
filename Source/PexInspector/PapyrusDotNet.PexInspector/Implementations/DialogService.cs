//     This file is part of PapyrusDotNet.
// 
//     PapyrusDotNet is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     PapyrusDotNet is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with PapyrusDotNet.  If not, see <http://www.gnu.org/licenses/>.
//  
//     Copyright 2016, Karl Patrik Johansson, zerratar@gmail.com

#region

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

#endregion

namespace PapyrusDotNet.PexInspector.Implementations
{
    public class DialogService : IDialogService
    {
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
    }
}