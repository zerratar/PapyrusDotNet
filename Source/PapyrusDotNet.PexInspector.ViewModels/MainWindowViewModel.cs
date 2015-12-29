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
using PapyrusDotNet.PapyrusAssembly.Extensions;

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public List<PapyrusAssemblyDefinition> LoadedAssemblies = new List<PapyrusAssemblyDefinition>();
        public Dictionary<string, string> LoadedAssemblyNames = new Dictionary<string, string>();
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


            CreateVariableCommand = new RelayCommand(CreateVariable);
            EditVariableCommand = new RelayCommand(EditVariable, CanEditVar);
            DeleteVariableCommand = new RelayCommand(DeleteVariable, CanEditVar);


            CreateParameterCommand = new RelayCommand(CreateParameter);
            EditParameterCommand = new RelayCommand(EditParameter, CanEditParameter);
            DeleteParameterCommand = new RelayCommand(DeleteParameter, CanEditParameter);

        }

        private void EditInstruction()
        {
            var dialog = new PapyrusInstructionEditorViewModel(dialogService, LoadedAssemblies, selectedMethod.DeclaringState?.DeclaringType?.Assembly,
                selectedMethod.DeclaringState?.DeclaringType,
                selectedMethod, SelectedMethodInstruction);
            var result = dialogService.ShowDialog(dialog);
            if (result == DialogResult.OK)
            {
                var inst = SelectedMethodInstruction;
                inst.Operand = dialog.Operand;
                inst.OpCode = dialog.SelectedOpCode;
                inst.Arguments = dialog.Arguments;
                inst.OperandArguments = dialog.OperandArguments;
                selectedMethod.Body.Instructions.RecalculateOffsets();

                SelectedMethodInstructions = new ObservableCollection<PapyrusInstruction>(selectedMethod.Body.Instructions);
            }
        }

        private void InsertBefore()
        {
            var dialog = new PapyrusInstructionEditorViewModel(dialogService,
                LoadedAssemblies,
                selectedMethod.DeclaringState?.DeclaringType?.Assembly,
                selectedMethod.DeclaringState?.DeclaringType, selectedMethod);
            var result = dialogService.ShowDialog(dialog);
            if (result == DialogResult.OK)
            {
                var inst = SelectedMethodInstruction;
                var index = SelectedMethodInstructions.IndexOf(inst) - 1;
                if (index < 0) index = 0;

                var newInstruction = new PapyrusInstruction();
                newInstruction.OpCode = dialog.SelectedOpCode;
                newInstruction.Operand = dialog.Operand;
                newInstruction.Arguments = dialog.Arguments;
                newInstruction.OperandArguments = dialog.OperandArguments;
                selectedMethod.Body.Instructions.Insert(index, newInstruction);
                selectedMethod.Body.Instructions.RecalculateOffsets();

                SelectedMethodInstructions = new ObservableCollection<PapyrusInstruction>(selectedMethod.Body.Instructions);
            }
        }

        private void InsertAfter()
        {
            var dialog = new PapyrusInstructionEditorViewModel(dialogService, LoadedAssemblies,
                selectedMethod.DeclaringState?.DeclaringType?.Assembly,
                selectedMethod.DeclaringState?.DeclaringType, selectedMethod);
            var result = dialogService.ShowDialog(dialog);
            if (result == DialogResult.OK)
            {
                var inst = SelectedMethodInstruction;
                var index = SelectedMethodInstructions.IndexOf(inst) + 1;

                var newInstruction = new PapyrusInstruction();
                newInstruction.OpCode = dialog.SelectedOpCode;
                newInstruction.Operand = dialog.Operand;
                newInstruction.Arguments = dialog.Arguments;
                newInstruction.OperandArguments = dialog.OperandArguments;
                selectedMethod.Body.Instructions.Insert(index, newInstruction);
                selectedMethod.Body.Instructions.RecalculateOffsets();

                SelectedMethodInstructions = new ObservableCollection<PapyrusInstruction>(selectedMethod.Body.Instructions);
            }
        }

        private void EditVariable()
        {
            var loadedTypes = LoadedAssemblies.SelectMany(t => t.Types.Select(j => j.Name.Value));
            var dialog = new PapyrusVariableEditorViewModel(loadedTypes, SelectedMethodVariable);
            var result = dialogService.ShowDialog(dialog);
            if (result == DialogResult.OK)
            {
                var asm = selectedMethod.DeclaringState.DeclaringType.Assembly;
                var var = SelectedMethodVariable;
                var typeName = dialog.SelectedType;
                var varName = dialog.Name;

                var.Name = varName.Ref(asm);
                var.TypeName = typeName.ToString().Ref(asm);

                SelectedMethodVariables = new ObservableCollection<PapyrusVariableReference>(
                        selectedMethod.GetVariables()
                    );
            }
        }

        private void CreateVariable()
        {
            var loadedTypes = LoadedAssemblies.SelectMany(t => t.Types.Select(j => j.Name.Value));
            var dialog = new PapyrusVariableEditorViewModel(loadedTypes);
            var result = dialogService.ShowDialog(dialog);
            if (result == DialogResult.OK)
            {
                var asm = selectedMethod.DeclaringState.DeclaringType.Assembly;
                var typeName = dialog.SelectedType;
                var varName = dialog.Name;

                if (varName.ToLower().StartsWith("::temp"))
                {
                    selectedMethod.Body.TempVariables.Add(new PapyrusVariableReference
                    {
                        Name = varName.Ref(asm),
                        TypeName = typeName.ToString().Ref(asm)
                    });
                }
                else
                {
                    selectedMethod.Body.Variables.Add(new PapyrusVariableReference
                    {
                        Name = varName.Ref(asm),
                        TypeName = typeName.ToString().Ref(asm)
                    });
                }

                SelectedMethodVariables = new ObservableCollection<PapyrusVariableReference>(
                    selectedMethod.GetVariables()
                );
            }
        }


        private void EditParameter()
        {
            var loadedTypes = LoadedAssemblies.SelectMany(t => t.Types.Select(j => j.Name.Value));
            var dialog = new PapyrusParameterEditorViewModel(loadedTypes, SelectedMethodParameter);
            var result = dialogService.ShowDialog(dialog);
            if (result == DialogResult.OK)
            {
                var asm = selectedMethod.DeclaringState.DeclaringType.Assembly;
                var var = SelectedMethodParameter;
                var typeName = dialog.SelectedType;
                var varName = dialog.Name;

                var.Name = varName.Ref(asm);
                var.TypeName = typeName.ToString().Ref(asm);

                SelectedMethodParameters = new ObservableCollection<PapyrusParameterDefinition>(
                        selectedMethod.Parameters
                    );
            }
        }

        private void CreateParameter()
        {
            var loadedTypes = LoadedAssemblies.SelectMany(t => t.Types.Select(j => j.Name.Value));
            var dialog = new PapyrusParameterEditorViewModel(loadedTypes);
            var result = dialogService.ShowDialog(dialog);
            if (result == DialogResult.OK)
            {
                var asm = selectedMethod.DeclaringState.DeclaringType.Assembly;
                var typeName = dialog.SelectedType;
                var varName = dialog.Name;

                selectedMethod.Parameters.Add(new PapyrusParameterDefinition()
                {
                    Name = varName.Ref(asm),
                    TypeName = typeName.ToString().Ref(asm)
                });

                SelectedMethodParameters = new ObservableCollection<PapyrusParameterDefinition>(
                        selectedMethod.Parameters
                    );
            }
        }


        private void DeleteParameter()
        {
            var obj = SelectedMethodParameter;
            string name = obj.Name?.Value ?? "";
            if (MessageBox.Show("WARNING: It could be used by any existing instructions, and if this method is being called from somewhere else, that call needs to be updated or the scripts will stop working.\r\n----------------------\r\nDeleting this parameter will not modify any existing instructions.\r\nAre you sure you want to delete this parameter?",
                "Delete Parameter " + name, MessageBoxButton.OKCancel)
                == MessageBoxResult.OK)
            {
                var method = selectedMethod;

                if (method.Parameters.Contains(obj))
                    method.Parameters.Remove(obj);

                SelectedMethodParameters = new ObservableCollection<PapyrusParameterDefinition>(
                        method.Parameters
                    );
            }
        }

        private void DeleteVariable()
        {
            var obj = SelectedMethodVariable;
            string name = obj.Name?.Value ?? "";
            if (MessageBox.Show("WARNING: It could be used by any existing instructions.\r\n----------------------\r\nDeleting this variable will not modify any existing instructions.\r\nAre you sure you want to delete this variable?",
                "Delete Variable " + name, MessageBoxButton.OKCancel)
                == MessageBoxResult.OK)
            {
                var method = selectedMethod;

                if (method.Body.TempVariables.Contains(obj))
                    method.Body.TempVariables.Remove(obj);
                else
                    method.Body.Variables.Remove(obj);

                SelectedMethodVariables = new ObservableCollection<PapyrusVariableReference>(
                        method.GetVariables()
                    );
            }
        }

        private void RemoveInstruction()
        {
            var obj = SelectedMethodInstruction;
            if (MessageBox.Show("WARNING: Are you sure you want to delete this instruction?",
                "Delete Instruction L_" + obj.Offset.ToString("000") + ": " + obj.OpCode, MessageBoxButton.OKCancel)
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



        private void Exit()
        {
            Application.Current.Shutdown(-1);
        }

        private void SavePexAs()
        {

        }
        private bool CanInsert() => SelectedMethodInstruction != null;

        private bool CanEditParameter() => SelectedMethodParameter != null;

        private bool CanEditVar() => SelectedMethodVariable != null;

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

            if (LoadedAssemblyNames.ContainsKey(name.ToLower()))
            {
                if (MessageBox.Show("This file has already been loaded.\r\nDo you want to reload it?", "Reload?",
                        MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            var loadedAssembly = PapyrusAssemblyDefinition.ReadAssembly(fileName);
            int loadIndex = -1;
            if (LoadedAssemblyNames.ContainsKey(name.ToLower()))
            {
                loadIndex = Array.IndexOf(LoadedAssemblyNames.Values.ToArray(), name.ToLower());
            }

            if (loadIndex == -1)
            {
                LoadedAssemblies.Add(loadedAssembly);

                LoadedAssemblyNames.Add(name.ToLower(), name.ToLower());
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
            var asmnames = LoadedAssemblyNames.Values.ToArray();
            var RootNodes = new List<PapyrusViewModel>();
            for (int index = 0; index < LoadedAssemblies.Count; index++)
            {
                var asm = LoadedAssemblies[index];
                var root = new PapyrusViewModel();
                root.Item = "root";
                root.Text = asmnames[index];
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
                        foreach (var method in item.Methods.OrderBy(i => i.Name.Value))
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

                    foreach (var item in type.Properties.OrderBy(i => i.Name.Value))
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
            lower = lower.Replace("[]", "");
            // do not try and load any value types
            if (lower == "int" || lower == "string" || lower == "bool" || lower == "none" || lower == "float") return false;

            if (discoveredScriptNames == null)
                discoveredScriptNames =
                    new Dictionary<string, string>();

            if (!LoadedAssemblyNames.ContainsKey(lower + ".pex"))
            {
                if (discoveredScripts == null)
                {
                    discoveredScripts = LoadedAssemblyFolders.SelectMany(
                          i => System.IO.Directory.GetFiles(i, "*.pex", SearchOption.AllDirectories)).ToList();

                    var items = discoveredScripts.Select(
                        i => new { Name = Path.GetFileNameWithoutExtension(i).ToLower(), FullPath = i });



                    items.ForEach(i => { if (!discoveredScriptNames.ContainsKey(i.Name)) discoveredScriptNames.Add(i.Name, i.FullPath); });
                }
                if (discoveredScriptNames.ContainsKey(lower))
                {
                    var targetScriptFile = discoveredScriptNames[lower];
                    if (targetScriptFile != null)
                    {
                        // Load the script and enforce to reload the tree.
                        LoadPex(targetScriptFile);
                        return true;
                    }
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

        public ICommand CreateVariableCommand { get; set; }

        public RelayCommand EditVariableCommand { get; set; }

        public RelayCommand DeleteVariableCommand { get; set; }

        public ICommand CreateParameterCommand { get; set; }

        public RelayCommand EditParameterCommand { get; set; }

        public RelayCommand DeleteParameterCommand { get; set; }

        private static MainWindowViewModel designInstance;

        public static MainWindowViewModel DesignInstance = designInstance ??
                                                           (designInstance = new MainWindowViewModel(null));

        private Interfaces.IDialogService dialogService;
        private PapyrusMethodDefinition selectedMethod;
        private List<string> discoveredScripts;
        private Dictionary<string, string> discoveredScriptNames;
        private PapyrusVariableReference selectedMethodVariable;
        private PapyrusParameterDefinition selectedMethodParameter;
    }
}
