using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private PapyrusAssemblyDefinition loadedAssembly;
        private string loadedAssemblyName;
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
            var result = dialogService.ShowDialog(new PapyrusInstructionEditorViewModel(SelectedMethodInstruction));
        }

        private void InsertBefore()
        {
            var result = dialogService.ShowDialog(new PapyrusInstructionEditorViewModel());
        }

        private bool CanInsert() => SelectedMethodInstruction != null;

        private void InsertAfter()
        {
            var result = dialogService.ShowDialog(new PapyrusInstructionEditorViewModel());
        }

        private void Exit()
        {

        }

        private void SavePexAs()
        {

        }

        private bool CanSave() => loadedAssembly != null;

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
            loadedAssembly = PapyrusAssemblyDefinition.ReadAssembly(fileName);
            loadedAssemblyName = System.IO.Path.GetFileName(fileName);
            BuildPexTree();

            SavePexCommand.RaiseCanExecuteChanged();
            SavePexAsCommand.RaiseCanExecuteChanged();
        }

        private void BuildPexTree()
        {
            var asm = loadedAssembly;
            var root = new PapyrusViewModel();
            root.Item = "root";
            root.Text = loadedAssemblyName;
            foreach (var type in asm.Types)
            {
                var typeNode = new PapyrusViewModel(root);
                typeNode.Item = type;
                typeNode.Text = type.Name.Value;
                foreach (var structType in type.NestedTypes)
                {
                    var structTypeNode = new PapyrusViewModel(typeNode);
                    structTypeNode.Item = structType;
                    structTypeNode.Text = structType.Name.Value;
                    foreach (var field in structType.Fields)
                    {
                        var fieldNode = new PapyrusViewModel(structTypeNode);
                        fieldNode.Item = field;
                        fieldNode.Text = field.Name.Value + " : " + field.TypeName;
                    }
                }

                var statesNode = new PapyrusViewModel(typeNode);
                statesNode.Item = "states";
                statesNode.Text = "States";
                foreach (var item in type.States)
                {
                    var stateNode = new PapyrusViewModel(statesNode);
                    stateNode.Item = item;
                    stateNode.Text = (!string.IsNullOrEmpty(item.Name.Value) ? item.Name.Value : "default");
                    foreach (var method in item.Methods)
                    {
                        var m = new PapyrusViewModel(stateNode);
                        m.Item = method;
                        m.Text = method.Name.Value + GetParameterString(method.Parameters) + " : " +
                                 method.ReturnTypeName.Value;
                    }
                }

                foreach (var field in type.Fields)
                {
                    var fieldNode = new PapyrusViewModel(typeNode);
                    fieldNode.Item = field;
                    fieldNode.Text = field.Name.Value + " : " + field.TypeName;
                }

                foreach (var item in type.Properties)
                {
                    var fieldNode = new PapyrusViewModel(typeNode);
                    fieldNode.Item = item;
                    fieldNode.Text = item.Name.Value + " : " + item.TypeName.Value;
                }


            }
            PexTree = new ObservableCollection<PapyrusViewModel>(new[] { root });
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
    }
}
