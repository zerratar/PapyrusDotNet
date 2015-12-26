using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private PapyrusAssemblyDefinition loadedAssembly;
        private string loadedAssemblyName;
        public MainWindowViewModel()
        {
            ExitCommand = new RelayCommand(Exit);
            OpenPexCommand = new RelayCommand(OpenPex);
            SavePexCommand = new RelayCommand(SavePex, CanSave);
            SavePexAsCommand = new RelayCommand(SavePexAs, CanSave);

            SelectedMemberCommand = new RelayCommand<PapyrusViewModel>(SelectMember);

            InsertAfterCommand = new RelayCommand<PapyrusInstruction>(InsertAfter, CanInsert);
            InsertBeforeCommand = new RelayCommand<PapyrusInstruction>(InsertBefore, CanInsert);
            EditInstructionCommand = new RelayCommand<PapyrusInstruction>(EditInstruction, CanInsert);
            RemoveInstructionCommand = new RelayCommand<PapyrusInstruction>(RemoveInstruction, CanInsert);
        }

        private void RemoveInstruction(PapyrusInstruction obj)
        {
        }

        private void EditInstruction(PapyrusInstruction obj)
        {
        }

        private void InsertBefore(PapyrusInstruction obj)
        {
        }

        private bool CanInsert(PapyrusInstruction arg) => arg != null;

        private void InsertAfter(PapyrusInstruction obj)
        {

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


        public RelayCommand<PapyrusInstruction> InsertBeforeCommand { get; set; }

        public RelayCommand<PapyrusInstruction> InsertAfterCommand { get; set; }

        public RelayCommand<PapyrusInstruction> EditInstructionCommand { get; set; }

        public RelayCommand<PapyrusInstruction> RemoveInstructionCommand { get; set; }

        public RelayCommand<PapyrusViewModel> SelectedMemberCommand { get; set; }


        private static MainWindowViewModel designInstance;

        public static MainWindowViewModel DesignInstance = designInstance ??
                                                           (designInstance = new MainWindowViewModel());

    }
}
