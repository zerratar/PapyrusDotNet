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
        private readonly PapyrusInstruction instruction;
        private bool isNewInstruction;
        private PapyrusOpCodes selectedOpCode;
        private ObservableCollection<PapyrusOpCodes> availableOpCodes;
        private string selectedOpCodeDescription;
        private string argumentsDescription;
        private string operandArgumentsDescription;
        private bool operandArgumentsVisible;
        public PapyrusInstructionEditorViewModel(PapyrusInstruction instruction = null)
        {
            opCodeDescriptionReader = new OpCodeDescriptionReader();
            opCodeDescriptionDefinition = opCodeDescriptionReader.Read("OpCodeDescriptions.xml");

            this.instruction = instruction;
            if (instruction == null)
            {
                isNewInstruction = true;
            }
            else
            {
                SelectedOpCode = instruction.OpCode;
                SelectedOpCodeDescription = instruction.OpCode.GetDescription();
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
                    SelectedOpCodeDescription = selectedOpCode.GetDescription();
                    ArgumentsDescription = selectedOpCode.GetArgumentsDescription();
                    OperandArgumentsDescription = selectedOpCode.GetOperandArgumentsDescription();
                    OperandArgumentsVisible = !operandArgumentsDescription.ToLower().Contains("no operand");
                }
            }
        }

        public string SelectedOpCodeDescription
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
                                                                             new PapyrusInstructionEditorViewModel(new PapyrusInstruction
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
