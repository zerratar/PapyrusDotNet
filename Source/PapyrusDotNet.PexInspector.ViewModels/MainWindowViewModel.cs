using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Microsoft.Win32;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public List<PapyrusAssemblyDefinition> LoadedAssemblies = new List<PapyrusAssemblyDefinition>();
        public List<string> LoadedAssemblyNames = new List<string>();
        public List<string> LoadedAssemblyFolders = new List<string>();

        public MainWindowViewModel(Interfaces.IDialogService dialogService)
        {
            this.dialogService = dialogService;
            ExitCommand = new RelayCommand(Exit);
            OpenPexCommand = new RelayCommand(OpenPex);
            SavePexCommand = new RelayCommand(SavePex, CanSave);
            SavePexAsCommand = new RelayCommand(SavePexAs, CanSave);

            SelectedMemberCommand = new RelayCommand<PapyrusViewModel>(SelectMember);

            InsertAfterCommand = new RelayCommand(InsertAfter, CanInsert);
            InsertBeforeCommand = new RelayCommand(InsertBefore, CanInsert);
            EditInstructionCommand = new RelayCommand(EditInstruction, CanInsert);
            RemoveInstructionCommand = new RelayCommand(RemoveInstruction, CanInsert);
        }

        private void RemoveInstruction()
        {
            var obj = SelectedMethodInstruction;
            if (MessageBox.Show("Are you sure you want to delete this instruction?",
                "Delete Instruction ( " + obj.Offset + " ) " + obj.OpCode, MessageBoxButton.OKCancel)
                == MessageBoxResult.OK)
            {
                var method = obj.Method;

                method.Body.Instructions.Remove(obj);

                SelectedMethodInstructions = new ObservableCollection<PapyrusInstruction>(
                        method.Body.Instructions
                    );

                SelectedMethodParameters = new ObservableCollection<PapyrusParameterDefinition>(
                        method.Parameters
                    );

                SelectedMethodVariables = new ObservableCollection<PapyrusVariableReference>(
                        method.GetVariables()
                    );
            }
        }

        private void EditInstruction()
        {
            var result = dialogService.ShowDialog(new PapyrusInstructionEditorViewModel(dialogService, LoadedAssemblies, selectedMethod.DeclaringState?.DeclaringType?.Assembly,
                selectedMethod.DeclaringState?.DeclaringType,
                selectedMethod, SelectedMethodInstruction));
            if (result == DialogResult.OK)
            {

            }
        }

        private void InsertBefore()
        {
            var result = dialogService.ShowDialog(new PapyrusInstructionEditorViewModel(dialogService,
                LoadedAssemblies,
                selectedMethod.DeclaringState?.DeclaringType?.Assembly,
                selectedMethod.DeclaringState?.DeclaringType, selectedMethod));
            if (result == DialogResult.OK)
            {

            }
        }

        private bool CanInsert() => SelectedMethodInstruction != null;

        private void InsertAfter()
        {
            var result = dialogService.ShowDialog(new PapyrusInstructionEditorViewModel(dialogService, LoadedAssemblies,
                selectedMethod.DeclaringState?.DeclaringType?.Assembly,
                selectedMethod.DeclaringState?.DeclaringType, selectedMethod));
            if (result == DialogResult.OK)
            {

            }
        }

        private void Exit()
        {

        }

        private void SavePexAs()
        {

        }

        private bool CanSave() => false;

        private void SavePex()
        {

        }

        private void OpenPex()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Papyrus Script Binary (*.pex)|*.pex";
            if (ofd.ShowDialog().GetValueOrDefault())
            {
                LoadPex(ofd.FileName);
            }
        }

        private void LoadPex(string fileName)
        {
            var name = System.IO.Path.GetFileName(fileName);
            var directoryName = System.IO.Path.GetDirectoryName(fileName);

            if (LoadedAssemblyNames.Contains(name))
            {
                if (MessageBox.Show("This file has already been loaded.\r\nDo you want to reload it?", "Reload?",
                        MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            var loadedAssembly = PapyrusAssemblyDefinition.ReadAssembly(fileName);
            int loadIndex = -1;
            if (LoadedAssemblyNames.Contains(name))
            {
                loadIndex = LoadedAssemblyNames.IndexOf(name);
            }

            if (loadIndex == -1)
            {
                LoadedAssemblies.Add(loadedAssembly);

                LoadedAssemblyNames.Add(name);
            }
            else
            {
                LoadedAssemblies[loadIndex] = loadedAssembly;
            }

            if (!LoadedAssemblyFolders.Contains(directoryName))
                LoadedAssemblyFolders.Add(directoryName);

            BuildPexTree();

            SavePexCommand.RaiseCanExecuteChanged();
            SavePexAsCommand.RaiseCanExecuteChanged();
        }

        private void BuildPexTree()
        {
            var RootNodes = new List<PapyrusViewModel>();
            for (int index = 0; index < LoadedAssemblies.Count; index++)
            {
                var asm = LoadedAssemblies[index];
                var root = new PapyrusViewModel();
                root.Item = "root";
                root.Text = LoadedAssemblyNames[index];
                foreach (var type in asm.Types)
                {
                    var typeNode = new PapyrusViewModel(root);
                    typeNode.Item = type;
                    typeNode.Text = type.Name.Value + (!string.IsNullOrEmpty(type.BaseTypeName.Value) ? " : " + type.BaseTypeName.Value : "");

                    if (!string.IsNullOrEmpty(type.BaseTypeName.Value))
                    {
                        if (EnsureAssemblyLoaded(type.BaseTypeName.Value)) return; // the tree will be reloaded, so we don't wanna finish it here.
                    }

                    foreach (var structType in type.NestedTypes.OrderBy(i => i.Name.Value))
                    {
                        var structTypeNode = new PapyrusViewModel(typeNode);
                        structTypeNode.Item = structType;
                        structTypeNode.Text = structType.Name.Value;
                        foreach (var field in structType.Fields)
                        {
                            var fieldNode = new PapyrusViewModel(structTypeNode);
                            fieldNode.Item = field;
                            fieldNode.Text = field.Name.Value + " : " + field.TypeName;

                            if (!string.IsNullOrEmpty(field.TypeName))
                            {
                                if (EnsureAssemblyLoaded(type.BaseTypeName.Value)) return; // the tree will be reloaded, so we don't wanna finish it here.
                            }
                        }
                    }

                    var statesNode = new PapyrusViewModel(typeNode);
                    statesNode.Item = "states";
                    statesNode.Text = "States";
                    foreach (var item in type.States.OrderBy(i => i.Name.Value))
                    {
                        var stateNode = new PapyrusViewModel(statesNode);
                        stateNode.Item = item;
                        stateNode.Text = (!string.IsNullOrEmpty(item.Name.Value) ? item.Name.Value : "default");
                        foreach (var method in item.Methods.OrderBy(i=>i.Name.Value))
                        {
                            var m = new PapyrusViewModel(stateNode);
                            m.Item = method;
                            m.Text = method.Name.Value + GetParameterString(method.Parameters) + " : " +
                                     method.ReturnTypeName.Value;

                            if (!string.IsNullOrEmpty(method.ReturnTypeName.Value))
                            {
                                if (EnsureAssemblyLoaded(method.ReturnTypeName.Value)) return; // the tree will be reloaded, so we don't wanna finish it here.
                            }
                        }
                    }

                    foreach (var field in type.Fields.OrderBy(i => i.Name.Value))
                    {
                        var fieldNode = new PapyrusViewModel(typeNode);
                        fieldNode.Item = field;
                        fieldNode.Text = field.Name.Value + " : " + field.TypeName;

                        if (!string.IsNullOrEmpty(field.TypeName))
                        {
                            if (EnsureAssemblyLoaded(field.TypeName)) return; // the tree will be reloaded, so we don't wanna finish it here.
                        }
                    }

                    foreach (var item in type.Properties.OrderBy(i=>i.Name.Value))
                    {
                        var fieldNode = new PapyrusViewModel(typeNode);
                        fieldNode.Item = item;
                        fieldNode.Text = item.Name.Value + " : " + item.TypeName.Value;

                        if (!string.IsNullOrEmpty(item.TypeName.Value))
                        {
                            if (EnsureAssemblyLoaded(item.TypeName.Value)) return; // the tree will be reloaded, so we don't wanna finish it here.
                        }
                    }
                }
                RootNodes.Add(root);
            }
            PexTree = new ObservableCollection<PapyrusViewModel>(RootNodes.OrderBy(i => i.Text));
        }

        private bool EnsureAssemblyLoaded(string value)
        {
            if (value == null) return false;

            var lower = value.ToLower();
            // do not try and load any value types
            if (lower == "int" || lower == "string" || lower == "bool" || lower == "none" || lower == "float") return false;

            if (LoadedAssemblyNames.All(a => !string.Equals(Path.GetFileNameWithoutExtension(a.ToLower()), value, StringComparison.CurrentCultureIgnoreCase)))
            {
                var scripts = LoadedAssemblyFolders.SelectMany(
                    i => System.IO.Directory.GetFiles(i, "*.pex", SearchOption.AllDirectories)).ToList();

                var targetScriptFile = scripts.FirstOrDefault(s => Path.GetFileNameWithoutExtension(s).ToLower() == lower);
                if (targetScriptFile != null)
                {
                    // Load the script and enforce to reload the tree.
                    LoadPex(targetScriptFile);
                    return true;
                }
            }
            return false;
        }

        private string GetParameterString(List<PapyrusParameterDefinition> parameters, bool includeParameterNames = false)
        {
            var paramDefs = string.Join(", ", parameters.Select(p => p.TypeName.Value +
            (includeParameterNames ? (" " + p.Name.Value) : "")));

            return "(" + paramDefs + ")";
        }

        public ObservableCollection<PapyrusViewModel> PexTree
        {
            get { return pexTree; }
            set { Set(ref pexTree, value); }
        }

        private void SelectMember(PapyrusViewModel item)
        {
            var method = item.Item as PapyrusMethodDefinition;

            if (method != null)
            {
                selectedMethod = method;
                SelectedMethodInstructions = new ObservableCollection<PapyrusInstruction>(
                        method.Body.Instructions
                    );

                SelectedMethodParameters = new ObservableCollection<PapyrusParameterDefinition>(
                        method.Parameters
                    );

                SelectedMethodVariables = new ObservableCollection<PapyrusVariableReference>(
                        method.GetVariables()
                    );
            }
        }

        private ObservableCollection<PapyrusInstruction> selectedMethodInstructions;
        private ObservableCollection<PapyrusViewModel> pexTree;
        private ObservableCollection<PapyrusParameterDefinition> selectedMethodParameters;
        private ObservableCollection<PapyrusVariableReference> selectedMethodVariables;
        private PapyrusInstruction selectedMethodInstruction;

        public ObservableCollection<PapyrusInstruction> SelectedMethodInstructions
        {
            get { return selectedMethodInstructions; }
            set { Set(ref selectedMethodInstructions, value); }
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

        public ICommand OpenPexCommand { get; set; }

        public RelayCommand SavePexCommand { get; set; }

        public RelayCommand SavePexAsCommand { get; set; }

        public ICommand ExitCommand { get; set; }


        public RelayCommand InsertBeforeCommand { get; set; }

        public RelayCommand InsertAfterCommand { get; set; }

        public RelayCommand EditInstructionCommand { get; set; }

        public RelayCommand RemoveInstructionCommand { get; set; }

        public RelayCommand<PapyrusViewModel> SelectedMemberCommand { get; set; }

        public PapyrusInstruction SelectedMethodInstruction
        {
            get { return selectedMethodInstruction; }
            set { Set(ref selectedMethodInstruction, value); }
        }


        private static MainWindowViewModel designInstance;

        public static MainWindowViewModel DesignInstance = designInstance ??
                                                           (designInstance = new MainWindowViewModel(null));

        private Interfaces.IDialogService dialogService;
        private PapyrusMethodDefinition selectedMethod;
    }
}
