using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PexInspector.ViewModels.Extensions;
using PapyrusDotNet.PexInspector.ViewModels.Implementations;
using PapyrusDotNet.PexInspector.ViewModels.Interfaces;

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class PapyrusInstructionEditorViewModel : ViewModelBase
    {
        private readonly IOpCodeDescriptionReader opCodeDescriptionReader;
        private IOpCodeDescriptionDefinition opCodeDescriptionDefinition;
        private readonly IDialogService dialogService;
        private readonly List<PapyrusAssemblyDefinition> loadedAssemblies;
        private readonly PapyrusAssemblyDefinition loadedAssembly;
        private readonly PapyrusTypeDefinition currentType;
        private readonly PapyrusMethodDefinition currentMethod;
        private readonly PapyrusInstruction instruction;
        private bool isNewInstruction;
        private PapyrusOpCodes selectedOpCode;
        private ObservableCollection<PapyrusOpCodes> availableOpCodes;
        private InstructionArgumentEditorViewModel selectedOpCodeDescription;
        private string argumentsDescription;
        private string operandArgumentsDescription;
        private bool operandArgumentsVisible;
        private string selectedOpCodeDescriptionString;

        public PapyrusInstructionEditorViewModel(IDialogService dialogService, List<PapyrusAssemblyDefinition> loadedAssemblies, PapyrusAssemblyDefinition loadedAssembly, PapyrusTypeDefinition currentType, PapyrusMethodDefinition currentMethod, PapyrusInstruction instruction = null)
        {
            opCodeDescriptionReader = new OpCodeDescriptionReader();

            if (System.IO.File.Exists("OpCodeDescriptions.xml"))
                opCodeDescriptionDefinition = opCodeDescriptionReader.Read("OpCodeDescriptions.xml");
            else
            {
                opCodeDescriptionDefinition = opCodeDescriptionReader.Read(@"C:\git\PapyrusDotNet\Source\PapyrusDotNet.PexInspector\OpCodeDescriptions.xml");
            }


            this.dialogService = dialogService;
            this.loadedAssemblies = loadedAssemblies;
            this.loadedAssembly = loadedAssembly;
            this.currentType = currentType;
            this.currentMethod = currentMethod;
            this.instruction = instruction;
            if (instruction == null)
            {
                isNewInstruction = true;
                var defaultOpCode = PapyrusOpCodes.Nop;
                SelectedOpCodeDescription = new InstructionArgumentEditorViewModel(dialogService, loadedAssemblies, loadedAssembly, currentType, currentMethod, opCodeDescriptionDefinition.GetDesc(defaultOpCode));

                SelectedOpCode = defaultOpCode;

                ArgumentsDescription = defaultOpCode.GetArgumentsDescription();
                OperandArgumentsDescription = defaultOpCode.GetOperandArgumentsDescription();
                OperandArgumentsVisible = !operandArgumentsDescription.ToLower().Contains("no operand");
            }
            else
            {
                SelectedOpCode = instruction.OpCode;
                SelectedOpCodeDescriptionString = instruction.OpCode.GetDescription();
                SelectedOpCodeDescription = new InstructionArgumentEditorViewModel(dialogService, loadedAssemblies, loadedAssembly, 
                    currentType, currentMethod, opCodeDescriptionDefinition.GetDesc(instruction.OpCode)); // instruction.OpCode.GetDescription();
                ArgumentsDescription = instruction.OpCode.GetArgumentsDescription();
                OperandArgumentsDescription = instruction.OpCode.GetOperandArgumentsDescription();
                OperandArgumentsVisible = !operandArgumentsDescription.ToLower().Contains("no operand");
            }
            AvailableOpCodes = new ObservableCollection<PapyrusOpCodes>(Enum.GetValues(typeof(PapyrusOpCodes)).Cast<PapyrusOpCodes>());
        }


        public ObservableCollection<PapyrusOpCodes> AvailableOpCodes
        {
            get { return availableOpCodes; }
            set { Set(ref availableOpCodes, value); }
        }

        public PapyrusOpCodes SelectedOpCode
        {
            get { return selectedOpCode; }
            set
            {
                if (Set(ref selectedOpCode, value))
                {
                    SelectedOpCodeDescription = new InstructionArgumentEditorViewModel(dialogService, loadedAssemblies, loadedAssembly,
                        currentType, currentMethod, opCodeDescriptionDefinition.GetDesc(selectedOpCode));
                    SelectedOpCodeDescriptionString = selectedOpCode.GetDescription();
                    ArgumentsDescription = selectedOpCode.GetArgumentsDescription();
                    OperandArgumentsDescription = selectedOpCode.GetOperandArgumentsDescription();
                    OperandArgumentsVisible = !operandArgumentsDescription.ToLower().Contains("no operand");
                }
            }
        }

        public string SelectedOpCodeDescriptionString
        {
            get { return selectedOpCodeDescriptionString; }
            set { Set(ref selectedOpCodeDescriptionString, value); }
        }

        public InstructionArgumentEditorViewModel SelectedOpCodeDescription
        {
            get { return selectedOpCodeDescription; }
            set { Set(ref selectedOpCodeDescription, value); }
        }

        public string ArgumentsDescription
        {
            get { return argumentsDescription; }
            set { Set(ref argumentsDescription, value); }
        }

        public string OperandArgumentsDescription
        {
            get { return operandArgumentsDescription; }
            set { Set(ref operandArgumentsDescription, value); }
        }

        public bool OperandArgumentsVisible
        {
            get { return operandArgumentsVisible; }
            set { Set(ref operandArgumentsVisible, value); }
        }

        public static PapyrusInstructionEditorViewModel DesignInstance = designInstance ??
                                                                         (designInstance =
                                                                             new PapyrusInstructionEditorViewModel(null, null, null, null, null,
                                                                                 new PapyrusInstruction
                                                                                 {
                                                                                     OpCode = PapyrusOpCodes.Jmp,
                                                                                     Arguments = new List<PapyrusVariableReference>(new[]
                                                                                 {
                                                                                     new PapyrusVariableReference()
                                                                                     { ValueType = PapyrusPrimitiveType.Integer, Value = 0 },
                                                                                 })
                                                                                 }));

        private static PapyrusInstructionEditorViewModel designInstance;
    }
}
