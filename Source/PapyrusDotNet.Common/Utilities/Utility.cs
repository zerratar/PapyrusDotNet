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
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.PapyrusAssembly;
using VariableReference = PapyrusDotNet.Common.Papyrus.VariableReference;

#endregion

namespace PapyrusDotNet.Common.Utilities
{
    public class Utility : IUtility
    {
        public static T GetKeyByValue<T, T2>(Dictionary<T, List<T2>> dict, T2 item)
        {
            foreach (var i in dict)
            {
                if (i.Value.Contains(item))
                    return i.Key;
            }
            return default(T);
        }

        public static string GetInitialValue(FieldDefinition variable)
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

        //public static string GetPapyrusReturnType(TypeReference reference, bool stripGenericMarkers)
        //{
        //    var retType = GetPapyrusReturnType(reference);


        //    if (stripGenericMarkers && reference.IsGenericInstance)
        //    {
        //        retType = retType.Replace("`1", "_" + GetPapyrusBaseType(reference.FullName.Split('<')[1].Split('>')[0], caller));
        //    }

        //    return retType;
        //}

        public static string GetPapyrusReturnType(TypeReference reference, TypeDefinition caller, bool stripGenericMarkers)
        {
            var retType = GetPapyrusReturnType(reference, caller);


            if (stripGenericMarkers && reference.IsGenericInstance)
            {
                retType = retType.Replace("`1", "_" + GetPapyrusBaseType(reference.FullName.Split('<')[1].Split('>')[0], caller));
            }

            return retType;
        }

        public static string GetPapyrusReturnType(TypeReference reference, TypeDefinition caller = null)
        {
            return GetPapyrusReturnType(reference.Name, reference.Namespace, caller);
        }

        public static string GetPapyrusReturnType(string reference, TypeDefinition caller = null)
        {
            if (reference.Contains("."))
                return GetPapyrusReturnType(reference.Split('.').LastOrDefault(),
                    reference.Remove(reference.LastIndexOf('.')), caller);
            else return GetPapyrusReturnType(reference, "", caller);
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

        public static string GetPapyrusBaseType(string fullName, TypeDefinition caller)
        {
            if (fullName == "Object")
                return "";

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
                return GetPapyrusReturnType(name, Namespace, caller);
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

        public static string GetPapyrusReturnType(string type, string Namespace, TypeDefinition caller)
        {
            var swtype = type;
            var swExt = "";
            var isArray = swtype.Contains("[]");

            if (type.ToLower().Contains("myteststruct"))
            {

            }
            if (caller != null)
            {

                if (caller.Namespace == Namespace || Namespace == "")
                {
                    if (caller.NestedTypes.Any(t => t.Name.ToLower() == type.ToLower().Replace("[]", "")))
                    {
                        swtype = caller.Name + "#" + type;
                        type = caller.Name + "#" + type;
                    }
                }
            }
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
                case StackBehaviour.Popref_pop1:
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

        public static string GetPropertyName(string name)
        {
            var outName = name;
            if (name.StartsWith("::"))
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

        public static PapyrusPrimitiveType GetPapyrusPrimitiveType(string type)
        {
            var s = type.ToLower();
            if (s.StartsWith("none") || s.StartsWith("void"))
                return PapyrusPrimitiveType.None;
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


        public static PapyrusOpCodes GetPapyrusMathOrEqualityOpCode(Code code, bool isFloat)
        {
            if (InstructionHelper.IsLessThan(code))
            {
                return PapyrusOpCodes.CmpLt;
            }
            if (InstructionHelper.IsEqualTo(code))
            {
                return PapyrusOpCodes.CmpEq;
            }
            if (InstructionHelper.IsGreaterThan(code))
            {
                return PapyrusOpCodes.CmpGt;
            }
            switch (code)
            {
                case Code.Add_Ovf:
                case Code.Add_Ovf_Un:
                case Code.Add:
                    return isFloat ? PapyrusOpCodes.Fadd : PapyrusOpCodes.Iadd;
                case Code.Sub:
                case Code.Sub_Ovf:
                case Code.Sub_Ovf_Un:
                    return isFloat ? PapyrusOpCodes.Fsub : PapyrusOpCodes.Isub;
                case Code.Div_Un:
                case Code.Div:
                    return isFloat ? PapyrusOpCodes.Fdiv : PapyrusOpCodes.Idiv;
                case Code.Mul:
                case Code.Mul_Ovf:
                case Code.Mul_Ovf_Un:
                    return isFloat ? PapyrusOpCodes.Fmul : PapyrusOpCodes.Imul;
                default:
                    return isFloat ? PapyrusOpCodes.Fadd : PapyrusOpCodes.Iadd;
            }
        }

        public static PapyrusPrimitiveType GetPrimitiveTypeFromType(TypeReference type)
        {
            var typeName = GetPapyrusReturnType(type.FullName);

            return GetPapyrusPrimitiveType(typeName);
        }

        public static PapyrusPrimitiveType GetPrimitiveTypeFromType(Type type)
        {
            var typeName = GetPapyrusReturnType(type.FullName);

            return GetPapyrusPrimitiveType(typeName);
        }

        public static PapyrusPrimitiveType GetPrimitiveTypeFromValue(object val)
        {
            if (val == null)
                return PapyrusPrimitiveType.None;
            var type = val.GetType();

            var typeName = GetPapyrusReturnType(type.FullName);

            return GetPapyrusPrimitiveType(typeName);
        }

        public static string PapyrusValueTypeToString(PapyrusPrimitiveType valueType)
        {
            switch (valueType)
            {
                case PapyrusPrimitiveType.Boolean:
                    return "Bool";
                case PapyrusPrimitiveType.Float:
                    return "Float";
                case PapyrusPrimitiveType.Integer:
                    return "Int";
                case PapyrusPrimitiveType.String:
                    return "String";
                default:
                case PapyrusPrimitiveType.Reference:
                case PapyrusPrimitiveType.None:
                    return null;
            }
        }

        public static bool IsConstantValue(object value)
        {
            return value is int || value is byte || value is short || value is long || value is double || value is float || value is string || value is bool;
        }

        public static bool IsVoid(TypeReference typeReference)
        {
            return typeReference.FullName.ToLower().Equals("system.void")
                   || typeReference.Name.ToLower().Equals("void");
        }
    }
}