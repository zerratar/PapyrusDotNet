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

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using PapyrusDotNet.Common.Papyrus;
using PapyrusDotNet.PapyrusAssembly.Enums;
using MemberAttributes = PapyrusDotNet.Common.Papyrus.FieldAttributes;
using VariableReference = PapyrusDotNet.Common.Papyrus.VariableReference;

#endregion

namespace PapyrusDotNet.Common
{
    public class Utility
    {
        /*[HKEY_CLASSES_ROOT\Local Settings\Software\Microsoft\Windows\Shell\MuiCache]
        "D:\\The Elder Scrolls V Skyrim\\CreationKit.exe"="Creation Kit"
        "D:\\The Elder Scrolls V Skyrim\\TESV.exe"="Skyrim"
        "D:\\The Elder Scrolls V Skyrim\\SkyrimLauncher.exe"="Skyrim Launcher"
        "D:\\The Elder Scrolls V Skyrim\\unins000.exe"="Setup/Uninstall"*/


        public static SkyrimInstallationPath GetInstallationFolder()
        {
            try
            {
                var subLocalSettings = Registry.ClassesRoot.OpenSubKey("Local Settings");
                var subSoftware = subLocalSettings?.OpenSubKey("Software");
                var subMicrosoft = subSoftware?.OpenSubKey("Microsoft");
                var subWindows = subMicrosoft?.OpenSubKey("Windows");
                var subShell = subWindows?.OpenSubKey("Shell");
                var subMuiCache = subShell?.OpenSubKey("MuiCache");
                var test = subMuiCache?.GetValueNames();

                var installationFolder =
                    test?.FirstOrDefault(
                        s => s.ToLower().Contains("tesv.exe") || s.ToLower().Contains("skyrimlauncher.exe"));
                var creationkitFolder = test?.FirstOrDefault(s => s.ToLower().Contains("creationkit.exe"));

                var ifo = "";
                var ckfo = "";

                if (!String.IsNullOrEmpty(installationFolder))
                    ifo = Path.GetDirectoryName(installationFolder);

                if (!String.IsNullOrEmpty(creationkitFolder))
                    ckfo = Path.GetDirectoryName(creationkitFolder);

                return new SkyrimInstallationPath(ifo, ckfo);
            }
            catch (Exception)
            {
                // Ignored
            }
            return null;
        }


        public static string InitialValue(FieldDefinition variable)
        {
            var initialValue = "None";
            if (variable.InitialValue != null && variable.InitialValue.Length > 0)
            {
                if (variable.FieldType.FullName.ToLower().Contains("system.int")
                    || variable.FieldType.FullName.ToLower().Contains("system.byte")
                    || variable.FieldType.FullName.ToLower().Contains("system.short")
                    || variable.FieldType.FullName.ToLower().Contains("system.long"))
                {
                    initialValue = BitConverter.ToInt32(variable.InitialValue, 0).ToString();
                    // "\"" + Encoding.Default.GetString(v) + "\"";
                }
                if (variable.FieldType.FullName.ToLower().Contains("system.double")
                    || variable.FieldType.FullName.ToLower().Contains("system.float"))
                {
                    initialValue = BitConverter.ToDouble(variable.InitialValue, 0).ToString();
                    // "\"" + Encoding.Default.GetString(v) + "\"";
                }

                if (variable.FieldType.FullName.ToLower().Contains("system.string"))
                {
                    initialValue = "\"" + Encoding.Default.GetString(variable.InitialValue) + "\"";
                }

                if (variable.FieldType.FullName.ToLower().Contains("system.bool"))
                {
                    initialValue = (variable.InitialValue[0] == 1).ToString();
                }
            }
            return initialValue;
        }

        public static string CustomAttributeValue(CustomAttribute varAttr)
        {
            var ctrArg = varAttr.ConstructorArguments.FirstOrDefault();
            if (ctrArg.Value != null)
            {
                if (ctrArg.Value is CustomAttributeArgument)
                {
                    var arg = (CustomAttributeArgument)ctrArg.Value;
                    var val = arg.Value;

                    return TypeValueConvert(arg.Type.Name, val).ToString();
                }
                return ctrArg.Value.ToString();
            }
            return null;
        }

        public static MemberAttributes GetFlagsAndProperties(TypeDefinition variable)
        {
            var attributes = GetFlagsAndProperties(variable.CustomAttributes);

            if (variable.HasGenericParameters)
            {
                attributes.IsGeneric = true;
                var genericParameters = variable.GenericParameters;
            }

            foreach (var varAttr in variable.CustomAttributes)
            {
                if (varAttr.AttributeType.Name.Equals("InitialValueAttribute"))
                {
                    attributes.InitialValue = CustomAttributeValue(varAttr);
                }
            }
            return attributes;
        }

        public static MemberAttributes GetFlagsAndProperties(FieldDefinition variable)
        {
            var attributes = GetFlagsAndProperties(variable.CustomAttributes);

            if (variable.FieldType.Name == "T") attributes.IsGeneric = true;

            return attributes;
        }

        public static MemberAttributes GetFlagsAndProperties(PropertyDefinition variable)
        {
            var attributes = GetFlagsAndProperties(variable.CustomAttributes);

            if (variable.PropertyType.Name == "T") attributes.IsGeneric = true;

            return attributes;
        }


        public static MemberAttributes GetFlagsAndProperties(MethodDefinition method)
        {
            var attributes = GetFlagsAndProperties(method.CustomAttributes);

            //if (variable.FieldType.Name == "T") attributes.IsGeneric = true;

            return attributes;
        }

        public static MemberAttributes GetFlagsAndProperties(Collection<CustomAttribute> customAttributes)
        {
            string initialValue = null, docString = null;
            bool isProperty = false,
                isAuto = false,
                isAutoReadOnly = false,
                isHidden = false,
                isConditional = false,
                isGeneric = false;

            foreach (var varAttr in customAttributes)
            {
                if (varAttr.AttributeType.Name.Equals("PropertyAttribute"))
                    isProperty = true;
                if (varAttr.AttributeType.Name.Equals("GenericMemberAttribute"))
                {
                    isGeneric = true;
                    foreach (var arg in varAttr.ConstructorArguments)
                    {
                        // Not implemented yet
                    }
                }
                if (varAttr.AttributeType.Name.Equals("DocStringAttribute"))
                {
                    docString = CustomAttributeValue(varAttr);
                }
                if (varAttr.AttributeType.Name.Equals("InitialValueAttribute"))
                {
                    var ctrArg = varAttr.ConstructorArguments.FirstOrDefault();
                    if (ctrArg.Value != null)
                    {
                        if (ctrArg.Value is CustomAttributeArgument)
                        {
                            var arg = (CustomAttributeArgument)ctrArg.Value;
                            var val = arg.Value;

                            initialValue = TypeValueConvert(arg.Type.Name, val).ToString();
                        }
                        else
                            initialValue = ctrArg.Value.ToString();
                    }
                }
                if (varAttr.AttributeType.Name.Equals("AutoAttribute"))
                    isAuto = true;

                if (varAttr.AttributeType.Name.Equals("AutoReadOnlyAttribute"))
                    isAutoReadOnly = true;

                if (varAttr.AttributeType.Name.Equals("HiddenAttribute"))
                    isHidden = true;

                if (varAttr.AttributeType.Name.Equals("ConditionalAttribute"))
                    isConditional = true;
            }
            return new MemberAttributes
            {
                IsGeneric = isGeneric,
                InitialValue = initialValue,
                DocString = docString,
                IsAuto = isAuto,
                IsAutoReadOnly = isAutoReadOnly,
                IsConditional = isConditional,
                IsHidden = isHidden,
                IsProperty = isProperty
            };
        }


        public static string GetString(object p)
        {
            if (p is string) return (string)p;
            return "";
        }


        public static string GetPapyrusReturnType(TypeReference reference, bool stripGenericMarkers)
        {
            var retType = GetPapyrusReturnType(reference);


            if (stripGenericMarkers && reference.IsGenericInstance)
            {
                retType = retType.Replace("`1", "_" + GetPapyrusBaseType(reference.FullName.Split('<')[1].Split('>')[0]));
            }

            return retType;
        }

        public static string GetPapyrusReturnType(TypeReference reference)
        {
            return GetPapyrusReturnType(reference.Name, reference.Namespace);
        }

        public static string GetPapyrusReturnType(string reference)
        {
            return GetPapyrusReturnType(reference.Split('.').LastOrDefault(),
                reference.Remove(reference.LastIndexOf('.')));
        }

        public static string GetPapyrusBaseType(TypeReference typeRef)
        {
            var name = typeRef.Name;
            var Namespace = typeRef.Namespace;
            if (name == "Object" || Namespace.ToLower().Equals("system") || Namespace.ToLower().StartsWith("system."))
                return "";

            if (Namespace.ToLower().StartsWith("papyrusdotnet.core.") || Namespace.StartsWith("PapyrusDotNet.System"))
            {
                return "PDN_" + name;
            }
            if (Namespace.StartsWith("PapyrusDotNet.Core"))
            {
                return typeRef.Name;
            }

            if (String.IsNullOrEmpty(Namespace))
                return name;

            return Namespace.Replace(".", "_") + "_" + name;
        }

        public static string GetPapyrusBaseType(string fullName)
        {
            switch (fullName)
            {
                case "Object":
                    return "";
            }

            var name = fullName;
            var Namespace = "";

            if (fullName.Contains("."))
            {
                name = fullName.Split('.').LastOrDefault();
                Namespace = fullName.Remove(fullName.LastIndexOf("."));
            }

            if (name == "Object") return "";

            if (Namespace.ToLower().StartsWith("system"))
            {
                return GetPapyrusReturnType(name, Namespace);
            }

            if (Namespace.ToLower().StartsWith("papyrusdotnet.core."))
            {
                return "DotNet" + name;
            }
            if (Namespace.StartsWith("PapyrusDotNet.Core"))
            {
                return name;
            }

            if (String.IsNullOrEmpty(Namespace))
                return name;

            return Namespace.Replace(".", "_") + "_" + name;
        }

        public static int ConvertToTimestamp(DateTime value)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            var span = value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();

            //return the total seconds (which is a UNIX timestamp)
            return (int)span.TotalSeconds;
        }

        public static string GetPapyrusReturnType(string type, string Namespace)
        {
            var swtype = type;
            var swExt = "";
            var isArray = swtype.Contains("[]");

            if (!String.IsNullOrEmpty(Namespace))
            {
                if (Namespace.ToLower().StartsWith("system"))
                {
                    swExt = "";
                }

                else if (Namespace.ToLower().StartsWith("papyrusdotnet.core."))
                {
                    swExt = "DotNet";
                }
                else
                {
                    if (Namespace.ToLower().Equals("papyrusdotnet.core"))
                        swExt = "";
                    else
                        swExt = Namespace.Replace('.', '_') + "_";
                }
            }
            if (isArray)
            {
                swtype = swtype.Split(new[] { "[]" }, StringSplitOptions.None)[0];
            }
            switch (swtype.ToLower())
            {
                // for now..
                case "enum":
                    return "";
                case "none":
                case "nil":
                case "nnull":
                case "null":
                case "void":
                    return "None" + (isArray ? "[]" : "");
                case "bool":
                case "boolean":
                    return "Bool" + (isArray ? "[]" : "");
                case "long":
                case "int64":
                case "integer64":
                case "int32":
                case "integer":
                case "integer32":
                case "int":
                    return "Int" + (isArray ? "[]" : "");
                case "float":
                case "float32":
                case "double":
                case "double32":
                case "single":
                    return "Float" + (isArray ? "[]" : "");
                case "string":
                    return "String" + (isArray ? "[]" : "");
                default:
                    return swExt + type;
                    // case "Bool":
            }
        }

        public static string Indent(int indents, string line, bool newLine = true)
        {
            var output = "";
            for (var j = 0; j < indents; j++) output += '\t';
            output += line;

            if (newLine) output += Environment.NewLine;

            return output;
        }


        public static object TypeValueConvert(string typeName, object op)
        {
            if (typeName.ToLower().StartsWith("bool") || typeName.ToLower().StartsWith("system.bool"))
            {
                if (op is int || op is float || op is short || op is double || op is long || op is byte)
                    return (int)Double.Parse(op.ToString()) == 1;
                if (op is bool) return (bool)op;
                if (op is string) return (string)op == "1" || op.ToString().ToLower() == "true";
            }
            if (typeName.ToLower().StartsWith("string") || typeName.ToLower().StartsWith("system.string"))
            {
                if (!op.ToString().Contains("\"")) return "\"" + op + "\"";
            }
            else if (op is float || op is decimal || op is double)
            {
                if (op.ToString().Contains(","))
                {
                    return op.ToString().Replace(',', '.');
                }
            }

            if (typeName.ToLower().StartsWith("int"))
            {
                if (op is int || op is float || op is short || op is double || op is long || op is byte)
                {
                    return Int32.Parse(op.ToString());
                }
            }

            return op;
        }

        public static string GetVariableName(Instruction instruction)
        {
            var i = instruction.OpCode.Name;
            if (i.Contains("."))
            {
                var index = i.Split('.').LastOrDefault();
                return "V_" + index;
            }

            // "V_0"
            return "V_" + instruction.Operand;
        }

        public static string OptimizeLabels(string input)
        {
            var output = input;
            output = RemoveUnusedLabels(output);
            output = RemoveUnnecessaryLabels(output);
            return output;
        }

        public static string RemoveUnusedLabels(string output)
        {
            var rows = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

            var codeBlocks = ParseCodeBlocks(rows);

            var labelsToRemove = new Dictionary<int, string>();

            foreach (var block in codeBlocks)
            {
                foreach (var lbl in block.Labels)
                {
                    var isCalled = false;
                    foreach (var ulbl in block.UsedLabels)
                    {
                        if (lbl.Name == ulbl.Name)
                        {
                            isCalled = true;
                        }
                    }
                    if (!isCalled)
                    {
                        labelsToRemove.Add(lbl.Row, lbl.Name);
                    }
                }
            }

            var ordered = labelsToRemove.OrderByDescending(i => i.Key).ToArray();

            foreach (var row in ordered)
            {
                rows.RemoveAt(row.Key);
            }

            return String.Join(Environment.NewLine, rows.ToArray());
        }

        public static CodeBlock ParseCodeBlock(string codeBlock)
        {
            var rows = codeBlock.Split('\n');
            return ParseCodeBlocks(rows.ToList()).FirstOrDefault();
        }

        public static List<CodeBlock> ParseCodeBlocks(List<string> rows)
        {
            var codeBlocks = new List<CodeBlock>();
            CodeBlock latestCodeBlock = null;
            var rowI = 0;

            foreach (var row in rows)
            {
                if (row.Replace("\t", "").Trim().StartsWith(".code"))
                {
                    latestCodeBlock = new CodeBlock();
                    latestCodeBlock.StartRow = rowI;
                }
                else if (row.Replace("\t", "").Trim().StartsWith(".endCode"))
                {
                    if (latestCodeBlock != null)
                    {
                        latestCodeBlock.EndRow = rowI;
                        codeBlocks.Add(latestCodeBlock);
                    }
                }
                else if (latestCodeBlock != null)
                {
                    if (row.Replace("\t", "").StartsWith("_") && row.Trim().EndsWith(":"))
                    {
                        latestCodeBlock.Labels.Add(new LabelDefinition(rowI, row.Replace("\t", "").Trim()));
                    }
                    else if (row.Replace("\t", "").Contains("_label") /* && !row.Contains(":")*/&&
                             row.ToLower().Contains("jump"))
                    {
                        latestCodeBlock.UsedLabels.Add(
                            new LabelReference(row.Substring(row.IndexOf("_label")).Split(' ')[0] + ":", rowI));
                    }
                }
                rowI++;
            }
            return codeBlocks;
        }


        public static string RemoveUnnecessaryLabels(string output)
        {
            var rows = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            var labelReplacements =
                new List<ObjectReplacementHolder<LabelDefinition, LabelDefinition, LabelReference>>();
            var codeBlocks = ParseCodeBlocks(rows);
            var lastReplacement = new ObjectReplacementHolder<LabelDefinition, LabelDefinition, LabelReference>();
            foreach (var codeBlock in codeBlocks)
            {
                for (var i = 0; i < codeBlock.Labels.Count; i++)
                {
                    var currentLabel = codeBlock.Labels[i];

                    var lastRowIndex = currentLabel.Row;
                    while (i + 1 < codeBlock.Labels.Count)
                    {
                        if (lastReplacement == null)
                        {
                            lastReplacement =
                                new ObjectReplacementHolder<LabelDefinition, LabelDefinition, LabelReference>();
                        }

                        var label = codeBlock.GetLabelDefinition(lastRowIndex + 1);
                        if (label != null)
                        {
                            if (lastReplacement.Replacement == null) lastReplacement.Replacement = currentLabel;

                            lastReplacement.ToReplace.Add(label);

                            var usedAreas = codeBlock.UsedLabels.Where(b => b.Name == label.Name);
                            if (usedAreas.Any())
                            {
                                lastReplacement.ToReplaceSecondary.AddRange(usedAreas.ToArray());
                            }


                            lastRowIndex = label.Row;
                            // We have a previous label one row behind us.
                        }
                        else
                        {
                            break;
                        }
                        i++;
                    }
                    if (lastReplacement != null && lastReplacement.ToReplace.Count > 0)
                    {
                        labelReplacements.Add(lastReplacement);
                        lastReplacement = null;
                    }
                }
            }
            var rowsToRemove = new List<int>();
            foreach (var replacer in labelReplacements)
            {
                foreach (var old in replacer.ToReplace)
                {
                    rows[old.Row] = rows[old.Row].Replace(old.Name, replacer.Replacement.Name);
                    rowsToRemove.Add(old.Row);
                }
                foreach (var old in replacer.ToReplaceSecondary)
                {
                    rows[old.RowReference] = rows[old.RowReference].Replace(
                        old.Name.Remove(old.Name.Length - 1),
                        replacer.Replacement.Name.Remove(replacer.Replacement.Name.Length - 1));
                }
            }

            foreach (var r in rowsToRemove.OrderByDescending(v => v))
            {
                rows.RemoveAt(r);
            }

            return String.Join(Environment.NewLine, rows.ToArray());
        }

        public static string InjectTempVariables(string output, int indentDepth, List<VariableReference> TempVariables)
        {
            var rows = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

            // foreach(var )
            var insertIndex = Array.IndexOf(rows.ToArray(),
                rows.FirstOrDefault(r => r.ToLower().Contains(".endlocaltable")));


            foreach (var variable in TempVariables)
            {
                rows.Insert(insertIndex, Indent(indentDepth, variable.Definition, false));
            }


            return String.Join(Environment.NewLine, rows.ToArray());
        }

        public static int GetStackPopCount(StackBehaviour stackBehaviour)
        {
            switch (stackBehaviour)
            {
                case StackBehaviour.Pop0:
                    return 0;
                case StackBehaviour.Varpop:
                case StackBehaviour.Popi:
                case StackBehaviour.Pop1:
                case StackBehaviour.Popref:
                    return 1;
                case StackBehaviour.Popi_pop1:
                case StackBehaviour.Popi_popi:
                case StackBehaviour.Popi_popi8:
                case StackBehaviour.Popi_popr8:
                case StackBehaviour.Popi_popr4:
                case StackBehaviour.Pop1_pop1:
                case StackBehaviour.Popref_popi:
                    return 2;
                case StackBehaviour.Popi_popi_popi:
                case StackBehaviour.Popref_popi_popi:
                case StackBehaviour.Popref_popi_popi8:
                case StackBehaviour.Popref_popi_popr4:
                case StackBehaviour.Popref_popi_popr8:
                case StackBehaviour.Popref_popi_popref:
                    return 3;
                case StackBehaviour.PopAll:
                    return 9999;
            }
            return 0;
        }


        public static string GetPapyrusMathOp(Code code)
        {
            switch (code)
            {
                case Code.Add_Ovf:
                case Code.Add_Ovf_Un:
                case Code.Add:
                    return "IAdd";
                case Code.Sub:
                case Code.Sub_Ovf:
                case Code.Sub_Ovf_Un:
                    return "ISubtract";
                case Code.Div_Un:
                case Code.Div:
                    return "IDivide";
                case Code.Mul:
                case Code.Mul_Ovf:
                case Code.Mul_Ovf_Un:
                    return "IMultiply";
                default:
                    return "IAdd";
            }
        }




        public static string GetPropertyName(string p)
        {
            var outName = p;
            if (p.StartsWith("::"))
            {
                outName = outName.Substring(2);
            }
            return "p" + outName;
        }

        public static bool IsCallMethodInsideNamespace(Instruction instruction, string targetNamespace,
            out MethodReference methodRef)
        {
            methodRef = null;
            if (InstructionHelper.IsCallMethod(instruction.OpCode.Code))
            {
                var method = instruction.Operand as MethodReference;
                if (method != null && method.FullName.Contains(targetNamespace))
                {
                    methodRef = method;
                    return true;
                }
            }
            return false;
        }

        public static PapyrusPrimitiveType GetPapyrusValueType(string name)
        {
            var s = name.ToLower();
            if (s.StartsWith("bool"))
                return PapyrusPrimitiveType.Boolean;
            if (s.StartsWith("string"))
                return PapyrusPrimitiveType.String;
            if (s.StartsWith("float"))
                return PapyrusPrimitiveType.Float;
            if (s.StartsWith("int") || s.StartsWith("sbyte") || s.StartsWith("short") || s.StartsWith("long"))
                return PapyrusPrimitiveType.Integer;
            return PapyrusPrimitiveType.Reference;
        }


        public static PapyrusOpCode GetPapyrusMathOrEqualityOpCode(Code code, bool isFloat)
        {
            if (InstructionHelper.IsLessThan(code))
            {
                return PapyrusOpCode.CmpLt;
            }
            if (InstructionHelper.IsEqualTo(code))
            {
                return PapyrusOpCode.CmpEq;
            }
            if (InstructionHelper.IsGreaterThan(code))
            {
                return PapyrusOpCode.CmpGt;
            }
            switch (code)
            {
                case Code.Add_Ovf:
                case Code.Add_Ovf_Un:
                case Code.Add:
                    return isFloat ? PapyrusOpCode.Fadd : PapyrusOpCode.Iadd;
                case Code.Sub:
                case Code.Sub_Ovf:
                case Code.Sub_Ovf_Un:
                    return isFloat ? PapyrusOpCode.Fsub : PapyrusOpCode.Isub;
                case Code.Div_Un:
                case Code.Div:
                    return isFloat ? PapyrusOpCode.Fdiv : PapyrusOpCode.Idiv;
                case Code.Mul:
                case Code.Mul_Ovf:
                case Code.Mul_Ovf_Un:
                    return isFloat ? PapyrusOpCode.Fmul : PapyrusOpCode.Imul;
                default:
                    return isFloat ? PapyrusOpCode.Fadd : PapyrusOpCode.Iadd;
            }
        }

        public static PapyrusPrimitiveType GetPrimitiveType(object val)
        {
            var type = val.GetType();

            var typeName = GetPapyrusReturnType(type.FullName);

            return GetPapyrusValueType(typeName);
        }
    }
}