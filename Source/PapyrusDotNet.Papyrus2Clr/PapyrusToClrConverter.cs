using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.Converters.Papyrus2Clr.Base;
using PapyrusDotNet.Converters.Papyrus2Clr.Implementations;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Classes;

namespace PapyrusDotNet.Converters.Papyrus2Clr
{
    public class PapyrusToClrConverter : PapyrusToClrConverterBase
    {
        public IList<TypeReference> AddedTypeReferences = new List<TypeReference>();
        public IList<string> ReservedTypeNames = new List<string>();
        private AssemblyDefinition clrAssembly;
        // private PapyrusAssemblyDefinition papyrusAssembly;
        private ModuleDefinition mainModule;

        protected override ClrAssemblyOutput ConvertAssembly(PapyrusAssemblyInput input)
        {
            var name = "Core";
            clrAssembly = AssemblyDefinition.CreateAssembly(
                new AssemblyNameDefinition(NamespaceResolver.Resolve(name),
                    new Version(2, 0)), name, ModuleKind.Dll);

            mainModule = clrAssembly.MainModule;

            foreach (var inputAssembly in input.Assemblies)
            {
                AddAssemblyReferences(inputAssembly);
            }
            foreach (var papyrusAssembly in input.Assemblies)
            {
                foreach (var type in papyrusAssembly.Types)
                {
                    mainModule.Types.Add(ResolveTypeDefinition(type.Name, type));
                }
            }
            return new ClrAssemblyOutput(clrAssembly);
        }

        private void AddAssemblyReferences(PapyrusAssemblyDefinition papyrusAssembly)
        {
            foreach (var type in papyrusAssembly.Types)
            {
                var typeRef = ResolveTypeReference(null, type.Name);
                if (!AddedTypeReferences.Contains(typeRef))
                    AddedTypeReferences.Add(typeRef);
                foreach (var nestedType in type.NestedTypes)
                {
                    var nestedTypeRef = ResolveTypeReference(null, nestedType.Name);
                    if (!AddedTypeReferences.Contains(nestedTypeRef))
                        AddedTypeReferences.Add(nestedTypeRef);
                }
            }
        }

        private TypeDefinition ResolveTypeDefinition(string name, PapyrusTypeDefinition type)
        {
            var newType = new TypeDefinition(NamespaceResolver.Resolve(name), name,
                TypeAttributes.Public | TypeAttributes.Class);

            AddEmptyConstructor(newType);

            if (!string.IsNullOrEmpty(type.BaseClass))
            {
                var baseType = ResolveTypeReference(null, type.BaseClass);
                if (baseType != null)
                {
                    newType.BaseType = baseType;
                }
            }

            foreach (var prop in type.Properties)
            {
                var typeRef = ResolveTypeReference(null, prop.TypeName);

                var propDef = new PropertyDefinition(prop.Name, PropertyAttributes.None, typeRef);
                newType.Properties.Add(propDef);
            }

            foreach (var field in type.Fields)
            {
                var fieldType = field.FieldType;
                var typeName = fieldType.Name;
                var typeRef = ResolveTypeReference(null, typeName);

                var attributes = FieldAttributes.Public;

                if (field.IsConst)
                {
                    attributes |= FieldAttributes.InitOnly;
                }

                var fieldDef = new FieldDefinition(field.Name.Replace("::", ""), attributes, typeRef);
                newType.Fields.Add(fieldDef);
            }

            foreach (var structure in type.NestedTypes)
            {
                newType.NestedTypes.Add(ResolveTypeDefinition(structure.Name, structure));
            }

            foreach (var state in type.States)
            {
                foreach (var method in state.Methods)
                {
                    var typeRef = ResolveTypeReference(null, method.ReturnTypeName);
                    var attributes = MethodAttributes.Public;

                    if (method.IsGlobal || method.IsNative)
                    {
                        attributes |= MethodAttributes.Static;
                    }
                    else if (method.IsEvent)
                    {
                        attributes |= MethodAttributes.Virtual;
                    }

                    var methodDef = new MethodDefinition(method.Name, attributes, typeRef);
                    methodDef.IsNative = method.IsNative;
                    foreach (var param in method.Parameters)
                    {
                        var paramTypeRef = ResolveTypeReference(null, param.TypeName);
                        var paramDef = new ParameterDefinition(param.Name, ParameterAttributes.None, paramTypeRef);
                        methodDef.Parameters.Add(paramDef);
                    }

                    CreateEmptyFunctionBody(ref methodDef);
                    newType.Methods.Add(methodDef);
                }
            }
            return newType;
        }

        public void AddEmptyConstructor(TypeDefinition type)
        {
            var method = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig |
                                                       MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, mainModule.TypeSystem.Void);

            //TODO: might need to fix this later so that PEVERIFY can verify the outputted library properly.
            // var baseEmptyConstructor = new MethodReference(".ctor", MainModule.TypeSystem.Void, MainModule.TypeSystem.Object);// MainModule.TypeSystem.Object
            // method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            // method.Body.Instructions.Add(Instruction.Create(OpCodes.Call, baseEmptyConstructor));


            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            type.Methods.Add(method);
        }
        public void CreateEmptyFunctionBody(ref MethodDefinition function)
        {
            if (function.Body == null)
            {
                function.Body = new MethodBody(function);
            }
            var fnl = function.ReturnType.FullName.ToLower();
            if (fnl.Equals("system.void"))
            {
                // Do nothing	
            }

            else if (fnl.Contains("[]"))
            {
                function.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
            }

            else if (fnl.StartsWith("system.string") || fnl.StartsWith("system.object") ||
                     fnl.StartsWith("papyrusdotnet.core"))
                function.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));

            else if (fnl.StartsWith("system.int") || fnl.StartsWith("system.bool") || fnl.StartsWith("system.long") ||
                     fnl.StartsWith("system.byte") || fnl.StartsWith("system.short"))
            {
                function.Body.Instructions.Add(fnl.StartsWith("system.long")
                    ? Instruction.Create(OpCodes.Ldc_I8, 0L)
                    : Instruction.Create(OpCodes.Ldc_I4_0));
            }

            else if (fnl.StartsWith("system.float"))
                function.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_R4, 0f));

            else if (fnl.StartsWith("system.double"))
                function.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_R8, 0d));


            function.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        public TypeReference ResolveTypeReference(TypeDefinition newType, string targetTypeName = null)
        {
            return TypeReferenceResolver.Resolve(ref ReservedTypeNames, ref AddedTypeReferences, mainModule, newType, targetTypeName);
        }

        public PapyrusToClrConverter(INamespaceResolver namespaceResolver, ITypeReferenceResolver typeReferenceResolver) : base(namespaceResolver, typeReferenceResolver)
        {
        }
    }
}
