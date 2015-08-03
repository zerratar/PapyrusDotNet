using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PapyrusDotNet.Models
{
    using Mono.Cecil;

    using PapyrusDotNet.Common;

    public class PapyrusAssembly
    {
        private Mono.Cecil.TypeDefinition sourceType;

        public string GenericTypeReplacement;

        public bool IsEnum { get; set; }

        public PapyrusHeader Header { get; set; }

        public PapyrusObjectTable ObjectTable { get; set; }


        public string BaseType { get; set; }

        public string OutputName { get; set; }

        // public string AssemblyCode { get; set; }

        public PapyrusAssembly()
        {
            Header = new PapyrusHeader();

            this.DelegateMethodDefinitions = new List<MethodDefinition>();
        }

        public PapyrusAssembly(Mono.Cecil.TypeDefinition type, string genericTypeReplacement)
        {
            // TODO: Complete member initialization
            this.Header = new PapyrusHeader();
            this.sourceType = type;
            this.GenericTypeReplacement = genericTypeReplacement;
            this.BaseType = Utility.GetPapyrusBaseType(type);
            this.ObjectTable = CreateObjectTable(type);
            this.OutputName = this.BaseType;

            this.DelegateMethodDefinitions = new List<MethodDefinition>();
        }

        public PapyrusAssembly(Mono.Cecil.TypeDefinition type)
        {

            // TODO: Complete member initialization
            this.Header = new PapyrusHeader();

            this.sourceType = type;
            this.BaseType = Utility.GetPapyrusBaseType(type);
            this.ObjectTable = CreateObjectTable(type);
            this.OutputName = this.BaseType;

            this.DelegateMethodDefinitions = new List<MethodDefinition>();
            // this.AssemblyCode = this.ObjectTable.ToString();
        }


        public List<MethodDefinition> DelegateMethodDefinitions;

        public Dictionary<MethodDefinition, List<FieldDefinition>> DelegateMethodFieldPair;

        public List<FieldDefinition> DelegateFields;

        public PapyrusObjectTable CreateObjectTable(TypeDefinition type)
        {
            this.DelegateMethodDefinitions = new List<MethodDefinition>();
            this.DelegateMethodFieldPair = new Dictionary<MethodDefinition, List<FieldDefinition>>();
            this.DelegateFields = new List<FieldDefinition>();

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
                            m.Name = m.Name.Replace("<", "_").Replace(">", "_");
                            DelegateMethodDefinitions.Add(m);
                            var fieldDefinitions = new List<FieldDefinition>();
                            foreach (var instruction in m.Body.Instructions)
                            {
                                var op = instruction.Operand;
                                if (op is FieldReference)
                                {
                                    var f = (op as FieldReference);
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

            foreach (var m in type.Methods)
            {
                if (IsDelegateMethod(type, m))
                {
                    m.Name = m.Name.Replace("<", "_").Replace(">", "_");
                    DelegateMethodDefinitions.Add(m);
                }
            }


            // It is important to know if this object is an enum or not
            // as we will have to manage it differently than a normal class.
            // -------
            // To get Enums working, we can't generate a seperate class for the enum.
            // Instead, we will have to get all the fields for the enum and then inject these fields to any class
            // that uses the enum.
            this.IsEnum = type.IsEnum;

            var table = new PapyrusObjectTable();

            table.Name = Utility.GetPapyrusBaseType(type);
            table.BaseType = type.BaseType != null ? Utility.GetPapyrusBaseType(type.BaseType) : "";
            table.Info = Utility.GetFlagsAndProperties(type);


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

                if (this.IsEnum)
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

                var newVar = new PapyrusVariableReference("::" + variableName, variableType);

                newVar.Properties = varProps;

                table.VariableTable.Add(newVar);

                PapyrusAsmWriter.Fields.Add(newVar);
            }

            foreach (var propField in PapyrusAsmWriter.Fields.Where(f => f.Properties.IsProperty))
            {
                var propName = Utility.GetPropertyName(propField.Name);
                var fieldType = propField.TypeName;

                var prop = new PapyrusVariableReference(propName, fieldType);
                prop.AutoVarName = propField.Name;
                prop.Properties = propField.Properties;

                table.PropertyTable.Add(prop);
            }

            // Enums do not have any functions or states.
            if (!this.IsEnum)
            {
                var defaultObjectState = new PapyrusObjectState();

                var methods = type.Methods.OrderByDescending(c => c.Name).ToList();
                var onInitAvailable = methods.Any(m => m.Name.ToLower().Contains("oninit"));
                var ctorAvailable = methods.Any(m => m.Name.ToLower().Contains(".ctor"));
                var mergeCtorAndOnInit = ctorAvailable && onInitAvailable;

                foreach (var method in DelegateMethodDefinitions)
                {
                    if (!defaultObjectState.Functions.Any(c => c.Name == method.Name))
                        defaultObjectState.Functions.Add(CreatePapyrusFunction(this, type, method, mergeCtorAndOnInit, onInitAvailable, ctorAvailable));
                }

                foreach (var method in methods)
                {
                    if (!defaultObjectState.Functions.Any(c => c.Name == method.Name))
                        defaultObjectState.Functions.Add(CreatePapyrusFunction(this, type, method, mergeCtorAndOnInit, onInitAvailable, ctorAvailable));
                }

                table.StateTable.Add(defaultObjectState);
            }
            return table;
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

        public PapyrusFunction CreatePapyrusFunction(
            PapyrusAssembly asm,
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
