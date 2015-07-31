using System;
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
        }

        public PapyrusAssembly(Mono.Cecil.TypeDefinition type)
        {
            // TODO: Complete member initialization
            this.Header = new PapyrusHeader();

            this.sourceType = type;
            this.BaseType = Utility.GetPapyrusBaseType(type);
            this.ObjectTable = CreateObjectTable(type);
            this.OutputName = this.BaseType;
            // this.AssemblyCode = this.ObjectTable.ToString();
        }

        public PapyrusObjectTable CreateObjectTable(TypeDefinition type)
        {
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

            foreach (var variable in type.Fields)
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

                foreach (var method in methods)
                {
                    defaultObjectState.Functions.Add(CreatePapyrusFunction(type, method, mergeCtorAndOnInit, onInitAvailable, ctorAvailable));
                }

                table.StateTable.Add(defaultObjectState);
            }
            return table;
        }

        public PapyrusFunction CreatePapyrusFunction(
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

            var function = functionWriter.CreateFunction(method, overrideFunctionName);

            if (!method.IsConstructor && method.Name.ToLower() == "oninit" && hasConstructor)
            {
                function.InsertCodeInstruction(0, "CallMethod __ctor self ::NoneVar");
            }

            return function;
        }

    }
}
