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
using System.IO;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using PapyrusDotNet.Common.Utilities;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Extensions;
using PapyrusDotNet.PexInspector.ViewModels.Extensions;
using PapyrusDotNet.PexInspector.ViewModels.Implementations;
using PapyrusDotNet.PexInspector.ViewModels.Interfaces;
using PapyrusDotNet.PexInspector.ViewModels.Selectors;

#endregion

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class PapyrusInstructionEditorViewModel : ViewModelBase
    {
        public static PapyrusInstructionEditorViewModel DesignInstance = designInstance ??
                                                                         (designInstance =
                                                                             new PapyrusInstructionEditorViewModel(
                                                                                 null, null, null, null, null,
                                                                                 new PapyrusInstruction
                                                                                 {
                                                                                     OpCode = PapyrusOpCodes.Jmp,
                                                                                     Arguments =
                                                                                         new List
                                                                                             <PapyrusVariableReference>(new[
                                                                                                 ]
                                                                                             {
                                                                                                 new PapyrusVariableReference
                                                                                                 {
                                                                                                     Type =
                                                                                                         PapyrusPrimitiveType
                                                                                                             .Integer,
                                                                                                     Value = 0
                                                                                                 }
                                                                                             })
                                                                                 }));

        private static PapyrusInstructionEditorViewModel designInstance;
        private readonly PapyrusMethodDefinition currentMethod;
        private readonly PapyrusTypeDefinition currentType;
        private readonly IDialogService dialogService;
        private readonly PapyrusInstruction instruction;
        private readonly List<PapyrusAssemblyDefinition> loadedAssemblies;
        private readonly PapyrusAssemblyDefinition loadedAssembly;
        private readonly IOpCodeDescriptionReader opCodeDescriptionReader;
        private string argumentsDescription;
        private ObservableCollection<PapyrusOpCodes> availableOpCodes;
        private bool isNewInstruction;
        private readonly IOpCodeDescriptionDefinition opCodeDescriptionDefinition;
        private ObservableCollection<PapyrusVariableReference> operandArguments;
        private string operandArgumentsDescription;
        private bool operandArgumentsVisible;
        private PapyrusOpCodes selectedOpCode;
        private InstructionArgumentEditorViewModel selectedOpCodeDescription;
        private string selectedOpCodeDescriptionString;
        private PapyrusVariableReference selectedOperandArgument;

        public PapyrusInstructionEditorViewModel(IDialogService dialogService,
            List<PapyrusAssemblyDefinition> loadedAssemblies, PapyrusAssemblyDefinition loadedAssembly,
            PapyrusTypeDefinition currentType, PapyrusMethodDefinition currentMethod,
            PapyrusInstruction instruction = null)
        {
            opCodeDescriptionReader = new OpCodeDescriptionReader();

            if (File.Exists("OpCodeDescriptions.xml"))
                opCodeDescriptionDefinition = opCodeDescriptionReader.Read("OpCodeDescriptions.xml");
            else if (File.Exists(
                @"C:\git\PapyrusDotNet\Source\PapyrusDotNet.PexInspector\OpCodeDescriptions.xml"))
                opCodeDescriptionDefinition =
                    opCodeDescriptionReader.Read(
                        @"C:\git\PapyrusDotNet\Source\PapyrusDotNet.PexInspector\OpCodeDescriptions.xml");
            else
                opCodeDescriptionDefinition =
                    opCodeDescriptionReader.Read(
                        @"D:\git\PapyrusDotNet\Source\PapyrusDotNet.PexInspector\OpCodeDescriptions.xml");

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
                SelectedOpCodeDescription = new InstructionArgumentEditorViewModel(dialogService, loadedAssemblies,
                    loadedAssembly, currentType, currentMethod, null, opCodeDescriptionDefinition.GetDesc(defaultOpCode));

                SelectedOpCode = defaultOpCode;

                // ArgumentsDescription = defaultOpCode.GetArgumentsDescription();
                OperandArgumentsDescription = defaultOpCode.GetOperandArgumentsDescription();
                OperandArgumentsVisible = !operandArgumentsDescription.ToLower().Contains("no operand");

                OperandArguments = new ObservableCollection<PapyrusVariableReference>();
            }
            else
            {
                SelectedOpCode = instruction.OpCode;
                SelectedOpCodeDescriptionString = instruction.OpCode.GetDescription();
                SelectedOpCodeDescription = new InstructionArgumentEditorViewModel(dialogService, loadedAssemblies,
                    loadedAssembly,
                    currentType, currentMethod, instruction, opCodeDescriptionDefinition.GetDesc(instruction.OpCode));
                    // instruction.OpCode.GetDescription();
                // ArgumentsDescription = instruction.OpCode.GetArgumentsDescription();
                OperandArgumentsDescription = instruction.OpCode.GetOperandArgumentsDescription();
                OperandArgumentsVisible = !operandArgumentsDescription.ToLower().Contains("no operand");
                OperandArguments = new ObservableCollection<PapyrusVariableReference>(instruction.OperandArguments);
            }
            AvailableOpCodes =
                new ObservableCollection<PapyrusOpCodes>(Enum.GetValues(typeof (PapyrusOpCodes)).Cast<PapyrusOpCodes>());

            AddOperandArgumentCommand = new RelayCommand(AddOpArg);
            RemoveOperandArgumentCommand = new RelayCommand(RemoveOpArg, CanEdit);
            EditOperandArgumentCommand = new RelayCommand(EditOpArg, CanEdit);
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
                    SelectedOpCodeDescription = new InstructionArgumentEditorViewModel(dialogService, loadedAssemblies,
                        loadedAssembly,
                        currentType, currentMethod, instruction, opCodeDescriptionDefinition.GetDesc(selectedOpCode));
                    SelectedOpCodeDescriptionString = selectedOpCode.GetDescription();
                    // ArgumentsDescription = selectedOpCode.GetArgumentsDescription();
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

        //private List<PapyrusVariableReference> GetOperandArguments()
        //{
        //    return new List<PapyrusVariableReference>();
        //}

        public List<PapyrusVariableReference> Arguments => SelectedOpCodeDescription.GetArguments();

        public ObservableCollection<PapyrusVariableReference> OperandArguments
        {
            get { return operandArguments; }
            set { Set(ref operandArguments, value); }
        }

        public PapyrusVariableReference SelectedOperandArgument
        {
            get { return selectedOperandArgument; }
            set
            {
                if (Set(ref selectedOperandArgument, value))
                {
                    RemoveOperandArgumentCommand.RaiseCanExecuteChanged();
                    EditOperandArgumentCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public RelayCommand RemoveOperandArgumentCommand { get; set; }

        public RelayCommand EditOperandArgumentCommand { get; set; }

        public RelayCommand AddOperandArgumentCommand { get; set; }

        private void AddOpArg()
        {
            var dialog = new PapyrusReferenceAndConstantValueViewModel(loadedAssemblies, currentType, currentMethod,
                null, null);
            var result = dialogService.ShowDialog(dialog);
            if (result == DialogResult.OK)
            {
                var papyrusVariableReference = new PapyrusVariableReference();
                OperandArguments.Add(UpdateOperandArgument(dialog, ref papyrusVariableReference));
            }
        }

        private void EditOpArg()
        {
            var dialog = new PapyrusReferenceAndConstantValueViewModel(loadedAssemblies, currentType, currentMethod,
                null, null);
            var result = dialogService.ShowDialog(dialog);
            if (result == DialogResult.OK)
            {
                var i = OperandArguments.IndexOf(selectedOperandArgument);

                OperandArguments.RemoveAt(i);

                var papyrusVariableReference = new PapyrusVariableReference();

                UpdateOperandArgument(dialog, ref papyrusVariableReference);

                OperandArguments.Insert(i, papyrusVariableReference);

                SelectedOperandArgument = papyrusVariableReference;

                //// FFS! Just update? But nooo.. ObservableCollection refresh only triggers on remove/add :p
                //var dummy = new PapyrusVariableReference();
                //OperandArguments.Add(dummy);
                //OperandArguments.Remove(dummy);
            }
        }

        private void RemoveOpArg()
        {
            var obj = SelectedOperandArgument;
            var name = obj.Name?.Value ?? "";
            if (MessageBox.Show("Are you sure you want to delete this Argument?",
                "Delete Argument " + name, MessageBoxButton.OKCancel)
                == MessageBoxResult.OK)
            {
                OperandArguments.Remove(SelectedOperandArgument);
            }
        }

        private PapyrusVariableReference UpdateOperandArgument(PapyrusReferenceAndConstantValueViewModel dialog,
            ref PapyrusVariableReference papyrusVariableReference)
        {
            var asm = currentType.Assembly;
            var targetType = Utility.GetPapyrusPrimitiveType(Utility.GetPapyrusReturnType(dialog.SelectedTypeName));
            if (dialog.SelectedReferenceValue != null)
            {
                var paramRef = dialog.SelectedReferenceValue as PapyrusParameterDefinition;
                var fieldRef = dialog.SelectedReferenceValue as PapyrusFieldDefinition;
                var varRef = dialog.SelectedReferenceValue as PapyrusVariableReference;
                if (varRef != null)
                {
                    papyrusVariableReference.Value = varRef.Value;
                    papyrusVariableReference.Name = varRef.Name;
                    papyrusVariableReference.TypeName = varRef.TypeName;
                    papyrusVariableReference.Type = PapyrusPrimitiveType.Reference;
                }
                else if (fieldRef != null)
                {
                    papyrusVariableReference.Value = fieldRef.Name.Value;
                    papyrusVariableReference.Name = fieldRef.Name;
                    papyrusVariableReference.TypeName = fieldRef.TypeName.Ref(asm);
                    papyrusVariableReference.Type = PapyrusPrimitiveType.Reference;
                }
                else if (paramRef != null)
                {
                    papyrusVariableReference.Value = paramRef.Name.Value;
                    papyrusVariableReference.Name = paramRef.Name;
                    papyrusVariableReference.TypeName = paramRef.TypeName;
                    papyrusVariableReference.Type = PapyrusPrimitiveType.Reference;
                }
            }
            else
            {
                if (dialog.SelectedConstantValue == null && dialog.SelectedReferenceName != null)
                {
                    return CreateReferenceFromName(dialog.SelectedReferenceName);
                }
                var val = dialog.SelectedConstantValue;
                if (val is string)
                {
                    val = val.ToString().Ref(asm).Value;
                }

                papyrusVariableReference.Value = val;
                papyrusVariableReference.Type = targetType;
            }
            return papyrusVariableReference;
        }

        private bool CanEdit()
        {
            return SelectedOperandArgument != null;
        }

        public PapyrusVariableReference CreateReferenceFromName(string name)
        {
            var asm = currentType.Assembly;
            var nameRef = name.Ref(asm);
            return new PapyrusVariableReference
            {
                Name = nameRef,
                Value = nameRef.Value,
                Type = PapyrusPrimitiveType.Reference
            };
        }
    }
}