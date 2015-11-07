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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.CoreBuilder.Interfaces;
using PapyrusDotNet.CoreBuilder.Papyrus.Assembly;
using EventAttributes = Mono.Cecil.EventAttributes;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace PapyrusDotNet.CoreBuilder.Implementation
{
    /// <summary>
    ///     Used for building .NET Assemblies using Papyrus as input
    /// </summary>
    public abstract class PapyrusCilAssemblyBuilder : IPapyrusCilAssemblyBuilder
    {
        protected IAssemblyNameResolver AssemblyNameResolver;
        private int filesParsed;
        protected IPapyrusAssemblyParser PapyrusAssemblyParser;

        // public const string OutputFileName = "PapyrusDotNet.Core.dll";
        // public const string CoreNamespace = "PapyrusDotNet.Core";

        protected IPapyrusScriptParser PapyrusScriptParser;
        protected IStatusCallbackService StatusCallback;

        private int totalFilesToParse;
        protected IPapyrusTypeDefinitionResolver TypeDefinitionResolver;
        protected IPapyrusTypeReferenceResolver TypeReferenceResolver;

        protected PapyrusCilAssemblyBuilder()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="scriptParser"></param>
        /// <param name="assemblyParser"></param>
        /// <param name="typeDefinitionResolver"></param>
        /// <param name="typeReferenceResolver"></param>
        /// <param name="nameResolver"></param>
        /// <param name="callback"></param>
        protected PapyrusCilAssemblyBuilder(IPapyrusScriptParser scriptParser,
            IPapyrusAssemblyParser assemblyParser,
            IPapyrusTypeDefinitionResolver typeDefinitionResolver,
            IPapyrusTypeReferenceResolver typeReferenceResolver,
            IAssemblyNameResolver nameResolver,
            IStatusCallbackService callback)
        {
            AssemblyNameResolver = nameResolver;
            StatusCallback = callback;
            PapyrusScriptParser = scriptParser;
            PapyrusAssemblyParser = assemblyParser;
            TypeDefinitionResolver = typeDefinitionResolver;
            TypeReferenceResolver = typeReferenceResolver;
            TypeDefinitionResolver.Initialize(this);
            TypeReferenceResolver.Initialize(this);
        }

        public AssemblyDefinition CoreAssembly { get; set; }
        public ModuleDefinition MainModule { get; set; }

        public List<string> ReservedTypeNames { get; set; } = new List<string>();

        public List<TypeReference> AddedTypeReferences { get; set; } = new List<TypeReference>();

        /// <summary>
        ///     Builds a .NET Assembly using the input Papyrus Source
        /// </summary>
        /// <param name="inputSourceFiles"></param>
        public async void BuildAssembly(string[] inputSourceFiles)
        {
            // @"C:\The Elder Scrolls V Skyrim\Data\scripts\Source"
            var files = inputSourceFiles;

            var resolvedAssemblyName = AssemblyNameResolver.Resolve(null);
            CoreAssembly = AssemblyDefinition.CreateAssembly(
                resolvedAssemblyName,
                resolvedAssemblyName.Name, ModuleKind.Dll);

            MainModule = CoreAssembly.MainModule;

            var papyrusObjects = new List<PapyrusAssemblyObject>();

            StatusCallback.WriteLine("Parsing Papyrus... This usually takes about 30 seconds.");

            totalFilesToParse = files.Length;

            var asyncParse = files.Select(Parse);
            papyrusObjects.AddRange(await Task.WhenAll(asyncParse.ToArray()));

            StatusCallback.Title = "PapyrusDotNet";

            StatusCallback.WriteLine("Adding object references... This usually takes about a minute.");
            foreach (var pasObj in papyrusObjects)
                AddedTypeReferences.Add(TypeReferenceResolver.Resolve(MainModule, null, pasObj.Name));

            foreach (var pas in papyrusObjects)
                MainModule.Types.Add(TypeDefinitionResolver.Resolve(MainModule, pas));

            StatusCallback.WriteLine("Resolving object references... This usually takes about 30 seconds.");
            foreach (var t in MainModule.Types)
            {
                foreach (var f in t.Methods)
                {
                    var typeDefinition = MainModule.Types.FirstOrDefault(ty => ty.FullName == f.ReturnType.FullName);
                    f.ReturnType = TypeReferenceResolver.Resolve(MainModule, typeDefinition, f.ReturnType.FullName);
                    foreach (var p in f.Parameters)
                    {
                        var td = MainModule.Types.FirstOrDefault(ty => ty.FullName == p.ParameterType.FullName);
                        if (td != null)
                            /*	// Most likely a System object.							
                                p.ParameterType = GetTypeReference(null, p.ParameterType.FullName);
                            else */
                            p.ParameterType = TypeReferenceResolver.Resolve(MainModule, typeDefinition, td.FullName);
                    }
                }
            }

            StatusCallback.WriteLine("Importing Papyrus specific attributes...");


            var allAttributesToInclude = Assembly.GetExecutingAssembly().GetTypes().Where(
                t => t.Name.ToLower().EndsWith("attribute")
                //TODO: we should include the PapyrusDotNet.System here as well.
                );

            foreach (var attr in allAttributesToInclude)
            {
                ImportType(MainModule, attr);
            }

            CoreAssembly.Write(AssemblyNameResolver.OutputLibraryFilename);

            StatusCallback.ForegroundColor = ConsoleColor.Green;
            StatusCallback.WriteLine(AssemblyNameResolver.OutputLibraryFilename + " successefully generated.");
            StatusCallback.ResetColor();
        }


        public void CreateEmptyFunctionBody(ref MethodDefinition function)
        {
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
                     fnl.StartsWith(AssemblyNameResolver.BaseNamespace.ToLower()))
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

        public void AddVirtualOnInit(TypeDefinition newType)
        {
            // ReSharper disable once SimplifyLinqExpression
            if (!newType.Methods.Any(v => v.Name == "OnInit"))
            {
                var methodAttributes = MethodAttributes.Public;
                var method = new MethodDefinition("OnInit", methodAttributes, MainModule.TypeSystem.Void)
                {
                    IsVirtual = true
                };


                method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                newType.Methods.Add(method);
            }
        }


        public void AddEmptyConstructor(TypeDefinition type)
        {
            var method = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig |
                                                       MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                MainModule.TypeSystem.Void);

            //TODO: might need to fix this later so that PEVERIFY can verify the outputted library properly.
            // var baseEmptyConstructor = new MethodReference(".ctor", MainModule.TypeSystem.Void, MainModule.TypeSystem.Object);// MainModule.TypeSystem.Object
            // method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            // method.Body.Instructions.Add(Instruction.Create(OpCodes.Call, baseEmptyConstructor));


            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            type.Methods.Add(method);
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

                var newType = new TypeDefinition(AssemblyNameResolver.BaseNamespace, definition.Name,
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

        public async Task<PapyrusAssemblyObject> Parse(string file)
        {
            return await Task.Run(() =>
            {
                PapyrusAssemblyObject obj = null;
                var ext = Path.GetExtension(file);
                if (!string.IsNullOrEmpty(ext))
                {
                    // StatusCallback.WriteLine("... " + Path.GetFileNameWithoutExtension(file));
                    obj = ext.ToLower().EndsWith("pas")
                        ? PapyrusAssemblyParser.ParseAssembly(file)
                        : PapyrusScriptParser.ParseScript(file);
                }
                filesParsed++;
                StatusCallback.Title = "PapyrusDotNet - Parsing Papyrus: " + filesParsed + "/" + totalFilesToParse;
                return obj;
            });
        }

        // public static Compiler compiler = new PCompiler.Compiler();
    }
}