#region License

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
//     Copyright 2015, Karl Patrik Johansson, zerratar@gmail.com

#endregion

#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PapyrusDotNet.Common;
using PapyrusDotNet.Old.Papyrus;
using VariableReference = PapyrusDotNet.Common.Papyrus.VariableReference;

#endregion

namespace PapyrusDotNet.Old
{
    public class PapyrusAsmWriter
    {
        public static List<VariableReference> Fields;

        public static List<Assembly> ParsedAssemblies = new List<Assembly>();

        public static List<MethodCallPair> CallStack = new List<MethodCallPair>();
        private readonly AssemblyDefinition assembly;

        private readonly Stack<EvaluationStackItem> evaluationStack;

        private readonly Function function;

        private readonly TypeDefinition type;

        private bool invertedBranch;

        public string LastSaughtTypeName = "";

        private bool skipNextInstruction;

        private List<ParameterDefinition> skippedParameters = new List<ParameterDefinition>();

        private int skipToOffset = -1;

        private MethodDefinition targetMethod;

        public PapyrusAsmWriter(AssemblyDefinition assembly, TypeDefinition type, List<VariableReference> fields)
        {
            this.assembly = assembly;
            this.type = type;
            evaluationStack = new Stack<EvaluationStackItem>();

            function = new Function { Fields = fields };
        }

        private static void GetCallStack(TypeDefinition type)
        {
            foreach (var m in type.Methods)
            {
                if (m.HasBody)
                {
                    foreach (var i in m.Body.Instructions)
                    {
                        if (InstructionHelper.IsCallMethod(i.OpCode.Code))
                        {
                            var mRef = i.Operand as MethodReference;
                            if (mRef != null)
                            {
                                CallStack.Add(new MethodCallPair(m, mRef));
                            }
                        }
                    }
                }
            }
        }

        public static void GetCallStackRecursive(TypeDefinition type)
        {
            if (!type.FullName.ToLower().Contains("<module>"))
            {
                GetCallStack(type);
                if (type.HasNestedTypes)
                {
                    foreach (var nt in type.NestedTypes)
                    {
                        GetCallStackRecursive(nt);
                    }
                }
            }
        }

        public static void GeneratePapyrusFromAssembly(AssemblyDefinition currentAssembly,
            ref Dictionary<string, string> outputPasFiles, AssemblyDefinition mainAssembly = null)
        {
            // CallStack.Clear();


            foreach (var module in currentAssembly.Modules)
            {
                if (!module.HasTypes) continue;

                var types = module.Types;
                foreach (var type in types)
                {
                    if (type.FullName.ToLower().Contains("<module>")) continue;

                    Console.WriteLine("Generating Papyrus Asm for " + type.FullName);

                    Fields = new List<VariableReference>();

                    var outputPapyrus = "";

                    var properties = Utility.GetFlagsAndProperties(type);
                    if (properties.IsGeneric)
                    {
                        // Get all usages to know which generic types are necessary to be generated.
                        // then for each of those, we will generate a new class to represent it.
                        // replacing all 'T' with the types used.

                        var references = AssemblyHelper.GetAllGenericReferences(type, mainAssembly ?? currentAssembly);

                        // Output values are not being used. But the actual method needs to be called.
                        AssemblyHelper.GetAllGenericReferences(type, currentAssembly);
                        // Output values are not being used. But the actual method needs to be called.
                        AssemblyHelper.GetAllReferences(type, currentAssembly);

                        foreach (var r in references)
                        {
                            if (r.Type == "T") continue;

                            var outputAssembly = new Assembly(type, r.Type);

                            outputAssembly.OutputName = outputAssembly.BaseType.Replace("`1",
                                "_" + Utility.GetPapyrusBaseType(r.Type));

                            ParsedAssemblies.Add(outputAssembly);
                        }
                    }
                    else
                    {
                        ParsedAssemblies.Add(new Assembly(type));
                    }

                    var assembly = ParsedAssemblies.LastOrDefault();
                    // Don't write any assembly in case it is an enum.
                    // However, we still want the type to be parsed, 
                    // so creating a new Instance of assembly is still necessary
                    if (assembly == null || assembly.IsEnum) continue;

                    outputPapyrus += assembly.Header.SourceInfo.ToString();
                    outputPapyrus += assembly.Header.UserFlagReferenceRef.ToString();
                    outputPapyrus += WritePapyrusObjectTable(assembly);

                    assembly.FinalAssemblyCode = outputPapyrus;

                    if (!outputPasFiles.ContainsKey(assembly.OutputName + ".pas") &&
                        !assembly.SourceType.Namespace.EndsWith("System.Linq"))
                    {
                        outputPasFiles.Add(assembly.OutputName + ".pas", outputPapyrus);
                    }
                }
            }
        }

        public Function CreateFunction(Assembly asm, MethodDefinition method, string overrideFunctionName = null)
        {
            skippedParameters = new List<ParameterDefinition>();
            // callStack = new List<MethodCallPair>();
            var sourceBuilder = new StringBuilder();

            targetMethod = method;

            if (!string.IsNullOrEmpty(overrideFunctionName))
                targetMethod.Name = overrideFunctionName;

            var returnType = Utility.GetPapyrusReturnType(method.ReturnType);

            var staticMarker = method.IsStatic ? " static" : "";

            function.Assembly = asm;
            function.IsGlobal = method.IsStatic;
            function.Name = method.Name;
            function.MethodDefinition = method;
            function.ReturnType = returnType;

            // Native methods or functions can be declared			
            foreach (var attr in method.CustomAttributes)
            {
                if (!attr.AttributeType.Name.Equals("NativeAttribute")) continue;
                method.IsNative = true;
                break;
            }

            function.IsNative = method.IsNative;

            var nativeMarker = method.IsNative ? " native" : "";

            var indentDepth = 4;

            sourceBuilder.AppendLine(Utility.Indent(indentDepth++,
                ".function " + method.Name + staticMarker + nativeMarker, false));
            sourceBuilder.AppendLine(Utility.Indent(indentDepth, ".userFlags " + function.UserFlags, false));
            sourceBuilder.AppendLine(Utility.Indent(indentDepth, ".docString \"\"", false));
            sourceBuilder.AppendLine(Utility.Indent(indentDepth, ".return " + function.ReturnType, false));
            sourceBuilder.AppendLine(Utility.Indent(indentDepth++, ".paramTable", false));

            foreach (var parameter in method.Parameters)
            {
                var parameterOutput = ParseParameter(parameter);
                if (!string.IsNullOrEmpty(parameterOutput))
                    sourceBuilder.AppendLine(Utility.Indent(indentDepth, parameterOutput, false));
            }

            sourceBuilder.AppendLine(Utility.Indent(--indentDepth, ".endParamTable", false));
            sourceBuilder.AppendLine(Utility.Indent(indentDepth++, ".localTable", false));

            if (method.HasBody)
            {
                if (HasVoidCalls(method.Body))
                    sourceBuilder.AppendLine(Utility.Indent(indentDepth, ".local ::NoneVar None", false));

                foreach (var variable in method.Body.Variables)
                {
                    if (variable.VariableType.Name.StartsWith("<>"))
                        continue; // Delegate variables should not be added.

                    sourceBuilder.AppendLine(Utility.Indent(indentDepth, ParseLocalVariable(variable), false));
                }
            }

            sourceBuilder.AppendLine(Utility.Indent(--indentDepth, ".endLocalTable", false));
            sourceBuilder.AppendLine(Utility.Indent(indentDepth++, ".code", false));
            if (method.HasBody)
            {
                foreach (var instruction in method.Body.Instructions)
                {
                    // This is currently being used for debugging and trying to solve the FirstOrDefault() Linq extension
                    if (targetMethod.Name.Contains("FirstOrDefault") &&
                        InstructionHelper.IsCallMethod(instruction.OpCode.Code))
                    {
                    }

                    // We will add a new label for each instruction.
                    // And at the end we will remove any unused instructions.
                    sourceBuilder.AppendLine(Utility.Indent(indentDepth - 1, "_label" + instruction.Offset + ":", false));

                    if (skipNextInstruction)
                    {
                        skipNextInstruction = false;
                        continue;
                    }

                    if (skipToOffset > 0)
                    {
                        if (instruction.Offset <= skipToOffset)
                        {
                            continue;
                        }
                        skipToOffset = -1;
                    }

                    if (instruction.OpCode.Code == Code.Ldnull)
                    {
                        // Do nothing for now.
                    }

                    var value = ParseInstruction(instruction);
                    // We should never call the original constructor :p
                    // We do however call the renamed constructor from inside OnInit
                    // That way we are able to "merge" OnInit and the Constructor. :-)
                    if (value.Contains(".ctor"))
                    {
                        continue;
                    }
                    // If nothing was returned from the ParseInstruction, we will just continue with the next one.
                    if (string.IsNullOrEmpty(value)) continue;
                    // If the returned value returns a new line (The parsed CIL instruction returned more than one Papyrus Instruction)
                    // We will have to format it properly and add each instruction to our function.
                    if (value.Contains(Environment.NewLine))
                    {
                        var rows = value.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
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
            sourceBuilder.AppendLine(Utility.Indent(--indentDepth, ".endCode", false));
            sourceBuilder.AppendLine(Utility.Indent(--indentDepth, ".endFunction", false));

            // Post Optimizations

            var output = sourceBuilder.ToString();

            output = Utility.OptimizeLabels(output);
            output = Utility.InjectTempVariables(output, indentDepth + 2, function.TempVariables);

            sourceBuilder = new StringBuilder(output);

            function.Source = sourceBuilder;

            return function;
        }

        public static string WritePapyrusObjectTable(Assembly asm)
        {
            var papyrus = WritePapyrusObjectTableCore(asm);

            if (string.IsNullOrEmpty(asm.GenericTypeReplacement)) return papyrus;

            var ptype = Utility.GetPapyrusBaseType(asm.GenericTypeReplacement);

            var papyrusRow = papyrus.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            for (var j = 0; j < papyrusRow.Length; j++)
            {
                // If the the code contains a Generic Indicator we need to replace it to the 
                // actual used Type instead.
                if (papyrusRow[j].EndsWith(" T") || papyrusRow[j].EndsWith("_T"))
                {
                    papyrusRow[j] = papyrusRow[j].Remove(papyrusRow[j].LastIndexOf('T')) + ptype;
                }
                if (papyrusRow[j].Contains(".object ") && papyrusRow[j].Contains("`1"))
                {
                    papyrusRow[j] = papyrusRow[j].Replace("`1", "_" + ptype);
                }
                else if (papyrusRow[j].Contains("`1"))
                {
                    papyrusRow[j] = papyrusRow[j].Replace("`1", "_" + ptype);
                }
            }
            return string.Join(Environment.NewLine, papyrusRow);
        }

        public static string WritePapyrusObjectTableCore(Assembly asm)
        {
            var papyrus = "";

            papyrus += ".objectTable" + Environment.NewLine;
            papyrus += ("\t.object " + asm.ObjectTable.Name + " " + asm.ObjectTable.BaseType).Trim() +
                       Environment.NewLine;

            papyrus += "\t\t.userFlags " + asm.ObjectTable.Info.UserFlagsValue + Environment.NewLine;
            papyrus += "\t\t.docString \"" + asm.ObjectTable.Info.DocString + "\"" + Environment.NewLine;


#warning autoState is the default state to be used, however if we are to enable the use of more states. This needs to be fixed.

            papyrus += "\t\t.autoState" + Environment.NewLine;
            papyrus += "\t\t.variableTable" + Environment.NewLine;


            foreach (var variable in asm.ObjectTable.VariableTable)
            {
                var userFlagsVal = variable.Attributes.UserFlagsValue;
                if (variable.Attributes.IsHidden && variable.Attributes.IsProperty)
                {
                    userFlagsVal -= 1;
                    // The hidden flag only effects the Property field, declared after the variable field
                }

                // Replace all Enums with Integers instead.
                var reference = ParsedAssemblies.FirstOrDefault(a => a.OutputName == variable.TypeName);
                if (reference != null && reference.IsEnum)
                {
                    variable.TypeName = "Int";
                }
                // variable.TypeName

                papyrus += Utility.Indent(3, ".variable " + variable.Name + " " + variable.TypeName);
                papyrus += Utility.Indent(4, ".userFlags " + userFlagsVal);
                papyrus += Utility.Indent(4, ".initialValue " + variable.Attributes.InitialValue);
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

                var autoMarker = fieldProp.Attributes.IsAuto ? " auto" : "";
                papyrus += Utility.Indent(3, ".property " + fieldProp.Name + " " + fieldProp.TypeName + autoMarker);
                papyrus += Utility.Indent(4, ".userFlags " + fieldProp.Attributes.UserFlagsValue);
                papyrus += Utility.Indent(4, ".docString \"\"");
                papyrus += Utility.Indent(4, ".autoVar " + fieldProp.AutoVarName);
                papyrus += Utility.Indent(3, ".endProperty");
            }

            papyrus += "\t\t.endPropertyTable" + Environment.NewLine;
            papyrus += "\t\t.stateTable" + Environment.NewLine;
            papyrus += "\t\t\t.state" + Environment.NewLine;

            // We could use SelectMany(i => i.Functions) instead
            // but since we do want to support StateTables in the future,
            // the enumeration will be kept like this for now.
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
                if (!InstructionHelper.IsCallMethod(s.OpCode.Code)) continue;

                var methodReference = s.Operand as MethodReference;

                if (methodReference == null) continue;

                if (IsVoid(methodReference.ReturnType)) return true;
            }
            return false;
        }

        private string ParseParameter(ParameterDefinition parameter, bool parseFuncParameter = false)
        {
            var name = parameter.Name;
            var typeFullName = parameter.ParameterType.FullName;
            var typeName = parameter.ParameterType.Name;

            if (typeName.StartsWith("Func`"))
            {
                /* Following code below would allow a proper conversion of the Func parameter name to be used
                 * however, when it comes to FirstOrDefault/LastOrDefault, etc, the predicate is parameter is not necessary as it wont be used. 
                 * Which means, we can actually skip to parse this parameter completely. However we still want to keep track on this param */
                if (!parseFuncParameter)
                {
                    skippedParameters.Add(parameter);
                    return "";
                }

                var genericType = parameter.ParameterType as GenericInstanceType;
                var args = genericType?.GenericArguments;
                if (args?.Count > 1)
                {
                    typeFullName = args[1].FullName;
                    typeName = args[1].Name;
                }
            }

            var val = Utility.GetPapyrusReturnType(typeName, parameter.ParameterType.Namespace);
            if (parameter.ParameterType.IsGenericInstance)
            {
                if (typeFullName.Contains("<"))
                {
                    var targetName = typeFullName.Split('<')[1].Split('>')[0];
                    var papName = Utility.GetPapyrusBaseType(targetName);

                    val = val.Replace("`1", "_" + papName);
                }
                else
                {
                    var papName = Utility.GetPapyrusBaseType(typeFullName);

                    val = val.Replace("`1", "_" + papName);
                }
            }
            else if (parameter.ParameterType.Name.Contains("<T>"))
            {
                val = Utility.GetPapyrusBaseType(typeFullName).Replace("<T>", "");
            }

            val = val.Replace("<T>", "");

            function.Parameters.Add(new VariableReference(name, typeFullName));

            return ".param " + name + " " + val;
        }

        private string ParseLocalVariable(VariableDefinition variable)
        {
            var name = variable.Name;
            var type = variable.VariableType.FullName;
            var typeN = variable.VariableType.Name;

            if (string.IsNullOrEmpty(name))
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

                    var functionName = function.Name;

                    // In case this is a delegate inside a delegate...
                    // _UtilizeDelegate4_b__0
                    if (functionName.StartsWith("_") && functionName.Contains("b_"))
                    {
                        functionName = functionName.Split('_')[1];
                        function.DelegateInvokeCount++;
                    }


                    var delegateMethod =
                        function.Assembly.DelegateMethodDefinitions.FirstOrDefault(
                            del =>
                                del.Name.Contains("_" + functionName + "_") &&
                                del.Name.EndsWith("_" + function.DelegateInvokeCount));

                    if (delegateMethod == null)
                    {
                        delegateMethod =
                            function.Assembly.DelegateMethodDefinitions.FirstOrDefault(
                                del =>
                                    del.Name.Contains("_" + functionName + "_") && del.Name.Contains("b_") &&
                                    del.Name != function.Name);
                    }

                    function.DelegateInvokeCount++;

                    delegateInvokeRef = delegateMethod?.Name;
                    // "_" + function.Name + "_b__1_" + (delegatesUsed++); // varType.Name;

                    // var tn = ;
                }
            }

            function.Variables.Add(new VariableReference(name, type)
            {
                IsDelegateInstance = delegateInstanceVar,
                DelegateInvokeReference = delegateInvokeRef
            });

            return ".local " + name + " " + val; //Utility.GetPapyrusReturnType(typeN, variable.VariableType.Namespace);
        }


        private string ParseInstruction(Instruction instruction)
        {
            var passThroughConditional = false;
            // ArrayGetElement	<outputVarName>	<arrayName>	<int:index>
            // ArraySetElement <arrayName> <int:index> <valueOrVariable>?
            // ArrayLength	<outputVarName>	<arrayName>

            if (InstructionHelper.IsConverToNumber(instruction.OpCode.Code))
            {
                return "";
            }

            if (InstructionHelper.IsBoxing(instruction.OpCode.Code))
            {
                // EX: 
                // int a = 0;
                // MyEnum b = MyEnum.First;
                // a++;
                // b = (MyEnum)a;


                //var heapStack = evaluationStack;
                //var obj1 = heapStack.Pop();
                //var obj2 = heapStack.Pop();
                //var vars = function.AllVariables;
                //var varIndex = 0;

                //string value1 = "";
                //string value2 = "";

                //if (obj1.Value is VariableReference)
                //{
                //    var oo = (obj1.Value as VariableReference);
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

                //if (obj2.Value is VariableReference)
                //{
                //    var oo = (obj2.Value as VariableReference);
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

            if (InstructionHelper.IsLoadStaticField(instruction.OpCode.Code))
            {
                return "";
            }

            if (InstructionHelper.IsLoadElement(instruction.OpCode.Code))
            {
                var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
                if (evaluationStack.Count >= popCount)
                {
                    var itemIndex = evaluationStack.Pop();
                    var itemArray = evaluationStack.Pop();

                    VariableReference sourceArray = null;

                    var tar = "";
                    var idx = -1;
                    var oidx = "0";
                    if (itemIndex.Value is VariableReference)
                    {
                        oidx = (itemIndex.Value as VariableReference).Name;
                    }
                    else if (itemIndex.Value != null)
                        idx = int.Parse(itemIndex.Value.ToString());

                    if (idx > 128) idx = 128;
                    if (idx != -1)
                        oidx = idx.ToString();

                    if (itemArray.Value is VariableReference)
                    {
                        sourceArray = itemArray.Value as VariableReference;
                        tar = sourceArray.Name;
                    }


                    // Supports:
                    // var obj = array[x]

                    // Not yet supporting? note: i have not tried it yet.
                    // Function(array[x],...)

                    if (InstructionHelper.IsCallMethod(instruction.Next.OpCode.Code))
                    {
                        var targetMethod = instruction.Next.Operand as MethodReference;
                        if (targetMethod != null && targetMethod.HasParameters)
                        {
                            var tarVar = GetTargetVariable(instruction, targetMethod,
                                sourceArray.TypeName.Replace("[]", ""));

                            return "ArrayGetElement " + tarVar + " " + tar + " " + oidx;
                        }
                    }

                    var targetVariableIndex = 0;
                    var tarIn = GetNextStoreLocalVariableInstruction(instruction, out targetVariableIndex);
                    if (tarIn != null)
                    {
                        return "ArrayGetElement " + function.AllVariables[targetVariableIndex].Name + " " + tar + " " +
                               oidx;
                    }
                }
            }

            if (InstructionHelper.IsStoreElement(instruction.OpCode.Code))
            {
                var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
                if (evaluationStack.Count >= popCount)
                {
                    var newValue = evaluationStack.Pop();
                    var itemIndex = evaluationStack.Pop();
                    var itemArray = evaluationStack.Pop();

                    var val = "";
                    var tar = "";
                    var idx = -1;
                    var oidx = "0";
                    if (itemIndex.Value is VariableReference)
                        oidx = (itemIndex.Value as VariableReference).Name;
                    else if (itemIndex.Value != null)
                        idx = int.Parse(itemIndex.Value.ToString());

                    if (idx > 128) idx = 128;
                    if (idx != -1)
                        oidx = idx.ToString();
                    if (itemArray.Value is VariableReference)
                        tar = (itemArray.Value as VariableReference).Name;

                    if (newValue.Value is VariableReference)
                        val = (newValue.Value as VariableReference).Name;

                    else if (newValue.Value != null) val = newValue.Value.ToString();

                    return "ArraySetElement " + tar + " " + oidx + " " + val;
                }
            }

            if (Utility.IsLoadLength(instruction.OpCode.Code))
            {
                var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
                if (evaluationStack.Count >= popCount)
                {
                    var val = evaluationStack.Pop();
                    if (val.TypeName.EndsWith("[]"))
                    {
                        if (val.Value is VariableReference)
                        {
                            var variableIndex = 0;
                            var storeInstruction = GetNextStoreLocalVariableInstruction(instruction, out variableIndex);
                            if (storeInstruction != null ||
                                InstructionHelper.IsConverToNumber(instruction.Next.OpCode.Code))
                            {
                                if (InstructionHelper.IsConverToNumber(instruction.Next.OpCode.Code))
                                {
                                    skipNextInstruction = false;
                                    skipToOffset = 0;

                                    var targetVariable = GetTargetVariable(instruction, null, "Int", true);

                                    var vari = function.AllVariables.FirstOrDefault(va => va.Name == targetVariable);

                                    //evaluationStack.Push(new EvaluationStackItem() { IsMethodCall = false, IsThis = false, TypeName = vari.TypeName, Value = vari });
                                    return "ArrayLength " + targetVariable + " " + (val.Value as VariableReference).Name;
                                }
                                evaluationStack.Push(new EvaluationStackItem
                                {
                                    IsMethodCall = false,
                                    IsThis = false,
                                    TypeName = function.AllVariables[variableIndex].TypeName,
                                    Value = function.AllVariables[variableIndex]
                                });
                                return "ArrayLength " + function.AllVariables[variableIndex].Name + " " +
                                       (val.Value as VariableReference).Name;
                            }
                        }
                    }
                }
                // ArrayLength <outputVariableName> <arrayName>
            }

            if (InstructionHelper.IsNewArrayInstance(instruction.OpCode.Code))
            {
                var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
                if (evaluationStack.Count >= popCount)
                {
                    var val = evaluationStack.Pop();

                    var targetVariableIndex = 0;
                    var tarIn = GetNextStoreLocalVariableInstruction(instruction, out targetVariableIndex);
                    if (tarIn != null)
                    {
                        if (tarIn.Operand is FieldReference)
                        {
                            var fref = tarIn.Operand as FieldReference;
                            // if the evaluationStack.Count == 0
                            // The previous instruction might have been a call that returned a value
                            // Something we did not store...

                            var definedField =
                                function.Fields.FirstOrDefault(
                                    f => f.Name == "::" + fref.Name.Replace('<', '_').Replace('>', '_'));

                            if (definedField != null)
                            {
                                if (evaluationStack.Count > 0)
                                {
                                    var obj = evaluationStack.Pop();

                                    if (obj.Value is VariableReference)
                                    {
                                        var varRef = obj.Value as VariableReference;
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
            if (InstructionHelper.IsNewObjectInstance(instruction.OpCode.Code))
            {
                var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
                // the obj objN = new obj
                // is not supported by Papyrus.
                // this opCode should be ignored, but we should still pop the values from the stack
                // to maintain correct work flow.

                for (var pops = 0; pops < popCount; pops++)
                {
                    if (evaluationStack.Count > 0)
                        evaluationStack.Pop();
                }
                int oi;
                GetNextStoreLocalVariableInstruction(instruction, out oi);
            }
            if (InstructionHelper.IsLoadArgs(instruction.OpCode.Code))
            {
                var index = IntValue(instruction);
                if (targetMethod.IsStatic && (int)index == 0 && targetMethod.Parameters.Count == 0)
                {
                    evaluationStack.Push(new EvaluationStackItem { IsThis = true, Value = type, TypeName = type.FullName });
                }
                else
                {
                    if (!targetMethod.IsStatic && index > 0) index--;
                    if (index < function.Parameters.Count)
                    {
                        evaluationStack.Push(new EvaluationStackItem
                        {
                            Value = function.Parameters[(int)index],
                            TypeName = function.Parameters[(int)index].TypeName
                        });
                    }
                }
            }
            if (InstructionHelper.IsLoadInteger(instruction.OpCode.Code))
            {
                var index = IntValue(instruction);
                evaluationStack.Push(new EvaluationStackItem { Value = index, TypeName = "Int" });
            }

            if (InstructionHelper.IsLoadNull(instruction.OpCode.Code))
            {
                evaluationStack.Push(new EvaluationStackItem { Value = "None", TypeName = "None" });
            }

            if (InstructionHelper.IsLoadField(instruction.OpCode.Code))
            {
                if (instruction.Operand is FieldReference)
                {
                    var fref = instruction.Operand as FieldReference;

                    var definedField =
                        function.Fields.FirstOrDefault(
                            f => f.Name == "::" + fref.Name.Replace('<', '_').Replace('>', '_'));
                    if (definedField != null)
                    {
                        evaluationStack.Push(new EvaluationStackItem
                        {
                            Value = definedField,
                            TypeName = definedField.TypeName
                        });
                    }
                }
            }


            if (InstructionHelper.IsLoadLocalVariable(instruction.OpCode.Code))
            {
                var index = IntValue(instruction);
                if (index < function.AllVariables.Count)
                {
                    evaluationStack.Push(new EvaluationStackItem
                    {
                        Value = function.AllVariables[(int)index],
                        TypeName = function.AllVariables[(int)index].TypeName
                    });
                }
            }

            if (InstructionHelper.IsLoadString(instruction.OpCode.Code))
            {
                var value = Utility.GetString(instruction.Operand);

                evaluationStack.Push(new EvaluationStackItem { Value = "\"" + value + "\"", TypeName = "String" });
            }


            if (InstructionHelper.IsStoreLocalVariable(instruction.OpCode.Code) ||
                InstructionHelper.IsStoreField(instruction.OpCode.Code))
            {
                if (instruction.Operand is FieldReference)
                {
                    var fref = instruction.Operand as FieldReference;
                    // if the evaluationStack.Count == 0
                    // The previous instruction might have been a call that returned a value
                    // Something we did not store...
                    if (evaluationStack.Count > 0)
                    {
                        var obj = evaluationStack.Pop();

                        var definedField =
                            function.Fields.FirstOrDefault(
                                f => f.Name == "::" + fref.Name.Replace('<', '_').Replace('>', '_'));
                        if (definedField != null)
                        {
                            if (obj.Value is VariableReference)
                            {
                                var varRef = obj.Value as VariableReference;
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
                    if (evaluationStack.Count > 0)
                    {
                        var heapObj = evaluationStack.Pop();
                        if (heapObj.Value is VariableReference)
                        {
                            var varRef = heapObj.Value as VariableReference;
                            function.AllVariables[(int)index].Value = varRef.Value;
                            return "Assign " + function.AllVariables[(int)index].Name + " " + varRef.Name;
                        }


                        function.AllVariables[(int)index].Value =
                            Utility.TypeValueConvert(function.AllVariables[(int)index].TypeName, heapObj.Value);
                    }
                    var valout = function.AllVariables[(int)index].Value;
                    var valoutStr = valout + "";
                    if (string.IsNullOrEmpty(valoutStr)) valoutStr = "None";
                    return "Assign " + function.AllVariables[(int)index].Name + " " + valoutStr;
                }
            }

            if (InstructionHelper.IsMath(instruction.OpCode.Code))
            {
                if (evaluationStack.Count >= Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
                {
                    // should be 2.
                    // Make sure we have a temp variable if necessary
                    var concatTargetVar = GetTargetVariable(instruction, null, "Int");

                    // Equiviliant Papyrus: StrCat <output> <val1> <val2>

                    string cast;
                    var value = GetConditional(instruction, out cast);
                    var retVal = Utility.GetPapyrusMathOp(instruction.OpCode.Code) + " " + value;
                    if (!string.IsNullOrEmpty(cast)) return cast + Environment.NewLine + retVal;
                    return retVal;
                }
            }
            string overrideMethodCallName = null;
            if (InstructionHelper.IsCallMethod(instruction.OpCode.Code))
            {
                if (instruction.Operand is MethodReference)
                {
                    var methodRef = instruction.Operand as MethodReference;

                    /* Check if we are calling a invoke function from a skipped Func parameter */
                    if (methodRef.Name.Contains("Invoke"))
                    {
                        var loadArgIndex = -1;
                        if (instruction.Previous != null &&
                            InstructionHelper.IsLoadArgs(instruction.Previous.OpCode.Code))
                        {
                            loadArgIndex = InstructionHelper.GetCodeIndex(instruction.Previous.OpCode.Code);
                        }
                        if (instruction.Previous != null && instruction.Previous.Previous != null &&
                            InstructionHelper.IsLoadArgs(instruction.Previous.Previous.OpCode.Code))
                        {
                            loadArgIndex = InstructionHelper.GetCodeIndex(instruction.Previous.Previous.OpCode.Code);
                        }
                        if (loadArgIndex > 0 && loadArgIndex >= function.Parameters.Count)
                        {
                            var callHierarchy = CallStack;
                            if (callHierarchy.Count > 0)
                            {
                                var skippedParam = skippedParameters.LastOrDefault();
                                if (skippedParam != null)
                                {
                                    // skippedParam should be a predicate, but at this  point we already know that.
                                    // And by knowing that we know we are actually calling a compiler-time generated method
                                    var caller =
                                        callHierarchy.FirstOrDefault(f => f.TargetMethod.Name == targetMethod.Name);
                                    if (caller.CallerMethod != null)
                                    {
                                        overrideMethodCallName = "_" + caller.CallerMethod.Name + "_b__" +
                                                                 function.ExtensionInvokeCount++;
                                    }
                                }
                            }
                        }
                    }


                    if (methodRef.Name.ToLower().Contains("concat"))
                    {
                        // Make sure we have a temp variable if necessary
                        var concatTargetVar = GetTargetVariable(instruction, methodRef);

                        // Equiviliant Papyrus: StrCat <output> <val1> <val2>

                        var para = methodRef.Parameters;
                        var popCount = para.Count;

                        var targetVariable = evaluationStack.Peek();
                        //	var val=evaluationStack.Peek()
                        if (targetVariable.Value is VariableReference)
                        {
                            targetVariable = evaluationStack.Pop();
                        }
                        else targetVariable = null;

                        var concatValues = new List<EvaluationStackItem>();
                        for (var idx = 0; idx < popCount; idx++)
                        {
                            concatValues.Add(evaluationStack.Pop());
                        }


                        //var leftOverCount = concatValues.Count % 2;

                        var strCats = new List<string>();

                        //var outputStr = "";

                        for (var j = concatValues.Count - 1; j >= 0; j--)
                        {
                            if (concatValues[j].Value is VariableReference)
                            {
                                var name = (concatValues[j].Value as VariableReference).Name;

                                if (!concatValues[j].TypeName.ToLower().Contains("string"))
                                    strCats.Add("Cast " + concatTargetVar + " " + name);

                                strCats.Add("StrCat " + concatTargetVar + " " + concatTargetVar + " " + name);
                            }
                            else
                            {
                                strCats.Add("StrCat " + concatTargetVar + " " + concatTargetVar + " " +
                                            concatValues[j].Value);
                            }
                        }
                        var possibleMethodCall = instruction.Next;
                        if (possibleMethodCall.OpCode.Code == Code.Nop)
                        {
                            possibleMethodCall = possibleMethodCall.Next;
                        }
                        if (possibleMethodCall != null && InstructionHelper.IsCallMethod(possibleMethodCall.OpCode.Code))
                        {
                            if (targetVariable != null)
                            {
                                evaluationStack.Push(new EvaluationStackItem
                                {
                                    TypeName = targetVariable.TypeName,
                                    Value = targetVariable
                                });
                            }
                            else
                                evaluationStack.Push(new EvaluationStackItem
                                {
                                    TypeName =
                                        function.AllVariables.FirstOrDefault(n => n.Name == concatTargetVar).TypeName,
                                    Value = function.AllVariables.FirstOrDefault(n => n.Name == concatTargetVar)
                                });
                        }

                        return string.Join(Environment.NewLine, strCats.ToArray());
                    }

                    if (methodRef.Name.ToLower().Contains("op_equal") ||
                        methodRef.Name.ToLower().Contains("op_inequal"))
                    {
                        invertedBranch = methodRef.Name.ToLower().Contains("op_inequal");

                        if (!InstructionHelper.IsStore(instruction.Next.OpCode.Code))
                        {
                            skipToOffset = instruction.Next.Offset;
                            return "";
                        }
                        // evaluationStack.Push(new EvaluationStackItem { IsMethodCall = true, Value = methodRef, TypeName = methodRef.ReturnType.FullName });
                        passThroughConditional = true;
                        goto EqualityCheck;
                    }

                    var isCalledByThis = false;
                    var param = new List<EvaluationStackItem>();


                    var itemsToPop = 0;
                    if (instruction.OpCode.StackBehaviourPop == StackBehaviour.Varpop)
                        itemsToPop = methodRef.Parameters.Count;
                    else itemsToPop = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);

                    for (var j = 0; j < itemsToPop; j++)
                    {
                        if (evaluationStack.Count > 0)
                        {
                            var parameter = evaluationStack.Pop();
                            if (parameter.IsThis && evaluationStack.Count > methodRef.Parameters.Count
                                || methodRef.CallingConvention == MethodCallingConvention.ThisCall)
                            {
                                isCalledByThis = true;
                                // this.CallMethod();
                            }

                            param.Insert(0, parameter);
                        }
                    }
                    if (!isCalledByThis && evaluationStack.Count > 0)
                    {
                        param.Insert(0, evaluationStack.Pop());
                    }

                    MethodDefinition definition = null;
                    foreach (var ty in assembly.MainModule.Types)
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

                    var targetVar = GetTargetVariable(instruction, methodRef);


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
                                var values = fName.Split(new[] { "::" }, StringSplitOptions.None)[0];
                                if (values.Contains("."))
                                {
                                    values = values.Split('.').LastOrDefault();
                                    if (!string.IsNullOrEmpty(values) && values.ToLower() != type.Name.ToLower())
                                    {
                                        callerType = values;
                                    }
                                }
                            }

                            MethodReference methodRef2 = null;

                            if (!Utility.IsCallMethodInsideNamespace(
                                instruction,
                                "PapyrusDotNet.System.Linq",
                                out methodRef2))

                                return "CallStatic " + callerType + " " +
                                       (!string.IsNullOrEmpty(overrideMethodCallName)
                                           ? overrideMethodCallName
                                           : methodRef.Name) + " " + targetVar + " "
                                       + FormatParameters(methodRef, param); //definition;                            
                            if (!string.IsNullOrEmpty(LastSaughtTypeName))
                            {
                                var defaultState = function.Assembly.ObjectTable.StateTable.FirstOrDefault();
                                if (defaultState != null)
                                {
                                    var linqFunction =
                                        defaultState.Functions.FirstOrDefault(f => f.Name == methodRef.Name);
                                    if (linqFunction != null)
                                    {
                                        linqFunction.InstanceCaller = targetMethod.Name;
                                        linqFunction.ReplaceGenericTypesWith(
                                            Utility.GetPapyrusReturnType(LastSaughtTypeName));
                                    }
                                }
                            }
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

                        return "CallMethod " +
                               (!string.IsNullOrEmpty(overrideMethodCallName) ? overrideMethodCallName : methodRef.Name) +
                               " " + callerType + " " + targetVar + " " + FormatParameters(methodRef, param);
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

                            if (callerType.ToLower() == type.Name.ToLower()) callerType = "self";
                            if (caller.Value is VariableReference)
                            {
                                var varRef = caller.Value as VariableReference;

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

                        return "CallMethod " +
                               (!string.IsNullOrEmpty(overrideMethodCallName) ? overrideMethodCallName : targetMethod) +
                               " " + callerType + " " + targetVar + " " + FormatParameters(methodRef, param);
                    }
                }
            }

            if (instruction.OpCode.Code == Code.Ret)
            {
                if (IsVoid(targetMethod.ReturnType))
                {
                    return "Return None";
                }

                if (evaluationStack.Count >= Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
                {
                    var topValue = evaluationStack.Pop();
                    if (topValue.Value is VariableReference)
                    {
                        var variable = topValue.Value as VariableReference;
                        return "Return " + variable.Name;
                    }
                }
                else
                {
                    return "Return None";
                }
            }


            EqualityCheck:

            if (Utility.IsConditional(instruction.OpCode.Code) || passThroughConditional)
            {
                if (InstructionHelper.IsGreaterThan(instruction.OpCode.Code))
                {
                    string cast;
                    var outputVal = GetConditional(instruction, out cast);
                    if (!string.IsNullOrEmpty(outputVal))
                    {
                        if (!string.IsNullOrEmpty(cast))
                            return cast + Environment.NewLine + "CompareGT " + outputVal;
                        return "CompareGT " + outputVal;
                    }
                }
                else if (InstructionHelper.IsLessThan(instruction.OpCode.Code))
                {
                    string cast;
                    var outputVal = GetConditional(instruction, out cast);
                    if (!string.IsNullOrEmpty(outputVal))
                    {
                        if (!string.IsNullOrEmpty(cast))
                            return cast + Environment.NewLine + "CompareLT " + outputVal;
                        return "CompareLT " + outputVal;
                    }
                }
                else if (InstructionHelper.IsEqualTo(instruction.OpCode.Code) || passThroughConditional)
                {
                    string cast;
                    var outputVal = GetConditional(instruction, out cast);
                    if (!string.IsNullOrEmpty(outputVal))
                    {
                        if (!string.IsNullOrEmpty(cast))
                            return cast + Environment.NewLine + "CompareEQ " + outputVal;
                        return "CompareEQ " + outputVal;
                    }
                }
            }

            if (InstructionHelper.IsBranchConditional(instruction.OpCode.Code))
            {
                var heapStack = evaluationStack;

                var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
                if (evaluationStack.Count >= popCount)
                {
                    var obj1 = evaluationStack.Pop();
                    var obj2 = evaluationStack.Pop();

                    // Make sure we have a temp variable if necessary
                    var temp = GetTargetVariable(instruction, null, "Bool");

                    var value1 = "";
                    var value2 = "";

                    if (obj1.Value is VariableReference)
                        value1 = (obj1.Value as VariableReference).Name;
                    else
                        value1 = obj1.Value.ToString();

                    if (obj2.Value is VariableReference) value2 = (obj2.Value as VariableReference).Name;
                    else
                        value2 = obj2.Value.ToString();

                    skipNextInstruction = false;
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


                    if (InstructionHelper.IsBranchConditionalEq(instruction.OpCode.Code))
                        output.Add("CompareEQ " + temp + " " + value1 + " " + value2);
                    else if (InstructionHelper.IsBranchConditionalLt(instruction.OpCode.Code))
                        output.Add("CompareLT " + temp + " " + value1 + " " + value2);
                    else if (InstructionHelper.IsBranchConditionalGt(instruction.OpCode.Code))
                        output.Add("CompareGT " + temp + " " + value1 + " " + value2);
                    else if (InstructionHelper.IsBranchConditionalGe(instruction.OpCode.Code))
                        output.Add("CompareGE " + temp + " " + value1 + " " + value2);
                    else if (InstructionHelper.IsBranchConditionalGe(instruction.OpCode.Code))
                        output.Add("CompareLE " + temp + " " + value1 + " " + value2);

                    if (!invertedBranch)
                        output.Add("JumpT " + temp + " " + targetVal);
                    else
                        output.Add("JumpF " + temp + " " + targetVal);

                    return string.Join(Environment.NewLine, output.ToArray());
                }
            }
            else if (Utility.IsBranch(instruction.OpCode.Code))
            {
                var heapStack = evaluationStack;
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
                    if (value.Value is VariableReference)
                    {
                        var variable = value.Value as VariableReference;
                        compareVal = variable.Name;
                    }
                    else compareVal = value.Value.ToString();


                    if (instruction.OpCode.Code == Code.Brtrue || instruction.OpCode.Code == Code.Brtrue_S)
                    {
                        if (invertedBranch)
                        {
                            invertedBranch = false;
                            return "JumpT " + compareVal + " " + targetVal;
                        }

                        return "JumpF " + compareVal + " " + targetVal;
                    }
                    if (instruction.OpCode.Code == Code.Brfalse || instruction.OpCode.Code == Code.Brfalse_S)
                    {
                        if (invertedBranch)
                        {
                            invertedBranch = false;
                            return "JumpF " + compareVal + " " + targetVal;
                        }
                        return "JumpT " + compareVal + " " + targetVal;
                    }
                }
                return "Jump " + targetVal;
            }


            return "";
        }

        public VariableReference GetFieldFromSTFLD(Instruction whereToPlace)
        {
            if (InstructionHelper.IsStoreField(whereToPlace.OpCode.Code))
            {
                if (whereToPlace.Operand is FieldReference)
                {
                    var fref = whereToPlace.Operand as FieldReference;
                    // if the evaluationStack.Count == 0
                    // The previous instruction might have been a call that returned a value
                    // Something we did not store...
                    var definedField =
                        function.Fields.FirstOrDefault(
                            f => f.Name == "::" + fref.Name.Replace('<', '_').Replace('>', '_'));
                    if (definedField != null)
                    {
                        return definedField;
                    }
                }
            }
            return null;
        }

        private string GetTargetVariable(Instruction instruction, MethodReference methodRef, string fallbackType = null,
            bool forceNew = false)
        {
            string targetVar = null;
            var whereToPlace = instruction.Next;

            if (whereToPlace != null &&
                (InstructionHelper.IsStoreLocalVariable(whereToPlace.OpCode.Code) ||
                 InstructionHelper.IsStoreField(whereToPlace.OpCode.Code)))
            {
                if (InstructionHelper.IsStoreField(whereToPlace.OpCode.Code))
                {
                    var fieldData = GetFieldFromSTFLD(whereToPlace);
                    if (fieldData != null)
                    {
                        targetVar = fieldData.Name;
                        LastSaughtTypeName = fieldData.TypeName;
                    }
                }
                else
                {
                    var index = IntValue(whereToPlace);
                    if (index < function.AllVariables.Count)
                    {
                        targetVar = function.AllVariables[(int)index].Name;
                        LastSaughtTypeName = function.AllVariables[(int)index].TypeName;
                    }
                }
                skipNextInstruction = true;

                // else 
                //evaluationStack.Push(new EvaluationStackItem { IsMethodCall = true, Value = methodRef, TypeName = methodRef.ReturnType.FullName });
            }
            else if (whereToPlace != null &&
                     (InstructionHelper.IsLoad(whereToPlace.OpCode.Code) ||
                      InstructionHelper.IsCallMethod(whereToPlace.OpCode.Code) ||
                      InstructionHelper.IsBranchConditional(instruction.OpCode.Code)
                      || Utility.IsLoadLength(instruction.OpCode.Code)))
            {
                // Most likely this function call have a return value other than Void
                // and is used for an additional method call, witout being assigned to a variable first.

                // evaluationStack.Push(new EvaluationStackItem { IsMethodCall = true, Value = methodRef, TypeName = methodRef.ReturnType.FullName });

                var tVar =
                    function.CreateTempVariable(
                        !string.IsNullOrEmpty(fallbackType) ? fallbackType : methodRef.ReturnType.FullName,
                        methodRef);
                targetVar = tVar.Name;
                evaluationStack.Push(new EvaluationStackItem { Value = tVar, TypeName = tVar.TypeName });
                LastSaughtTypeName = tVar.TypeName;
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
            var heapStack = evaluationStack;


            if (heapStack.Count >= 2) //Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
            {
                var obj1 = heapStack.Pop();
                var obj2 = heapStack.Pop();
                var vars = function.AllVariables;
                var varIndex = 0;

                var value1 = "";
                var value2 = "";

                if (obj1.Value is VariableReference)
                {
                    var oo = obj1.Value as VariableReference;
                    value1 = oo.Name;
                    if (!oo.TypeName.ToLower().Equals("int") && !oo.TypeName.ToLower().Equals("system.int32")
                        && !oo.TypeName.ToLower().Equals("system.string") && !oo.TypeName.ToLower().Equals("string"))
                    {
                        // CAST BOOL TO INT
                        var typeVariable = GetTargetVariable(instruction, null, "Int");
                        cast = "Cast " + typeVariable + " " + value1;
                    }
                }
                else
                    value1 = obj1.Value.ToString();

                if (obj2.Value is VariableReference)
                {
                    var oo = obj2.Value as VariableReference;
                    value2 = oo.Name;
                    if (!oo.TypeName.ToLower().Equals("int") && !oo.TypeName.ToLower().Equals("system.int32")
                        && !oo.TypeName.ToLower().Equals("system.string") && !oo.TypeName.ToLower().Equals("string"))
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
                    while (next != null && !InstructionHelper.IsStoreLocalVariable(next.OpCode.Code) &&
                           !InstructionHelper.IsStoreField(next.OpCode.Code) &&
                           !InstructionHelper.IsCallMethod(next.OpCode.Code))
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
                        evaluationStack.Push(new EvaluationStackItem { Value = tVar, TypeName = tVar.TypeName });
                        return targetVar + " " + value2 + " " + value1;
                    }

                    if (next == null)
                    {
                        // No intentions to store this value into a variable, 
                        // Its to be used in a function call.
                        return "NULLPTR " + value2 + " " + value1;
                    }

                    skipToOffset = next.Offset;
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
            while (next != null && !InstructionHelper.IsStore(next.OpCode.Code)
                /* && !Utility.IsStoreElement(next.OpCode.Code) && !Utility.IsStoreLocalVariable(next.OpCode.Code) && !Utility.IsStoreField(next.OpCode.Code)*/)
            {
                next = next.Next;
            }

            if (next != null)
            {
                varIndex = (int)IntValue(next);
                skipToOffset = next.Offset;
            }
            return next;
        }


        public bool HasMethod(MethodReference methodRef)
        {
            if (type.Methods.Any(m => m.FullName == methodRef.FullName)) return true;
            if (type.BaseType != null)
            {
                try
                {
                    var typeDef = type.BaseType.Resolve();
                    if (typeDef != null)
                    {
                        if (typeDef.Methods.Any(m => m.FullName == methodRef.FullName)) return true;
                    }
                }
                catch
                {
                }
                //.m
            }
            return false;
        }

        public string FormatParameters(MethodReference methodRef, List<EvaluationStackItem> parameters)
        {
            var outp = new List<string>();
            if (parameters != null && parameters.Count > 0)
            {
                var index = 0;
                foreach (var it in parameters)
                {
                    var item = it;
                    while (item != null && item.Value is EvaluationStackItem)
                    {
                        item = it.Value as EvaluationStackItem;
                    }

                    if (item.Value is VariableReference)
                    {
                        outp.Add((item.Value as VariableReference).Name);
                        continue;
                    }
                    if (item.TypeName.ToLower().Equals("int"))
                    {
                        if (methodRef.Parameters[index].ParameterType == assembly.MainModule.TypeSystem.Boolean)
                        {
                            var val = int.Parse(item.Value.ToString()) == 1;
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
            return string.Join(" ", outp.ToArray());
        }

        public static bool IsVoid(TypeReference typeReference)
        {
            return typeReference.FullName.ToLower().Equals("system.void")
                || typeReference.Name.ToLower().Equals("void");
        }


        public double IntValue(Instruction instruction)
        {
            double index = InstructionHelper.GetCodeIndex(instruction.OpCode.Code);
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
                if (instruction.Operand is Mono.Cecil.Cil.VariableReference)
                {
                    var v = instruction.Operand as Mono.Cecil.Cil.VariableReference;
                    if (v != null)
                    {
                        return Array.IndexOf(function.AllVariables.ToArray(),
                            function.AllVariables.FirstOrDefault(va => va.Name == "V_" + v.Index));
                    }
                }
                else if (instruction.Operand is double || instruction.Operand is long || instruction.Operand is float ||
                         instruction.Operand is decimal)
                    index = double.Parse(instruction.Operand.ToString());
                else
                    index = int.Parse(instruction.Operand.ToString());
            }
            return index;
        }

        public static void GenerateCallStack(AssemblyDefinition asm)
        {
            foreach (var module in asm.Modules)
            {
                if (module.HasTypes)
                {
                    var types = module.Types;

                    foreach (var type in types)
                    {
                        GetCallStackRecursive(type);
                    }
                }
            }
        }
    }
}