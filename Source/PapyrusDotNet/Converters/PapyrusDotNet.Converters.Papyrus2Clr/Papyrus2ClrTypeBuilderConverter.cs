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
//     Copyright 2016, Karl Patrik Johansson, zerratar@gmail.com

#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.Converters.Papyrus2Clr.Base;
using PapyrusDotNet.Converters.Papyrus2Clr.Implementations;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Converters.Papyrus2Clr
{
    public class Papyrus2ClrTypeBuilderConverter : Papyrus2ClrTypeBuilderConverterBase
    {
        private readonly string outputDirectory;
        private AssemblyBuilder assembly;
        public IList<TypeBuilder> DefinedTypes = new List<TypeBuilder>();
        private ModuleBuilder mainModule;
        public IList<Type> ReferenceTypes = new List<Type>();

        public IList<string> ReservedTypeNames = new List<string>();

        public Papyrus2ClrTypeBuilderConverter(string outputDirectory, INamespaceResolver namespaceResolver,
            ITypeReferenceResolver typeReferenceResolver)
            : base(namespaceResolver, typeReferenceResolver)
        {
            this.outputDirectory = outputDirectory;
        }

        protected override ClrAssemblyOutput ConvertAssembly(PapyrusAssemblyInput input)
        {
            var exeAsm = Assembly.GetExecutingAssembly();
            var assemblyName = NamespaceResolver.Resolve("Core");
            assembly = Thread.GetDomain().DefineDynamicAssembly(
                new AssemblyName(assemblyName),
                AssemblyBuilderAccess.RunAndSave, outputDirectory);

            mainModule = assembly.DefineDynamicModule(assemblyName, assemblyName + ".dll");

            var i = 1;
            var x = Console.CursorLeft;
            var y = Console.CursorTop;
            foreach (var inputAssembly in input.Assemblies)
            {
                Console.SetCursorPosition(x, y);
                Console.WriteLine("Adding assembly references... " + i++ + "/" + input.Assemblies.Length);
                PrepareTypes(inputAssembly);
            }

            foreach (var inputAssembly in input.Assemblies)
            {
                foreach (var ptype in inputAssembly.Types)
                {
                    foreach (var type in DefinedTypes)
                    {
                        var targetBaseType =
                            ReferenceTypes.FirstOrDefault(t => t.Name.ToLower() == ptype.BaseTypeName.Value.ToLower());

                        if (targetBaseType != null)
                            type.SetParent(targetBaseType);
                    }
                }
            }


            i = 1;
            Console.SetCursorPosition(0, y + 1);
            foreach (var papyrusAssembly in input.Assemblies)
            {
                foreach (var type in papyrusAssembly.Types)
                {
                    Console.SetCursorPosition(0, y + 1);
                    Console.WriteLine("Building Classes... " + i++ + "/" + input.Assemblies.Length);
                    var definedType = DefinedTypes.FirstOrDefault(n => n.Name == type.Name.Value);
                    if (definedType == null)
                        definedType = mainModule.DefineType(type.Name.Value,
                            TypeAttributes.Public);
                    BuildType(definedType, type);
                }
            }

            foreach (var t in DefinedTypes)
            {
                t.CreateType();
            }

            //exeAsm.FindTypes("attribute")
            //    .ForEach(attr => ImportType(mainModule, attr));

            Console.WriteLine("Process Completed.");

            return new ClrAssemblyOutput(assembly);
        }

        private void PrepareTypes(PapyrusAssemblyDefinition inputAssembly)
        {
            foreach (var type in inputAssembly.Types)
            {
                if (DefinedTypes.All(t => t.Name != type.Name.Value))
                {
                    // NamespaceResolver.Resolve(type.Name.Value) + "." +
                    var defType = mainModule.DefineType(type.Name.Value,
                        TypeAttributes.Public);


                    DefinedTypes.Add(defType);
                    ReferenceTypes.Add(defType);
                }
            }
        }

        private void BuildType(TypeBuilder definedType, PapyrusTypeDefinition type)
        {
            //definedType.BaseType = DefinedTypes[0].DeclaringType


            var myCtorBuilder = definedType.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.HasThis, Type.EmptyTypes);

            BuildConstructor(myCtorBuilder.GetILGenerator());


            foreach (var f in type.Fields)
            {
                //    Type[] paramTypes = GetParameterTypes(m.Parameters);
                //    Type returnType = GetReturnType(m.ReturnTypeName.Value);
                //    MethodAttributes accessAttributes = GetMethodAttributes(m);

                //    var method = definedType.DefineMethod(m.Name.Value, accessAttributes, CallingConventions.Standard, returnType, paramTypes);
                //    CreateEmptyBody(method.GetILGenerator(), returnType);
            }

            foreach (var p in type.Properties)
            {
                //    Type[] paramTypes = GetParameterTypes(m.Parameters);
                //    Type returnType = GetReturnType(m.ReturnTypeName.Value);
                //    MethodAttributes accessAttributes = GetMethodAttributes(m);

                //    var method = definedType.DefineMethod(m.Name.Value, accessAttributes, CallingConventions.Standard, returnType, paramTypes);
                //    CreateEmptyBody(method.GetILGenerator(), returnType);
            }

            // Structs
            foreach (var t in type.NestedTypes)
            {
            }

            foreach (var state in type.States)
            {
                foreach (var m in state.Methods)
                {
                    var paramTypes = GetParameterTypes(m.Parameters);
                    var returnType = GetType(m.ReturnTypeName.Value);

                    var callingConvention = CallingConventions.Standard;
                    if (m.IsGlobal || m.IsNative)
                        callingConvention = CallingConventions.HasThis;

                    var method = definedType.DefineMethod(m.Name.Value, MethodAttributes.Public, callingConvention,
                        returnType, paramTypes);

                    //var pi = 1;
                    //foreach (var p in m.Parameters)
                    //{
                    //    method.DefineParameter(pi++, ParameterAttributes.None, p.Name.Value);
                    //}

                    if (callingConvention == CallingConventions.HasThis)
                        method.GetILGenerator().Emit(OpCodes.Ldarg_0);

                    CreateEmptyBody(method.GetILGenerator(), returnType);
                }
            }
        }

        private void BuildConstructor(ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);

            var objType = typeof (object);

            var objCtor = objType.GetConstructor(new Type[] {});

            il.Emit(OpCodes.Call, objCtor);

            il.Emit(OpCodes.Ret);
        }

        private void CreateEmptyBody(ILGenerator il, Type returnType)
        {
            if (returnType == typeof (void))
            {
                //il.Emit(OpCodes.Ret);
                il.Emit(OpCodes.Ret);
                return;
            }
            if (!returnType.IsPrimitive)
            {
                il.Emit(OpCodes.Ldnull);
            }
            else
            {
                if (returnType == typeof (string))
                {
                    il.Emit(GetDefaultPrimitiveValueOpCode(returnType), "Hello World");
                }
                else
                {
                    il.Emit(GetDefaultPrimitiveValueOpCode(returnType));
                }
            }

            il.Emit(OpCodes.Ret);
        }

        private OpCode GetDefaultPrimitiveValueOpCode(Type returnType)
        {
            if (returnType == typeof (float) || returnType == typeof (int) || returnType == typeof (bool))
                return OpCodes.Ldc_I4_0;
            if (returnType == typeof (string) || returnType == typeof (char))
                return OpCodes.Ldstr;

            return OpCodes.Ldc_I4_0;
        }

        private MethodAttributes GetMethodAttributes(PapyrusMethodDefinition m)
        {
            var attrs = MethodAttributes.Public;

            if (m.IsNative || m.IsGlobal)
                attrs |= MethodAttributes.Static;

            return attrs;
        }

        private Type GetType(string typeName)
        {
            var papyrusType = ReferenceTypes.FirstOrDefault(t => t.Name.ToLower() == typeName.ToLower());
            if (papyrusType != null)
            {
                return papyrusType;
            }

            switch (typeName.ToLower())
            {
                case "none":
                    return typeof (void);
                case "int":
                    return typeof (int);
                case "float":
                    return typeof (float);
                case "boolean":
                case "bool":
                    return typeof (bool);
                case "string":
                    return typeof (string);
            }

            return typeof (object);
        }

        private Type[] GetParameterTypes(IEnumerable<PapyrusParameterDefinition> parameters)
        {
            return parameters.Select(t => GetType(t.TypeName.Value)).ToArray();
        }

        //public TypeReference ResolveTypeReference(TypeDefinition newType, PapyrusStringRef targetTypeName = null)
        //}
        //    }
        //        }
        //                AddedTypeReferences.Add(nestedTypeRef);
        //            if (!AddedTypeReferences.Contains(nestedTypeRef))
        //            var nestedTypeRef = ResolveTypeReference(null, nestedType.Name);
        //        {
        //        foreach (var nestedType in type.NestedTypes)
        //            AddedTypeReferences.Add(typeRef);
        //        if (!AddedTypeReferences.Contains(typeRef))
        //        var typeRef = ResolveTypeReference(null, type.Name);
        //    {
        //    foreach (var type in inputAssembly.Types)
        //{

        //private void AddAssemblyReferences(PapyrusAssemblyDefinition inputAssembly)
        //{
        //    return ResolveTypeReference(newType, targetTypeName?.Value);
        //}

        //public TypeReference ResolveTypeReference(TypeDefinition newType, string targetTypeName = null)
        //{
        //    return TypeReferenceResolver.Resolve(ref ReservedTypeNames, ref AddedTypeReferences, mainModule, newType,
        //        targetTypeName);
        //}
    }
}