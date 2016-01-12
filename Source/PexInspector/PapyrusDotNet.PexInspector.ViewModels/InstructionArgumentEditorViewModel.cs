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
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using PapyrusDotNet.Common.Utilities;
using PapyrusDotNet.Decompiler.HelperClasses;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Extensions;
using PapyrusDotNet.PexInspector.ViewModels.Implementations;
using PapyrusDotNet.PexInspector.ViewModels.Interfaces;
using PapyrusDotNet.PexInspector.ViewModels.Selectors;

#endregion

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class InstructionArgumentEditorViewModel : ViewModelBase
    {
        private static readonly Lazy<InstructionArgumentEditorViewModel> lazyDesignInstance =
            new Lazy<InstructionArgumentEditorViewModel>(CreateViewModel);

        public static InstructionArgumentEditorViewModel DesignInstance = lazyDesignInstance.Value;
        private readonly PapyrusInstruction currentInstruction;
        private readonly PapyrusMethodDefinition currentMethod;
        private readonly PapyrusTypeDefinition currentType;
        private readonly IDialogService dialogService;
        private readonly List<PapyrusAssemblyDefinition> loadedAssemblies;
        private readonly PapyrusAssemblyDefinition loadedAssembly;

        private string argument0Alias;
        private string argument0Description;
        private string argument1Alias;
        private string argument1Description;
        private string argument2Alias;
        private string argument2Description;
        private string argument3Alias;
        private string argument3Description;
        private string argument4Alias;
        private string argument4Description;

        public object[] Arguments = new object[5];

        public PapyrusPrimitiveType[] ArgumentTypes = new PapyrusPrimitiveType[5];
        private readonly OpCodeDescription desc;
        private Visibility fiveArgsVisibility;
        private Visibility fourArgsVisibility;
        private Visibility mathVisibility;
        private readonly Map<string, string> varTypes = new Map<string, string>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="InstructionArgumentEditorViewModel" /> class.
        /// </summary>
        /// <param name="dialogService">The dialog service.</param>
        /// <param name="loadedAssemblies">The loaded assemblies.</param>
        /// <param name="loadedAssembly">The loaded assembly.</param>
        /// <param name="currentType">Type of the current.</param>
        /// <param name="currentMethod">The current method.</param>
        /// <param name="currentInstruction">The current instruction.</param>
        /// <param name="desc">The desc.</param>
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


            if (loadedAssembly != null && currentMethod != null)
                BuildVarTypeMap(loadedAssembly, currentMethod);


            Argument0Command = new RelayCommand(SelectArgument0);
            Argument1Command = new RelayCommand(SelectArgument1);
            Argument2Command = new RelayCommand(SelectArgument2);
            Argument3Command = new RelayCommand(SelectArgument3);
            Argument4Command = new RelayCommand(SelectArgument4);

            var maxArgCount = 5;

            if (this.desc != null)
            {
                maxArgCount = this.desc.Arguments.Count;
                Arguments = new object[maxArgCount];
                ArgumentTypes = new PapyrusPrimitiveType[maxArgCount];
            }

            if (currentInstruction != null)
            {
                var args = currentInstruction.Arguments.ToArray().Cast<object>().ToList();

                while (args.Count < maxArgCount)
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

        public object Operand { get; set; }

        public Visibility AssignmentVisibility { get; set; }

        public Visibility CallMethodVisibility { get; set; }

        public Visibility CallStaticVisibility { get; set; }

        public Visibility CallParentVisibility { get; set; }


        public Visibility JumpVisibility { get; set; }


        public Visibility JumpConditionalVisibility { get; set; }


        public Visibility MathConditionalVisibility { get; set; }

        public Visibility OneArgsVisibility { get; set; }

        public Visibility TwoArgsVisibility { get; set; }

        public Visibility ThreeArgsVisibility { get; set; }

        public Visibility MathVisibility
        {
            get { return mathVisibility; }
            set { Set(ref mathVisibility, value); }
        }

        public Visibility FourArgsVisibility
        {
            get { return fourArgsVisibility; }
            set { Set(ref fourArgsVisibility, value); }
        }

        public Visibility FiveArgsVisibility
        {
            get { return fiveArgsVisibility; }
            set { Set(ref fiveArgsVisibility, value); }
        }

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

        public string Argument3Alias
        {
            get { return argument3Alias; }
            set { Set(ref argument3Alias, value); }
        }

        public string Argument4Alias
        {
            get { return argument4Alias; }
            set { Set(ref argument4Alias, value); }
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

        public string Argument3Description
        {
            get { return argument3Description; }
            set { Set(ref argument3Description, value); }
        }


        public string Argument4Description
        {
            get { return argument4Description; }
            set { Set(ref argument4Description, value); }
        }


        public ICommand Argument0Command { get; set; }

        public ICommand Argument1Command { get; set; }

        public ICommand Argument2Command { get; set; }

        public ICommand Argument3Command { get; set; }

        public ICommand Argument4Command { get; set; }

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

        private object[] OperandLookup(object[] toArray)
        {
            if (currentInstruction.Operand != null)
            {
                if (currentInstruction.OpCode == PapyrusOpCodes.Jmp)
                {
                    toArray[0] = currentInstruction.Operand;
                    ArgumentTypes[0] = PapyrusPrimitiveType.Integer;
                }
                else if (currentInstruction.OpCode == PapyrusOpCodes.Jmpt ||
                         currentInstruction.OpCode == PapyrusOpCodes.Jmpf)
                {
                    toArray[1] = currentInstruction.Operand;
                    ArgumentTypes[1] = PapyrusPrimitiveType.Integer;
                }
                else if (currentInstruction.OpCode == PapyrusOpCodes.Callmethod)
                {
                    if (!(currentInstruction.Operand is string))
                    {
                        toArray[0] = currentInstruction.Operand;

                        ArgumentTypes[0] = PapyrusPrimitiveType.String;
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

                        ArgumentTypes[1] = PapyrusPrimitiveType.String;
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
                //for (var i = 0; i < argCount; i++)
                //{
                //    Argument0Alias = desc.Arguments[i].Alias;
                //    Argument0Description = desc.Arguments[i].Description;

                //    if (Arguments[i] != null)
                //    {
                //        Argument0Alias = GetStringRepresentation(Arguments[i]);
                //    }
                //}

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
                if (argCount >= 4)
                {
                    Argument3Alias = desc.Arguments[3].Alias;
                    Argument3Description = desc.Arguments[3].Description;

                    if (Arguments[3] != null)
                    {
                        Argument3Alias = GetStringRepresentation(Arguments[3]);
                    }
                }
                if (argCount >= 5)
                {
                    Argument4Alias = desc.Arguments[4].Alias;
                    Argument4Description = desc.Arguments[4].Description;

                    if (Arguments[4] != null)
                    {
                        Argument4Alias = GetStringRepresentation(Arguments[4]);
                    }
                }
            }
        }

        private string GetStringRepresentation(object o)
        {
            var i = Array.IndexOf(Arguments, o);
            var inst = o as PapyrusInstruction;
            if (inst != null)
            {
                ArgumentTypes[i] = PapyrusPrimitiveType.Integer;
                return "L_" + inst.Offset.ToString("000") + ": " + inst.OpCode;
            }
            var methodRef = o as PapyrusMethodDefinition;
            if (methodRef != null)
            {
                ArgumentTypes[i] = PapyrusPrimitiveType.String;
                return methodRef.Name.Value + GetParameterString(methodRef.Parameters) + " : " +
                       methodRef.ReturnTypeName.Value;
            }
            var varRef = o as PapyrusVariableReference;
            if (varRef != null)
            {
                var tp = varRef.TypeName?.Value;
                if (tp != null)
                    ArgumentTypes[i] = Utility.GetPapyrusPrimitiveType(tp);
                else
                    ArgumentTypes[i] = varRef.Type;
                return (varRef.Value ?? varRef.Name?.Value)?.ToString();
            }

            var fieldRef = o as PapyrusFieldDefinition;
            if (fieldRef != null)
            {
                ArgumentTypes[i] = Utility.GetPapyrusPrimitiveType(fieldRef.TypeName);
                return fieldRef.Name.Value;
            }

            return o.ToString();
        }

        private string GetParameterString(List<PapyrusParameterDefinition> parameters,
            bool includeParameterNames = false)
        {
            var paramDefs = string.Join(", ", parameters.Select(p => p.TypeName.Value +
                                                                     (includeParameterNames ? " " + p.Name.Value : "")));

            return "(" + paramDefs + ")";
        }

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

        private void SelectArgument3()
        {
            ShowArgumentSelectionDialog(desc.Arguments[3]);
        }

        private void SelectArgument4()
        {
            ShowArgumentSelectionDialog(desc.Arguments[4]);
        }


        public List<PapyrusVariableReference> GetArguments()
        {
            return Arguments.Select(ToPapyrusVariableRef).ToList();
        }

        private PapyrusVariableReference ToPapyrusVariableRef(object arg)
        {
            if (arg == null) return null;
            var i = Array.IndexOf(Arguments, arg);
            if (arg is PapyrusVariableReference)
            {
                var varRef = arg as PapyrusVariableReference;

                var tp = varRef.TypeName?.Value;
                if (tp != null)
                    ArgumentTypes[i] = Utility.GetPapyrusPrimitiveType(tp);
                else
                    ArgumentTypes[i] = varRef.Type;

                if (varRef.Value == null && varRef.Name != null && varRef.Name.Value.Length > 0 &&
                    varRef.Type == PapyrusPrimitiveType.Reference)
                {
                    varRef.Value = varRef.Name.Value;
                }

                return varRef;
            }
            if (arg is PapyrusMethodDefinition)
            {
                ArgumentTypes[i] = PapyrusPrimitiveType.Reference;
                var m = arg as PapyrusMethodDefinition;
                Operand = arg;
                return new PapyrusVariableReference
                {
                    Value = m.Name.Value,
                    Type = PapyrusPrimitiveType.Reference
                };
            }
            if (arg is PapyrusParameterDefinition)
            {
                var m = arg as PapyrusParameterDefinition;
                ArgumentTypes[i] = Utility.GetPapyrusPrimitiveType(m.TypeName.Value);
                return new PapyrusVariableReference
                {
                    Value = m.Name.Value,
                    Type = PapyrusPrimitiveType.Reference
                };
            }
            if (arg is PapyrusFieldDefinition)
            {
                var m = arg as PapyrusFieldDefinition;
                ArgumentTypes[i] = Utility.GetPapyrusPrimitiveType(m.TypeName);
                    // m.FieldVariable?.ValueType ?? PapyrusPrimitiveType.Reference;
                return new PapyrusVariableReference
                {
                    Value = m.Name.Value,
                    Type = PapyrusPrimitiveType.Reference
                };
            }
            if (arg is PapyrusInstruction)
            {
                var m = arg as PapyrusInstruction;
                Operand = arg;
                ArgumentTypes[i] = PapyrusPrimitiveType.Integer;
                return new PapyrusVariableReference
                {
                    Value = currentInstruction.Offset + m.Offset,
                    Type = PapyrusPrimitiveType.Integer
                };
            }
            var valType = Utility.GetPrimitiveTypeFromValue(arg);
            ArgumentTypes[i] = valType;
            return new PapyrusVariableReference
            {
                Value = arg,
                Type = valType
            };
        }

        //public object GetOperand()
        //{
        //    OperandLookup()
        //    throw new System.NotImplementedException();
        //}

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
                var targetType = GetTargetType(currentType);

                var dialog = new PapyrusMethodSelectorViewModel(loadedAssemblies, targetType, d);
                var result = dialogService.ShowDialog(dialog); // change currentType to selected type later
                if (result == DialogResult.OK)
                {
                    PapyrusMethodDefinition method = null;
                    var selMethod = dialog.SelectedMethod;
                    if (selMethod != null)
                        method = dialog.SelectedMethod.Item as PapyrusMethodDefinition;

                    if ((method != null && dialog.SelectedMethodName != null &&
                         (method.Name.Value != dialog.SelectedMethodName)) ||
                        (method == null && dialog.SelectedMethodName != null))
                    {
                        Arguments[d.Index] = CreateReferenceFromName(dialog.SelectedMethodName);
                    }
                    else
                        Arguments[d.Index] = method;
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
                        if (dialog.SelectedReference == null)
                        {
                            var name = dialog.SelectedReferenceName;
                            Arguments[d.Index] = CreateReferenceFromName(name);
                        }
                        else
                        {
                            Arguments[d.Index] = dialog.SelectedReference;
                        }
                    }
                }
                else
                {
                    UpdateArgumentConstraintTypes();

                    var dialog = new PapyrusReferenceAndConstantValueViewModel(loadedAssemblies, currentType,
                        currentMethod, d, ArgumentTypes);
                    var result = dialogService.ShowDialog(dialog);
                    if (result == DialogResult.OK)
                    {
                        if (dialog.SelectedConstantValue == null && dialog.SelectedReferenceValue == null)
                        {
                            var name = dialog.SelectedReferenceName;
                            Arguments[d.Index] = CreateReferenceFromName(name);
                        }
                        else
                        {
                            Arguments[d.Index] = dialog.SelectedItem;
                        }
                    }
                }
            }
            UpdateArguments();
        }

        private PapyrusTypeDefinition GetTargetType(PapyrusTypeDefinition defaultType)
        {
            if (desc.OpCode == PapyrusOpCodes.Callstatic)
            {
                return GetTypeFromLocation(Arguments[0], defaultType);
            }
            if (desc.OpCode == PapyrusOpCodes.Callmethod)
            {
                return GetTypeFromLocation(Arguments[1], defaultType);
            }
            if (desc.OpCode == PapyrusOpCodes.Callparent) // Call Base
            {
                return GetTypeFromLocation(defaultType.BaseTypeName.Value, defaultType);
            }
            return defaultType;
        }

        private PapyrusTypeDefinition GetTypeFromLocation(object variableNameOrTypeName,
            PapyrusTypeDefinition defaultType)
        {
            // If the input is a VariableReference, we may have the typename already.
            var targetTypeNameOrVariable = variableNameOrTypeName.ToString();
            var vref = variableNameOrTypeName as PapyrusVariableReference;
            if (!string.IsNullOrEmpty(vref?.TypeName?.Value))
                return GetTypeDefinition(vref.TypeName.Value, defaultType);
            // if no type was found, we try to take the name
            if (!string.IsNullOrEmpty(vref?.Name?.Value))
                targetTypeNameOrVariable = vref?.Name?.Value;
            // if no name was found, we try and take the value.
            if (!string.IsNullOrEmpty(vref?.Value?.ToString()))
                targetTypeNameOrVariable = vref?.Value?.ToString();


            // if we are accessing a variable to call a method from we will need to get the type of the variable.
            var possibleType = GetTypeOfVariable(targetTypeNameOrVariable);
            if (possibleType != null)
                return GetTypeDefinition(possibleType, defaultType);

            // if it is a static call, we will have the target type name from the input string
            return GetTypeDefinition(targetTypeNameOrVariable, defaultType);
        }

        private PapyrusTypeDefinition GetTypeDefinition(string typeName, PapyrusTypeDefinition defaultType)
        {
            var targetAsm =
                loadedAssemblies.FirstOrDefault(t => t.Types.Any(o => o.Name.Value.ToLower() == typeName.ToLower()));
            if (targetAsm != null)
            {
                return targetAsm.Types.First();
            }
            return defaultType;
        }

        private string GetTypeOfVariable(string variable)
        {
            return varTypes.Find(variable.ToLower());
        }

        private void UpdateArgumentConstraintTypes()
        {
            for (var i = 0; i < ArgumentTypes.Length; i++)
            {
                if (ArgumentTypes[i] == PapyrusPrimitiveType.Reference)
                {
                    var arg = Arguments[i] as PapyrusVariableReference;
                    if (arg != null)
                    {
                        var tomatch = arg.Name?.Value;
                        if (string.IsNullOrEmpty(tomatch))
                            tomatch = arg.Value.ToString();
                        tomatch = tomatch.ToLower();
                        var m = currentMethod;
                        var t = currentType;
                        if (m != null && t != null)
                        {
                            var targetVar =
                                m.GetVariables().FirstOrDefault(v => v.Name.Value.ToLower() == tomatch);
                            if (targetVar != null)
                            {
                                ArgumentTypes[i] = Utility.GetPapyrusPrimitiveType(targetVar.TypeName.Value);
                                continue;
                            }

                            var targetField =
                                t.Fields.FirstOrDefault(v => v.Name.Value.ToLower() == tomatch);
                            if (targetField != null)
                            {
                                ArgumentTypes[i] = Utility.GetPapyrusPrimitiveType(targetField.TypeName);
                                continue;
                            }

                            var targetParam =
                                m.Parameters.FirstOrDefault(v => v.Name.Value.ToLower() == tomatch);
                            if (targetParam != null)
                            {
                                ArgumentTypes[i] = Utility.GetPapyrusPrimitiveType(targetParam.TypeName.Value);
                            }
                        }
                    }
                }
            }
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

        /// <summary>
        ///     Builds the type index, mapping a table reference to its type.
        /// </summary>
        public void BuildVarTypeMap(PapyrusAssemblyDefinition asm, PapyrusMethodDefinition method)
        {
            varTypes.Clear();
            var obj = asm.Types.First();
            foreach (var t in obj.Fields)
                varTypes[t.Name.Value.ToLower()] = t.TypeName;
            foreach (var t in method.Parameters)
                varTypes[t.Name.Value.ToLower()] = t.TypeName.Value;
            foreach (var t in method.GetVariables())
                varTypes[t.Name.Value.ToLower()] = t.TypeName.Value;
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
                else if (desc.OpCode == PapyrusOpCodes.Isub || desc.OpCode == PapyrusOpCodes.Fsub ||
                         desc.OpCode == PapyrusOpCodes.Idiv || desc.OpCode == PapyrusOpCodes.Fdiv
                         || desc.OpCode == PapyrusOpCodes.Iadd || desc.OpCode == PapyrusOpCodes.Fadd ||
                         desc.OpCode == PapyrusOpCodes.Imul || desc.OpCode == PapyrusOpCodes.Fmul
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
                else if (desc.Arguments.Count == 4)
                {
                    FourArgsVisibility = Visibility.Visible;
                }
                else if (desc.Arguments.Count == 5)
                {
                    FiveArgsVisibility = Visibility.Visible;
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
            FourArgsVisibility = Visibility.Collapsed;
            FiveArgsVisibility = Visibility.Collapsed;
        }

        private static InstructionArgumentEditorViewModel CreateViewModel()
        {
            var reader = new OpCodeDescriptionReader();
            IOpCodeDescriptionDefinition data = null;
            if (File.Exists("OpCodeDescriptions.xml"))
                data = reader.Read("OpCodeDescriptions.xml");
            else if (File.Exists(@"C:\git\PapyrusDotNet\Source\PapyrusDotNet.PexInspector\OpCodeDescriptions.xml"))
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
    }
}