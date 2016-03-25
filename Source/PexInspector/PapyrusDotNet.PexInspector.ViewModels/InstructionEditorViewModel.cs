using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PexInspector.ViewModels.Interfaces;

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class InstructionEditorViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel main;
        private readonly IPexLoader pexLoader;
        private readonly IDialogService dialogService;
        private static InstructionEditorViewModel vm;
        public static InstructionEditorViewModel DesignInstance = vm ?? (vm = new InstructionEditorViewModel(null, null, null));
        private PapyrusInstruction selectedMethodInstruction;
        private ObservableCollection<PapyrusInstruction> selectedMethodInstructions;

        public InstructionEditorViewModel(MainWindowViewModel main, IPexLoader pexLoader, IDialogService dialogService)
        {
            this.main = main;
            this.pexLoader = pexLoader;
            this.dialogService = dialogService;
            if (main != null)
            {
                InsertAfterCommand = new RelayCommand(InsertInstructionAfter, CanInsert);
                InsertBeforeCommand = new RelayCommand(InsertInstructionBefore, CanInsert);
                EditInstructionCommand = new RelayCommand(EditInstruction, CanInsert);
                RemoveInstructionCommand = new RelayCommand(RemoveInstruction, CanInsert);

                CreateInstructionCommand = new RelayCommand(CreateInstruction, CanCreate);
            }
        }

        private bool CanCreate()
        {
            return main.selectedMethod != null;
        }
        private void RemoveInstruction()
        {
            var obj = SelectedMethodInstruction;
            if (MessageBox.Show("WARNING: Are you sure you want to delete this instruction?",
                "Delete Instruction L_" + obj.Offset.ToString("000") + ": " + obj.OpCode, MessageBoxButton.OKCancel)
                == MessageBoxResult.OK)
            {
                var method = main.selectedMethod;

                method.Body.Instructions.Remove(obj);

                SelectedMethodInstructions = new ObservableCollection<PapyrusInstruction>(
                    method.Body.Instructions
                    );

                main.SelectedMethodParameters = new ObservableCollection<PapyrusParameterDefinition>(
                    method.Parameters
                    );

                main.SelectedMethodVariables = new ObservableCollection<PapyrusVariableReference>(
                    method.GetVariables()
                    );
                main.selectedMethodNode.SetDirty(true);

                main.RaiseCommandsCanExecute();
            }
        }

        private void EditInstruction()
        {
            var loadedAssemblies = pexLoader.GetLoadedAssemblies().ToList();
            var dialog = new PapyrusInstructionEditorViewModel(dialogService, loadedAssemblies,
                main.selectedMethod.DeclaringState?.DeclaringType?.Assembly,
                main.selectedMethod.DeclaringState?.DeclaringType,
                main.selectedMethod, SelectedMethodInstruction);
            var result = dialogService.ShowDialog(dialog);
            if (result == DialogResult.OK)
            {
                var inst = SelectedMethodInstruction;
                //inst.Operand = dialog.Operand;
                inst.OpCode = dialog.SelectedOpCode;
                inst.Arguments = dialog.Arguments;
                inst.OperandArguments = new List<PapyrusVariableReference>(dialog.OperandArguments);
                main.selectedMethod.Body.Instructions.RecalculateOffsets();
                main.selectedMethod.UpdateInstructionOperands();
                SelectedMethodInstructions =
                    new ObservableCollection<PapyrusInstruction>(main.selectedMethod.Body.Instructions);
                main.selectedMethodNode.SetDirty(true);

                main.RaiseCommandsCanExecute();
            }
        }

        private void CreateInstruction()
        {
            var loadedAssemblies = pexLoader.GetLoadedAssemblies().ToList();
            var dialog = new PapyrusInstructionEditorViewModel(dialogService,
                loadedAssemblies,
                main.selectedMethod.DeclaringState?.DeclaringType?.Assembly,
                main.selectedMethod.DeclaringState?.DeclaringType, main.selectedMethod);
            var result = dialogService.ShowDialog(dialog);
            if (result == DialogResult.OK)
            {
                CreateAndInsertInstructionAt(int.MaxValue, dialog);
                main.RaiseCommandsCanExecute();
            }
        }

        private void InsertInstructionBefore()
        {
            var loadedAssemblies = pexLoader.GetLoadedAssemblies().ToList();
            var dialog = new PapyrusInstructionEditorViewModel(dialogService,
                loadedAssemblies,
                main.selectedMethod.DeclaringState?.DeclaringType?.Assembly,
                main.selectedMethod.DeclaringState?.DeclaringType, main.selectedMethod);
            var result = dialogService.ShowDialog(dialog);
            if (result == DialogResult.OK)
            {
                var inst = SelectedMethodInstruction;
                var index = SelectedMethodInstructions.IndexOf(inst);
                if (index < 0) index = 0;

                CreateAndInsertInstructionAt(index, dialog);
                main.RaiseCommandsCanExecute();
            }
        }

        private void InsertInstructionAfter()
        {
            var loadedAssemblies = pexLoader.GetLoadedAssemblies().ToList();
            var dialog = new PapyrusInstructionEditorViewModel(dialogService, loadedAssemblies,
                main.selectedMethod.DeclaringState?.DeclaringType?.Assembly,
                main.selectedMethod.DeclaringState?.DeclaringType, main.selectedMethod);
            var result = dialogService.ShowDialog(dialog);
            if (result == DialogResult.OK)
            {
                var inst = SelectedMethodInstruction;
                var index = SelectedMethodInstructions.IndexOf(inst) + 1;

                CreateAndInsertInstructionAt(index, dialog);
                main.RaiseCommandsCanExecute();
            }
        }

        private void CreateAndInsertInstructionAt(int index, PapyrusInstructionEditorViewModel dialog)
        {
            var newInstruction = new PapyrusInstruction();
            newInstruction.OpCode = dialog.SelectedOpCode;
            //newInstruction.Operand = dialog.Operand;
            newInstruction.Arguments = dialog.Arguments;
            newInstruction.OperandArguments = new List<PapyrusVariableReference>(dialog.OperandArguments);
            main.selectedMethod.Body.Instructions.Insert(index, newInstruction);
            main.selectedMethod.Body.Instructions.RecalculateOffsets();
            main.selectedMethod.UpdateInstructionOperands();
            SelectedMethodInstructions =
                new ObservableCollection<PapyrusInstruction>(main.selectedMethod.Body.Instructions);
            main.selectedMethodNode.SetDirty(true);
        }


        public ObservableCollection<PapyrusInstruction> SelectedMethodInstructions
        {
            get { return selectedMethodInstructions; }
            set { Set(ref selectedMethodInstructions, value); }
        }


        public PapyrusInstruction SelectedMethodInstruction
        {
            get { return selectedMethodInstruction; }
            set { Set(ref selectedMethodInstruction, value); }
        }

        public RelayCommand InsertBeforeCommand { get; set; }
        public RelayCommand InsertAfterCommand { get; set; }
        public RelayCommand EditInstructionCommand { get; set; }
        public RelayCommand CreateInstructionCommand { get; set; }
        public RelayCommand RemoveInstructionCommand { get; set; }


        private bool CanInsert() => SelectedMethodInstruction != null;
    }
}