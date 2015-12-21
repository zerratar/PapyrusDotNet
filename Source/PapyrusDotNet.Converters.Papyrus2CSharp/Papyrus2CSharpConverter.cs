using System;
using System.Linq;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.Converters.Papyrus2Clr.Implementations;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Extensions;

namespace PapyrusDotNet.Converters.Papyrus2CSharp
{
    public class Papyrus2CSharpConverter : Papyrus2CSharpConverterBase
    {
        private SourceBuilder activeBuilder;

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

            var sourceBuilder = new SourceBuilder();
            var outputFileContent = WriteType(sourceBuilder, asm, type).Replace(" ::", " ").Replace("(::", "(").Replace(",::", ",").Replace("!::", "!");

            return new CSharpOutput(outputFileName, outputFileContent);
        }


        private string WriteType(SourceBuilder source, PapyrusAssemblyDefinition asm, PapyrusTypeDefinition type, int indent = 0)
        {
            activeBuilder = source;

            if (type.Documentation != null && !string.IsNullOrEmpty(type.Documentation.Value))
            {
                WriteDoc(type.Documentation, indent);
            }

            if (type.IsStruct)
            {
                Append("public struct " + (string)type.Name + " ", indent);
            }
            else
            {
                Append("public class " + (string)type.Name + " ", indent);
            }

            if (!RefIsNull(type.BaseTypeName))
            {
                Append(": " + (string)type.BaseTypeName + Environment.NewLine);
            }

            AppendLine("{", indent);


            foreach (var t in type.NestedTypes)
            {
                WriteType(source, asm, t, indent + 1);
            }

            foreach (var field in type.Fields)
            {
                if (field.Documentation != null && !string.IsNullOrEmpty(field.Documentation))
                {
                    WriteDoc(field.Documentation, indent + 1);
                }
                AppendLine("public " + field.TypeName + " " + ((string)field.Name) + ";", indent + 1);
            }

            foreach (var prop in type.Properties)
            {
                if (prop.Documentation != null && !string.IsNullOrEmpty(prop.Documentation.Value))
                {
                    WriteDoc(prop.Documentation, indent + 1);
                }
                if (!string.IsNullOrEmpty(prop.AutoName))
                {
                    AppendLine("public " + (string)prop.TypeName + " " + (string)prop.Name + " { get; set; }", indent + 1);
                }
                else
                {
                    Append("public " + (string)prop.TypeName + " " + (string)prop.Name, indent + 1);
                    if (!prop.HasSetter && !prop.HasGetter)
                    {
                        AppendLine("{ get; set; }", indent + 1);
                    }
                    else
                    {
                        AppendLine("");
                        AppendLine("{", 1);
                        if (prop.HasGetter)
                        {
                            Append("get ", 2);
                            WriteMethod(prop.GetMethod, type, asm, indent + 3, prop, true);
                        }
                        if (prop.HasSetter)
                        {
                            Append("set ", 2);
                            WriteMethod(prop.SetMethod, type, asm, indent + 3, prop, false, true);
                        }
                        AppendLine("}", indent + 1);
                    }
                }
            }

            foreach (var s in type.States)
            {
                foreach (var m in s.Methods)
                {
                    WriteMethod(m, type, asm, indent + 1);

                }
            }




            AppendLine("}", indent);

            return source.ToString();
        }

        private void WriteDoc(string doc, int indent)
        {
            var lines = doc.Split('\n');
            AppendLine("/// <summary>", indent);
            lines.ForEach(i => AppendLine("/// " + i.Replace("\t", ""), indent));
            AppendLine("/// </summary>", indent);
        }
        private void WriteDoc(PapyrusStringRef doc, int indent)
        {
            WriteDoc(doc.Value, indent);
        }

        /// <summary>
        /// Writes the method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="type">The type.</param>
        /// <param name="asm">The asm.</param>
        /// <param name="indent">The indent.</param>
        /// <param name="isGetter">if set to <c>true</c> [is getter].</param>
        /// <param name="isSetter">if set to <c>true</c> [is setter].</param>
        private void WriteMethod(PapyrusMethodDefinition method, PapyrusTypeDefinition type, PapyrusAssemblyDefinition asm, int indent, PapyrusPropertyDefinition prop = null, bool isGetter = false, bool isSetter = false)
        {
            // SetGlobalIndent(indent);
            if (!isGetter && !isSetter)
            {
                if (method.Documentation != null && !string.IsNullOrEmpty(method.Documentation.Value))
                {
                    WriteDoc(method.Documentation, indent);
                }
                Append("public " +
                       (method.IsGlobal ? "static " : "") +
                       (method.IsNative ? "extern " : "") +
                       ((string) method.ReturnTypeName).Replace("None", "void") + " " +
                       (string) method.Name
                    , indent);
                Append("(");
                Append(string.Join(",",
                    method.Parameters.Select(i => (string) i.TypeName + " " + (string) i.Name)));
                AppendLine(")");
                
            }
            AppendLine("{", indent);
            //if (isGetter)
            //{
            //    if (prop != null)
            //    {
            //        prop.
            //    }
            //    // type.Fields.FirstOrDefault(f=>f.Name.Value.Contains());
            //    AppendLine("return <backing_field>;", indent + 1);
            //}
            //else if (isSetter)
            //{
            //    AppendLine("<backing_field> = value;", indent + 1);
            //}
            //else
            {
                if (method.HasBody)
                {
                    var debug = asm.DebugInfo.MethodDescriptions.FirstOrDefault(m => m.Name.Value == method.Name.Value);
                    if (debug != null)
                    {
                        AppendLine("// DEBUG LINE NUMBER: " + string.Join(",", debug.BodyLineNumbers.ToArray()), indent + 1);
                    }
                    foreach (var var in method.GetVariables())
                    {
                        if (var.Name.Value.ToLower() == "::nonevar") continue;
                        AppendLine((string)var.TypeName + " " + ((string)var.Name) + ";", indent + 1);
                    }



                    //var flowGraphBuilder = new PapyrusControlFlowGraphBuilder(method);
                    //var graph = flowGraphBuilder.Build();
                    //graph.ComputeDominance();
                    //graph.ComputeDominanceFrontier();
                    //var nodes = graph.Nodes.Skip(2).ToList();
                    //var count = nodes.Count;

                    foreach (var instruction in method.Body.Instructions)
                    {
                        var instructionString = WriteInstruction(instruction, method, type);
                        foreach (var row in instructionString.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                        {
                            AppendLine("/* " + instruction.Offset + " */ " + row, indent + 1);
                        }
                    }

                    //List<PapyrusInstruction> targetEnd = new List<PapyrusInstruction>();
                    //foreach (var n in nodes)
                    //{

                    //    var end = n.End;

                    //    //if (count > 1)
                    //    //    AppendLine("/* ====== Node Start ====== */", 1);

                    //    foreach (var instruction in n.Instructions)
                    //    {
                    //        Append("/* " + instruction.Offset + " */ ", 1);

                    //        WriteInstruction(indent, instruction, method, type, asm);
                    //    }

                    //    if (end != null && IsJump(end.OpCode))
                    //    {

                    //        var direction = int.Parse(end.GetArg(IsJumpf(end.OpCode) || IsJumpt(end.OpCode) ? 1 : 0));
                    //        if (direction < 0)
                    //        {
                    //            // LOOP?
                    //        }
                    //        else
                    //        {
                    //            // BRANCHING
                    //        }
                    //    }
                    //}
                }
            }
            AppendLine("}", indent);
        }

        //private void WriteInstructionConditional(SourceBuilder source, int indent, PapyrusInstruction instruction, PapyrusMethodDefinition method, PapyrusTypeDefinition type, PapyrusAssemblyDefinition asm)
        //{
        //    SetBuilder(source, indent);
        //    var i = instruction;
        //    var boolean = instruction.GetArg(0);
        //    AppendLine("if (" + boolean + ")");
        //}

        //private bool IsJumpf(PapyrusOpCode opCode)
        //{
        //    return opCode == PapyrusOpCode.Jmpf;
        //}
        //private bool IsJumpt(PapyrusOpCode opCode)
        //{
        //    return opCode == PapyrusOpCode.Jmpt;
        //}
        //private bool IsJump(PapyrusOpCode opCode)
        //{
        //    return opCode == PapyrusOpCode.Jmp || opCode == PapyrusOpCode.Jmpf || opCode == PapyrusOpCode.Jmpt;
        //}

        private string WriteInstruction(PapyrusInstruction instruction, PapyrusMethodDefinition method, PapyrusTypeDefinition type)
        {
            var i = instruction;

            switch (i.OpCode)
            {
                case PapyrusOpCodes.Nop:
                    {
                        return ("// Do Nothing Operator (NOP)");
                    }
                case PapyrusOpCodes.Callstatic:
                    {
                        var val = string.Join(",", i.Arguments.Skip(2).Take(i.Arguments.Count - 3).Select(GetArgumentValue));
                        var location = GetArgumentValue(i.Arguments[0]);
                        var functionName = GetArgumentValue(i.Arguments[1]);
                        var assignee = GetArgumentValue(i.Arguments.LastOrDefault());

                        var args = string.Join(",", val, string.Join(",", i.OperandArguments.Select(GetArgumentValue))).Trim(',');

                        if (assignee != null)
                        {
                            assignee += " = ";
                            if (assignee.ToLower().Contains("::nonevar"))
                            {
                                assignee = string.Empty;
                            }
                        }
                        return assignee + (location + ".").Replace("self.", "") + functionName + "(" + args + ");";

                    }
                case PapyrusOpCodes.Callmethod:
                    {
                        var val = string.Join(",", i.Arguments.Skip(2).Take(i.Arguments.Count - 3).Select(GetArgumentValue));
                        var location = GetArgumentValue(i.Arguments[1]);
                        var functionName = GetArgumentValue(i.Arguments[0]);
                        var assignee = GetArgumentValue(i.Arguments.LastOrDefault());

                        var args = string.Join(",", val, string.Join(",", i.OperandArguments.Select(GetArgumentValue))).Trim(',');

                        if (assignee != null)
                        {
                            assignee += " = ";
                            if (assignee.ToLower().Contains("::nonevar"))
                            {
                                assignee = string.Empty;
                            }
                        }

                        return assignee + (location + ".").Replace("self.", "") + functionName + "(" + args + ");";
                    }
                case PapyrusOpCodes.Return:
                    {
                        string val;
                        var firstArg = GetArgumentValue(i.Arguments.FirstOrDefault());
                        var firstVarArg = GetArgumentValue(i.OperandArguments.FirstOrDefault());
                        if (firstArg != null)
                            val = " " + firstArg;
                        else if (firstVarArg != null)
                            val = " " + firstVarArg;
                        else
                            val = "";

                        return (("return" + val).Trim() + ";");
                        //WritePapyrusInstruction(i);
                    }
                case PapyrusOpCodes.Assign:
                    {
                        var val1 = GetArgumentValue(i.Arguments.FirstOrDefault());
                        var val2 = GetArgumentValue(i.Arguments.LastOrDefault());
                        var val3 = GetArgumentValue(i.OperandArguments.FirstOrDefault());
                        var val4 = GetArgumentValue(i.OperandArguments.LastOrDefault());

                        var var0 = val1 ?? val3;
                        var var1 = val2 ?? val4;
                        if (string.IsNullOrEmpty(var1))
                        {
                            return (var0 + " = null;");
                        }
                        return (var0 + " = " + var1 + ";");
                    }
                case PapyrusOpCodes.ArrayRemoveElements:
                    {
                        var comment = WritePapyrusInstruction(i) + Environment.NewLine;

                        var array = GetArgumentValue(i.Arguments[0]);
                        var index = GetArgumentValue(i.Arguments[1]);
                        var count = GetArgumentValue(i.Arguments[2]);

                        return comment + "Array.RemoveItems(" + array + ", " + index + ", " + count + ");";
                    }
                case PapyrusOpCodes.ArrayFindElement:
                    {
                        var comment = WritePapyrusInstruction(i) + Environment.NewLine;

                        var array = GetArgumentValue(i.Arguments[0]);
                        var destination = GetArgumentValue(i.Arguments[1]);
                        var elementToFind = GetArgumentValue(i.Arguments[2]);
                        var startIndex = GetArgumentValue(i.Arguments[3]);

                        return comment + destination + " = Array.IndexOf(" + array + ", " + elementToFind + ", " + startIndex + ");";
                    }
                case PapyrusOpCodes.ArrayFindStruct:
                    {
                        var comment = WritePapyrusInstruction(i) + Environment.NewLine;

                        var array = GetArgumentValue(i.Arguments[0]);
                        var destination = GetArgumentValue(i.Arguments[1]);
                        var fieldToMatch = GetArgumentValue(i.Arguments[2]);
                        var value = GetArgumentValue(i.Arguments[3]);
                        var startIndex = GetArgumentValue(i.Arguments[4]);

                        return comment + destination + " = Structs.IndexOf(" + array + ", " + fieldToMatch + ", " + value + ", " + startIndex + ");";
                    }
                case PapyrusOpCodes.PropGet:
                    {
                        var comment = WritePapyrusInstruction(i) + Environment.NewLine;

                        var property = GetArgumentValue(i.Arguments[0]);
                        var owner = GetArgumentValue(i.Arguments[1]);
                        var targetVar = GetArgumentValue(i.Arguments[2]);

                        return comment + targetVar + " = " + owner + "." + property + ";";
                    }
                case PapyrusOpCodes.PropSet:
                    {
                        var comment = WritePapyrusInstruction(i) + Environment.NewLine;

                        var property = GetArgumentValue(i.Arguments[0]);
                        var owner = GetArgumentValue(i.Arguments[1]);
                        var value = GetArgumentValue(i.Arguments[2]);


                        return comment + owner + "." + property + " = " + value + ";";
                    }
                case PapyrusOpCodes.StructSet:
                    {
                        var structVar = GetArgumentValue(i.Arguments[0]);
                        var structField = GetArgumentValue(i.Arguments[1]);
                        var value = GetArgumentValue(i.Arguments[2]);
                        return structVar + "." + structField + " = " + value + ";";
                        // (value + "." + structField + " = " + structVar + "." + structField + ";");
                    }
                case PapyrusOpCodes.StructGet:
                    {
                        var asignee = GetArgumentValue(i.Arguments[0]);
                        var structVar = GetArgumentValue(i.Arguments[1]);
                        var structField = GetArgumentValue(i.Arguments[2]);
                        return (asignee + " = " + structVar + "." + structField + ";");
                    }
                case PapyrusOpCodes.ArraySetElement:
                    {
                        var array = GetArgumentValue(i.Arguments[0]);
                        var index = GetArgumentValue(i.Arguments[1]);
                        var value = GetArgumentValue(i.Arguments[2]);
                        return (array + "[" + index + "] = " + value + ";");
                    }
                case PapyrusOpCodes.ArrayGetElement:
                    {
                        var asignee = GetArgumentValue(i.Arguments[0]);
                        var array = GetArgumentValue(i.Arguments[1]);
                        var index = GetArgumentValue(i.Arguments[2]);
                        return (asignee + " = " + array + "[" + index + "];");
                    }
                case PapyrusOpCodes.ArrayLength:
                    {
                        var asignee = GetArgumentValue(i.Arguments[0]);
                        var array = GetArgumentValue(i.Arguments[1]);
                        return (asignee + " = " + array + ".Length;");
                    }
                case PapyrusOpCodes.ArrayCreate:
                    {
                        var asignee = GetArgumentValue(i.Arguments[0]);
                        var size = GetArgumentValue(i.Arguments[1]);
                        var possibleVar =
                            method.GetVariables().FirstOrDefault(v => v.Name.Value == asignee);
                        var field = type.Fields.FirstOrDefault(v => v.Name.Value == asignee);
                        var prop = type.Properties.FirstOrDefault(v => v.Name.Value == asignee);
                        if (possibleVar != null)
                            return (asignee + " = new " + possibleVar.TypeName.Value.Replace("[]", "") + "[" + size + "];");
                        if (field != null)
                            return (asignee + " = new " + field.TypeName.Replace("[]", "") + "[" + size + "];");
                        if (prop != null)
                            return (asignee + " = new " + prop.TypeName.Value.Replace("[]", "") + "[" + size + "];");
                    }
                    break;
                case PapyrusOpCodes.Strcat:
                    {
                        var asignee = GetArgumentValue(i.Arguments[0]);
                        var str = GetArgumentValue(i.Arguments[1]);
                        var value = GetArgumentValue(i.Arguments[2]);
                        return (asignee + " = " + str + " + " + value + ";");
                    }
                case PapyrusOpCodes.Fsub:
                case PapyrusOpCodes.Isub:
                    {
                        return DoMath(i, "-");
                    }
                case PapyrusOpCodes.Fadd:
                case PapyrusOpCodes.Iadd:
                    {
                        return DoMath(i, "+");
                    }
                case PapyrusOpCodes.Ineg:
                    {
                        var asignee = GetArgumentValue(i.Arguments[0]);
                        var target = GetArgumentValue(i.Arguments[1]);
                        return (asignee + " = " + target + " < 0;");
                    }
                case PapyrusOpCodes.CmpEq:
                    {
                        return DoMath(i, "==");
                    }
                case PapyrusOpCodes.CmpGt:
                    {
                        return DoMath(i, ">");
                    }
                case PapyrusOpCodes.CmpGte:
                    {
                        return DoMath(i, ">=");
                    }
                case PapyrusOpCodes.CmpLt:
                    {
                        return DoMath(i, "<");
                    }
                case PapyrusOpCodes.CmpLte:
                    {
                        return DoMath(i, "<=");
                    }
                case PapyrusOpCodes.Not:
                    {
                        // var comment = WritePapyrusInstruction(i) + Environment.NewLine;

                        var assignee = i.GetArg(0);
                        var target = i.GetArg(1);
                        return (assignee + " = !" + target + ";");
                    }
                case PapyrusOpCodes.Jmp:
                    {
                        var destination = GetArgumentValue(i.Arguments[0]);
                        return ("// " + i.OpCode + " " + destination + " (offset: " +
                                   (i.Offset + int.Parse(destination)) + ")");
                    }
                case PapyrusOpCodes.Jmpt:
                case PapyrusOpCodes.Jmpf:
                    {
                        var boolVal = GetArgumentValue(i.Arguments[0]);
                        var destination = GetArgumentValue(i.Arguments[1]);
                        return ("// " + i.OpCode + " " + boolVal + " " + destination + " (offset: " +
                                   (i.Offset + int.Parse(destination)) + ")");
                    }
                default:
                    {
                        return WritePapyrusInstruction(i);
                    }
            }
            return null;
        }

        private string WritePapyrusInstruction(PapyrusInstruction i)
        {
            var instructionParams = string.Join(" ", i.Arguments.Select(GetArgumentValue));
            var instructionObjectParams = string.Join(" ", i.OperandArguments.Select(GetArgumentValue));

            return ("// " + i.OpCode + " " + instructionParams + " " + instructionObjectParams);
        }

        private string DoMath(PapyrusInstruction i, string op)
        {
            var asignee = GetArgumentValue(i.Arguments[0]);
            var target = GetArgumentValue(i.Arguments[1]);
            var value = GetArgumentValue(i.Arguments[2]);
            if (string.IsNullOrEmpty(value))
            {
                value = "null";
            }
            return (asignee + " = " + target + " " + op + " " + value + ";"); //, indent, i);
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
                            return (arg.Value.Equals(true) || arg.Value.Equals(1)) ? "true" : "false";
                        }
                    }
                    break;
                case PapyrusPrimitiveType.Integer:
                    if (arg.Value != null)
                    {
                        return arg.Value.ToString();
                    }
                    break;
                case PapyrusPrimitiveType.Float:
                    if (arg.Value != null)
                    {
                        return arg.Value.ToString().Replace(",", ".") + "f";
                    }
                    break;
            }

            if (arg.Name != null)
            {
                return (string)arg.Name;
            }
            return null;
        }

        /*
                private void SetGlobalIndent(int startingIndent)
                {
                    activeIndent = startingIndent;
                }
        */


        private void Append(string value, int indent = 0, PapyrusInstruction key = null)
        {
            activeBuilder.Append(value, key, indent);
        }

        private void AppendLine(string value, int indent = 0, PapyrusInstruction key = null)
        {
            activeBuilder.AppendLine(value, key, indent);
        }

        private bool RefIsNull(PapyrusStringRef baseTypeName)
        {
            return baseTypeName == null || string.IsNullOrEmpty(baseTypeName.Value);
        }
    }

}
