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
using PapyrusDotNet.PapyrusAssembly.Extensions;
using EventAttributes = Mono.Cecil.EventAttributes;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using MethodBody = Mono.Cecil.Cil.MethodBody;
using OpCodes = Mono.Cecil.Cil.OpCodes;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;
using PropertyAttributes = Mono.Cecil.PropertyAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

#endregion

namespace PapyrusDotNet.Converters.Papyrus2Clr
{
    public class Papyrus2ClrCecilConverter : Papyrus2ClrCecilConverterBase
    {
        private readonly IUiRenderer uiRenderer;
        private readonly ITypeNameResolver nameConventionResolver;
        private AssemblyDefinition clrAssembly;
        // private PapyrusAssemblyDefinition papyrusAssembly;
        private ModuleDefinition mainModule;
        public IList<string> ReservedTypeNames = new List<string>();
        public IList<TypeReference> AddedTypeReferences = new List<TypeReference>();
        private TypeReference objectType;

        public Papyrus2ClrCecilConverter(IUiRenderer uiRenderer, INameConvetionResolver nameConventionResolver,
            INamespaceResolver namespaceResolver, ITypeReferenceResolver typeReferenceResolver)
            : base(namespaceResolver, typeReferenceResolver)
        {
            this.uiRenderer = uiRenderer;
            this.nameConventionResolver = nameConventionResolver;
        }

        protected override CecilAssemblyOutput ConvertAssembly(PapyrusAssemblyInput input)
        {
            var exeAsm = Assembly.GetExecutingAssembly();
            var name = "Core";
            clrAssembly = AssemblyDefinition.CreateAssembly(
                new AssemblyNameDefinition(NamespaceResolver.Resolve(name),
                    new Version(2, 0)), NamespaceResolver.Resolve(name), ModuleKind.Dll);

            mainModule = clrAssembly.MainModule;
            objectType = mainModule.Import(typeof(object));

            int i = 1, ij = 0;

            uiRenderer.DrawInterface("(2/3) Adding assembly references.");
            foreach (var inputAssembly in input.Assemblies)
            {
                var redrawProgress = ij >= 100 || i == input.Assemblies.Length || input.Assemblies.Length < 500;
                if (redrawProgress)
                {
                    //Console.SetCursorPosition(x, y);
                    //Console.WriteLine("Adding assembly references... " + i + "/" + input.Assemblies.Length);

                    uiRenderer.DrawProgressBarWithInfo(i, input.Assemblies.Length);
                    ij = 0;
                }
                ij++;
                i++;
                AddAssemblyReferences(inputAssembly);
            }
            i = 1; ij = 0;

            //Console.SetCursorPosition(0, y + 1);
            uiRenderer.DrawInterface("(3/3) Creating CLR types.");

            foreach (var papyrusAssembly in input.Assemblies)
            {
                foreach (var type in papyrusAssembly.Types)
                {
                    var redrawProgress = ij >= 100 || i == input.Assemblies.Length || input.Assemblies.Length < 500;
                    if (redrawProgress)
                    {
                        //Console.SetCursorPosition(0, y + 1);
                        //Console.WriteLine("Building Classes... " + i + "/" + input.Assemblies.Length);
                        uiRenderer.DrawProgressBarWithInfo(i, input.Assemblies.Length);
                        ij = 0;
                    }
                    ij++;
                    i++;
                    AddTypeDefinition(null, type.Name, type, false);

                }
            }

            exeAsm.FindTypes("attribute")
                .ForEach(attr => ImportType(mainModule, attr));

            uiRenderer.DrawResult("Building Core Library Completed.");



            return new CecilAssemblyOutput(clrAssembly);
        }


        /// <summary>
        ///     Imports the Type specified to the target module
        /// </summary>
        /// <param name="module"></param>
        /// <param name="typeToImport"></param>
        public void ImportType(ModuleDefinition module, Type typeToImport)
        {
            try
            {
                var reference = module.Import(typeToImport);

                var definition = reference.Resolve();

                var newType = new TypeDefinition("PapyrusDotNet.Core", definition.Name,
                    TypeAttributes.Class)
                {
                    IsPublic = true,
                    BaseType = definition.BaseType
                };

                module.Types.Add(newType);

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
                    module.Import(constructor);

                if (definition.BaseType != null)
                {
                    try
                    {
                        var baseDef = definition.BaseType.Resolve();
                        constructor = baseDef.Methods.FirstOrDefault(m => m.IsConstructor);
                        if (constructor != null)
                            module.Import(constructor);
                    }
                    catch
                    {
                        // ignored
                    }

                }

                // if (constructor != null && !constructor.HasParameters)
                AddEmptyConstructor(newType);

                foreach (var method in definition.Methods)
                {
                    if (method.IsConstructor && !method.HasParameters) continue;

                    var newField = new MethodDefinition(method.Name, method.Attributes, method.ReturnType);

                    module.Import(method);

                    foreach (var fp in method.Parameters)
                    {
                        if (fp.Name.Contains("<"))
                        {
                        }
                        var newParam = new ParameterDefinition(fp.Name, fp.Attributes, fp.ParameterType);
                        newField.Parameters.Add(newParam);
                    }
                    CreateEmptyFunctionBody(ref newField);
                    newType.Methods.Add(newField);
                }

            }
            catch
            {
            }
        }

        private void AddAssemblyReferences(PapyrusAssemblyDefinition papyrusAssembly)
        {
            // Add types
            foreach (var type in papyrusAssembly.Types)
            {
                type.Name.Value = nameConventionResolver.Resolve(type.Name.Value);
                var typeRef = ResolveTypeReference(null, type.Name);
                if (!AddedTypeReferences.Contains(typeRef))
                    AddedTypeReferences.Add(typeRef);
                // Add structs
                foreach (var nestedType in type.NestedTypes)
                {
                    nestedType.Name.Value = nameConventionResolver.Resolve(nestedType.Name.Value);
                    var nestedTypeRef = ResolveTypeReference(null, nestedType.Name);
                    if (!AddedTypeReferences.Contains(nestedTypeRef))
                        AddedTypeReferences.Add(nestedTypeRef);
                }
            }
        }

        private void AddTypeDefinition(TypeDefinition owningType, PapyrusStringRef name, PapyrusTypeDefinition type, bool isNested)
        {
            AddTypeDefinition(owningType, name.Value, type, isNested);
        }

        private void AddTypeDefinition(TypeDefinition owningType, string name, PapyrusTypeDefinition type, bool isNested)
        {
            if (mainModule.Types.Any(t => t.Name.ToLower() == name.ToLower()))
            {
                // Type already exists? Don't do anything.
                return;
            }

            var newType = new TypeDefinition(NamespaceResolver.Resolve(name), name,
                isNested
                ? TypeAttributes.NestedPublic | TypeAttributes.SequentialLayout | TypeAttributes.BeforeFieldInit | TypeAttributes.Sealed
                : TypeAttributes.Public | TypeAttributes.Class);

            if (isNested)
            {
                newType.IsClass = false;
                newType.BaseType = mainModule.Import(typeof(System.ValueType));

                if (owningType.NestedTypes.Any(t => t.Name.ToLower() == name.ToLower()))
                {
                    // Structure already exists? Don't do anything.
                    return;
                }
                owningType.NestedTypes.Add(newType);
            }
            else
                mainModule.Types.Add(newType);

            AddEmptyConstructor(newType);

            if (!isNested)
            {
                if (!string.IsNullOrEmpty(type.BaseTypeName?.Value))
                {
                    var baseType = ResolveTypeReference(null, type.BaseTypeName.Value);
                    newType.BaseType = baseType ?? objectType;
                }
                else
                {
                    newType.BaseType = objectType;
                }
            }

            foreach (var field in type.Fields)
            {
                var fieldType = field.DefaultValue;
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

                //if (field.IsConst)
                //{
                //    attributes |= FieldAttributes.InitOnly;
                //}

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

                newType.AddProperty(nameConventionResolver.Resolve(prop.Name.Value), typeRef, targetField);
            }


            foreach (var structure in type.NestedTypes)
            {
                AddTypeDefinition(newType, structure.Name, structure, true);
            }

            foreach (var state in type.States)
            {
                foreach (var method in state.Methods)
                {
                    method.Name.Value = nameConventionResolver.Resolve(method.Name.Value);

                    var typeRef = ResolveTypeReference(null, method.ReturnTypeName);
                    var attributes = MethodAttributes.Public;

                    if (method.IsGlobal /* || method.IsNative */)
                    {
                        attributes |= MethodAttributes.Static;
                    }
                    else if (method.IsEvent)
                    {
                        attributes |= MethodAttributes.Virtual;
                        attributes |= MethodAttributes.NewSlot;
                    }


                    var methodDef = new MethodDefinition(method.Name.Value, attributes, typeRef);

                    // methodDef.IsNative = method.IsNative;
                    foreach (var param in method.Parameters)
                    {
                        var paramTypeRef = ResolveTypeReference(null, param.TypeName);
                        var paramDef = new ParameterDefinition(param.Name.Value, ParameterAttributes.None, paramTypeRef);
                        methodDef.Parameters.Add(paramDef);
                    }

                    var existingMethod =
                        newType.Methods.Any(m => m.Name == methodDef.Name
                                                 && methodDef.ReturnType == typeRef
                                                 && methodDef.Parameters.Count == m.Parameters.Count
                            );

                    if (!existingMethod)
                    {
                        CreateEmptyFunctionBody(ref methodDef);
                        newType.Methods.Add(methodDef);
                    }
                }
            }
            // return newType;
        }

        //private MethodDefinition CreatePropertyGetMethod(PapyrusPropertyDefinition prop, FieldReference field, TypeReference typeRef)
        //{
        //    var get = new MethodDefinition("get_" + prop.Name.Value,
        //        MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, typeRef);
        //    var processor = get.Body.GetILProcessor();


        //    if (field != null)
        //    {
        //        get.Body.Instructions.Add(processor.Create(OpCodes.Ldarg_0));
        //        get.Body.Instructions.Add(processor.Create(OpCodes.Ldfld, field));
        //    }
        //    else
        //    if (typeRef.IsValueType)
        //    {
        //        var var = GetMonoType(typeRef);

        //        var opcode = GetDefaultPrimitiveValueOpCode(var);

        //        get.Body.Instructions.Add(processor.Create(OpCodes.Ldc_I4_0));
        //        get.Body.Instructions.Add(processor.Create(OpCodes.Box, typeRef));

        //        //if (opcode == OpCodes.Ldstr)
        //        //    get.Body.Instructions.Add(processor.Create(opcode, "Hello World"));
        //        //else
        //        //    get.Body.Instructions.Add(processor.Create(opcode));
        //    }
        //    else
        //    {
        //        get.Body.Instructions.Add(processor.Create(OpCodes.Ldnull));
        //    }
        //    //if (field != null)
        //    //{
        //    //    get.Body.Instructions.Add(processor.Create(OpCodes.Ldfld, field));
        //    //}

        //    get.Body.Instructions.Add(processor.Create(OpCodes.Ret));
        //    get.SemanticsAttributes = MethodSemanticsAttributes.Getter;
        //    return get;
        //}

        public static Type GetMonoType(TypeReference type)
        {
            return Type.GetType(GetReflectionName(type), true);
        }

        private static string GetReflectionName(TypeReference type)
        {
            if (type.IsGenericInstance)
            {
                var genericInstance = (GenericInstanceType)type;
                return string.Format("{0}.{1}[{2}]", genericInstance.Namespace, type.Name, String.Join(",", genericInstance.GenericArguments.Select(GetReflectionName).ToArray()));
            }
            return type.FullName;
        }

        private OpCode GetDefaultPrimitiveValueOpCode(Type returnType)
        {
            if (returnType == typeof(float) || returnType == typeof(int) || returnType == typeof(bool))
                return OpCodes.Ldc_I4_0;
            if (returnType == typeof(string) || returnType == typeof(char))
                return OpCodes.Ldstr;

            return OpCodes.Ldc_I4_0;
        }


        //private MethodDefinition CreatePropertySetMethod(PapyrusPropertyDefinition prop, FieldReference field, TypeReference typeRef)
        //{

        //    var voidRef = mainModule.Import(typeof(void));
        //    var set = new MethodDefinition("set_" + prop.Name.Value,
        //        MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, voidRef);
        //    var processor = set.Body.GetILProcessor();
        //    set.Body.Instructions.Add(processor.Create(OpCodes.Ldarg_0));
        //    if (field != null)
        //    {
        //        set.Body.Instructions.Add(processor.Create(OpCodes.Ldarg_1));
        //        set.Body.Instructions.Add(processor.Create(OpCodes.Stfld, field));
        //    }
        //    set.Body.Instructions.Add(processor.Create(OpCodes.Ret));
        //    set.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, typeRef));
        //    set.SemanticsAttributes = MethodSemanticsAttributes.Setter;
        //    return set;
        //}

        MethodReference objectCtor = null;
        public void AddEmptyConstructor(TypeDefinition type)
        {
            type.AddDefaultConstructor();


            //var method = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig |
            //                                           MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
            //    mainModule.TypeSystem.Void);

            ////TODO: might need to fix this later so that PEVERIFY can verify the outputted library properly.
            //// var baseEmptyConstructor = new MethodReference(".ctor", MainModule.TypeSystem.Void, MainModule.TypeSystem.Object);// MainModule.TypeSystem.Object
            //method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));

            //if (objectTypeDef != null && objectCtor == null)
            //{
            //    var ctor = objectTypeDef.GetConstructors().FirstOrDefault();
            //    if (ctor != null)
            //        objectCtor = mainModule.Import(ctor);
            //}

            //if (objectCtor != null)
            //    method.Body.Instructions.Add(Instruction.Create(OpCodes.Call, objectCtor));

            //method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            //type.Methods.Add(method);
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
                function.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                return;
            }

            if (fnl.Contains("[]"))
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