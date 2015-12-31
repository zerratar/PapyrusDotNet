using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Extensions;
using PapyrusDotNet.Common.Extensions;

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
            SavePexCommand = new RelayCommand<object>(SavePex, CanSave);
            SavePexAsCommand = new RelayCommand<object>(SavePexAs, CanSave);

            ReloadPexCommand = new RelayCommand<object>(ReloadPex, (o) => o != null);

            SelectedMemberCommand = new RelayCommand<PapyrusViewModel>(SelectMember);

            InsertAfterCommand = new RelayCommand(InsertAfter, CanInsert);
            InsertBeforeCommand = new RelayCommand(InsertBefore, CanInsert);
            EditInstructionCommand = new RelayCommand(EditInstruction, CanInsert);
            RemoveInstructionCommand = new RelayCommand(RemoveInstruction, CanInsert);

            CreateInstructionCommand = new RelayCommand(CreateInstruction, CanCreate);


            CreateVariableCommand = new RelayCommand(CreateVariable, CanCreate);
            EditVariableCommand = new RelayCommand(EditVariable, CanEditVar);
            DeleteVariableCommand = new RelayCommand(DeleteVariable, CanEditVar);


            CreateParameterCommand = new RelayCommand(CreateParameter, CanCreate);
            EditParameterCommand = new RelayCommand(EditParameter, CanEditParameter);
            DeleteParameterCommand = new RelayCommand(DeleteParameter, CanEditParameter);

            TargetGameName = "Unknown";
            SelectedMemberFlags = "<none>";
            SelectedMemberName = new ObservableCollection<Inline>(new[] { new Run("Nothing Selected") });
        }

        private void ReloadPex(object i)
        {
            var item = i as PapyrusViewModel;
            if (item != null)
            {
                var top = item.GetTopParent();
                var asm = top.Item as PapyrusAssemblyDefinition;
                if (asm != null)
                {
                    var index = LoadedAssemblies.IndexOf(asm);

                    LoadedAssemblies[index] = PapyrusAssemblyDefinition.ReloadAssembly(asm);

                    BuildPexTree(top);

                    SelectedMethodInstructions = new ObservableCollection<PapyrusInstruction>();

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


        private void EditInstruction()
        {
            var dialog = new PapyrusInstructionEditorViewModel(dialogService, LoadedAssemblies, selectedMethod.DeclaringState?.DeclaringType?.Assembly,
                selectedMethod.DeclaringState?.DeclaringType,
                selectedMethod, SelectedMethodInstruction);
            var result = dialogService.ShowDialog(dialog);
            if (result == DialogResult.OK)
            {
                var inst = SelectedMethodInstruction;
                //inst.Operand = dialog.Operand;
                inst.OpCode = dialog.SelectedOpCode;
                inst.Arguments = dialog.Arguments;
                inst.OperandArguments = new List<PapyrusVariableReference>(dialog.OperandArguments);
                selectedMethod.Body.Instructions.RecalculateOffsets();
                selectedMethod.UpdateInstructionOperands();
                SelectedMethodInstructions = new ObservableCollection<PapyrusInstruction>(selectedMethod.Body.Instructions);
                selectedMethodNode.SetDirty(true);
            }
        }

        private void CreateInstruction()
        {
            var dialog = new PapyrusInstructionEditorViewModel(dialogService,
            LoadedAssemblies,
            selectedMethod.DeclaringState?.DeclaringType?.Assembly,
            selectedMethod.DeclaringState?.DeclaringType, selectedMethod);
            var result = dialogService.ShowDialog(dialog);
            if (result == DialogResult.OK)
            {
                var newInstruction = new PapyrusInstruction();
                newInstruction.OpCode = dialog.SelectedOpCode;
                //newInstruction.Operand = dialog.Operand;
                newInstruction.Arguments = dialog.Arguments;
                newInstruction.OperandArguments = new List<PapyrusVariableReference>(dialog.OperandArguments);
                selectedMethod.Body.Instructions.Add(newInstruction);
                selectedMethod.Body.Instructions.RecalculateOffsets();
                selectedMethod.UpdateInstructionOperands();
                SelectedMethodInstructions = new ObservableCollection<PapyrusInstruction>(selectedMethod.Body.Instructions);
                selectedMethodNode.SetDirty(true);
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
                var index = SelectedMethodInstructions.IndexOf(inst);
                if (index < 0) index = 0;

                var newInstruction = new PapyrusInstruction();
                newInstruction.OpCode = dialog.SelectedOpCode;
                //newInstruction.Operand = dialog.Operand;
                newInstruction.Arguments = dialog.Arguments;
                newInstruction.OperandArguments = new List<PapyrusVariableReference>(dialog.OperandArguments);
                selectedMethod.Body.Instructions.Insert(index, newInstruction);
                selectedMethod.Body.Instructions.RecalculateOffsets();
                selectedMethod.UpdateInstructionOperands();
                SelectedMethodInstructions = new ObservableCollection<PapyrusInstruction>(selectedMethod.Body.Instructions);
                selectedMethodNode.SetDirty(true);
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
                //newInstruction.Operand = dialog.Operand;
                newInstruction.Arguments = dialog.Arguments;
                newInstruction.OperandArguments = new List<PapyrusVariableReference>(dialog.OperandArguments);
                selectedMethod.Body.Instructions.Insert(index, newInstruction);
                selectedMethod.Body.Instructions.RecalculateOffsets();

                selectedMethod.UpdateInstructionOperands();
                //selectedMethod.Body.Instructions.ForEach(i => selectedMethod.DeclaringState.DeclaringType.UpdateOperand(i, selectedMethod.Body.Instructions));

                SelectedMethodInstructions = new ObservableCollection<PapyrusInstruction>(selectedMethod.Body.Instructions);
                selectedMethodNode.SetDirty(true);
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
                var typeName = dialog.SelectedType ?? dialog.SelectedTypeName;
                var varName = dialog.Name;

                var.Name = varName.Ref(asm);
                var.TypeName = typeName.ToString().Ref(asm);

                SelectedMethodVariables = new ObservableCollection<PapyrusVariableReference>(selectedMethod.GetVariables());
                selectedMethodNode.SetDirty(true);
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
                var typeName = dialog.SelectedType ?? dialog.SelectedTypeName;
                var varName = dialog.Name;

                if (varName.ToLower().StartsWith("::temp"))
                {
                    selectedMethod.Body.Variables.Add(new PapyrusVariableReference
                    {
                        Name = varName.Ref(asm),
                        Value = varName,
                        TypeName = typeName.ToString().Ref(asm),
                        ValueType = PapyrusPrimitiveType.Reference
                    });
                }
                else
                {
                    selectedMethod.Body.Variables.Add(new PapyrusVariableReference
                    {
                        Name = varName.Ref(asm),
                        Value = varName,
                        TypeName = typeName.ToString().Ref(asm),
                        ValueType = PapyrusPrimitiveType.Reference
                    });
                }

                SelectedMethodVariables = new ObservableCollection<PapyrusVariableReference>(
                    selectedMethod.GetVariables()
                );
                selectedMethodNode.SetDirty(true);
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
                var typeName = dialog.SelectedType ?? dialog.SelectedTypeName;
                var varName = dialog.Name;

                var.Name = varName.Ref(asm);
                var.TypeName = typeName.ToString().Ref(asm);

                SelectedMethodParameters = new ObservableCollection<PapyrusParameterDefinition>(
                        selectedMethod.Parameters
                    );
                selectedMethodNode.SetDirty(true);
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
                var typeName = dialog.SelectedType ?? dialog.SelectedTypeName;

                var varName = dialog.Name;

                selectedMethod.Parameters.Add(new PapyrusParameterDefinition()
                {
                    Name = varName.Ref(asm),
                    TypeName = typeName.ToString().Ref(asm)
                });

                SelectedMethodParameters = new ObservableCollection<PapyrusParameterDefinition>(
                        selectedMethod.Parameters
                    );
                selectedMethodNode.SetDirty(true);
            }
        }

        public PapyrusVariableReference CreateReferenceFromName(string name, PapyrusAssemblyDefinition asm)
        {
            var nameRef = name.Ref(asm);
            return new PapyrusVariableReference()
            {
                Name = nameRef,
                Value = nameRef.Value,
                ValueType = PapyrusPrimitiveType.Reference
            };
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
                selectedMethodNode.SetDirty(true);
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

                if (method.Body.Variables.Contains(obj))
                    method.Body.Variables.Remove(obj);

                SelectedMethodVariables = new ObservableCollection<PapyrusVariableReference>(
                        method.GetVariables()
                    );
                selectedMethodNode.SetDirty(true);
            }
        }

        private void RemoveInstruction()
        {
            var obj = SelectedMethodInstruction;
            if (MessageBox.Show("WARNING: Are you sure you want to delete this instruction?",
                "Delete Instruction L_" + obj.Offset.ToString("000") + ": " + obj.OpCode, MessageBoxButton.OKCancel)
                == MessageBoxResult.OK)
            {
                var method = selectedMethod;

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
                selectedMethodNode.SetDirty(true);
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

        private bool CanInsert() => SelectedMethodInstruction != null;

        private bool CanEditParameter() => SelectedMethodParameter != null;

        private bool CanEditVar() => SelectedMethodVariable != null;

        private bool CanSave(object o)
        {
            var i = o as PapyrusViewModel;
            return i != null && i.IsDirty;
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
            var name = Path.GetFileName(fileName);
            var directoryName = Path.GetDirectoryName(fileName);

            if (name != null && LoadedAssemblyNames.ContainsKey(name.ToLower()))
            {
                if (MessageBox.Show("This file has already been loaded.\r\nDo you want to reload it?", "Reload?",
                        MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            var loadedAssembly = PapyrusAssemblyDefinition.ReadAssembly(fileName);
            int loadIndex = -1;
            if (name != null && LoadedAssemblyNames.ContainsKey(name.ToLower()))
            {
                loadIndex = Array.IndexOf(LoadedAssemblyNames.Values.ToArray(), name.ToLower());
            }

            if (loadIndex == -1)
            {
                LoadedAssemblies.Add(loadedAssembly);

                if (name != null) LoadedAssemblyNames.Add(name.ToLower(), name.ToLower());
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

        private void BuildPexTree(PapyrusViewModel target = null)
        {
            if (target != null)
            {
                var itemIndex = PexTree.IndexOf(target);
                var asm = target.Item as PapyrusAssemblyDefinition;
                if (asm != null)
                {
                    var asmnames = LoadedAssemblyNames.Values.ToArray();
                    var asmIndex = LoadedAssemblies.IndexOf(i => i.Types.First().Name.Value == asm.Types.First().Name.Value);
                    PapyrusViewModel newNode;
                    if (BuildPexTree(asmIndex, asmnames, out newNode)) return;

                    PexTree.RemoveAt(itemIndex);
                    PexTree.Insert(itemIndex, newNode);
                }
            }
            else
            {
                var asmnames = LoadedAssemblyNames.Values.ToArray();
                var rootNodes = new List<PapyrusViewModel>();
                for (int index = 0; index < LoadedAssemblies.Count; index++)
                {
                    PapyrusViewModel newNode;
                    if (BuildPexTree(index, asmnames, out newNode)) return; // the tree will be reloaded, so we don't wanna finish it here.
                    if (newNode != null)
                    {
                        rootNodes.Add(newNode);
                    }
                }
                PexTree = new ObservableCollection<PapyrusViewModel>(rootNodes.OrderBy(i => i.Text));
            }
        }

        private bool BuildPexTree(int assemblyIndex, string[] asmnames, out PapyrusViewModel root)
        {
            var asm = LoadedAssemblies[assemblyIndex];
            root = new PapyrusViewModel();
            root.Item = asm;
            root.Text = asmnames[assemblyIndex];
            foreach (var type in asm.Types)
            {
                var typeNode = new PapyrusViewModel(root);
                typeNode.Item = type;
                typeNode.Text = type.Name.Value +
                                (type.BaseTypeName != null && !string.IsNullOrEmpty(type.BaseTypeName.Value)
                                    ? " : " + type.BaseTypeName.Value
                                    : "");

                if (type.BaseTypeName != null && !string.IsNullOrEmpty(type.BaseTypeName.Value))
                {
                    if (EnsureAssemblyLoaded(type.BaseTypeName.Value)) return true;
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
                            if (type.BaseTypeName != null && EnsureAssemblyLoaded(type.BaseTypeName.Value)) return true;
                        }
                    }
                }

                //var statesNode = new PapyrusViewModel(typeNode);
                //statesNode.Item = "states";
                //statesNode.Text = "States";
                foreach (var item in type.States.OrderBy(i => i.Name.Value))
                {
                    var stateNode = new PapyrusViewModel(typeNode);
                    stateNode.Item = item;
                    stateNode.Text = (!string.IsNullOrEmpty(item.Name.Value) ? item.Name.Value : "<default>");
                    foreach (var method in item.Methods.OrderBy(i => i.Name.Value))
                    {
                        var m = new PapyrusViewModel(stateNode);
                        m.Item = method;
                        m.Text = method.Name.Value + GetParameterString(method.Parameters) + " : " +
                                 method.ReturnTypeName.Value;

                        if (!string.IsNullOrEmpty(method.ReturnTypeName.Value))
                        {
                            if (EnsureAssemblyLoaded(method.ReturnTypeName.Value)) return true;
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
                        if (EnsureAssemblyLoaded(field.TypeName)) return true;
                    }
                }

                foreach (var item in type.Properties.OrderBy(i => i.Name.Value))
                {
                    var fieldNode = new PapyrusViewModel(typeNode);
                    fieldNode.Item = item;
                    fieldNode.Text = item.Name.Value + " : " + item.TypeName.Value;

                    if (!string.IsNullOrEmpty(item.TypeName.Value))
                    {
                        if (EnsureAssemblyLoaded(item.TypeName.Value)) return true;
                    }

                    if (item.HasGetter && item.GetMethod != null)
                    {
                        var method = item.GetMethod;
                        var m = new PapyrusViewModel(fieldNode);
                        m.Item = method;
                        m.Text = "Getter" + GetParameterString(method.Parameters) + " : " +
                                 method.ReturnTypeName.Value;
                    }

                    if (item.HasSetter && item.SetMethod != null)
                    {
                        var method = item.SetMethod;
                        var m = new PapyrusViewModel(fieldNode);
                        m.Item = method;
                        m.Text = "Setter" + GetParameterString(method.Parameters) + " : " +
                                 method.ReturnTypeName.Value;
                    }
                }
            }

            return false;
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
                          i => Directory.GetFiles(i, "*.pex", SearchOption.AllDirectories)).ToList();

                    var items = discoveredScripts.Select(
                        i => new { Name = Path.GetFileNameWithoutExtension(i)?.ToLower(), FullPath = i });

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
            selectedNode = item;
            if (item == null) return;


            BuildMemberDisplay(item.Item);
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
                SelectedMethodInstructions = new ObservableCollection<PapyrusInstruction>(
                    method.Body.Instructions
                    );

                SelectedMethodParameters = new ObservableCollection<PapyrusParameterDefinition>(
                    method.Parameters
                    );

                SelectedMethodVariables = new ObservableCollection<PapyrusVariableReference>(
                    method.GetVariables()
                    );

                SelectedMemberFlags = "0x" + method.Flags.ToString("X");

                TargetGameName = method.DeclaringAssembly.VersionTarget.ToString();
            }

            CreateInstructionCommand.RaiseCanExecuteChanged();
            CreateParameterCommand.RaiseCanExecuteChanged();
            CreateVariableCommand.RaiseCanExecuteChanged();

            ReloadPexCommand.RaiseCanExecuteChanged();
            SavePexAsCommand.RaiseCanExecuteChanged();
            SavePexCommand.RaiseCanExecuteChanged();
        }

        public void BuildMemberDisplay(object item)
        {
            var displayItems = new List<Run>();
            var type = item as PapyrusTypeDefinition;
            if (type != null)
            {
                displayItems.Add(new Run(type.Name.Value) { Foreground = MethodColor, FontWeight = FontWeights.DemiBold });
                if (type.BaseTypeName != null && !string.IsNullOrEmpty(type.BaseTypeName.Value))
                {
                    displayItems.Add(new Run(" : "));
                    displayItems.Add(new Run(type.BaseTypeName.Value) { Foreground = TypeColor });
                }
            }
            var state = item as PapyrusStateDefinition;
            if (state != null)
            {
                displayItems.Add(new Run(string.IsNullOrEmpty(state.Name.Value) ? "<Default>" : state.Name.Value));
                displayItems.Add(new Run(" State"));
            }
            var assembly = item as PapyrusAssemblyDefinition;
            if (assembly != null)
            {
                displayItems.Add(new Run(assembly.Types.First().Name.Value + ".pex"));
            }
            var prop = item as PapyrusPropertyDefinition;
            if (prop != null)
            {
                if (prop.IsAuto)
                {
                    displayItems.Add(new Run("Auto") { Foreground = AttributeColor });
                    displayItems.Add(new Run(" "));
                }

                displayItems.Add(new Run("Property") { Foreground = AttributeColor });
                displayItems.Add(new Run(" "));

                displayItems.Add(new Run(prop.TypeName.Value) { Foreground = TypeColor });
                displayItems.Add(new Run(" "));
                displayItems.Add(new Run(prop.Name.Value));
            }
            var field = item as PapyrusFieldDefinition;
            if (field != null)
            {
                displayItems.Add(new Run(field.TypeName) { Foreground = TypeColor });
                displayItems.Add(new Run(" "));
                displayItems.Add(new Run(field.Name.Value));
            }
            var method = item as PapyrusMethodDefinition;
            if (method != null)
            {
                if (method.IsNative)
                {
                    displayItems.Add(new Run("Native") { Foreground = AttributeColor });
                    displayItems.Add(new Run(" "));
                }
                if (method.IsGlobal)
                {
                    displayItems.Add(new Run("Global") { Foreground = AttributeColor });
                    displayItems.Add(new Run(" "));
                }

                if (method.IsEvent)
                {
                    displayItems.Add(new Run("Event") { Foreground = AttributeColor });
                    displayItems.Add(new Run(" "));
                }
                else
                {
                    displayItems.Add(new Run(method.ReturnTypeName.Value) { Foreground = TypeColor });
                    displayItems.Add(new Run(" "));
                }
                var nameRef = method.Name;
                var name = nameRef?.Value ?? (method.IsSetter ? method.PropName + ".Setter" : method.IsGetter ? method.PropName + ".Getter" : "?????");
                displayItems.Add(new Run(name) { Foreground = MethodColor, FontWeight = FontWeights.DemiBold });
                displayItems.AddRange(GetParameterRuns(method.Parameters));
                displayItems.Add(new Run(";"));
            }
            SelectedMemberName = new ObservableCollection<Inline>(displayItems.ToArray());
        }


        private List<Run> GetParameterRuns(List<PapyrusParameterDefinition> parameters)
        {
            var output = new List<Run>();
            output.Add(new Run("("));
            for (int index = 0; index < parameters.Count; index++)
            {
                var p = parameters[index];
                output.Add(new Run(p.TypeName.Value) { Foreground = TypeColor });
                output.Add(new Run(" "));
                output.Add(new Run(p.Name.Value));

                if (index != parameters.Count - 1)
                {
                    output.Add(new Run(", "));
                }
            }

            output.Add(new Run(")"));
            return output;
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

        public PapyrusInstruction SelectedMethodInstruction
        {
            get { return selectedMethodInstruction; }
            set { Set(ref selectedMethodInstruction, value); }
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

        public ICommand ExitCommand { get; set; }
        public RelayCommand InsertBeforeCommand { get; set; }
        public RelayCommand InsertAfterCommand { get; set; }
        public RelayCommand EditInstructionCommand { get; set; }
        public RelayCommand RemoveInstructionCommand { get; set; }
        public RelayCommand<PapyrusViewModel> SelectedMemberCommand { get; set; }
        public RelayCommand CreateVariableCommand { get; set; }
        public RelayCommand EditVariableCommand { get; set; }
        public RelayCommand DeleteVariableCommand { get; set; }
        public RelayCommand CreateParameterCommand { get; set; }
        public RelayCommand EditParameterCommand { get; set; }
        public RelayCommand DeleteParameterCommand { get; set; }
        public RelayCommand CreateInstructionCommand { get; set; }

        private static MainWindowViewModel designInstance;
        public static MainWindowViewModel DesignInstance = designInstance ??
                                                           (designInstance = new MainWindowViewModel(null));

        private static SolidColorBrush AttributeColor = new SolidColorBrush(Color.FromRgb(30, 78, 135));
        private static SolidColorBrush TypeColor = new SolidColorBrush(Color.FromRgb(30, 135, 75));
        private static SolidColorBrush MethodColor = new SolidColorBrush(Color.FromRgb(44, 62, 80));

        private Interfaces.IDialogService dialogService;
        private PapyrusMethodDefinition selectedMethod;
        private List<string> discoveredScripts;
        private Dictionary<string, string> discoveredScriptNames;
        private PapyrusVariableReference selectedMethodVariable;
        private PapyrusParameterDefinition selectedMethodParameter;
        private ObservableCollection<PapyrusInstruction> selectedMethodInstructions;
        private ObservableCollection<PapyrusViewModel> pexTree;
        private ObservableCollection<PapyrusParameterDefinition> selectedMethodParameters;
        private ObservableCollection<PapyrusVariableReference> selectedMethodVariables;
        private PapyrusInstruction selectedMethodInstruction;
        private ObservableCollection<Inline> selectedMemberName;
        private string targetGameName;
        private object selectedMemberFlags;
        private PapyrusViewModel selectedMethodNode;
        private PapyrusViewModel selectedNode;
        private RelayCommand<object> savePexCommand;
        private RelayCommand<object> savePexAsCommand;
        private RelayCommand<object> reloadPexCommand;
    }
}
