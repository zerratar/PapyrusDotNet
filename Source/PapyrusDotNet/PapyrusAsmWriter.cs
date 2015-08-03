/*
    This file is part of PapyrusDotNet.

    PapyrusDotNet is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    PapyrusDotNet is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with PapyrusDotNet.  If not, see <http://www.gnu.org/licenses/>.
	
	Copyright 2014, Karl Patrik Johansson, zerratar@gmail.com
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PapyrusDotNet
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    using PapyrusDotNet.Common;
    using PapyrusDotNet.Models;

    public class PapyrusAsmWriter
    {
        private AssemblyDefinition Assembly;

        private TypeDefinition Type;

        private Stack<EvaluationStackItem> EvaluationStack;

        private PapyrusFunction function;

        private bool _skipNextInstruction = false;

        private MethodDefinition _targetMethod;

        private bool _invertedBranch = false;

        private int _skipToOffset = -1;

        public static List<PapyrusVariableReference> Fields;

        public static List<PapyrusAssembly> ParsedAssemblies = new List<PapyrusAssembly>();



        public PapyrusAsmWriter(AssemblyDefinition assembly, TypeDefinition type, List<PapyrusVariableReference> fields)
        {
            this.Assembly = assembly;
            this.Type = type;
            this.EvaluationStack = new Stack<EvaluationStackItem>();

            this.function = new PapyrusFunction();
            this.function.Fields = fields;
        }

        public static void GeneratePapyrusFromAssembly(AssemblyDefinition currentAssembly, ref Dictionary<string, string> outputPasFiles, AssemblyDefinition mainAssembly = null)
        {
            foreach (var module in currentAssembly.Modules)
            {
                if (module.HasTypes)
                {
                    var types = module.Types;
                    foreach (var type in types)
                    {
                        if (!type.FullName.ToLower().Contains("<module>"))
                        {
                            Console.WriteLine("Generating Papyrus Asm for " + type.FullName);

                            Fields = new List<PapyrusVariableReference>();

                            string outputPapyrus = "";

                            var properties = Utility.GetFlagsAndProperties(type);
                            if (properties.IsGeneric)
                            {
                                // Get all usages to know which generic types are necessary to be generated.
                                // then for each of those, we will generate a new class to represent it.
                                // replacing all 'T' with the types used.

                                var references = AssemblyHelper.GetAllGenericReferences(type, mainAssembly != null ? mainAssembly : currentAssembly);

                                var ref2 = AssemblyHelper.GetAllGenericReferences(type, currentAssembly);

                                var ref3 = AssemblyHelper.GetAllReferences(type, currentAssembly);

                                foreach (var r in references)
                                {
                                    if (r.Type == "T") continue;

                                    var assembly = new PapyrusAssembly(type, r.Type);

                                    assembly.OutputName = assembly.BaseType.Replace("`1", "_" + Utility.GetPapyrusBaseType(r.Type));

                                    ParsedAssemblies.Add(assembly);
                                }
                            }
                            else
                            {
                                ParsedAssemblies.Add(new PapyrusAssembly(type));
                            }



                            {
                                var assembly = ParsedAssemblies.LastOrDefault();
                                // Don't write any assembly in case it is an enum.
                                // However, we still want the type to be parsed, 
                                // so creating a new Instance of PapyrusAssembly is still necessary
                                if (!assembly.IsEnum)
                                {
                                    outputPapyrus += assembly.Header.Info.ToString();
                                    outputPapyrus += assembly.Header.UserFlagRef.ToString();
                                    outputPapyrus += WritePapyrusObjectTable(assembly);

                                    if (!outputPasFiles.ContainsKey(assembly.OutputName + ".pas"))
                                    {
                                        outputPasFiles.Add(assembly.OutputName + ".pas", outputPapyrus);
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }

        public PapyrusFunction CreateFunction(PapyrusAssembly asm, MethodDefinition method, string overrideFunctionName = null)
        {

            var sourceBuilder = new StringBuilder();

            this._targetMethod = method;

            if (!String.IsNullOrEmpty(overrideFunctionName))
                this._targetMethod.Name = overrideFunctionName;

            var returnType = Utility.GetPapyrusReturnType(method.ReturnType);

            var staticMarker = method.IsStatic ? " static" : "";

            function.PapyrusAssembly = asm;
            function.IsGlobal = method.IsStatic;
            function.Name = method.Name;
            function.MethodDefinition = method;
            function.ReturnType = returnType;


            // Native methods or functions can be declared			
            foreach (var attr in method.CustomAttributes)
            {
                if (attr.AttributeType.Name.Equals("NativeAttribute"))
                {
                    method.IsNative = true;
                    break;
                }
            }

            function.IsNative = method.IsNative;

            var nativeMarker = method.IsNative ? " native" : "";

            int indentDepth = 4;

            sourceBuilder.AppendLine(Utility.Indent(indentDepth++, ".function " + method.Name + staticMarker + nativeMarker, false));
            sourceBuilder.AppendLine(Utility.Indent(indentDepth, ".userFlags " + function.UserFlags, false));
            sourceBuilder.AppendLine(Utility.Indent(indentDepth, ".docString \"\"", false));
            sourceBuilder.AppendLine(Utility.Indent(indentDepth, ".return " + function.ReturnType, false));
            sourceBuilder.AppendLine(Utility.Indent(indentDepth++, ".paramTable", false));

            foreach (var parameter in method.Parameters)
            {
                sourceBuilder.AppendLine(Utility.Indent(indentDepth, ParseParameter(parameter), false));
            }

            sourceBuilder.AppendLine(Utility.Indent(--indentDepth, ".endParamTable", false));
            sourceBuilder.AppendLine(Utility.Indent(indentDepth++, ".localTable", false));

            if (method.HasBody)
            {
                if (HasVoidCalls(method.Body))
                    sourceBuilder.AppendLine(Utility.Indent(indentDepth, ".local ::NoneVar None", false));

                foreach (var variable in method.Body.Variables)
                {
                    if (variable.VariableType.Name.StartsWith("<>")) continue; // Delegate variables should not be added.

                    sourceBuilder.AppendLine(Utility.Indent(indentDepth, ParseLocalVariable(variable), false));
                }
            }

            sourceBuilder.AppendLine(Utility.Indent(--indentDepth, ".endLocalTable", false));
            sourceBuilder.AppendLine(Utility.Indent(indentDepth++, ".code", false));
            if (method.HasBody)
            {
                foreach (var instruction in method.Body.Instructions)
                {
                    // We will add a new label for each instruction.
                    // And at the end we will remove any unused instructions.
                    sourceBuilder.AppendLine(Utility.Indent(indentDepth - 1, "_label" + instruction.Offset + ":", false));

                    if (_skipNextInstruction)
                    {
                        _skipNextInstruction = false;
                        continue;
                    }

                    if (_skipToOffset > 0)
                    {
                        if (instruction.Offset <= _skipToOffset)
                        {
                            continue;
                        }
                        _skipToOffset = -1;
                    }

                    var value = ParseInstruction(instruction);

                    // We should never call the original constructor :p
                    // We do however call the renamed constructor from inside OnInit
                    // That way we are able to "merge" OnInit and the Constructor. :-)
                    if (value.Contains(".ctor"))
                    {
                        continue;
                    }

                    if (!String.IsNullOrEmpty(value))
                    {
                        if (value.Contains(Environment.NewLine))
                        {
                            var rows = value.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                            foreach (var row in rows)
                            {
                                var codeInstruction = Utility.Indent(indentDepth, row, false);
                                function.CodeInstructions.Add(codeInstruction);
                                sourceBuilder.AppendLine(codeInstruction);
                            }
                        }
                        else
                        {
                            var codeInstruction = Utility.Indent(indentDepth, value, false);
                            function.CodeInstructions.Add(codeInstruction);
                            sourceBuilder.AppendLine(codeInstruction);
                        }
                    }
                }
            }
            sourceBuilder.AppendLine(Utility.Indent(--indentDepth, ".endCode", false));
            sourceBuilder.AppendLine(Utility.Indent(--indentDepth, ".endFunction", false));

            // Post Optimizations

            string output = sourceBuilder.ToString();

            output = Utility.OptimizeLabels(output);
            output = Utility.InjectTempVariables(output, indentDepth + 2, function.TempVariables);

            sourceBuilder = new StringBuilder(output);

            function.Source = sourceBuilder;

            return function;
        }

        public static string WritePapyrusObjectTable(PapyrusAssembly asm)
        {
            var papyrus = WritePapyrusObjectTableCore(asm);

            if (!string.IsNullOrEmpty(asm.GenericTypeReplacement))
            {
                var ptype = Utility.GetPapyrusBaseType(asm.GenericTypeReplacement);

                var pap = papyrus.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                for (int j = 0; j < pap.Length; j++)
                {
                    if (pap[j].EndsWith(" T") || pap[j].EndsWith("_T"))
                    {
                        pap[j] = pap[j].Remove(pap[j].LastIndexOf('T')) + ptype;
                    }

                    if (pap[j].Contains(".object ") && pap[j].Contains("`1"))
                    {
                        pap[j] = pap[j].Replace("`1", "_" + ptype);
                    }
                    else if (pap[j].Contains("`1"))
                    {
                        pap[j] = pap[j].Replace("`1", "_" + ptype);
                    }
                }

                papyrus = String.Join(Environment.NewLine, pap);
            }
            return papyrus;
        }

        public static string WritePapyrusObjectTableCore(PapyrusAssembly asm)
        {
            string papyrus = "";

            papyrus += ".objectTable" + Environment.NewLine;
            papyrus += ("\t.object " + asm.ObjectTable.Name + " " + asm.ObjectTable.BaseType).Trim() + Environment.NewLine;

            papyrus += "\t\t.userFlags " + asm.ObjectTable.Info.UserFlagsValue + Environment.NewLine;
            papyrus += "\t\t.docString \"" + asm.ObjectTable.Info.DocString + "\"" + Environment.NewLine;


#warning autoState is the default state to be used, however if we are to enable the use of more states. This needs to be fixed.

            papyrus += "\t\t.autoState" + Environment.NewLine;
            papyrus += "\t\t.variableTable" + Environment.NewLine;




            foreach (var variable in asm.ObjectTable.VariableTable)
            {
                var userFlagsVal = variable.Properties.UserFlagsValue;
                if (variable.Properties.IsHidden && variable.Properties.IsProperty)
                {
                    userFlagsVal -= 1;
                    // The hidden flag only effects the Property field, declared after the variable field
                }

                var reference = ParsedAssemblies.FirstOrDefault(a => a.OutputName == variable.TypeName);
                if (reference != null && reference.IsEnum)
                {
                    variable.TypeName = "Int";
                }
                // variable.TypeName

                papyrus += Utility.Indent(3, ".variable " + variable.Name + " " + variable.TypeName);
                papyrus += Utility.Indent(4, ".userFlags " + userFlagsVal);
                papyrus += Utility.Indent(4, ".initialValue " + variable.Properties.InitialValue);
                papyrus += Utility.Indent(3, ".endVariable");

            }
            papyrus += "\t\t.endVariableTable" + Environment.NewLine;
            papyrus += "\t\t.propertyTable" + Environment.NewLine;

            foreach (var fieldProp in asm.ObjectTable.PropertyTable)
            {
                var reference = ParsedAssemblies.FirstOrDefault(a => a.OutputName == fieldProp.TypeName);
                if (reference != null && reference.IsEnum)
                {
                    fieldProp.TypeName = "Int";
                }

                string autoMarker = fieldProp.Properties.IsAuto ? " auto" : "";
                papyrus += Utility.Indent(3, ".property " + fieldProp.Name + " " + fieldProp.TypeName + autoMarker);
                papyrus += Utility.Indent(4, ".userFlags " + fieldProp.Properties.UserFlagsValue);
                papyrus += Utility.Indent(4, ".docString \"\"");
                papyrus += Utility.Indent(4, ".autoVar " + fieldProp.AutoVarName);
                papyrus += Utility.Indent(3, ".endProperty");
            }

            papyrus += "\t\t.endPropertyTable" + Environment.NewLine;
            papyrus += "\t\t.stateTable" + Environment.NewLine;
            papyrus += "\t\t\t.state" + Environment.NewLine;

            foreach (var state in asm.ObjectTable.StateTable)
            {
                foreach (var function in state.Functions)
                {
                    papyrus += function;
                }
            }
            papyrus += "\t\t\t.endState" + Environment.NewLine;
            papyrus += "\t\t.endStateTable" + Environment.NewLine;
            papyrus += "\t.endObject" + Environment.NewLine;
            papyrus += ".endObjectTable";

            return papyrus; // String.Join(Environment.NewLine, rows.ToArray());
        }



        public static string GetPapyrusType(TypeReference reference)
        {

            switch (reference.Name)
            {
                // for now...
                default:
                    return "";
            }

            return reference.Name;
        }


        public static bool HasVoidCalls(MethodBody body)
        {
            foreach (var s in body.Instructions)
            {
                if (Utility.IsCallMethod(s.OpCode.Code))
                {
                    var mRef = s.Operand as MethodReference;
                    if (mRef != null)
                    {
                        if (IsVoid(mRef.ReturnType)) return true;
                    }
                }
            }
            return false;
        }

        private string ParseParameter(ParameterDefinition parameter)
        {
            var name = parameter.Name;
            var type = parameter.ParameterType.FullName;
            var typeN = parameter.ParameterType.Name;

            var val = Utility.GetPapyrusReturnType(typeN, parameter.ParameterType.Namespace);
            if (parameter.ParameterType.IsGenericInstance)
            {
                if (type.Contains("<"))
                {
                    var targetName = type.Split('<')[1].Split('>')[0];
                    var papName = Utility.GetPapyrusBaseType(targetName);

                    val = val.Replace("`1", "_" + papName);
                }
                else
                {
                    var papName = Utility.GetPapyrusBaseType(type);

                    val = val.Replace("`1", "_" + papName);
                }
            }
            else if (parameter.ParameterType.Name.Contains("<T>"))
            {
                val = Utility.GetPapyrusBaseType(type).Replace("<T>", "");
            }

            val = val.Replace("<T>", "");

            function.Parameters.Add(new PapyrusVariableReference(name, type));

            return ".param " + name + " " + val;
        }

        private string ParseLocalVariable(VariableDefinition variable)
        {
            var name = variable.Name;
            var type = variable.VariableType.FullName;
            var typeN = variable.VariableType.Name;

            if (String.IsNullOrEmpty(name))
                name = "V_" + function.Variables.Count;


            var val = Utility.GetPapyrusReturnType(typeN, variable.VariableType.Namespace);
            if (variable.VariableType.IsGenericInstance)
            {
                if (type.Contains("<"))
                {
                    var targetName = type.Split('<')[1].Split('>')[0];
                    var papName = Utility.GetPapyrusBaseType(targetName);
                    val = val.Replace("`1", "_" + papName);
                }
                else
                {
                    var papName = Utility.GetPapyrusBaseType(type);
                    val = val.Replace("`1", "_" + papName);
                }
            }
            else if (variable.VariableType.Name.Contains("<T>"))
            {
                val = Utility.GetPapyrusBaseType(type).Replace("<T>", "");
            }

            val = val.Replace("<T>", "");

#warning We are replacing Delegate vars with Int
            string delegateInvokeRef = null;
            var delegateInstanceVar = false;
            var varType = variable.VariableType;
            if (varType.Name.ToLower().Contains("delegate"))
            {

                var d1 = varType.Resolve();
                if (d1.BaseType.FullName.ToLower().Contains("multicastdelegate"))
                {
                    // Just replace it with a dummy Int for now.
                    // We then are going to check if we are trying to use the function "invoke" on "Int"
                    // What we will do is replace the "invoke" with the delegate generated method and
                    // replace the "int" with "self".
                    // By doing so, we are able to use simple delegates in papyrus!
                    type = "Int";
                    val = "Int";
                    delegateInstanceVar = true;

                    var functionName = this.function.Name;

                    // In case this is a delegate inside a delegate...
                    // _UtilizeDelegate4_b__0
                    if (functionName.StartsWith("_") && functionName.Contains("b_"))
                    {
                        functionName = functionName.Split('_')[1];
                        function.DelegateInvokeCount++;
                    }


                    var delegateMethod =
                        function.PapyrusAssembly.DelegateMethodDefinitions.FirstOrDefault(
                            del => del.Name.Contains("_" + functionName + "_") && del.Name.EndsWith("_" + function.DelegateInvokeCount));

                    if (delegateMethod == null)
                    {
                        delegateMethod =
                            function.PapyrusAssembly.DelegateMethodDefinitions.FirstOrDefault(
                                del => del.Name.Contains("_" + functionName + "_") && del.Name.Contains("b_") && del.Name != this.function.Name);
                    }

                    function.DelegateInvokeCount++;

                    delegateInvokeRef = delegateMethod.Name;
                    // "_" + function.Name + "_b__1_" + (delegatesUsed++); // varType.Name;

                    // var tn = ;

                }
            }

            function.Variables.Add(new PapyrusVariableReference(name, type) { IsDelegateInstance = delegateInstanceVar, DelegateInvokeReference = delegateInvokeRef });

            return ".local " + name + " " + val; //Utility.GetPapyrusReturnType(typeN, variable.VariableType.Namespace);
        }


        private string ParseInstruction(Instruction instruction)
        {

            // ArrayGetElement	<outputVarName>	<arrayName>	<int:index>
            // ArraySetElement <arrayName> <int:index> <valueOrVariable>?
            // ArrayLength	<outputVarName>	<arrayName>

            if (Utility.IsConverToNumber(instruction.OpCode.Code))
            {

                return "";
            }

            if (Utility.IsBoxing(instruction.OpCode.Code))
            {
                // EX: 
                // int a = 0;
                // MyEnum b = MyEnum.First;
                // a++;
                // b = (MyEnum)a;


                //var heapStack = EvaluationStack;
                //var obj1 = heapStack.Pop();
                //var obj2 = heapStack.Pop();
                //var vars = function.AllVariables;
                //var varIndex = 0;

                //string value1 = "";
                //string value2 = "";

                //if (obj1.Value is PapyrusVariableReference)
                //{
                //    var oo = (obj1.Value as PapyrusVariableReference);
                //    value1 = oo.Name;
                //    if (!oo.TypeName.ToLower().Equals("int") && !oo.TypeName.ToLower().Equals("system.int32"))
                //    {
                //        // CAST BOOL TO INT
                //        var typeVariable = GetTargetVariable(instruction, null, "Int");
                //    //    cast = "Cast " + typeVariable + " " + value1;
                //    }
                //}
                //else
                //    value1 = obj1.Value.ToString();

                //if (obj2.Value is PapyrusVariableReference)
                //{
                //    var oo = (obj2.Value as PapyrusVariableReference);
                //    value2 = oo.Name;
                //    if (!oo.TypeName.ToLower().Equals("int") && !oo.TypeName.ToLower().Equals("system.int32"))
                //    {
                //        // CAST BOOL TO INT
                //        var typeVariable = GetTargetVariable(instruction, null, "Int");
                //    //    cast = "Cast " + typeVariable + " " + value2;
                //    }

                //}
                //else
                //    value2 = obj2.Value.ToString();


            }


            if (Utility.IsLoadElement(instruction.OpCode.Code))
            {
                var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
                if (EvaluationStack.Count >= popCount)
                {
                    var itemIndex = EvaluationStack.Pop();
                    var itemArray = EvaluationStack.Pop();

                    string tar = "";
                    var idx = -1;
                    var oidx = "0";
                    if (itemIndex.Value is PapyrusVariableReference)
                        oidx = (itemIndex.Value as PapyrusVariableReference).Name;
                    else if (itemIndex.Value != null)
                        idx = Int32.Parse(itemIndex.Value.ToString());

                    if (idx > 128) idx = 128;
                    if (idx != -1)
                        oidx = idx.ToString();

                    if (itemArray.Value is PapyrusVariableReference)
                        tar = (itemArray.Value as PapyrusVariableReference).Name;
                    else
                    { /* Unsupported */ }


                    // Supports:
                    // var obj = array[x]

                    // Not yet supporting? note: i have not tried it yet.
                    // Function(array[x],...)
                    int targetVariableIndex = 0;
                    var tarIn = GetNextStoreLocalVariableInstruction(instruction, out targetVariableIndex);
                    if (tarIn != null)
                    {

                        return "ArrayGetElement " + function.AllVariables[targetVariableIndex].Name + " " + tar + " " + oidx;
                    }



                }
            }

            if (Utility.IsStoreElement(instruction.OpCode.Code))
            {
                var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
                if (EvaluationStack.Count >= popCount)
                {
                    var newValue = EvaluationStack.Pop();
                    var itemIndex = EvaluationStack.Pop();
                    var itemArray = EvaluationStack.Pop();

                    string val = "";
                    string tar = "";
                    var idx = -1;
                    var oidx = "0";
                    if (itemIndex.Value is PapyrusVariableReference)
                        oidx = (itemIndex.Value as PapyrusVariableReference).Name;
                    else if (itemIndex.Value != null)
                        idx = Int32.Parse(itemIndex.Value.ToString());

                    if (idx > 128) idx = 128;
                    if (idx != -1)
                        oidx = idx.ToString();
                    if (itemArray.Value is PapyrusVariableReference)
                        tar = (itemArray.Value as PapyrusVariableReference).Name;
                    else
                    { /* Unsupported */ }

                    if (newValue.Value is PapyrusVariableReference)
                        val = (newValue.Value as PapyrusVariableReference).Name;

                    else if (newValue.Value != null) val = newValue.Value.ToString();

                    return "ArraySetElement " + tar + " " + oidx + " " + val;

                }
            }

            if (Utility.IsLoadLength(instruction.OpCode.Code))
            {
                var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
                if (EvaluationStack.Count >= popCount)
                {
                    var val = EvaluationStack.Pop();
                    if (val.TypeName.EndsWith("[]"))
                    {
                        if (val.Value is PapyrusVariableReference)
                        {
                            int variableIndex = 0;
                            var storeInstruction = GetNextStoreLocalVariableInstruction(instruction, out variableIndex);
                            if (storeInstruction != null || Utility.IsConverToNumber(instruction.Next.OpCode.Code))
                            {
                                if (Utility.IsConverToNumber(instruction.Next.OpCode.Code))
                                {
                                    _skipNextInstruction = false;
                                    _skipToOffset = 0;

                                    var targetVariable = GetTargetVariable(instruction, null, "Int", true);

                                    var vari = function.AllVariables.FirstOrDefault(va => va.Name == targetVariable);

                                    //EvaluationStack.Push(new EvaluationStackItem() { IsMethodCall = false, IsThis = false, TypeName = vari.TypeName, Value = vari });
                                    return "ArrayLength " + targetVariable + " " + (val.Value as PapyrusVariableReference).Name;

                                }
                                else
                                {
                                    EvaluationStack.Push(new EvaluationStackItem() { IsMethodCall = false, IsThis = false, TypeName = function.AllVariables[variableIndex].TypeName, Value = function.AllVariables[variableIndex] });
                                    return "ArrayLength " + function.AllVariables[variableIndex].Name + " " + (val.Value as PapyrusVariableReference).Name;

                                }
                            }
                        }
                        else
                        {
                            // NOT SUPPORTED
                        }
                    }
                    else
                    {
                        // size of ?
                    }
                }
                // ArrayLength <outputVariableName> <arrayName>
            }

            if (Utility.IsNewArrayInstance(instruction.OpCode.Code))
            {
                var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
                if (EvaluationStack.Count >= popCount)
                {
                    var val = EvaluationStack.Pop();

                    int targetVariableIndex = 0;
                    var tarIn = GetNextStoreLocalVariableInstruction(instruction, out targetVariableIndex);
                    if (tarIn != null)
                    {

                        if (tarIn.Operand is FieldReference)
                        {
                            var fref = tarIn.Operand as FieldReference;
                            // if the EvaluationStack.Count == 0
                            // The previous instruction might have been a call that returned a value
                            // Something we did not store...

                            var definedField =
                                    this.function.Fields.FirstOrDefault(f => f.Name == "::" + fref.Name.Replace('<', '_').Replace('>', '_'));

                            if (definedField != null)
                            {
                                if (EvaluationStack.Count > 0)
                                {
                                    var obj = EvaluationStack.Pop();

                                    if (obj.Value is PapyrusVariableReference)
                                    {
                                        var varRef = obj.Value as PapyrusVariableReference;
                                        definedField.Value = varRef.Value;
                                        // return "Assign " + definedField.Name + " " + varRef.Name;
                                        return "ArrayCreate " + definedField.Name + " " + val.Value;
                                    }
                                }
                                definedField.Value = Utility.TypeValueConvert(definedField.TypeName, val.Value);
                                // return "Assign " + definedField.Name + " " + definedField.Value;
                                return "ArrayCreate " + definedField.Name + " " + val.Value;
                            }
                        }


                        return "ArrayCreate " + function.AllVariables[targetVariableIndex].Name + " " + val.Value;
                    }
                }
            }
            if (Utility.IsNewObjectInstance(instruction.OpCode.Code))
            {
                var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
                // the obj objN = new obj
                // is not supported by Papyrus.
                // this opCode should be ignored, but we should still pop the values from the stack
                // to maintain correct work flow.

                for (int pops = 0; pops < popCount; pops++)
                {
                    if (EvaluationStack.Count > 0)
                        EvaluationStack.Pop();
                }
                int oi;
                GetNextStoreLocalVariableInstruction(instruction, out oi);
            }
            if (Utility.IsLoadArgs(instruction.OpCode.Code))
            {
                var index = IntValue(instruction);
                if (_targetMethod.IsStatic && (int)index == 0 && _targetMethod.Parameters.Count == 0)
                {
                    EvaluationStack.Push(new EvaluationStackItem { IsThis = true, Value = this.Type, TypeName = this.Type.FullName });
                }
                else
                {
                    if (!_targetMethod.IsStatic && index > 0) index--;
                    if (index < function.Parameters.Count)
                    {
                        EvaluationStack.Push(new EvaluationStackItem { Value = function.Parameters[(int)index], TypeName = function.Parameters[(int)index].TypeName });
                    }
                }

            }
            if (Utility.IsLoadInteger(instruction.OpCode.Code))
            {
                var index = IntValue(instruction);
                EvaluationStack.Push(new EvaluationStackItem { Value = index, TypeName = "Int" });
            }

            if (Utility.IsLoadNull(instruction.OpCode.Code))
            {
                EvaluationStack.Push(new EvaluationStackItem { Value = "None", TypeName = "None" });
            }

            if (Utility.IsLoadField(instruction.OpCode.Code))
            {
                if (instruction.Operand is FieldReference)
                {
                    var fref = instruction.Operand as FieldReference;

                    var definedField = this.function.Fields.FirstOrDefault(f => f.Name == "::" + fref.Name.Replace('<', '_').Replace('>', '_'));
                    if (definedField != null)
                    {
                        EvaluationStack.Push(new EvaluationStackItem { Value = definedField, TypeName = definedField.TypeName });
                    }
                }
            }


            if (Utility.IsLoadLocalVariable(instruction.OpCode.Code))
            {
                var index = IntValue(instruction);
                if (index < function.AllVariables.Count)
                {
                    EvaluationStack.Push(new EvaluationStackItem { Value = function.AllVariables[(int)index], TypeName = function.AllVariables[(int)index].TypeName });
                }
            }

            if (Utility.IsLoadString(instruction.OpCode.Code))
            {
                var value = Utility.GetString(instruction.Operand);

                EvaluationStack.Push(new EvaluationStackItem { Value = "\"" + value + "\"", TypeName = "String" });
            }



            if (Utility.IsStoreLocalVariable(instruction.OpCode.Code) || Utility.IsStoreField(instruction.OpCode.Code))
            {

                if (instruction.Operand is FieldReference)
                {
                    var fref = instruction.Operand as FieldReference;
                    // if the EvaluationStack.Count == 0
                    // The previous instruction might have been a call that returned a value
                    // Something we did not store...
                    if (EvaluationStack.Count > 0)
                    {
                        var obj = EvaluationStack.Pop();

                        var definedField = this.function.Fields.FirstOrDefault(f => f.Name == "::" + fref.Name.Replace('<', '_').Replace('>', '_'));
                        if (definedField != null)
                        {
                            if (obj.Value is PapyrusVariableReference)
                            {
                                var varRef = obj.Value as PapyrusVariableReference;
                                definedField.Value = varRef.Value;
                                return "Assign " + definedField.Name + " " + varRef.Name;
                            }
                            definedField.Value = Utility.TypeValueConvert(definedField.TypeName, obj.Value);
                            return "Assign " + definedField.Name + " " + definedField.Value;
                        }
                    }
                }
                var index = IntValue(instruction);
                if (index < function.AllVariables.Count)
                {
                    if (EvaluationStack.Count > 0)
                    {
                        var heapObj = EvaluationStack.Pop();
                        if (heapObj.Value is PapyrusVariableReference)
                        {
                            var varRef = heapObj.Value as PapyrusVariableReference;
                            function.AllVariables[(int)index].Value = varRef.Value;
                            return "Assign " + function.AllVariables[(int)index].Name + " " + varRef.Name;
                        }


                        function.AllVariables[(int)index].Value = Utility.TypeValueConvert(function.AllVariables[(int)index].TypeName, heapObj.Value);
                    }
                    var valout = function.AllVariables[(int)index].Value;
                    var valoutStr = valout + "";
                    if (String.IsNullOrEmpty(valoutStr)) valoutStr = "None";
                    return "Assign " + function.AllVariables[(int)index].Name + " " + valoutStr;
                }

            }

            if (Utility.IsMath(instruction.OpCode.Code))
            {
                if (EvaluationStack.Count >= Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
                {
                    // should be 2.
                    // Make sure we have a temp variable if necessary
                    string concatTargetVar = GetTargetVariable(instruction, null, "Int");

                    // Equiviliant Papyrus: StrCat <output> <val1> <val2>

                    string cast;
                    var value = GetConditional(instruction, out cast);
                    var retVal = Utility.GetPapyrusMathOp(instruction.OpCode.Code) + " " + value;
                    if (!String.IsNullOrEmpty(cast)) return cast + Environment.NewLine + retVal;
                    return retVal;
                }
            }

            if (Utility.IsCallMethod(instruction.OpCode.Code))
            {
                if (instruction.Operand is MethodReference)
                {



                    var methodRef = instruction.Operand as MethodReference;

                    if (methodRef.FullName == "System.Void PapyrusDotNet.Core.Debug::Trace(System.String,System.Int32)")
                    {

                    }


                    if (methodRef.Name.ToLower().Contains("concat"))
                    {
                        // Make sure we have a temp variable if necessary
                        string concatTargetVar = GetTargetVariable(instruction, methodRef);

                        // Equiviliant Papyrus: StrCat <output> <val1> <val2>

                        var para = methodRef.Parameters;
                        var popCount = para.Count;

                        var targetVariable = EvaluationStack.Peek();
                        //	var val=EvaluationStack.Peek()
                        if (targetVariable.Value is PapyrusVariableReference)
                        {
                            targetVariable = EvaluationStack.Pop();
                        }
                        else targetVariable = null;

                        var concatValues = new List<EvaluationStackItem>();
                        for (int idx = 0; idx < popCount; idx++)
                        {
                            concatValues.Add(EvaluationStack.Pop());
                        }


                        var leftOverCount = concatValues.Count % 2;

                        List<string> strCats = new List<string>();

                        var outputStr = "";

                        for (int j = concatValues.Count - 1; j >= 0; j--)
                        {
                            if (concatValues[j].Value is PapyrusVariableReference)
                            {
                                var name = (concatValues[j].Value as PapyrusVariableReference).Name;

                                if (!concatValues[j].TypeName.ToLower().Contains("string"))
                                    strCats.Add("Cast " + concatTargetVar + " " + name);

                                strCats.Add("StrCat " + concatTargetVar + " " + concatTargetVar + " " + name);
                            }
                            else
                            {
                                strCats.Add("StrCat " + concatTargetVar + " " + concatTargetVar + " " + concatValues[j].Value);
                            }
                        }
                        var possibleMethodCall = instruction.Next;
                        if (possibleMethodCall.OpCode.Code == Code.Nop)
                        {
                            possibleMethodCall = possibleMethodCall.Next;
                        }
                        if (possibleMethodCall != null && Utility.IsCallMethod(possibleMethodCall.OpCode.Code))
                        {
                            if (targetVariable != null)
                            {
                                EvaluationStack.Push(new EvaluationStackItem() { TypeName = targetVariable.TypeName, Value = targetVariable });
                            }
                            else
                                EvaluationStack.Push(new EvaluationStackItem() { TypeName = function.AllVariables.FirstOrDefault(n => n.Name == concatTargetVar).TypeName, Value = function.AllVariables.FirstOrDefault(n => n.Name == concatTargetVar) });
                        }

                        return String.Join(Environment.NewLine, strCats.ToArray());
                    }

                    if (methodRef.Name.ToLower().Contains("op_equal") ||
                        methodRef.Name.ToLower().Contains("op_inequal"))
                    {
                        _invertedBranch = methodRef.Name.ToLower().Contains("op_inequal");
                        _skipToOffset = instruction.Next.Offset;
                        // EvaluationStack.Push(new EvaluationStackItem { IsMethodCall = true, Value = methodRef, TypeName = methodRef.ReturnType.FullName });
                        return "";
                    }

                    bool isCalledByThis = false;
                    var param = new List<EvaluationStackItem>();



                    var itemsToPop = 0;
                    if (instruction.OpCode.StackBehaviourPop == StackBehaviour.Varpop) itemsToPop = methodRef.Parameters.Count;
                    else itemsToPop = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);

                    for (int j = 0; j < itemsToPop; j++)
                    {
                        if (EvaluationStack.Count > 0)
                        {
                            var parameter = EvaluationStack.Pop();
                            if (parameter.IsThis && EvaluationStack.Count > methodRef.Parameters.Count
                                || methodRef.CallingConvention == MethodCallingConvention.ThisCall)
                            {

                                isCalledByThis = true;
                                // this.CallMethod();
                            }

                            param.Insert(0, parameter);
                        }
                    }
                    if (!isCalledByThis && EvaluationStack.Count > 0)
                    {
                        param.Insert(0, EvaluationStack.Pop());
                    }

                    MethodDefinition definition = null;
                    foreach (var ty in Assembly.MainModule.Types)
                    {
                        definition = ty.Methods.FirstOrDefault(f => f.Name == methodRef.Name);
                        if (definition != null) break;
                    }

                    try
                    {
                        if (definition == null)
                        {
                            definition = methodRef.Resolve();
                        }
                    }
                    catch
                    {

                    }

                    string targetVar = GetTargetVariable(instruction, methodRef);


                    // methodRef.Module

                    var fn = methodRef.FullName;
                    var isSystemMethod = methodRef.FullName.ToLower().Contains("papyrusdotnet.system");
                    if (definition != null || (isSystemMethod && definition == null))
                    {
                        if (isSystemMethod || (definition != null && definition.IsStatic))
                        {
                            var callerType = "self";

#warning we may have a bug here!
                            var fName = methodRef.FullName;
                            if (fName.Contains("::"))
                            {
                                var values = fName.Split(new string[] { "::" }, StringSplitOptions.None)[0];
                                if (values.Contains("."))
                                {
                                    values = values.Split('.').LastOrDefault();
                                    if (!String.IsNullOrEmpty(values) && values.ToLower() != this.Type.Name.ToLower())
                                    {
                                        callerType = values;
                                    }
                                }
                            }

                            //if (methodRef.Parameters.Count != param.Count)
                            //{
                            //    var caller = param[0];
                            //    param.Remove(caller);
                            //    callerType = caller.TypeName;
                            //    if (callerType.Contains(".")) callerType = callerType.Split('.').LastOrDefault();
                            //}
                            //else
                            //{
                            //    var fName = methodRef.FullName;
                            //    if (fName.Contains("::"))
                            //    {
                            //        var values = fName.Split(new string[] { "::" }, StringSplitOptions.None)[0];
                            //        if (values.Contains("."))
                            //        {
                            //            values = values.Split('.').LastOrDefault();
                            //            if (!string.IsNullOrEmpty(values))
                            //            {
                            //                callerType = values;
                            //            }
                            //        }
                            //    }
                            //}

                            return "CallStatic " + callerType + " " + methodRef.Name + " " + targetVar + " " + FormatParameters(methodRef, param); //definition;
                        }
                    }

                    if (isCalledByThis)
                    {
                        var callerType = "self";
                        if (methodRef.Parameters.Count != param.Count)
                        {
                            var caller = param[0];
                            param.Remove(caller);
                            callerType = caller.TypeName;
                            if (callerType.Contains(".")) callerType = callerType.Split('.').LastOrDefault();
                        }

                        return "CallMethod " + methodRef.Name + " " + callerType + " " + targetVar + " " + FormatParameters(methodRef, param);
                    }
                    else
                    {
                        var targetMethod = methodRef.Name;
                        var callerType = "self";
                        if (methodRef.Parameters.Count != param.Count && param.Count > 0)
                        {
                            var caller = param[0];
                            param.Remove(caller);
                            callerType = caller.TypeName;
                            if (callerType.Contains(".")) callerType = callerType.Split('.').LastOrDefault();

                            if (callerType.ToLower() == Type.Name.ToLower()) callerType = "self";
                            if (caller.Value is PapyrusVariableReference)
                            {
                                var varRef = (caller.Value as PapyrusVariableReference);

                                callerType = varRef.Name;

                                if (varRef.IsDelegateInstance && methodRef.Name == "Invoke")
                                {
                                    callerType = "self";
                                    targetMethod = varRef.DelegateInvokeReference;
                                }
                            }
                        }

                        // function.AllVariables 
#warning check if the variable being used is a delegate, if it is. then run the function on self instead.




                        return "CallMethod " + targetMethod + " " + callerType + " " + targetVar + " " + FormatParameters(methodRef, param);
                    }
                }
            }

            if (instruction.OpCode.Code == Code.Ret)
            {
                if (IsVoid(this._targetMethod.ReturnType))
                {
                    return "Return None";
                }

                if (EvaluationStack.Count >= Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
                {
                    var topValue = EvaluationStack.Pop();
                    if (topValue.Value is PapyrusVariableReference)
                    {
                        var variable = topValue.Value as PapyrusVariableReference;
                        return "Return " + variable.Name;
                    }
                }
                else
                {
                    return "Return None";
                }
            }

            if (Utility.IsConditional(instruction.OpCode.Code))
            {
                if (Utility.IsGreaterThan(instruction.OpCode.Code))
                {
                    string cast;
                    var outputVal = GetConditional(instruction, out cast);
                    if (!String.IsNullOrEmpty(outputVal))
                    {
                        if (!String.IsNullOrEmpty(cast))
                            return cast + Environment.NewLine + "CompareGT " + outputVal;
                        return "CompareGT " + outputVal;
                    }
                }
                else if (Utility.IsLessThan(instruction.OpCode.Code))
                {
                    string cast;
                    var outputVal = GetConditional(instruction, out cast);
                    if (!String.IsNullOrEmpty(outputVal))
                    {
                        if (!String.IsNullOrEmpty(cast))
                            return cast + Environment.NewLine + "CompareLT " + outputVal;
                        return "CompareLT " + outputVal;
                    }
                }
                else if (Utility.IsEqualTo(instruction.OpCode.Code))
                {
                    string cast;
                    var outputVal = GetConditional(instruction, out cast);
                    if (!String.IsNullOrEmpty(outputVal))
                    {
                        if (!String.IsNullOrEmpty(cast))
                            return cast + Environment.NewLine + "CompareEQ " + outputVal;
                        return "CompareEQ " + outputVal;
                    }
                }

            }

            if (Utility.IsBranchConditional(instruction.OpCode.Code))
            {
                var heapStack = EvaluationStack;

                var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
                if (EvaluationStack.Count >= popCount)
                {

                    var obj1 = EvaluationStack.Pop();
                    var obj2 = EvaluationStack.Pop();

                    // Make sure we have a temp variable if necessary
                    string temp = GetTargetVariable(instruction, null, "Bool");

                    string value1 = "";
                    string value2 = "";

                    if (obj1.Value is PapyrusVariableReference)
                        value1 = (obj1.Value as PapyrusVariableReference).Name;
                    else
                        value1 = obj1.Value.ToString();

                    if (obj2.Value is PapyrusVariableReference) value2 = (obj2.Value as PapyrusVariableReference).Name;
                    else
                        value2 = obj2.Value.ToString();

                    _skipNextInstruction = false;
                    var output = new List<string>();
                    var target = instruction.Operand;
                    var targetVal = "";
                    if (target is Instruction)
                    {
                        targetVal = "_label" + (target as Instruction).Offset;
                    }
                    else if (target != null)
                    {
                        targetVal = target.ToString();
                    }


                    if (Utility.IsBranchConditionalEQ(instruction.OpCode.Code))
                        output.Add("CompareEQ " + temp + " " + value1 + " " + value2);
                    else if (Utility.IsBranchConditionalLT(instruction.OpCode.Code))
                        output.Add("CompareLT " + temp + " " + value1 + " " + value2);
                    else if (Utility.IsBranchConditionalGT(instruction.OpCode.Code))
                        output.Add("CompareGT " + temp + " " + value1 + " " + value2);
                    else if (Utility.IsBranchConditionalGE(instruction.OpCode.Code))
                        output.Add("CompareGE " + temp + " " + value1 + " " + value2);
                    else if (Utility.IsBranchConditionalGE(instruction.OpCode.Code))
                        output.Add("CompareLE " + temp + " " + value1 + " " + value2);

                    if (!_invertedBranch)
                        output.Add("JumpT " + temp + " " + targetVal);
                    else
                        output.Add("JumpF " + temp + " " + targetVal);

                    return String.Join(Environment.NewLine, output.ToArray());
                }
            }
            else if (Utility.IsBranch(instruction.OpCode.Code))
            {
                var heapStack = EvaluationStack;
                var target = instruction.Operand;

                var targetVal = "";

                if (target is Instruction)
                {
                    targetVal = "_label" + (target as Instruction).Offset;
                }
                else if (target != null)
                {
                    targetVal = target.ToString();
                }

                if (heapStack.Count >= Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
                {
                    var value = heapStack.Pop();
                    var compareVal = "";
                    if (value.Value is PapyrusVariableReference)
                    {
                        var variable = value.Value as PapyrusVariableReference;
                        compareVal = variable.Name;
                    }
                    else compareVal = value.Value.ToString();



                    if (instruction.OpCode.Code == Code.Brtrue || instruction.OpCode.Code == Code.Brtrue_S)
                    {
                        if (_invertedBranch)
                        {
                            _invertedBranch = false;
                            return "JumpT " + compareVal + " " + targetVal;
                        }

                        return "JumpF " + compareVal + " " + targetVal;
                    }
                    if (instruction.OpCode.Code == Code.Brfalse || instruction.OpCode.Code == Code.Brfalse_S)
                    {
                        if (_invertedBranch)
                        {
                            _invertedBranch = false;
                            return "JumpF " + compareVal + " " + targetVal;
                        }
                        return "JumpT " + compareVal + " " + targetVal;
                    }
                }
                return "Jump " + targetVal;
            }


            return "";
        }

        public PapyrusVariableReference GetFieldFromSTFLD(Instruction whereToPlace)
        {
            if (Utility.IsStoreField(whereToPlace.OpCode.Code))
            {

                if (whereToPlace.Operand is FieldReference)
                {
                    var fref = whereToPlace.Operand as FieldReference;
                    // if the EvaluationStack.Count == 0
                    // The previous instruction might have been a call that returned a value
                    // Something we did not store...
                    var definedField = this.function.Fields.FirstOrDefault(f => f.Name == "::" + fref.Name.Replace('<', '_').Replace('>', '_'));
                    if (definedField != null)
                    {
                        return definedField;
                    }
                }
            }
            return null;
        }

        private string GetTargetVariable(Instruction instruction, MethodReference methodRef, string fallbackType = null, bool forceNew = false)
        {
            string targetVar = null;
            var whereToPlace = instruction.Next;

            if (whereToPlace != null && (Utility.IsStoreLocalVariable(whereToPlace.OpCode.Code) || Utility.IsStoreField(whereToPlace.OpCode.Code)))
            {

                if (Utility.IsStoreField(whereToPlace.OpCode.Code))
                {
                    var fieldData = GetFieldFromSTFLD(whereToPlace);
                    if (fieldData != null)
                    {
                        targetVar = fieldData.Name;
                    }
                }
                else
                {
                    var index = IntValue(whereToPlace);
                    if (index < function.AllVariables.Count)
                    {
                        targetVar = function.AllVariables[(int)index].Name;
                    }
                }
                _skipNextInstruction = true;

                // else 
                //EvaluationStack.Push(new EvaluationStackItem { IsMethodCall = true, Value = methodRef, TypeName = methodRef.ReturnType.FullName });
            }
            else if (whereToPlace != null && (Utility.IsLoad(whereToPlace.OpCode.Code) || Utility.IsCallMethod(whereToPlace.OpCode.Code) || Utility.IsBranchConditional(instruction.OpCode.Code)
                || Utility.IsLoadLength(instruction.OpCode.Code)))
            {
                // Most likely this function call have a return value other than Void
                // and is used for an additional method call, witout being assigned to a variable first.

                // EvaluationStack.Push(new EvaluationStackItem { IsMethodCall = true, Value = methodRef, TypeName = methodRef.ReturnType.FullName });

                var tVar = function.CreateTempVariable(!String.IsNullOrEmpty(fallbackType) ? fallbackType : methodRef.ReturnType.FullName);
                targetVar = tVar.Name;
                EvaluationStack.Push(new EvaluationStackItem { Value = tVar, TypeName = tVar.TypeName });
            }
            else
            {
                targetVar = "::NoneVar";
            }
            return targetVar;
        }



        public string GetConditional(Instruction instruction, out string cast)
        {
            cast = null;
            var code = instruction.OpCode.Code;
            var heapStack = EvaluationStack;


            if (heapStack.Count >= 2)//Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
            {
                var obj1 = heapStack.Pop();
                var obj2 = heapStack.Pop();
                var vars = function.AllVariables;
                var varIndex = 0;

                string value1 = "";
                string value2 = "";

                if (obj1.Value is PapyrusVariableReference)
                {
                    var oo = (obj1.Value as PapyrusVariableReference);
                    value1 = oo.Name;
                    if (!oo.TypeName.ToLower().Equals("int") && !oo.TypeName.ToLower().Equals("system.int32"))
                    {
                        // CAST BOOL TO INT
                        var typeVariable = GetTargetVariable(instruction, null, "Int");
                        cast = "Cast " + typeVariable + " " + value1;
                    }
                }
                else
                    value1 = obj1.Value.ToString();

                if (obj2.Value is PapyrusVariableReference)
                {
                    var oo = (obj2.Value as PapyrusVariableReference);
                    value2 = oo.Name;
                    if (!oo.TypeName.ToLower().Equals("int") && !oo.TypeName.ToLower().Equals("system.int32"))
                    {
                        // CAST BOOL TO INT
                        var typeVariable = GetTargetVariable(instruction, null, "Int");
                        cast = "Cast " + typeVariable + " " + value2;
                    }

                }
                else
                    value2 = obj2.Value.ToString();

                // if (Utility.IsGreaterThan(code) || Utility.IsLessThan(code))
                {
                    var next = instruction.Next;
                    while (next != null && !Utility.IsStoreLocalVariable(next.OpCode.Code) && !Utility.IsStoreField(next.OpCode.Code) && !Utility.IsCallMethod(next.OpCode.Code))
                    {
                        next = next.Next;
                    }
                    //	if (next != null && next.Operand is MethodReference)
                    if (instruction.Operand is MethodReference)
                    {
                        // if none found, create a temp one.
                        var methodRef = instruction.Operand as MethodReference;
                        var tVar = function.CreateTempVariable(methodRef.ReturnType.FullName);
                        var targetVar = tVar.Name;
                        EvaluationStack.Push(new EvaluationStackItem { Value = tVar, TypeName = tVar.TypeName });
                        return targetVar + " " + value2 + " " + value1;
                    }

                    if (next == null)
                    {


                        // No intentions to store this value into a variable, 
                        // Its to be used in a function call.
                        return "NULLPTR " + value2 + " " + value1;
                    }

                    _skipToOffset = next.Offset;
                    if (next.Operand is FieldReference)
                    {
                        var field = GetFieldFromSTFLD(next);
                        if (field != null)
                            return field.Name + " " + value2 + " " + value1;
                    }


                    varIndex = (int)IntValue(next);

                }

                return vars[varIndex].Name + " " + value2 + " " + value1;
            }
            return null;
        }

        public Instruction GetNextStoreLocalVariableInstruction(Instruction input, out int varIndex)
        {
            varIndex = -1;
            var next = input.Next;
            while (next != null && !Utility.IsStoreLocalVariable(next.OpCode.Code) && !Utility.IsStoreField(next.OpCode.Code) && !Utility.IsStoreElement(next.OpCode.Code))
            {
                next = next.Next;
            }


            if (next != null)
            {

                {
                    varIndex = (int)IntValue(next);
                    _skipToOffset = next.Offset;
                }
            }
            return next;
        }


        public bool HasMethod(MethodReference methodRef)
        {
            if (this.Type.Methods.Any(m => m.FullName == methodRef.FullName)) return true;
            if (this.Type.BaseType != null)
            {
                try
                {
                    var typeDef = this.Type.BaseType.Resolve();
                    if (typeDef != null)
                    {
                        if (typeDef.Methods.Any(m => m.FullName == methodRef.FullName)) return true;
                    }
                }
                catch { }
                //.m
            }
            return false;
        }

        public string FormatParameters(MethodReference methodRef, List<EvaluationStackItem> parameters)
        {
            var outp = new List<string>();
            if (parameters != null && parameters.Count > 0)
            {
                int index = 0;
                foreach (var it in parameters)
                {
                    var item = it;
                    while (item != null && item.Value is EvaluationStackItem)
                    {
                        item = it.Value as EvaluationStackItem;
                    }

                    if (item.Value is PapyrusVariableReference)
                    {
                        outp.Add((item.Value as PapyrusVariableReference).Name);
                        continue;
                    }
                    if (item.TypeName.ToLower().Equals("int"))
                    {
                        if (methodRef.Parameters[index].ParameterType == Assembly.MainModule.TypeSystem.Boolean)
                        {
                            var val = Int32.Parse(item.Value.ToString()) == 1;
                            outp.Add(val.ToString());
                            continue;
                        }
                        outp.Add(item.Value.ToString());
                    }
                    if (item.TypeName.ToLower().Equals("string"))
                    {
                        if (!item.Value.ToString().Contains("\""))
                            outp.Add("\"" + item.Value + "\"");
                        else
                            outp.Add(item.Value.ToString());
                    }
                    index++;
                }
            }
            return String.Join(" ", outp.ToArray());
        }

        public static bool IsVoid(TypeReference typeReference)
        {
            return (typeReference.FullName.ToLower().Equals("system.void") || typeReference.Name.ToLower().Equals("void"));
        }


        public double IntValue(Instruction instruction)
        {
            double index = Utility.GetCodeIndex(instruction.OpCode.Code);
            if (index == -1)
            {
                if (instruction.Operand is ParameterReference)
                {
                    // not yet implemented
                    return 0;
                }
                if (instruction.Operand is FieldReference)
                {
                    // not yet implemented
                    return 0;
                }
                if (instruction.Operand is VariableReference)
                {
                    var v = instruction.Operand as VariableReference;
                    if (v != null)
                    {
                        return Array.IndexOf(function.AllVariables.ToArray(), function.AllVariables.FirstOrDefault(va => va.Name == "V_" + v.Index));
                    }
                }
                else if (instruction.Operand is double || instruction.Operand is long || instruction.Operand is float || instruction.Operand is decimal)
                    index = Double.Parse(instruction.Operand.ToString());
                else
                    index = Int32.Parse(instruction.Operand.ToString());
            }
            return index;
        }
    }

}
