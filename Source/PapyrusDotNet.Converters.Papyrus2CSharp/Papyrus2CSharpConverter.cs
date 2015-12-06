using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.Converters.Papyrus2Clr.Implementations;
using PapyrusDotNet.Converters.Papyrus2CSharp.FlowAnalyzer;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Classes;
using PapyrusDotNet.PapyrusAssembly.Enums;
using PapyrusDotNet.PapyrusAssembly.Extensions;

namespace PapyrusDotNet.Converters.Papyrus2CSharp
{
    public class Papyrus2CSharpConverter : Papyrus2CSharpConverterBase
    {
        private StringBuilder activeBuilder;
        private int activeIndent;

        public Papyrus2CSharpConverter(INamespaceResolver namespaceResolver, ITypeReferenceResolver typeReferenceResolver)
            : base(namespaceResolver, typeReferenceResolver)
        {
        }

        protected override MultiCSharpOutput ConvertAssembly(PapyrusAssemblyInput input)
        {
            return new MultiCSharpOutput(input.Assemblies.Select(CreateCSharpDump).ToList());
        }

        private CSharpOutput CreateCSharpDump(PapyrusAssemblyDefinition asm)
        {
            var type = asm.Types.First();

            var outputFileName = type.Name.Value + ".cs";

            var outputFileContent = WriteType(asm, type);

            return new CSharpOutput(outputFileName, outputFileContent);
        }

        private string Indent(string text, int num)
        {
            string output = "";
            return For(num, i => { return output += "\t"; }) + text;
        }

        private string For(int num, Func<int, string> func)
        {
            string output = "";
            for (var i = 0; i < num; i++)
            {
                output += func(i);
            }
            return output;
        }

        private string WriteType(PapyrusAssemblyDefinition asm, PapyrusTypeDefinition type)
        {
            var source = new StringBuilder();


            SetBuilder(source, 1);

            if (type.Documentation != null && !string.IsNullOrEmpty(type.Documentation.Value))
            {
                WriteDoc(type.Documentation);
            }

            source.Append("public class " + (string)type.Name);

            if (!RefIsNull(type.BaseTypeName))
                source.Append(" : " + (string)type.BaseTypeName + Environment.NewLine);
            source.AppendLine("{");


            foreach (var field in type.Fields)
            {
                if (field.Documentation != null && !string.IsNullOrEmpty(field.Documentation))
                {
                    WriteDoc(field.Documentation);
                }
                source.AppendLine(Indent("public " + (string)field.TypeName + " " + (string)field.Name + ";", 1));
            }

            foreach (var prop in type.Properties)
            {
                if (prop.Documentation != null && !string.IsNullOrEmpty(prop.Documentation.Value))
                {
                    WriteDoc(prop.Documentation);
                }
                if (!string.IsNullOrEmpty(prop.AutoName))
                {
                    source.AppendLine(Indent("public " + (string)prop.TypeName + " " + (string)prop.Name + " { get; set; }", 1));
                }
                else
                {
                    source.Append(Indent("public " + (string)prop.TypeName + " " + (string)prop.Name, 1));
                    if (!prop.HasSetter && !prop.HasGetter)
                    {
                        source.AppendLine(Indent("{ get; set; }", 1));
                    }
                    else
                    {
                        source.AppendLine();
                        source.AppendLine(Indent("{", 1));
                        if (prop.HasGetter)
                        {
                            source.AppendLine(Indent("get", 2));
                            WriteMethod(source, 3, prop.GetMethod, type, asm, true);
                        }
                        if (prop.HasSetter)
                        {
                            source.AppendLine(Indent("set", 2));
                            WriteMethod(source, 3, prop.SetMethod, type, asm, false, true);
                        }
                        source.AppendLine(Indent("}", 1));
                    }
                }
            }

            foreach (var s in type.States)
            {
                foreach (var m in s.Methods)
                {
                    WriteMethod(source, 1, m, type, asm);
                }
            }

            source.AppendLine("}");

            return source.ToString();
        }

        private void WriteDoc(string doc)
        {
            var lines = doc.Split('\n');
            AppendLine("/// <summary>");
            lines.ForEach(i => AppendLine("/// " + i.Replace("\t", "")));
            AppendLine("/// </summary>");
        }
        private void WriteDoc(PapyrusStringRef doc)
        {
            WriteDoc(doc.Value);
        }

        /// <summary>
        /// Writes the method.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="indent">The indent.</param>
        /// <param name="method">The method.</param>
        /// <param name="type">The type.</param>
        /// <param name="asm">The asm.</param>
        /// <param name="isGetter">if set to <c>true</c> [is getter].</param>
        /// <param name="isSetter">if set to <c>true</c> [is setter].</param>
        private void WriteMethod(StringBuilder source, int indent, PapyrusMethodDefinition method, PapyrusTypeDefinition type, PapyrusAssemblyDefinition asm, bool isGetter = false, bool isSetter = false)
        {
            SetBuilder(source, indent);
            if (!isGetter && !isSetter)
            {
                if (method.Documentation != null && !string.IsNullOrEmpty(method.Documentation.Value))
                {
                    WriteDoc(method.Documentation);
                }
                source.Append(Indent("public " +
                              (method.IsGlobal ? "static " : "") +
                              (method.IsNative ? "extern " : "") +
                              ((string)method.ReturnTypeName).Replace("None", "void") + " " +
                              (string)method.Name
                    , indent));
                source.Append("(");
                source.Append(string.Join(",",
                    method.Parameters.Select(i => (string)i.TypeName + " " + (string)i.Name)));
                source.AppendLine(")");
            }
            source.AppendLine(Indent("{", indent));
            if (isGetter)
            {
                // type.Fields.FirstOrDefault(f=>f.Name.Value.Contains());
                source.AppendLine(Indent("return <backing_field>;", indent + 1));
            }
            else if (isSetter)
            {
                source.AppendLine(Indent("<backing_field> = value;", indent + 1));
            }
            else
            {
                if (method.HasBody)
                {
                    var debug = asm.DebugInfo.MethodDescriptions.FirstOrDefault(m => m.Name.Value == method.Name.Value);
                    if (debug != null)
                    {
                        AppendLine("// DEBUG LINE NUMBER: " + string.Join(",", debug.BodyLineNumbers.ToArray()), 1);
                    }
                    foreach (var var in method.GetVariables())
                    {
                        if (var.Name.Value == "::nonevar") continue;
                        AppendLine((string)var.TypeName + " " + (string)var.Name + ";", 1);
                    }



                    var flowGraphBuilder = new PapyrusControlFlowGraphBuilder(method);

                    var graph = flowGraphBuilder.Build();

                    graph.ComputeDominance();
                    graph.ComputeDominanceFrontier();
                    var nodes = graph.Nodes.Skip(2).ToList();
                    var count = nodes.Count;
                    foreach (var n in nodes)
                    {
                        if (count > 1)
                            AppendLine("// --- Node Start ---", 1);
                        foreach (var instruction in n.Instructions)
                        {
                            Append("/* " + instruction.Offset + " */ ", 1);
                            WriteInstruction(source, indent, instruction, method, type, asm);
                        }
                        if (count > 1)
                            AppendLine("// --- Node End ---", 1);
                    }

                    //foreach (var instruction in method.Body.Instructions)
                    //{
                    //    Append("/* " + instruction.Offset + " */ ", 1);
                    //    WriteInstruction(source, indent, instruction, method, type, asm);
                    //}

                }
            }
            source.AppendLine(Indent("}", indent));
        }

        private bool IsJumpf(PapyrusOpCode opCode)
        {
            return opCode == PapyrusOpCode.Jmpf;
        }
        private bool IsJumpt(PapyrusOpCode opCode)
        {
            return opCode == PapyrusOpCode.Jmpt;
        }
        private bool IsJump(PapyrusOpCode opCode)
        {
            return opCode == PapyrusOpCode.Jmp || opCode == PapyrusOpCode.Jmpf || opCode == PapyrusOpCode.Jmpt;
        }

        private void WriteInstruction(StringBuilder source, int indent, PapyrusInstruction instruction, PapyrusMethodDefinition method, PapyrusTypeDefinition type, PapyrusAssemblyDefinition asm)
        {
            var i = instruction;
            SetBuilder(source, indent);

            switch (i.OpCode)
            {
                case PapyrusOpCode.Nop:
                    {
                        AppendLine("// Do Nothing Operator (NOP)");
                    }
                    break;
                case PapyrusOpCode.Callstatic:
                    {
                        var val = string.Join(",", i.Arguments.Skip(2).Take(i.Arguments.Count - 3).Select(GetArgumentValue));
                        var location = GetArgumentValue(i.Arguments[0]);
                        var functionName = GetArgumentValue(i.Arguments[1]);
                        var assignee = GetArgumentValue(i.Arguments.LastOrDefault());

                        var args = string.Join(",",
                            new[] { val, string.Join(",", i.OperandArguments.Select(GetArgumentValue)) }).Trim(',');

                        if (assignee != null)
                        {
                            assignee += " = ";
                            assignee = assignee.Replace("::none = ", "").Replace("::nonevar = ", "");
                        }
                        AppendLine(assignee + (location + ".").Replace("self.", "") + functionName + "(" + args + ");");

                    }
                    break;
                case PapyrusOpCode.Callmethod:
                    {
                        var val = string.Join(",", i.Arguments.Skip(2).Take(i.Arguments.Count - 3).Select(GetArgumentValue));
                        var location = GetArgumentValue(i.Arguments[1]);
                        var functionName = GetArgumentValue(i.Arguments[0]);
                        var assignee = GetArgumentValue(i.Arguments.LastOrDefault());

                        var args = string.Join(",",
                            new[] { val, string.Join(",", i.OperandArguments.Select(GetArgumentValue)) }).Trim(',');

                        if (assignee != null)
                        {
                            assignee += " = ";
                            assignee = assignee.Replace("::none = ", "").Replace("::nonevar = ", "");
                        }

                        AppendLine(assignee + (location + ".").Replace("self.", "") + functionName + "(" + args + ");");
                    }
                    break;
                case PapyrusOpCode.Return:
                    {
                        var val = "";
                        var firstArg = GetArgumentValue(i.Arguments.FirstOrDefault());
                        var firstVarArg = GetArgumentValue(i.OperandArguments.FirstOrDefault());
                        if (firstArg != null)
                            val = " " + firstArg;
                        else if (firstVarArg != null)
                            val = " " + firstVarArg;
                        else
                            val = "";

                        AppendLine("return" + val + ";");
                    }
                    break;
                case PapyrusOpCode.Assign:
                    {
                        var val1 = GetArgumentValue(i.Arguments.FirstOrDefault());
                        var val2 = GetArgumentValue(i.Arguments.LastOrDefault());
                        var val3 = GetArgumentValue(i.OperandArguments.FirstOrDefault());
                        var val4 = GetArgumentValue(i.OperandArguments.LastOrDefault());

                        var var_0 = val1 ?? val3;
                        var var_1 = val2 ?? val4;
                        if (var_1 == null || var_1 == "")
                        {
                            AppendLine(var_0 + " = null;");
                        }
                        else
                            AppendLine(var_0 + " = " + var_1 + ";");

                    }
                    break;
                case PapyrusOpCode.StructGet:
                    {
                        var asignee = GetArgumentValue(i.Arguments[0]);
                        var structVar = GetArgumentValue(i.Arguments[1]);
                        var structField = GetArgumentValue(i.Arguments[2]);
                        AppendLine(asignee + " = " + structVar + "." + structField + ";");
                    }
                    break;
                case PapyrusOpCode.ArrayGetelement:
                    {
                        var asignee = GetArgumentValue(i.Arguments[0]);
                        var array = GetArgumentValue(i.Arguments[1]);
                        var index = GetArgumentValue(i.Arguments[2]);
                        AppendLine(asignee + " = " + array + "[" + index + "];");
                    }
                    break;
                case PapyrusOpCode.ArrayLength:
                    {
                        var asignee = GetArgumentValue(i.Arguments[0]);
                        var array = GetArgumentValue(i.Arguments[1]);
                        AppendLine(asignee + " = " + array + ".Length;");
                    }
                    break;
                case PapyrusOpCode.ArrayCreate:
                    {
                        var asignee = GetArgumentValue(i.Arguments[0]);
                        var size = GetArgumentValue(i.Arguments[1]);
                        var possibleVar =
                            method.GetVariables().FirstOrDefault(v => v.Name.Value == asignee);
                        var field = type.Fields.FirstOrDefault(v => v.Name.Value == asignee);
                        var prop = type.Properties.FirstOrDefault(v => v.Name.Value == asignee);
                        if (possibleVar != null)
                            AppendLine(asignee + " = new " + possibleVar.TypeName.Value.Replace("[]", "") + "[" + size + "];");
                        else if (field != null)
                            AppendLine(asignee + " = new " + field.TypeName.Replace("[]", "") + "[" + size + "];");
                        else if (prop != null)
                            AppendLine(asignee + " = new " + prop.TypeName.Value.Replace("[]", "") + "[" + size + "];");
                    }
                    break;
                case PapyrusOpCode.Strcat:
                    {
                        var asignee = GetArgumentValue(i.Arguments[0]);
                        var str = GetArgumentValue(i.Arguments[1]);
                        var value = GetArgumentValue(i.Arguments[2]);
                        AppendLine(asignee + " = " + str + " + " + value + ";");
                    }
                    break;
                case PapyrusOpCode.Fsub:
                case PapyrusOpCode.Isub:
                    {
                        DoMath(i, "-");
                    }
                    break;
                case PapyrusOpCode.Fadd:
                case PapyrusOpCode.Iadd:
                    {
                        DoMath(i, "+");
                    }
                    break;
                case PapyrusOpCode.Ineg:
                    {
                        var asignee = GetArgumentValue(i.Arguments[0]);
                        var target = GetArgumentValue(i.Arguments[1]);
                        AppendLine(asignee + " = " + target + " < 0;");
                    }
                    break;
                case PapyrusOpCode.CmpEq:
                    {
                        DoMath(i, "==");
                    }
                    break;
                case PapyrusOpCode.CmpGt:
                    {
                        DoMath(i, ">");
                    }
                    break;

                case PapyrusOpCode.CmpGte:
                    {
                        DoMath(i, ">=");
                    }
                    break;
                case PapyrusOpCode.CmpLt:
                    {
                        DoMath(i, "<");
                    }
                    break;
                case PapyrusOpCode.CmpLte:
                    {
                        DoMath(i, "<=");
                    }
                    break;
                case PapyrusOpCode.Not:
                    {
                        var assignee = i.GetArg(0);
                        var target = i.GetArg(1);
                        AppendLine(assignee + " = !" + target + ";");
                    }
                    break;
                case PapyrusOpCode.Jmp:
                    {
                        var destination = GetArgumentValue(i.Arguments[0]);
                        AppendLine("// " + i.OpCode + " " + destination + " (offset: " +
                                   (i.Offset + int.Parse(destination)) + ")");
                    }
                    break;
                case PapyrusOpCode.Jmpt:
                case PapyrusOpCode.Jmpf:
                    {
                        var boolVal = GetArgumentValue(i.Arguments[0]);
                        var destination = GetArgumentValue(i.Arguments[1]);
                        AppendLine("// " + i.OpCode + " " + boolVal + " " + destination + " (offset: " +
                                   (i.Offset + int.Parse(destination)) + ")");
                    }
                    break;
                default:
                    {
                        var instructionParams = string.Join(" ", i.Arguments.Select(GetArgumentValue));
                        var instructionObjectParams = string.Join(" ", i.OperandArguments.Select(GetArgumentValue));

                        AppendLine("// " + i.OpCode + " " + instructionParams + " " + instructionObjectParams);
                    }
                    break;
            }
        }

        private void DoMath(PapyrusInstruction i, string op)
        {
            var asignee = GetArgumentValue(i.Arguments[0]);
            var target = GetArgumentValue(i.Arguments[1]);
            var value = GetArgumentValue(i.Arguments[2]);
            AppendLine(asignee + " = " + target + " " + op + " " + value + ";");
        }

        private string GetArgumentValue(PapyrusVariableReference arg)
        {
            if (arg == null) return null;
            switch (arg.ValueType)
            {
                case PapyrusPrimitiveType.Reference:
                    return arg.Value?.ToString();
                case PapyrusPrimitiveType.String:
                    return "\"" + arg.Value + "\"";
                case PapyrusPrimitiveType.Boolean:
                    {
                        if (arg.Value != null)
                        {
                            return arg.Value.Equals(1) ? "true" : "false";
                        }
                    }
                    break;
                case PapyrusPrimitiveType.Integer:
                    if (arg.Value != null)
                    {
                        return ((int)arg.Value).ToString();
                    }
                    break;
                case PapyrusPrimitiveType.Float:
                    if (arg.Value != null)
                    {
                        return ((float)arg.Value).ToString().Replace(",", ".") + "f";
                    }
                    break;
            }

            if (arg.Name != null)
            {
                return (string)arg.Name;
            }
            return null;
        }

        private void SetBuilder(StringBuilder builder, int startingIndent = 0)
        {
            activeIndent = startingIndent;
            activeBuilder = builder;
        }

        bool lastWasAppend = false;
        private void Append(string value, int indent = 0)
        {
            lastWasAppend = true;
            activeBuilder.Append(Indent(value, activeIndent + indent));
        }

        private void AppendLine(string value, int indent = 0)
        {
            if (lastWasAppend)
            {
                activeBuilder.AppendLine(value);
            }
            else
            {
                activeBuilder.AppendLine(Indent(value, activeIndent + indent));
            }
            lastWasAppend = false;
        }

        private bool RefIsNull(PapyrusStringRef baseTypeName)
        {
            return baseTypeName == null || string.IsNullOrEmpty(baseTypeName.Value);
        }
    }

}
