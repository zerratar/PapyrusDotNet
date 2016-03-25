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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using PapyrusDotNet.Decompiler;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Extensions;
using PapyrusDotNet.PexInspector.ViewModels.Interfaces;
using PapyrusDotNet.PexInspector.ViewModels.Selectors;
using PapyrusDotNet.PexInspector.ViewModels.Tools;

#endregion

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private static MainWindowViewModel vm;
        public static MainWindowViewModel DesignInstance = vm ?? (vm = new MainWindowViewModel(null, null, null, null));

        private bool canSaveItem;
        private RelayCommand createFieldCommand;
        private RelayCommand createMethodCommand;
        private RelayCommand createPropertyCommand;
        private RelayCommand createStateCommand;
        private string decompiledMemberText;
        private RelayCommand<object> deleteMemberCommand;

        private readonly IDialogService dialogService;
        private readonly IPexLoader pexLoader;
        private readonly IPexTreeBuilder pexTreeBuilder;
        private readonly IMemberDisplayBuilder memberDisplayBuilder;

        private RelayCommand<object> editMemberCommand;
        private ICommand findAllReferencesCommand;
        private RelayCommand<object> findAllUsagesCommand;
        private ObservableCollection<TreeViewItem> findResultTree;

        private ObservableCollection<PapyrusViewModel> pexTree;
        private RelayCommand<object> reloadPexCommand;
        private RelayCommand<object> savePexAsCommand;
        private RelayCommand<object> savePexCommand;
        private int selectedContentIndex;
        private object selectedMemberFlags;
        private ObservableCollection<Inline> selectedMemberName;
        internal PapyrusMethodDefinition selectedMethod;
        internal PapyrusViewModel selectedMethodNode;
        private PapyrusParameterDefinition selectedMethodParameter;
        private ObservableCollection<PapyrusParameterDefinition> selectedMethodParameters;
        private PapyrusVariableReference selectedMethodVariable;
        private ObservableCollection<PapyrusVariableReference> selectedMethodVariables;
        private PapyrusViewModel selectedNode;
        private string targetGameName;
        private ICommand injectCapricaInfoCommand;

        public MainWindowViewModel(IDialogService dialogService, IPexLoader pexLoader, IPexTreeBuilder pexTreeBuilder, IMemberDisplayBuilder memberDisplayBuilder)
        {
            this.dialogService = dialogService;
            this.pexLoader = pexLoader;
            this.pexTreeBuilder = pexTreeBuilder;
            this.memberDisplayBuilder = memberDisplayBuilder;

            InstructionEditor = new InstructionEditorViewModel(this, pexLoader, dialogService);

            // if dialogService == null then this is a design instance :-P
            if (dialogService != null)
            {
                ExitCommand = new RelayCommand(Exit);
                OpenPexCommand = new RelayCommand(OpenPex);
                SavePexCommand = new RelayCommand<object>(SavePex, CanSave);
                SavePexAsCommand = new RelayCommand<object>(SavePexAs, CanSave);

                ReloadPexCommand = new RelayCommand<object>(ReloadPex, o => o != null || selectedNode != null);

                EditMemberCommand = new RelayCommand<object>(EditMember, CanEditMember);

                CreatePropertyCommand = new RelayCommand(CreatePropertyItem, () => false /*CanCreateProperty*/);

                CreateFieldCommand = new RelayCommand(CreateFieldItem, CanCreateField);

                CreateStateCommand = new RelayCommand(CreateStateItem,
                    () =>
                        (selectedNode != null) && selectedNode.Item is PapyrusTypeDefinition &&
                        (selectedNode.Item as PapyrusTypeDefinition).IsClass);

                CreateMethodCommand = new RelayCommand(CreateMethod, () => false
                    /*() => selectedNode != null && (selectedNode.Item is PapyrusStateDefinition || selectedNode.Item is PapyrusMethodDefinition)*/);

                SelectedMemberCommand = new RelayCommand<PapyrusViewModel>(SelectMember);

                DeleteMemberCommand = new RelayCommand<object>(DeleteMember, CanDeleteMember);


                CreateVariableCommand = new RelayCommand(CreateVariable, CanCreate);
                EditVariableCommand = new RelayCommand(EditVariable, CanEditVar);
                DeleteVariableCommand = new RelayCommand(DeleteVariable, CanEditVar);


                CreateParameterCommand = new RelayCommand(CreateParameter, CanCreate);
                EditParameterCommand = new RelayCommand(EditParameter, CanEditParameter);
                DeleteParameterCommand = new RelayCommand(DeleteParameter, CanEditParameter);

                FindAllUsagesCommand = new RelayCommand<object>(FindAllusages, CanFindUsage);
                FindAllReferencesCommand = new RelayCommand<object>(FindAllReferences, CanFindReferences);

                OpenAboutCommand = new RelayCommand(OpenAboutWindow);

                InjectCapricaInfoCommand = new RelayCommand(InjectCapricaInfo);
            }

            DecompiledMemberText = "Select a method to view its source.";

            TargetGameName = "Unknown";
            SelectedMemberFlags = "<none>";
            SelectedMemberName = new ObservableCollection<Inline>(new[] { new Run("Nothing Selected") });
        }

        public void LoadPex(string fileName)
        {
            pexLoader.LoadPex(fileName);
        }

        private void InjectCapricaInfo()
        {
            if (selectedMethod != null)
            {
                var asm = selectedMethod.DeclaringAssembly;

                var desc = new PapyrusMethodDecription();
                desc.Name = ":::Caprica".Ref(asm);
                desc.MethodType = PapyrusMethodTypes.Method;
                desc.DeclaringTypeName = asm.Types.First().Name;
                desc.BodyLineNumbers.Add((short)'C');
                desc.BodyLineNumbers.Add((short)'A');
                desc.BodyLineNumbers.Add((short)'P');
                desc.BodyLineNumbers.Add((short)'R');
                desc.BodyLineNumbers.Add((short)'I');
                desc.BodyLineNumbers.Add((short)'C');
                desc.BodyLineNumbers.Add((short)'A');
                asm.DebugInfo.MethodDescriptions.Add(desc);
                selectedMethodNode.SetDirty(true);
            }
        }

        public ObservableCollection<PapyrusViewModel> PexTree
        {
            get { return pexTree; }
            set { Set(ref pexTree, value); }
        }

        public PapyrusVariableReference SelectedMethodVariable
        {
            get { return selectedMethodVariable; }
            set
            {
                if (Set(ref selectedMethodVariable, value))
                {
                    EditVariableCommand.RaiseCanExecuteChanged();
                    DeleteVariableCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public PapyrusParameterDefinition SelectedMethodParameter
        {
            get { return selectedMethodParameter; }
            set
            {
                if (Set(ref selectedMethodParameter, value))
                {
                    EditParameterCommand.RaiseCanExecuteChanged();
                    DeleteParameterCommand.RaiseCanExecuteChanged();
                }
            }
        }


        public ObservableCollection<PapyrusParameterDefinition> SelectedMethodParameters
        {
            get { return selectedMethodParameters; }
            set { Set(ref selectedMethodParameters, value); }
        }

        public ObservableCollection<PapyrusVariableReference> SelectedMethodVariables
        {
            get { return selectedMethodVariables; }
            set { Set(ref selectedMethodVariables, value); }
        }


        public ObservableCollection<Inline> SelectedMemberName
        {
            get { return selectedMemberName; }
            set { Set(ref selectedMemberName, value); }
        }

        public string TargetGameName
        {
            get { return targetGameName; }
            set { Set(ref targetGameName, value); }
        }

        public object SelectedMemberFlags
        {
            get { return selectedMemberFlags; }
            set { Set(ref selectedMemberFlags, value); }
        }

        public ICommand OpenPexCommand { get; set; }

        public RelayCommand<object> SavePexCommand
        {
            get { return savePexCommand; }
            set { Set(ref savePexCommand, value); }
        }

        public RelayCommand<object> SavePexAsCommand
        {
            get { return savePexAsCommand; }
            set { Set(ref savePexAsCommand, value); }
        }

        public RelayCommand<object> ReloadPexCommand
        {
            get { return reloadPexCommand; }
            set { Set(ref reloadPexCommand, value); }
        }

        public RelayCommand<object> FindAllUsagesCommand
        {
            get { return findAllUsagesCommand; }
            set { Set(ref findAllUsagesCommand, value); }
        }

        public ICommand ExitCommand { get; set; }

        public RelayCommand<PapyrusViewModel> SelectedMemberCommand { get; set; }
        public RelayCommand CreateVariableCommand { get; set; }
        public RelayCommand EditVariableCommand { get; set; }
        public RelayCommand DeleteVariableCommand { get; set; }
        public RelayCommand CreateParameterCommand { get; set; }
        public RelayCommand EditParameterCommand { get; set; }
        public RelayCommand DeleteParameterCommand { get; set; }


        public ICommand ValidateSelectedScriptCommand { get; set; }
        public ICommand ValidateSelectedScriptAndReferencesCommand { get; set; }
        public ICommand ValidateAllScriptsCommand { get; set; }
        public ICommand OpenSettingsCommand { get; set; }
        public ICommand OpenDocumentationCommand { get; set; }
        public ICommand OpenAboutCommand { get; set; }


        public ObservableCollection<TreeViewItem> FindResultTree
        {
            get { return findResultTree; }
            set { Set(ref findResultTree, value); }
        }

        public int SelectedContentIndex
        {
            get { return selectedContentIndex; }
            set { Set(ref selectedContentIndex, value); }
        }

        public ICommand FindAllReferencesCommand
        {
            get { return findAllReferencesCommand; }
            set { Set(ref findAllReferencesCommand, value); }
        }

        public RelayCommand<object> EditMemberCommand
        {
            get { return editMemberCommand; }
            set { Set(ref editMemberCommand, value); }
        }

        public RelayCommand CreatePropertyCommand
        {
            get { return createPropertyCommand; }
            set { Set(ref createPropertyCommand, value); }
        }

        public RelayCommand CreateFieldCommand
        {
            get { return createFieldCommand; }
            set { Set(ref createFieldCommand, value); }
        }

        public RelayCommand CreateStateCommand
        {
            get { return createStateCommand; }
            set { Set(ref createStateCommand, value); }
        }

        public RelayCommand CreateMethodCommand
        {
            get { return createMethodCommand; }
            set { Set(ref createMethodCommand, value); }
        }

        public RelayCommand<object> DeleteMemberCommand
        {
            get { return deleteMemberCommand; }
            set { Set(ref deleteMemberCommand, value); }
        }

        public string DecompiledMemberText
        {
            get { return decompiledMemberText; }
            set { Set(ref decompiledMemberText, value); }
        }

        public ICommand InjectCapricaInfoCommand
        {
            get { return injectCapricaInfoCommand; }
            set { Set(ref injectCapricaInfoCommand, value); }
        }

        public InstructionEditorViewModel InstructionEditor { get; set; }

        public event EventHandler SelectedContentIndexChanged;

        private bool CanCreateField()
        {
            return (selectedNode != null) &&
                   (selectedNode.Item is PapyrusPropertyDefinition || selectedNode.Item is PapyrusFieldDefinition ||
                    selectedNode.Item is PapyrusTypeDefinition);
        }


        private bool CanCreateProperty()
        {
            return (selectedNode != null) &&
                   (selectedNode.Item is PapyrusPropertyDefinition || selectedNode.Item is PapyrusFieldDefinition ||
                    (selectedNode.Item is PapyrusTypeDefinition && (selectedNode.Item as PapyrusTypeDefinition).IsClass));
        }

        private bool CanEditMember(object arg)
        {
            if (arg == null && selectedNode == null) return false;

            var pvm = arg as PapyrusViewModel;
            if (pvm == null)
                pvm = selectedNode;

            if (pvm != null)
            {
                if (pvm.Item is PapyrusStateDefinition /*|| pvm.Item is PapyrusMethodDefinition */||
                    /*pvm.Item is PapyrusPropertyDefinition ||*/ pvm.Item is PapyrusFieldDefinition)
                    return true;
            }

            return false;
        }

        private bool CanDeleteMember(object arg)
        {
            if (arg == null && selectedNode == null) return false;
            var pvm = arg as PapyrusViewModel;
            if (pvm == null)
                pvm = selectedNode;
            if (pvm != null)
            {
                if (pvm.Item is PapyrusFieldDefinition || pvm.Item is PapyrusMethodDefinition ||
                    pvm.Item is PapyrusStateDefinition || pvm.Item is PapyrusPropertyDefinition)
                {
                    return true;
                }
            }
            return false;
        }

        private void EditMember(object obj)
        {
            if (obj == null && selectedNode == null) return;
            var pvm = obj as PapyrusViewModel;
            if (pvm == null)
                pvm = selectedNode;
            if (pvm != null)
            {
                var pap =
                    pvm.Item as PapyrusMemberReference;
                if (pap == null) return;

                if (pvm.Item is PapyrusStateDefinition)
                {
                    EditState(pvm, pvm.Item as PapyrusStateDefinition);
                    return;
                }

                var method = pap as PapyrusMethodDefinition;
                if (method != null)
                {
                    EditMethod(pvm, method);
                }

                var field = pap as PapyrusFieldDefinition;
                if (field != null)
                {
                    EditField(pvm, field);
                }

                var prop = pap as PapyrusPropertyDefinition;
                if (prop != null)
                {
                    EditProperty(pvm, prop);
                }
            }
        }

        private void EditProperty(PapyrusViewModel node, PapyrusPropertyDefinition prop)
        {
            MessageBox.Show("Not yet Implemented");
        }

        private void EditField(PapyrusViewModel node, PapyrusFieldDefinition field)
        {
            var loadedAssemblies = pexLoader.GetLoadedAssemblies();
            var loadedTypes = loadedAssemblies.SelectMany(t => t.Types.Select(j => j.Name.Value));
            var dialog = new PapyrusFieldEditorViewModel(loadedTypes, field);
            if (dialogService.ShowDialog(dialog) == DialogResult.OK)
            {
                var asm = field.DeclaringAssembly;
                field.Name = dialog.Name.Ref(asm);

                if (dialog.IsArray)
                {
                    field.TypeName = dialog.SelectedTypeName.Replace("[]", "") + "[]";
                }
                else
                {
                    field.TypeName = dialog.SelectedTypeName.Replace("[]", "");
                }

                field.DefaultValue = dialog.GetDefaultValue();

                node.Item = field;
                node.Text = field.Name.Value + " : " + field.TypeName;
                node.SetDirty(true);
                RaiseCommandsCanExecute();
            }
        }

        private void EditMethod(PapyrusViewModel node, PapyrusMethodDefinition method)
        {
            MessageBox.Show("Not yet Implemented");
        }

        private void EditState(PapyrusViewModel node, PapyrusStateDefinition papyrusStateDefinition)
        {
            var dialog = new PapyrusStateEditorViewModel(papyrusStateDefinition);
            if (dialogService.ShowDialog(dialog) == DialogResult.OK)
            {
                papyrusStateDefinition.Name = dialog.Name.Ref(papyrusStateDefinition.DeclaringType.Assembly);
                node.Item = papyrusStateDefinition;
                node.Text = !string.IsNullOrEmpty(papyrusStateDefinition.Name.Value)
                    ? papyrusStateDefinition.Name.Value
                    : "<default>";
                node.SetDirty(true);
                RaiseCommandsCanExecute();
            }
        }

        private void DeleteMember(object obj)
        {
            if (obj == null && selectedNode == null) return;
            var pvm = obj as PapyrusViewModel;
            if (pvm == null)
                pvm = selectedNode;
            if (pvm != null)
            {
                var pap =
                    pvm.Item as PapyrusMemberReference;
                if (pap == null) return;

                var state = pap as PapyrusStateDefinition;
                if (state != null)
                {
                    DeleteState(pvm, state);
                }

                var method = pap as PapyrusMethodDefinition;
                if (method != null)
                {
                    DeleteMethod(pvm, method);
                }

                var field = pap as PapyrusFieldDefinition;
                if (field != null)
                {
                    DeleteField(pvm, field);
                }

                var prop = pap as PapyrusPropertyDefinition;
                if (prop != null)
                {
                    DeleteProperty(pvm, prop);
                }
            }
        }

        private void DeleteProperty(PapyrusViewModel pvm, PapyrusPropertyDefinition prop)
        {
            if (MessageBox.Show("Are you sure you want to delete '" + prop.Name.Value +
                                "' as it could be used and that would break the script if so.\r\n" +
                                "\r\n------------------------\r\n" +
                                "Recommended: Check all usages before deleting by right-clicking on this item and select 'Find All Usages'.",
                "Delete Property", MessageBoxButton.YesNo)
                == MessageBoxResult.Yes)
            {
                var type = prop.DeclaringType;
                type.Properties.Remove(prop);

                pvm.SetDirty(true);
                // This will set the parent as dirty as well. So we gotta do this before removing it.
                pvm.Parent.Children.Remove(pvm);
                RaiseCommandsCanExecute();
            }
        }


        private void DeleteState(PapyrusViewModel pvm, PapyrusStateDefinition state)
        {
            if (state.DeclaringType.States.Count(i => string.IsNullOrEmpty(i.Name.Value)) == 1 &&
                string.IsNullOrEmpty(state.Name.Value))
            {
                MessageBox.Show("You cannot delete this state as one <default> state must always exist.");
                return;
            }
            if (state.Methods.Any())
            {
                MessageBox.Show(
                    "This state contains one or more methods and cannot be deleted.\r\nDelete the methods first and then try again.");
            }
            else
            {
                if (MessageBox.Show(
                    "Are you sure you want to delete '" + state.Name.Value +
                    "' as it could be used and that would break the script if so.",
                    "Delete State", MessageBoxButton.YesNo)
                    == MessageBoxResult.Yes)
                {
                    var type = state.DeclaringType;
                    type.States.Remove(state);

                    pvm.SetDirty(true);
                    // This will set the parent as dirty as well. So we gotta do this before removing it.
                    pvm.Parent.Children.Remove(pvm);
                    RaiseCommandsCanExecute();
                }
            }
        }

        private void DeleteField(PapyrusViewModel pvm, PapyrusFieldDefinition field)
        {
            var prop = field.DeclaringType.Properties.FirstOrDefault(
                p =>
                    p.AutoName != null &&
                    string.Equals(p.AutoName.Replace("::", ""), field.Name.Value.Replace("::", ""), StringComparison.CurrentCultureIgnoreCase));
            if (prop != null)
            {
                MessageBox.Show("This field is being accessed from an Auto Property '" + prop.Name.Value +
                                "' and cannot be deleted.");
            }
            else
            {
                if (MessageBox.Show(
                    "Are you sure you want to delete '" + field.Name.Value +
                    "' as it could be used and that would break the script if so.\r\n" +
                    "\r\n------------------------\r\n" +
                    "Recommended: Check all usages before deleting by right-clicking on this item and select 'Find All Usages'.",
                    "Delete Field", MessageBoxButton.YesNo)
                    == MessageBoxResult.Yes)
                {
                    var type = field.DeclaringType;
                    type.Fields.Remove(field);

                    pvm.SetDirty(true);
                    // This will set the parent as dirty as well. So we gotta do this before removing it.
                    pvm.Parent.Children.Remove(pvm);
                    RaiseCommandsCanExecute();
                }
            }
        }

        private void DeleteMethod(PapyrusViewModel pvm, PapyrusMethodDefinition method)
        {
            if (method.IsNative)
            {
                MessageBox.Show("Deleting Native methods are not supported as it would definitely break the script.");
            }
            else
            {
                if (MessageBox.Show(
                    "Are you sure you want to delete '" + method.Name.Value +
                    "' as it could be used and would break the script if so.\r\n" + "\r\n------------------------\r\n" +
                    "Recommended: Check all usages before deleting by right-clicking on this item and select 'Find All Usages'.",
                    "Delete Method", MessageBoxButton.YesNo)
                    == MessageBoxResult.Yes)
                {
                    var state = method.DeclaringState;
                    var debugInfo = state.DeclaringType.Assembly.DebugInfo;

                    var desc =
                        debugInfo?.MethodDescriptions.FirstOrDefault(m => m.Name.Index == method.Name.Index);
                    if (desc != null)
                    {
                        debugInfo.MethodDescriptions.Remove(desc);
                    }

                    state.Methods.Remove(method);

                    pvm.SetDirty(true);
                    // This will set the parent as dirty as well. So we gotta do this before removing it.
                    pvm.Parent.Children.Remove(pvm);
                    RaiseCommandsCanExecute();
                }
            }
        }

        private void CreateMethod()
        {
            MessageBox.Show("Not yet Implemented");
        }

        private void CreateStateItem()
        {
            var dialog = new PapyrusStateEditorViewModel();
            if (dialogService.ShowDialog(dialog) == DialogResult.OK)
            {
                var root = selectedNode.GetTopParent();
                var asm = root.Item as PapyrusAssemblyDefinition;
                if (asm == null) return;

                var type = asm.Types.First();

                var state = new PapyrusStateDefinition(type) { Name = dialog.Name.Ref(asm) };

                var node = new PapyrusViewModel(root.Children.First())
                {
                    Item = state,
                    Text = !string.IsNullOrEmpty(state.Name.Value) ? state.Name.Value : "<default>"
                };
                node.SetDirty(true);
                RaiseCommandsCanExecute();
            }
        }

        private void CreateFieldItem()
        {
            var root =
                selectedNode.GetTopParent();
            var asm = root.Item as PapyrusAssemblyDefinition;

            PapyrusTypeDefinition type = null;
            var typeNode = selectedNode;

            type = selectedNode.Item as PapyrusTypeDefinition;
            if (type == null)
            {
                var f = selectedNode.Item as PapyrusFieldDefinition;
                if (f != null)
                {
                    typeNode = selectedNode.Parent;
                    type = f.DeclaringType;
                }
                var p = selectedNode.Item as PapyrusPropertyDefinition;
                if (p != null)
                {
                    typeNode = selectedNode.Parent;
                    type = p.DeclaringType;
                }
            }

            if (type == null)
            {
                MessageBox.Show("Unexpected Error: Unable to determine which type the new field should belong to.");
                return;
            }

            var loadedAssemblies = pexLoader.GetLoadedAssemblies();
            var loadedTypes = loadedAssemblies.SelectMany(t => t.Types.Select(j => j.Name.Value));
            var dialog = new PapyrusFieldEditorViewModel(loadedTypes);
            if (dialogService.ShowDialog(dialog) == DialogResult.OK)
            {
                var field = new PapyrusFieldDefinition(asm, type, dialog.Name, dialog.SelectedTypeName.Replace("[]", ""));

                if (dialog.IsArray)
                {
                    field.TypeName = dialog.SelectedTypeName.Replace("[]", "") + "[]";
                }

                field.DefaultValue = dialog.GetDefaultValue();

                var node = new PapyrusViewModel(typeNode);
                node.Item = field;
                node.Text = field.Name.Value + " : " + field.TypeName;
                node.SetDirty(true);
                RaiseCommandsCanExecute();
            }
        }

        private void CreatePropertyItem()
        {
            MessageBox.Show("Not yet Implemented");
        }


        public void RaiseCommandsCanExecute()
        {

            DecompileSelectedMethod();

            (FindAllReferencesCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
            FindAllUsagesCommand.RaiseCanExecuteChanged();
            ReloadPexCommand.RaiseCanExecuteChanged();
            SavePexAsCommand.RaiseCanExecuteChanged();
            SavePexCommand.RaiseCanExecuteChanged();
            CreatePropertyCommand.RaiseCanExecuteChanged();
            CreateFieldCommand.RaiseCanExecuteChanged();
            CreateMethodCommand.RaiseCanExecuteChanged();
            CreateStateCommand.RaiseCanExecuteChanged();
            EditMemberCommand.RaiseCanExecuteChanged();
            InstructionEditor.CreateInstructionCommand.RaiseCanExecuteChanged();
            CreateParameterCommand.RaiseCanExecuteChanged();
            CreateVariableCommand.RaiseCanExecuteChanged();
            DeleteMemberCommand.RaiseCanExecuteChanged();
        }

        private void OpenAboutWindow()
        {
            dialogService.ShowDialog(new AboutViewModel());
        }

        private bool CanFindReferences(object arg)
        {
            var pvm = arg as PapyrusViewModel;
            if (pvm == null) return false;
            return pvm.Item is PapyrusAssemblyDefinition || pvm.Item is PapyrusTypeDefinition;
        }


        private bool CanFindUsage(object arg)
        {
            var pvm = arg as PapyrusViewModel;
            if (pvm == null) return false;
            return pvm.Item is PapyrusFieldDefinition || pvm.Item is PapyrusPropertyDefinition ||
                   pvm.Item is PapyrusMethodDefinition;
        }

        public void GoToMember(PapyrusMemberReference papyrusMemberReference)
        {
            var targetMethod = papyrusMemberReference as PapyrusMethodDefinition;
            var inst = papyrusMemberReference as PapyrusInstruction;
            if (inst != null)
            {
                targetMethod = inst.Method;
            }

            if (targetMethod != null)
            {
                var targetState = targetMethod.DeclaringState;
                var targetType = targetState.DeclaringType;
                //var targetAssembly = targetType.Assembly;

                var file =
                    PexTree.FirstOrDefault(i => i.Text.ToLower() == targetType.Name.Value.ToLower() + ".pex");

                file.IsExpanded = true;
                file.IsSelected = true;

                var type =
                    file.Children.First();

                var state = string.IsNullOrEmpty(targetState.Name.Value)
                    ? type.Children.FirstOrDefault(s => s.Text.ToLower().Contains("<default>"))
                    : type.Children.FirstOrDefault(t => t.Text.ToLower().Contains(targetState.Name.Value.ToLower()));
                if (state != null)
                {
                    state.IsExpanded = true;
                    state.IsSelected = true;

                    var method = state.Children.FirstOrDefault(m => m.Item == targetMethod);
                    if (method != null)
                    {
                        method.IsSelected = true;
                        method.IsExpanded = true;
                        //if (inst != null && selectedMethodInstructions != null)
                        //{
                        //    SelectedMethodInstruction = selectedMethodInstructions.FirstOrDefault(s => s.Offset == inst.Offset);
                        //}
                    }
                }
                SelectedContentIndex = 0;
                SelectedContentIndexChanged?.Invoke(this, new EventArgs());
            }
        }

        private void FindAllReferences(object obj)
        {
            //var loadedAsm =
            //Directory.GetFiles(LoadedAssemblyFolders.FirstOrDefault(), "*.pex", SearchOption.AllDirectories)
            //    .Select(i => PapyrusAssemblyDefinition.ReadAssembly(i, false))
            //    .ToList();
            var loadedAssemblies = pexLoader.GetLoadedAssemblies();
            var refFinder =
                new PapyrusReferenceFinder(loadedAssemblies);


            var pvm = obj as PapyrusViewModel;
            if (pvm != null)
            {
                string searchText = null;
                var type = pvm.Item as PapyrusTypeDefinition;
                if (type != null)
                {
                    searchText =
                        type.Name.Value;
                }
                var asm = pvm.Item as PapyrusAssemblyDefinition;
                if (asm != null)
                {
                    searchText =
                        asm.Types.First().Name.Value;
                }
                if (searchText != null)
                {
                    var res = refFinder.FindTypeReference(searchText);
                    BuildUsageTree(res);
                }
            }
        }

        private void FindAllusages(object obj)
        {
            var loadedAssemblies = pexLoader.GetLoadedAssemblies();
            var usageFinder =
                new PapyrusUsageFinder(loadedAssemblies);

            var pvm = obj as PapyrusViewModel;
            if (pvm != null)
            {
                var field = pvm.Item as PapyrusFieldDefinition;
                if (field != null)
                {
                    var result =
                        usageFinder.FindFieldUsage(field.Name.Value);
                    BuildUsageTree(result);
                    return;
                }

                var prop = pvm.Item as PapyrusPropertyDefinition;
                if (prop != null)
                {
                    var result =
                        usageFinder.FindFieldUsage(prop.Name.Value);
                    BuildUsageTree(result);
                    return;
                }

                var meth = pvm.Item as PapyrusMethodDefinition;
                if (meth != null)
                {
                    var result =
                        usageFinder.FindFieldUsage(meth.Name.Value);
                    BuildUsageTree(result);
                }
            }
        }

        private void BuildUsageTree(IFindResult result)
        {
            var items = new List<TreeViewItem>();

            if (result.Results.Any(i => i.Method == null))
            {
                foreach (var i in result.Results)
                {
                    var item = new TreeViewItem();
                    item.IsExpanded = true;
                    item.Header = i.Type.Name.Value + " is based on " + i.SearchText;
                    items.Add(item);
                }
            }
            else
            {
                foreach (var j in result.Results.GroupBy(i => i.Method))
                {
                    var item = new TreeViewItem();
                    item.Tag = j.Key;
                    item.IsExpanded = true;
                    item.Header = j.Key.DeclaringState.DeclaringType.Name.Value + "." + j.Key.Name.Value +
                                  GetParameterString(j.Key.Parameters) + " : " + j.Key.ReturnTypeName.Value;

                    foreach (var res in j)
                    {
                        if (res.Method != null)
                        {
                            var child = new TreeViewItem();

                            child.MouseDoubleClick += Child_MouseDoubleClick;
                            child.Tag = res.Instruction;
                            child.Header = "L_" + res.Instruction.Offset.ToString("000") + ": " + res.Instruction.OpCode +
                                           " : " + res.SearchText;

                            item.Items.Add(child);
                            items.Add(item);
                        }
                    }
                }
            }
            if (items.Count == 0)
            {
                items.Add(new TreeViewItem
                {
                    Header =
                        "The search for '" + result.SearchText + "' gave no results within any of the loaded scripts."
                });
            }

            FindResultTree = new ObservableCollection<TreeViewItem>(items);
            SelectedContentIndex = 1;
            SelectedContentIndexChanged?.Invoke(this, new EventArgs());
        }

        private void Child_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            GoToMember((sender as TreeViewItem).Tag as PapyrusMemberReference);
        }

        private void ReloadPex(object i)
        {
            var item = i as PapyrusViewModel;
            if (item == null)
                item = selectedNode;
            if (item != null)
            {
                var top = item.GetTopParent();
                var asm = top.Item as PapyrusAssemblyDefinition;
                if (asm != null)
                {
                    var loadedAssemblies = pexLoader.GetLoadedAssemblies().ToList();
                    var index = loadedAssemblies.IndexOf(asm);

                    loadedAssemblies[index] = PapyrusAssemblyDefinition.ReloadAssembly(asm);

                    PexTree = pexTreeBuilder.BuildPexTree(PexTree, top);

                    InstructionEditor.SelectedMethodInstructions = new ObservableCollection<PapyrusInstruction>();

                    SelectedMethodVariables = new ObservableCollection<PapyrusVariableReference>();

                    SelectedMethodParameters = new ObservableCollection<PapyrusParameterDefinition>();
                    // RELOAD
                }
            }
        }

        private bool CanCreate()
        {
            return selectedMethod != null;
        }

        private void EditVariable()
        {
            var loadedAssemblies = pexLoader.GetLoadedAssemblies().ToList();
            var loadedTypes = loadedAssemblies.SelectMany(t => t.Types.Select(j => j.Name.Value));
            var dialog = new PapyrusVariableEditorViewModel(loadedTypes, SelectedMethodVariable);
            var result = dialogService.ShowDialog(dialog);
            if (result == DialogResult.OK)
            {
                var asm = selectedMethod.DeclaringState.DeclaringType.Assembly;
                var var = SelectedMethodVariable;
                var typeName = dialog.SelectedType ?? dialog.SelectedTypeName;
                var varName = dialog.Name;

                var.Name = varName.Ref(asm);
                var.TypeName = typeName.ToString().Ref(asm);

                SelectedMethodVariables =
                    new ObservableCollection<PapyrusVariableReference>(selectedMethod.GetVariables());
                selectedMethodNode.SetDirty(true);

                RaiseCommandsCanExecute();
            }
        }

        private void CreateVariable()
        {
            var loadedAssemblies = pexLoader.GetLoadedAssemblies().ToList();
            var loadedTypes = loadedAssemblies.SelectMany(t => t.Types.Select(j => j.Name.Value));
            var dialog = new PapyrusVariableEditorViewModel(loadedTypes);
            var result = dialogService.ShowDialog(dialog);
            if (result == DialogResult.OK)
            {
                var asm = selectedMethod.DeclaringState.DeclaringType.Assembly;
                var typeName = dialog.SelectedType ?? dialog.SelectedTypeName;
                var varName = dialog.Name;

                typeName = typeName.ToString().Replace("[]", "");

                if (dialog.IsArray)
                {
                    typeName = typeName + "[]";
                }

                if (varName.ToLower().StartsWith("::temp"))
                {
                    selectedMethod.Body.Variables.Add(new PapyrusVariableReference
                    {
                        Name = varName.Ref(asm),
                        Value = varName,
                        TypeName = typeName.ToString().Ref(asm),
                        Type = PapyrusPrimitiveType.Reference
                    });
                }
                else
                {
                    selectedMethod.Body.Variables.Add(new PapyrusVariableReference
                    {
                        Name = varName.Ref(asm),
                        Value = varName,
                        TypeName = typeName.ToString().Ref(asm),
                        Type = PapyrusPrimitiveType.Reference
                    });
                }

                SelectedMethodVariables = new ObservableCollection<PapyrusVariableReference>(
                    selectedMethod.GetVariables()
                    );
                selectedMethodNode.SetDirty(true);

                RaiseCommandsCanExecute();
            }
        }


        private void EditParameter()
        {
            var loadedAssemblies = pexLoader.GetLoadedAssemblies().ToList();
            var loadedTypes = loadedAssemblies.SelectMany(t => t.Types.Select(j => j.Name.Value));
            var dialog = new PapyrusParameterEditorViewModel(loadedTypes, SelectedMethodParameter);
            var result = dialogService.ShowDialog(dialog);
            if (result == DialogResult.OK)
            {
                var asm = selectedMethod.DeclaringState.DeclaringType.Assembly;
                var var = SelectedMethodParameter;
                var typeName = dialog.SelectedType ?? dialog.SelectedTypeName;
                var varName = dialog.Name;

                var.Name = varName.Ref(asm);
                var.TypeName = typeName.ToString().Ref(asm);

                SelectedMethodParameters = new ObservableCollection<PapyrusParameterDefinition>(
                    selectedMethod.Parameters
                    );
                selectedMethodNode.SetDirty(true);

                RaiseCommandsCanExecute();
            }
        }

        private void CreateParameter()
        {
            var loadedAssemblies = pexLoader.GetLoadedAssemblies().ToList();
            var loadedTypes = loadedAssemblies.SelectMany(t => t.Types.Select(j => j.Name.Value));
            var dialog = new PapyrusParameterEditorViewModel(loadedTypes);
            var result = dialogService.ShowDialog(dialog);
            if (result == DialogResult.OK)
            {
                var asm = selectedMethod.DeclaringState.DeclaringType.Assembly;
                var typeName = dialog.SelectedType ?? dialog.SelectedTypeName;

                var varName = dialog.Name;

                typeName = typeName.ToString().Replace("[]", "");

                if (dialog.IsArray)
                {
                    typeName = typeName + "[]";
                }

                selectedMethod.Parameters.Add(new PapyrusParameterDefinition
                {
                    Name = varName.Ref(asm),
                    TypeName = typeName.ToString().Ref(asm)
                });

                SelectedMethodParameters = new ObservableCollection<PapyrusParameterDefinition>(
                    selectedMethod.Parameters
                    );
                selectedMethodNode.SetDirty(true);

                RaiseCommandsCanExecute();
            }
        }

        public PapyrusVariableReference CreateReferenceFromName(string name, PapyrusAssemblyDefinition asm)
        {
            var nameRef = name.Ref(asm);
            return new PapyrusVariableReference
            {
                Name = nameRef,
                Value = nameRef.Value,
                Type = PapyrusPrimitiveType.Reference
            };
        }

        private void DeleteParameter()
        {
            var obj = SelectedMethodParameter;
            var name = obj.Name?.Value ?? "";
            if (MessageBox.Show(
                "WARNING: It could be used by any existing instructions, and if this method is being called from somewhere else," +
                "that call needs to be updated or the scripts will stop working.\r\n" +
                "----------------------\r\n" +
                "Deleting this parameter will not modify any existing instructions.\r\nAre you sure you want to delete this parameter?",
                "Delete Parameter " + name, MessageBoxButton.OKCancel)
                == MessageBoxResult.OK)
            {
                var method = selectedMethod;

                if (method.Parameters.Contains(obj))
                    method.Parameters.Remove(obj);

                SelectedMethodParameters = new ObservableCollection<PapyrusParameterDefinition>(
                    method.Parameters
                    );
                selectedMethodNode.SetDirty(true);
                RaiseCommandsCanExecute();
            }
        }

        private void DeleteVariable()
        {
            var obj = SelectedMethodVariable;
            var name = obj.Name?.Value ?? "";
            if (MessageBox.Show(
                "WARNING: It could be used by any existing instructions.\r\n" +
                "----------------------\r\n" +
                "Deleting this variable will not modify any existing instructions.\r\n" +
                "Are you sure you want to delete this variable?",
                "Delete Variable " + name, MessageBoxButton.OKCancel)
                == MessageBoxResult.OK)
            {
                var method = selectedMethod;

                if (method.Body.Variables.Contains(obj))
                    method.Body.Variables.Remove(obj);

                SelectedMethodVariables = new ObservableCollection<PapyrusVariableReference>(
                    method.GetVariables()
                    );
                selectedMethodNode.SetDirty(true);

                RaiseCommandsCanExecute();
            }
        }

        private void Exit() => Application.Current.Shutdown(-1);

        private void SavePexAs(object i)
        {
            var vm = i as PapyrusViewModel;
            if (vm != null)
            {
                var vmTop = vm.GetTopParent();
                var asm = vmTop.Item as PapyrusAssemblyDefinition;
                if (asm != null && vmTop.IsDirty)
                {
                    // TODO
                }
            }
        }

        private void SavePex(object i)
        {
            var vm = i as PapyrusViewModel;
            if (vm != null)
            {
                var vmTop = vm.GetTopParent();
                var asm = vmTop.Item as PapyrusAssemblyDefinition;
                if (asm != null && vmTop.IsDirty)
                {
                    if (MessageBox.Show(
                        "Are you sure you want to overwrite the existing script with your modifications?\r\nA backup will be made automatically.",
                        "Overwrite", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        asm.Backup();

                        asm.Write();

                        ReloadPex(vmTop);
                    }
                }
            }
        }

        private bool CanEditParameter() => SelectedMethodParameter != null;

        private bool CanEditVar() => SelectedMethodVariable != null;

        private bool CanSave(object o)
        {
            var i = o as PapyrusViewModel;
            return i != null && i.IsDirty || canSaveItem;
        }

        private void OpenPex()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Papyrus Script Binary (*.pex)|*.pex";
            if (ofd.ShowDialog().GetValueOrDefault())
            {
                pexLoader.LoadPex(ofd.FileName);

                PexTree = pexTreeBuilder.BuildPexTree(PexTree);
            }
        }


        private string GetParameterString(IEnumerable<PapyrusParameterDefinition> parameters,
            bool includeParameterNames = false)
        {
            return "(" + string.Join(", ", parameters.Select(p => p.TypeName.Value +
                                                                  (includeParameterNames ? " " + p.Name.Value : ""))) + ")";
        }

        private void SelectMember(PapyrusViewModel item)
        {
            selectedNode = item;
            if (item == null) return;

            canSaveItem = item.IsDirty || item.IsHierarchyDirty;

            SelectedMemberName = memberDisplayBuilder.BuildMemberDisplay(item.Item);
            var type = item.Item as PapyrusTypeDefinition;
            if (type != null)
            {
                TargetGameName = type.Assembly.VersionTarget.ToString();
                SelectedMemberFlags = "0x" + type.Flags.ToString("X");
            }
            var state = item.Item as PapyrusStateDefinition;
            if (state != null)
            {
                TargetGameName = state.DeclaringType.Assembly.VersionTarget.ToString();
                SelectedMemberFlags = "<none>";
            }
            var assembly = item.Item as PapyrusAssemblyDefinition;
            if (assembly != null)
            {
                TargetGameName = assembly.VersionTarget.ToString();
                SelectedMemberFlags = "<none>";
            }
            var prop = item.Item as PapyrusPropertyDefinition;
            if (prop != null)
            {
                TargetGameName = prop.DeclaringAssembly.VersionTarget.ToString();
                SelectedMemberFlags = "0x" + prop.Flags.ToString("X");
            }
            var field = item.Item as PapyrusFieldDefinition;
            if (field != null)
            {
                TargetGameName = field.DeclaringAssembly.VersionTarget.ToString();
                SelectedMemberFlags = "0x" + field.Flags.ToString("X");
            }
            var method = item.Item as PapyrusMethodDefinition;
            if (method != null)
            {
                selectedMethodNode = item;
                selectedMethod = method;

                InstructionEditor.SelectedMethodInstructions = new ObservableCollection<PapyrusInstruction>(
                    method.Body.Instructions);

                SelectedMethodParameters = new ObservableCollection<PapyrusParameterDefinition>(
                    method.Parameters);

                SelectedMethodVariables = new ObservableCollection<PapyrusVariableReference>(
                    method.GetVariables());

                SelectedMemberFlags = "0x" + method.Flags.ToString("X");

                TargetGameName = method.DeclaringAssembly.VersionTarget.ToString();

                //DecompiledMemberText =
                //new PapyrusDecompiler(new PapyrusControlFlowAnalyzer(), new PapyrusCodeGenerator()).Decompile(method);

                DecompileSelectedMethod();

                selectedContentIndex = 0;
                SelectedContentIndexChanged?.Invoke(this, new EventArgs());
            }

            RaiseCommandsCanExecute();
        }

        private void DecompileSelectedMethod()
        {
            if (selectedMethod == null) return;
            try
            {
                var assembly = selectedMethod.DeclaringAssembly;
                if (assembly != null)
                {
                    var decompiler = new PapyrusDecompiler(assembly);
                    var ctx = decompiler.CreateContext();
                    var result = decompiler.Decompile(ctx, selectedMethod);

                    if (!result.HasErrors)
                    {
                        DecompiledMemberText = result.DecompiledSourceCode;
                    }
                }
            }
            catch (Exception)
            {
                DecompiledMemberText = "Failed to decompile this method";
            }
        }


    }
}