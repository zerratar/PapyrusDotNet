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
	
	Copyright 2015, Karl Patrik Johansson, zerratar@gmail.com
 */

using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using PapyrusDotNet.Common;
using PapyrusDotNet.Common.Papyrus;

namespace PapyrusDotNet.Papyrus
{
    public class Assembly
    {
        // public string AssemblyCode { get; set; }

        public Assembly()
        {
            Header = new Header();

            DelegateMethodDefinitions = new List<MethodDefinition>();
            ObjectTable = new ObjectTable();
        }

        public Assembly(TypeDefinition type, string genericTypeReplacement)
        {
            Header = new Header();
            ObjectTable = new ObjectTable();
            SourceType = type;
            GenericTypeReplacement = genericTypeReplacement;
            BaseType = Utility.GetPapyrusBaseType(type);
            ObjectTable = CreateObjectTable(type);
            OutputName = BaseType;

            DelegateMethodDefinitions = new List<MethodDefinition>();
        }

        public Assembly(TypeDefinition type)
        {
            Header = new Header();
            ObjectTable = new ObjectTable();

            SourceType = type;
            BaseType = Utility.GetPapyrusBaseType(type);
            ObjectTable = CreateObjectTable(type);
            OutputName = BaseType;

            DelegateMethodDefinitions = new List<MethodDefinition>();
            // this.AssemblyCode = this.ObjectTable.ToString();
        }

        public TypeDefinition SourceType { get; set; }

        public string GenericTypeReplacement { get; set; }

        public bool IsEnum { get; set; }

        public Header Header { get; set; }

        public ObjectTable ObjectTable { get; set; }

        public string FinalAssemblyCode { get; set; }

        public string BaseType { get; set; }

        public string OutputName { get; set; }

        public List<MethodDefinition> DelegateMethodDefinitions { get; set; }

        public List<MethodReference> ExtensionMethodReferences { get; set; }

        public Dictionary<MethodDefinition, List<FieldDefinition>> DelegateMethodFieldPair { get; set; }

        public List<FieldDefinition> DelegateFields { get; set; }

        public ObjectTable CreateObjectTable(TypeDefinition type)
        {
            ExtensionMethodReferences = new List<MethodReference>();
            DelegateMethodDefinitions = new List<MethodDefinition>();
            DelegateMethodFieldPair = new Dictionary<MethodDefinition, List<FieldDefinition>>();
            DelegateFields = new List<FieldDefinition>();
            var defaultObjectState = new ObjectState();
            // Get any compile time generated classes
            // These are usually created when using a paremeterless delegate.
            // However, when parameters are used. The functions seem to be injected into the current class instead.
            foreach (var nt in type.NestedTypes)
            {
                if (nt.Name.StartsWith("<>"))
                {
                    foreach (var m in nt.Methods)
                    {
                        if (m.Name.StartsWith("<"))
                        {
                            m.IsStatic = false;
                            m.Name = m.Name.Replace("<", "_").Replace(">", "_");
                            DelegateMethodDefinitions.Add(m);
                            var fieldDefinitions = new List<FieldDefinition>();
                            foreach (var instruction in m.Body.Instructions)
                            {
                                var op = instruction.Operand;
                                if (op is FieldReference)
                                {
                                    var f = op as FieldReference;
                                    foreach (var field in nt.Fields)
                                    {
                                        if (f.FullName == field.FullName)
                                        {
                                            fieldDefinitions.Add(field);
                                            if (!DelegateFields.Contains(field))
                                                DelegateFields.Add(field);
                                        }
                                    }
                                }

                                /*nt.Fields*/
                            }
                            DelegateMethodFieldPair.Add(m, fieldDefinitions);
                        }
                    }
                }
            }
            // Search for delegate methods inside the current class
            foreach (var m in type.Methods)
            {
                if (IsDelegateMethod(type, m))
                {
                    m.Name = m.Name.Replace("<", "_").Replace(">", "_");
                    m.IsStatic = false;
                    DelegateMethodDefinitions.Add(m);
                }
            }

            // In case we are using some hokus pokus from PapyrusDotNet.System.Linq, 
            // 1. We need to inject those functions seperately into this type.
            // 2. Change the "linq" function name to match the name it is first executed from. Making sure it has a unique name.
            // 3. Replace any generic variables to match the target type.
            // 4. Remove the predicate variable and replace the CallMethod to the generated delegate function
            // 5. Done!
            foreach (var m in type.Methods)
            {
                MethodReference methodRef;
                if (CallsMethodInsideNamespace(m, "PapyrusDotNet.System.Linq", out methodRef))
                {
                    var targetAssembly =
                        PapyrusAsmWriter.ParsedAssemblies.FirstOrDefault(
                            a => a.SourceType.Name == methodRef.DeclaringType.Name);
                    if (targetAssembly != null)
                    {
                        var defaultState = targetAssembly.ObjectTable.StateTable.FirstOrDefault();
                        var targetFunction = defaultState?.Functions.FirstOrDefault(m2 => m2.Name == methodRef.Name);
                        if (targetFunction != null)
                        {
                            // Extension methods are static, however we are injecting this function
                            // directly into the same type. No need for this static call.
                            targetFunction.RemoveStaticKeyword();

                            defaultObjectState.Functions.Add(targetFunction);

                            ExtensionMethodReferences.Add(methodRef);
                        }
                    }
                    //try
                    //{
                    //    var mf = methodRef.Resolve();
                    //    ExtensionMethodDefinitions.Add(mf);
                    //}
                    //catch { }
                }
            }
            IsEnum = type.IsEnum;

            ObjectTable.Name = Utility.GetPapyrusBaseType(type);
            ObjectTable.BaseType = type.BaseType != null ? Utility.GetPapyrusBaseType(type.BaseType) : "";
            ObjectTable.Info = Utility.GetFlagsAndProperties(type);


            var allFields = new List<FieldDefinition>();
            allFields.AddRange(type.Fields);
            allFields.AddRange(DelegateFields);
            foreach (var variable in allFields)
            {
                var varProps = Utility.GetFlagsAndProperties(variable);

                var variableName = variable.Name.Replace('<', '_').Replace('>', '_');

                // Don't parse any temporarily variables necessary for delegates
                // At least not for now...
                if (variableName.Contains("CachedAnonymousMethodDelegate"))
                {
                    continue;
                }

                var variableType = Utility.GetPapyrusReturnType(variable.FieldType, true);

                var initialValue = Utility.InitialValue(variable);

                if (IsEnum)
                {
                    if (variable.FieldType.FullName == type.FullName)
                    {
                        variableType = "Int";
                    }

                    if (initialValue == "None" && variable.Constant != null)
                        initialValue = variable.Constant.ToString();
                }

                if (varProps.InitialValue == null)
                    varProps.InitialValue = initialValue;

                var newVar = new VariableReference("::" + variableName, variableType)
                {
                    Attributes = varProps
                };


                ObjectTable.VariableTable.Add(newVar);

                PapyrusAsmWriter.Fields.Add(newVar);
            }

            foreach (var propField in PapyrusAsmWriter.Fields.Where(f => f.Attributes.IsProperty))
            {
                var propName = Utility.GetPropertyName(propField.Name);
                var fieldType = propField.TypeName;

                var prop = new VariableReference(propName, fieldType)
                {
                    AutoVarName = propField.Name,
                    Attributes = propField.Attributes
                };

                ObjectTable.PropertyTable.Add(prop);
            }

            // Enums do not have any functions or states.
            if (!IsEnum)
            {
                ObjectTable.StateTable.Add(defaultObjectState);

                var methods = type.Methods.OrderByDescending(c => c.Name).ToList();
                var onInitAvailable = methods.Any(m => m.Name.ToLower().Contains("oninit"));
                var ctorAvailable = methods.Any(m => m.Name.ToLower().Contains(".ctor"));
                var mergeCtorAndOnInit = ctorAvailable && onInitAvailable;

                //foreach (var method in ExtensionMethodReferences)
                //{
                //    if (!defaultObjectState.Functions.Any(c => c.Name == method.Name))
                //        defaultObjectState.Functions.Add(CreatePapyrusFunction(this, type, method, mergeCtorAndOnInit, onInitAvailable, ctorAvailable));
                //}

                foreach (var method in DelegateMethodDefinitions)
                {
                    if (defaultObjectState.Functions.All(c => c.Name != method.Name))
                        defaultObjectState.Functions.Add(CreatePapyrusFunction(this, type, method, mergeCtorAndOnInit,
                            onInitAvailable, ctorAvailable));
                }

                foreach (var method in methods)
                {
                    if (defaultObjectState.Functions.All(c => c.Name != method.Name))
                        defaultObjectState.Functions.Add(CreatePapyrusFunction(this, type, method, mergeCtorAndOnInit,
                            onInitAvailable, ctorAvailable));
                }
            }
            return ObjectTable;
        }

        private static bool CallsMethodInsideNamespace(MethodDefinition m, string targetNamespace,
            out MethodReference methodRef)
        {
            methodRef = null;
            foreach (var instruction in m.Body.Instructions)
            {
                if (Utility.IsCallMethodInsideNamespace(instruction, targetNamespace, out methodRef))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsDelegateMethod(TypeDefinition type, MethodDefinition m)
        {
            var isDelegateMethod = false;
            if (m.Name.StartsWith("<") && m.Name.Contains(">") && m.Name.Contains("_"))
            {
                foreach (var m2 in type.Methods)
                {
                    if (!m2.Name.StartsWith("<") && !m2.Name.Contains(">") && m.Name.Contains(m2.Name))
                    {
                        isDelegateMethod = true;
                        break;
                    }
                }
            }
            return isDelegateMethod;
        }

        public Function CreatePapyrusFunction(
            Assembly asm,
            TypeDefinition type,
            MethodDefinition method,
            bool mergeConstructorAndOnInit = false,
            bool onInitAvailable = false,
            bool hasConstructor = false)
        {
            string overrideFunctionName = null;
            if (!onInitAvailable && method.IsConstructor)
            {
                overrideFunctionName = "OnInit";
                hasConstructor = false;
            }

            if (method.IsConstructor && mergeConstructorAndOnInit)
                method.Name = "__ctor";

            var functionWriter = new PapyrusAsmWriter(Program.CurrentAssembly, type, PapyrusAsmWriter.Fields);

            var function = functionWriter.CreateFunction(asm, method, overrideFunctionName);

            if (!method.IsConstructor && method.Name.ToLower() == "oninit" && hasConstructor)
            {
                function.InsertCodeInstruction(0, "CallMethod __ctor self ::NoneVar");
            }

            return function;
        }
    }
}