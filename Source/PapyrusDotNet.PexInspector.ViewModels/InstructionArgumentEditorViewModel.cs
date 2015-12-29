using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PexInspector.ViewModels.Implementations;
using PapyrusDotNet.PexInspector.ViewModels.Interfaces;
using PapyrusDotNet.PexInspector.ViewModels.Selectors;

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class InstructionArgumentEditorViewModel : ViewModelBase
    {
        public InstructionArgumentEditorViewModel(IDialogService dialogService,
            List<PapyrusAssemblyDefinition> loadedAssemblies,
            PapyrusAssemblyDefinition loadedAssembly,
            PapyrusTypeDefinition currentType,
            PapyrusMethodDefinition currentMethod,
            PapyrusInstruction currentInstruction,
            OpCodeDescription desc)
        {
            this.dialogService = dialogService;
            this.loadedAssemblies = loadedAssemblies;
            this.loadedAssembly = loadedAssembly;
            this.currentType = currentType;
            this.currentMethod = currentMethod;
            this.currentInstruction = currentInstruction;
            this.desc = desc;

            Argument0Command = new RelayCommand(SelectArgument0);
            Argument1Command = new RelayCommand(SelectArgument1);
            Argument2Command = new RelayCommand(SelectArgument2);

            if (currentInstruction != null)
            {

                var args = currentInstruction.Arguments.ToArray().Cast<object>().ToList();

                while (args.Count < 3)
                {
                    args.Add(null);
                }
                Arguments = OperandLookup(args.ToArray());
            }

            if (this.desc != null)
            {
                UpdateVisibilities();

                try
                {
                    UpdateArguments();
                }
                catch
                {
                    // this.desc was null after all.
                }
            }
        }

        private object[] OperandLookup(object[] toArray)
        {

            if (currentInstruction.Operand != null)
            {
                if (currentInstruction.OpCode == PapyrusOpCodes.Jmp)
                {
                    toArray[0] = currentInstruction.Operand;
                }
                else if (currentInstruction.OpCode == PapyrusOpCodes.Jmpt || currentInstruction.OpCode == PapyrusOpCodes.Jmpf)
                {
                    toArray[1] = currentInstruction.Operand;
                }
                else if (currentInstruction.OpCode == PapyrusOpCodes.Callmethod)
                {
                    if (!(currentInstruction.Operand is string))
                    {
                        toArray[0] = currentInstruction.Operand;
                    }
                }
                // --- We most likely won't have a operand for Callstatic.
                /* else if (currentInstruction.OpCode == PapyrusOpCodes.Callstatic)
                {

                } */
                else if (currentInstruction.OpCode == PapyrusOpCodes.Callparent)
                {
                    if (!(currentInstruction.Operand is string))
                    {
                        toArray[1] = currentInstruction.Operand;
                    }
                }
            }

            return toArray;
        }

        private void UpdateArguments()
        {
            var argCount = desc.Arguments.Count;
            if (argCount > 0)
            {
                if (argCount >= 1)
                {
                    Argument0Alias = desc.Arguments[0].Alias;
                    Argument0Description = desc.Arguments[0].Description;

                    if (Arguments[0] != null)
                    {
                        Argument0Alias = GetStringRepresentation(Arguments[0]);
                    }
                }

                if (argCount >= 2)
                {
                    Argument1Alias = desc.Arguments[1].Alias;
                    Argument1Description = desc.Arguments[1].Description;

                    if (Arguments[1] != null)
                    {
                        Argument1Alias = GetStringRepresentation(Arguments[1]);
                    }
                }

                if (argCount >= 3)
                {
                    Argument2Alias = desc.Arguments[2].Alias;
                    Argument2Description = desc.Arguments[2].Description;

                    if (Arguments[2] != null)
                    {
                        Argument2Alias = GetStringRepresentation(Arguments[2]);
                    }
                }
            }
        }

        private string GetStringRepresentation(object o)
        {
            var inst = o as PapyrusInstruction;
            if (inst != null)
            {
                return "L_" + inst.Offset.ToString("000") + ": " + inst.OpCode;
            }
            var methodRef = o as PapyrusMethodDefinition;
            if (methodRef != null)
            {
                return methodRef.Name.Value + GetParameterString(methodRef.Parameters) + " : " +
                       methodRef.ReturnTypeName.Value;
            }
            var varRef = o as PapyrusVariableReference;
            if (varRef != null)
            {
                return (varRef.Value ?? varRef.Name?.Value)?.ToString();
            }

            var fieldRef = o as PapyrusFieldDefinition;
            if (fieldRef != null)
            {
                return fieldRef.Name.Value;
            }

            return o.ToString();
        }

        private string GetParameterString(List<PapyrusParameterDefinition> parameters, bool includeParameterNames = false)
        {
            var paramDefs = string.Join(", ", parameters.Select(p => p.TypeName.Value +
            (includeParameterNames ? (" " + p.Name.Value) : "")));

            return "(" + paramDefs + ")";
        }

        public object[] Arguments = new object[3];

        private void SelectArgument0()
        {
            ShowArgumentSelectionDialog(desc.Arguments[0]);
        }

        private void SelectArgument1()
        {
            ShowArgumentSelectionDialog(desc.Arguments[1]);

        }

        private void SelectArgument2()
        {
            ShowArgumentSelectionDialog(desc.Arguments[2]);
        }

        public List<PapyrusVariableReference> GetArguments()
        {
            throw new System.NotImplementedException();
        }

        public List<PapyrusVariableReference> GetOperandArguments()
        {
            throw new System.NotImplementedException();
        }

        public object GetOperand()
        {
            throw new System.NotImplementedException();
        }

        private void ShowArgumentSelectionDialog(OpCodeArgumentDescription d)
        {
            if (d.Ref == OpCodeRef.Instruction)
            {
                var dialog = new PapyrusInstructionSelectorViewModel(currentMethod.Body.Instructions.ToArray(), d);
                var result = dialogService.ShowDialog(dialog);
                if (result == DialogResult.OK)
                {
                    Arguments[d.Index] = dialog.SelectedInstruction;
                }
            }
            else if (d.Ref == OpCodeRef.Type)
            {
                var dialog = new PapyrusTypeSelectorViewModel(loadedAssemblies, d);
                var result = dialogService.ShowDialog(dialog);
                if (result == DialogResult.OK)
                {
                    Arguments[d.Index] = dialog.SelectedType;
                }
            }
            else if (d.Ref == OpCodeRef.Method)
            {
                var dialog = new PapyrusMethodSelectorViewModel(loadedAssemblies, currentType, d);
                var result = dialogService.ShowDialog(dialog); // change currentType to selected type later
                if (result == DialogResult.OK)
                {
                    Arguments[d.Index] = dialog.SelectedMethod.Item;
                }
            }
            else
            {
                if (d.ValueType == OpCodeValueTypes.Constant)
                {
                    var dialog = new PapyrusConstantValueViewModel(d);
                    var result = dialogService.ShowDialog(dialog);
                    if (result == DialogResult.OK)
                    {
                        Arguments[d.Index] = dialog.SelectedValue;
                    }
                }
                else if (d.ValueType == OpCodeValueTypes.Reference)
                {
                    var dialog = new PapyrusReferenceValueViewModel(loadedAssemblies, currentType, currentMethod, d);
                    var result = dialogService.ShowDialog(dialog);
                    if (result == DialogResult.OK)
                    {
                        Arguments[d.Index] = dialog.SelectedReference;
                    }
                }
                else
                {
                    var dialog = new PapyrusReferenceAndConstantValueViewModel(loadedAssemblies, currentType, currentMethod, d);
                    var result = dialogService.ShowDialog(dialog);
                    if (result == DialogResult.OK)
                    {
                        Arguments[d.Index] = dialog.SelectedItem;
                    }
                }
            }
            UpdateArguments();
        }
        private void UpdateVisibilities()
        {
            HideAll();
            if (desc == null) return;
            try
            {
                if (desc.OpCode == PapyrusOpCodes.CmpEq || desc.OpCode == PapyrusOpCodes.CmpLte ||
                    desc.OpCode == PapyrusOpCodes.CmpLt || desc.OpCode == PapyrusOpCodes.CmpGte ||
                    desc.OpCode == PapyrusOpCodes.CmpGt)
                {
                    MathConditionalVisibility = Visibility.Visible;
                }
                else if (desc.OpCode == PapyrusOpCodes.Assign)
                {
                    AssignmentVisibility = Visibility.Visible;
                }
                else if (desc.OpCode == PapyrusOpCodes.Callmethod)
                {
                    CallMethodVisibility = Visibility.Visible;
                }
                else if (desc.OpCode == PapyrusOpCodes.Callstatic)
                {
                    CallStaticVisibility = Visibility.Visible;
                }
                else if (desc.OpCode == PapyrusOpCodes.Callparent)
                {
                    CallParentVisibility = Visibility.Visible;
                }
                else if (desc.OpCode == PapyrusOpCodes.Jmp)
                {
                    JumpVisibility = Visibility.Visible;
                }
                else if (desc.OpCode == PapyrusOpCodes.Jmpt || desc.OpCode == PapyrusOpCodes.Jmpf)
                {
                    JumpConditionalVisibility = Visibility.Visible;
                }
                else if (desc.OpCode == PapyrusOpCodes.Isub || desc.OpCode == PapyrusOpCodes.Fsub || desc.OpCode == PapyrusOpCodes.Idiv || desc.OpCode == PapyrusOpCodes.Fdiv
                    || desc.OpCode == PapyrusOpCodes.Iadd || desc.OpCode == PapyrusOpCodes.Fadd || desc.OpCode == PapyrusOpCodes.Imul || desc.OpCode == PapyrusOpCodes.Fmul
                    || desc.OpCode == PapyrusOpCodes.Imod)
                {
                    MathVisibility = Visibility.Visible;
                }
                else if (desc.Arguments.Count == 1)
                {
                    OneArgsVisibility = Visibility.Visible;
                }
                else if (desc.Arguments.Count == 2)
                {
                    TwoArgsVisibility = Visibility.Visible;
                }
                else if (desc.Arguments.Count == 3)
                {
                    ThreeArgsVisibility = Visibility.Visible;
                }
            }
            catch
            {
                // Ignored, if you're selecting a different opcode too fast, it seem like the 'desc' variable is still null when this method is invoked.
            }
        }

        private void HideAll()
        {
            AssignmentVisibility = Visibility.Collapsed;
            CallMethodVisibility = Visibility.Collapsed;
            CallStaticVisibility = Visibility.Collapsed;
            CallParentVisibility = Visibility.Collapsed;
            JumpVisibility = Visibility.Collapsed;
            JumpConditionalVisibility = Visibility.Collapsed;
            OneArgsVisibility = Visibility.Collapsed;
            TwoArgsVisibility = Visibility.Collapsed;
            ThreeArgsVisibility = Visibility.Collapsed;
            MathConditionalVisibility = Visibility.Collapsed;
            MathVisibility = Visibility.Collapsed;
        }

        public Visibility AssignmentVisibility
        {
            get { return assignmentVisibility; }
            set { assignmentVisibility = value; }
        }

        public Visibility CallMethodVisibility
        {
            get { return callMethodVisibility; }
            set { callMethodVisibility = value; }
        }

        public Visibility CallStaticVisibility
        {
            get { return callStaticVisibility; }
            set { callStaticVisibility = value; }
        }

        public Visibility CallParentVisibility
        {
            get { return callParentVisibility; }
            set { callParentVisibility = value; }
        }


        public Visibility JumpVisibility
        {
            get { return jumpVisibility; }
            set { jumpVisibility = value; }
        }


        public Visibility JumpConditionalVisibility
        {
            get { return jumpConditionalVisibility; }
            set { jumpConditionalVisibility = value; }
        }


        public Visibility MathConditionalVisibility
        {
            get { return mathConditionalVisibility; }
            set { mathConditionalVisibility = value; }
        }

        public Visibility OneArgsVisibility
        {
            get { return oneArgsVisibility; }
            set { oneArgsVisibility = value; }
        }

        public Visibility TwoArgsVisibility
        {
            get { return twoArgsVisibility; }
            set { twoArgsVisibility = value; }
        }

        public Visibility ThreeArgsVisibility
        {
            get { return threeArgsVisibility; }
            set { threeArgsVisibility = value; }
        }

        public static InstructionArgumentEditorViewModel DesignInstance = designInstance ?? (designInstance = CreateViewModel());


        public string Argument0Alias
        {
            get { return argument0Alias; }
            set { Set(ref argument0Alias, value); }
        }

        public string Argument1Alias
        {
            get { return argument1Alias; }
            set { Set(ref argument1Alias, value); }
        }

        public string Argument2Alias
        {
            get { return argument2Alias; }
            set { Set(ref argument2Alias, value); }
        }

        public string Argument0Description
        {
            get { return argument0Description; }
            set { Set(ref argument0Description, value); }
        }

        public string Argument1Description
        {
            get { return argument1Description; }
            set { Set(ref argument1Description, value); }
        }

        public string Argument2Description
        {
            get { return argument2Description; }
            set { Set(ref argument2Description, value); }
        }

        public ICommand Argument0Command { get; set; }

        public ICommand Argument1Command { get; set; }

        public ICommand Argument2Command { get; set; }

        public string ExpectedConditionValue
        {
            get
            {
                if (desc == null) return "";
                if (desc.OpCode == PapyrusOpCodes.Jmpf)
                    return "false";

                if (desc.OpCode == PapyrusOpCodes.Jmpt)
                    return "true";

                if (desc.OpCode == PapyrusOpCodes.CmpLt)
                    return "<";
                if (desc.OpCode == PapyrusOpCodes.CmpLte)
                    return "<=";
                if (desc.OpCode == PapyrusOpCodes.CmpGt)
                    return ">";
                if (desc.OpCode == PapyrusOpCodes.CmpGte)
                    return ">=";
                if (desc.OpCode == PapyrusOpCodes.CmpEq)
                    return "==";

                return "";
            }
        }

        public string MathOperatorValue
        {
            get
            {
                if (desc == null) return "";
                if (desc.OpCode == PapyrusOpCodes.Iadd || desc.OpCode == PapyrusOpCodes.Fadd)
                    return "+";

                if (desc.OpCode == PapyrusOpCodes.Isub || desc.OpCode == PapyrusOpCodes.Fsub)
                    return "-";

                if (desc.OpCode == PapyrusOpCodes.Idiv || desc.OpCode == PapyrusOpCodes.Fdiv)
                    return "/";

                if (desc.OpCode == PapyrusOpCodes.Imod)
                    return "%";

                if (desc.OpCode == PapyrusOpCodes.Imul || desc.OpCode == PapyrusOpCodes.Fmul)
                    return "*";

                return "";
            }
        }

        public Visibility MathVisibility
        {
            get { return mathVisibility; }
            set { Set(ref mathVisibility, value); }
        }

        private static InstructionArgumentEditorViewModel CreateViewModel()
        {
            var reader = new OpCodeDescriptionReader();
            IOpCodeDescriptionDefinition data = null;
            if (System.IO.File.Exists("OpCodeDescriptions.xml"))
                data = reader.Read("OpCodeDescriptions.xml");
            else if (System.IO.File.Exists(@"C:\git\PapyrusDotNet\Source\PapyrusDotNet.PexInspector\OpCodeDescriptions.xml"))
                data = reader.Read(@"C:\git\PapyrusDotNet\Source\PapyrusDotNet.PexInspector\OpCodeDescriptions.xml");
            else
                data = reader.Read(@"D:\git\PapyrusDotNet\Source\PapyrusDotNet.PexInspector\OpCodeDescriptions.xml");

            var desc = data.GetDesc(PapyrusOpCodes.Cast);

            if (desc.Arguments == null || desc.Arguments.Count == 0)
            {
                desc.Arguments = new List<OpCodeArgumentDescription>();

                var fallbackDesc = PapyrusInstructionOpCodeDescription.FromOpCode(desc.OpCode);

                for (var i = 0; i < fallbackDesc.ArgumentCount; i++)
                {
                    desc.Arguments.Add(new OpCodeArgumentDescription
                    {
                        Index = i,
                        Alias = "Value" + (i + 1),
                        Ref = OpCodeRef.None,
                        Description = "",
                        ValueType = OpCodeValueTypes.ReferenceOrConstant
                    });
                }
            }

            return new InstructionArgumentEditorViewModel(null, null, null, null, null, null, desc);
        }

        private static InstructionArgumentEditorViewModel designInstance;
        private string argument0Alias;
        private string argument1Alias;
        private string argument2Alias;
        private string argument0Description;
        private string argument1Description;
        private string argument2Description;
        private Visibility assignmentVisibility;
        private Visibility callMethodVisibility;
        private Visibility callStaticVisibility;
        private Visibility callParentVisibility;
        private Visibility oneArgsVisibility;
        private Visibility twoArgsVisibility;
        private Visibility threeArgsVisibility;
        private readonly IDialogService dialogService;
        private readonly List<PapyrusAssemblyDefinition> loadedAssemblies;
        private readonly PapyrusAssemblyDefinition loadedAssembly;
        private readonly PapyrusTypeDefinition currentType;
        private readonly PapyrusMethodDefinition currentMethod;
        private readonly PapyrusInstruction currentInstruction;
        private OpCodeDescription desc;
        private Visibility jumpVisibility;
        private Visibility jumpConditionalVisibility;
        private Visibility mathConditionalVisibility;
        private Visibility mathVisibility;


    }
}
