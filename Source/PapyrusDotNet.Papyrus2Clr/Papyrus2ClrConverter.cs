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
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PapyrusDotNet.Common.Extensions;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.Converters.Papyrus2Clr.Base;
using PapyrusDotNet.Converters.Papyrus2Clr.Implementations;
using PapyrusDotNet.PapyrusAssembly;
using EventAttributes = Mono.Cecil.EventAttributes;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using MethodBody = Mono.Cecil.Cil.MethodBody;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;
using PropertyAttributes = Mono.Cecil.PropertyAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

#endregion

namespace PapyrusDotNet.Converters.Papyrus2Clr
{
    public class Papyrus2ClrConverter : Papyrus2ClrConverterBase
    {
        public IList<TypeReference> AddedTypeReferences = new List<TypeReference>();
        private AssemblyDefinition clrAssembly;
        // private PapyrusAssemblyDefinition papyrusAssembly;
        private ModuleDefinition mainModule;
        public IList<string> ReservedTypeNames = new List<string>();

        public Papyrus2ClrConverter(INamespaceResolver namespaceResolver, ITypeReferenceResolver typeReferenceResolver)
            : base(namespaceResolver, typeReferenceResolver)
        {
        }

        protected override ClrAssemblyOutput ConvertAssembly(PapyrusAssemblyInput input)
        {
            var exeAsm = Assembly.GetExecutingAssembly();
            var name = "Core";
            clrAssembly = AssemblyDefinition.CreateAssembly(
                new AssemblyNameDefinition(NamespaceResolver.Resolve(name),
                    new Version(2, 0)), name, ModuleKind.Dll);

            mainModule = clrAssembly.MainModule;

            int i = 0;
            var x = Console.CursorLeft;
            var y = Console.CursorTop;
            foreach (var inputAssembly in input.Assemblies)
            {
                Console.SetCursorPosition(x, y);
                Console.WriteLine("Adding assembly references... " + i++ + "/" + input.Assemblies.Length);
                AddAssemblyReferences(inputAssembly);
            }
            i = 0;
            Console.SetCursorPosition(0, y + 1);
            foreach (var papyrusAssembly in input.Assemblies)
            {
                foreach (var type in papyrusAssembly.Types)
                {
                    Console.SetCursorPosition(0, y + 1);
                    Console.WriteLine("Building Classes... " + i++ + "/" + input.Assemblies.Length);
                    mainModule.Types.Add(ResolveTypeDefinition(type.Name, type));
                }
            }

            exeAsm.FindTypes("attribute")
                .ForEach(attr => ImportType(mainModule, attr));

            Console.WriteLine("Process Completed.");

            return new ClrAssemblyOutput(clrAssembly);
        }

        /// <summary>
        ///     Imports the Type specified to the target module
        /// </summary>
        /// <param name="mainModule"></param>
        /// <param name="typeToImport"></param>
        public void ImportType(ModuleDefinition mainModule, Type typeToImport)
        {
            try
            {
                var reference = mainModule.Import(typeToImport);

                var definition = reference.Resolve();

                var newType = new TypeDefinition("PapyrusDotNet.Core", definition.Name,
                    TypeAttributes.Class)
                {
                    IsPublic = true,
                    BaseType = definition.BaseType
                };

                foreach (var field in definition.Fields)
                {
                    var newField = new FieldDefinition(field.Name, FieldAttributes.Public, field.FieldType);
                    newType.Fields.Add(newField);
                }

                foreach (var field in definition.Events)
                {
                    var newField = new EventDefinition(field.Name, EventAttributes.None, field.EventType);
                    newType.Events.Add(newField);
                }

                var constructor = definition.Methods.FirstOrDefault(m => m.IsConstructor);
                if (constructor != null)
                    mainModule.Import(constructor);

                if (definition.BaseType != null)
                {
                    try
                    {
                        var baseDef = definition.BaseType.Resolve();
                        constructor = baseDef.Methods.FirstOrDefault(m => m.IsConstructor);
                        if (constructor != null)
                            mainModule.Import(constructor);
                    }
                    catch
                    {
                        // ignored
                    }
                }

                // if (constructor != null && !constructor.HasParameters)
                AddEmptyConstructor(newType);

                foreach (var field in definition.Methods)
                {
                    if (field.IsConstructor && !field.HasParameters) continue;

                    var newField = new MethodDefinition(field.Name, field.Attributes, field.ReturnType);

                    mainModule.Import(field);

                    foreach (var fp in field.Parameters)
                    {
                        if (fp.Name.Contains("<"))
                        {
                        }
                        var newParam = new ParameterDefinition(fp.Name, fp.Attributes, fp.ParameterType);
                        newField.Parameters.Add(newParam);
                    }
                    /*	
                        if (field.HasBody)
                        {
                            foreach (var inst in field.Body.Instructions)
                            {
                                if (inst.Operand is MethodReference)
                                    MainModule.Import(inst.Operand as MethodReference);
                                if (inst.Operand is FieldReference)
                                    MainModule.Import(inst.Operand as FieldReference);

                            
                                // newField.Body.Instructions.Add(inst);
                            }
                        }*/

                    CreateEmptyFunctionBody(ref newField);
                    newType.Methods.Add(newField);
                }

                mainModule.Types.Add(newType);
            }
            catch
            {
                // ignored
            }
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

        private TypeDefinition ResolveTypeDefinition(PapyrusStringRef name, PapyrusTypeDefinition type)
        {
            return ResolveTypeDefinition(name.Value, type);
        }

        private TypeDefinition ResolveTypeDefinition(string name, PapyrusTypeDefinition type)
        {
            var newType = new TypeDefinition(NamespaceResolver.Resolve(name), name,
                TypeAttributes.Public | TypeAttributes.Class);

            AddEmptyConstructor(newType);

            if (!string.IsNullOrEmpty(type.BaseTypeName?.Value))
            {
                var baseType = ResolveTypeReference(null, type.BaseTypeName.Value);
                if (baseType != null)
                {
                    newType.BaseType = baseType;
                }
            }

            foreach (var field in type.Fields)
            {
                var fieldType = field.FieldVariable;
                var typeName = fieldType.Name;
                var typeRef = ResolveTypeReference(null, typeName);

                var attributes = FieldAttributes.Public;

                if (field.Name.Value.ToLower().EndsWith("_var"))
                {
                    if (type.Properties.Any(
                            n => field.Name.Value.Contains('_') && n.Name.Value == field.Name.Value.Split('_')[0]
                            || n.AutoName == field.Name.Value))
                    {
                        attributes = FieldAttributes.Private;
                    }
                }

                if (field.IsConst)
                {
                    attributes |= FieldAttributes.InitOnly;
                }

                var fieldDef = new FieldDefinition(field.Name.Value.Replace("::", ""), attributes, typeRef);
                newType.Fields.Add(fieldDef);
            }

            foreach (var prop in type.Properties)
            {
                FieldDefinition targetField = null;
                foreach (var field in newType.Fields)
                {
                    if (!string.IsNullOrEmpty(prop.AutoName))
                    {
                        if (prop.AutoName.Contains(field.Name))
                        {
                            targetField = field;
                            break;
                        }
                    }
                    if (field.Name.ToLower().Contains(prop.Name.Value.ToLower() + "_var"))
                    {
                        targetField = field;
                        break;
                    }
                }

                var typeRef = ResolveTypeReference(null, prop.TypeName);

                var propDef = new PropertyDefinition(prop.Name.Value, PropertyAttributes.HasDefault, typeRef);
                propDef.SetMethod = CreatePropertySetMethod(prop, targetField, typeRef);
                propDef.GetMethod = CreatePropertyGetMethod(prop, targetField, typeRef);
                newType.Properties.Add(propDef);
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

                    var methodDef = new MethodDefinition(method.Name.Value, attributes, typeRef);
                    methodDef.IsNative = method.IsNative;
                    foreach (var param in method.Parameters)
                    {
                        var paramTypeRef = ResolveTypeReference(null, param.TypeName);
                        var paramDef = new ParameterDefinition(param.Name.Value, ParameterAttributes.None, paramTypeRef);
                        methodDef.Parameters.Add(paramDef);
                    }

                    CreateEmptyFunctionBody(ref methodDef);
                    newType.Methods.Add(methodDef);
                }
            }
            return newType;
        }

        private MethodDefinition CreatePropertyGetMethod(PapyrusPropertyDefinition prop, FieldReference field, TypeReference typeRef)
        {
            var get = new MethodDefinition("get_" + prop.Name.Value,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, typeRef);
            var processor = get.Body.GetILProcessor();
            get.Body.Instructions.Add(processor.Create(OpCodes.Ldarg_0));
            if (field != null)
            {
                get.Body.Instructions.Add(processor.Create(OpCodes.Ldfld, field));
            }
            else
            {
                get.Body.Instructions.Add(processor.Create(OpCodes.Ldnull));
            }
            get.Body.Instructions.Add(processor.Create(OpCodes.Ret));
            get.SemanticsAttributes = MethodSemanticsAttributes.Getter;
            return get;
        }

        private MethodDefinition CreatePropertySetMethod(PapyrusPropertyDefinition prop, FieldReference field, TypeReference typeRef)
        {

            var voidRef = mainModule.Import(typeof(void));
            var set = new MethodDefinition("set_" + prop.Name.Value,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, voidRef);
            var processor = set.Body.GetILProcessor();
            set.Body.Instructions.Add(processor.Create(OpCodes.Ldarg_0));
            if (field != null)
            {
                set.Body.Instructions.Add(processor.Create(OpCodes.Ldarg_1));
                set.Body.Instructions.Add(processor.Create(OpCodes.Stfld, field));
            }
            set.Body.Instructions.Add(processor.Create(OpCodes.Ret));
            set.Parameters.Add(new ParameterDefinition(typeRef));
            set.SemanticsAttributes = MethodSemanticsAttributes.Setter;
            return set;
        }

        public void AddEmptyConstructor(TypeDefinition type)
        {
            var method = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig |
                                                       MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                mainModule.TypeSystem.Void);

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

        public TypeReference ResolveTypeReference(TypeDefinition newType, PapyrusStringRef targetTypeName = null)
        {
            return ResolveTypeReference(newType, targetTypeName?.Value);
        }

        public TypeReference ResolveTypeReference(TypeDefinition newType, string targetTypeName = null)
        {
            return TypeReferenceResolver.Resolve(ref ReservedTypeNames, ref AddedTypeReferences, mainModule, newType,
                targetTypeName);
        }
    }
}