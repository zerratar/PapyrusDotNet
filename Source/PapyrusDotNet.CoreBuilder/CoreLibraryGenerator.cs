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
using ParameterAttributes = Mono.Cecil.ParameterAttributes;
using PropertyAttributes = Mono.Cecil.PropertyAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace PapyrusDotNet.CoreBuilder
{
    public class CoreLibraryGenerator : ICoreLibraryGenerator
    {
        public ModuleDefinition MainModule { get; set; }

        public AssemblyDefinition CoreAssembly;

        public List<string> PreservedTypeNames { get; set; } = new List<string>();

        public List<TypeReference> AddedTypeReferences { get; set; } = new List<TypeReference>();

        public const string OutputFileName = "PapyrusDotNet.Core.dll";

        public const string CoreNamespace = "PapyrusDotNet.Core";

        private readonly IPapyrusScriptParser papyrusScriptParser;
        private readonly IPapyrusAssemblyParser papyrusAssemblyParser;
        private readonly IStatusCallbackService statusCallback;

        private int totalFilesToParse;
        private int filesParsed;

        public CoreLibraryGenerator(IStatusCallbackService callback, IPapyrusScriptParser scriptParser, IPapyrusAssemblyParser assemblyParser)
        {
            statusCallback = callback;
            papyrusScriptParser = scriptParser;
            papyrusAssemblyParser = assemblyParser;
        }

        public async void GenerateCoreLibrary(string typeName, string inputDirectory, string searchFor)
        {
            statusCallback.WriteLine("Loading all " + typeName + " files...");

            // @"C:\The Elder Scrolls V Skyrim\Data\scripts\Source"
            var files = Directory.GetFiles(
                inputDirectory, searchFor, SearchOption.AllDirectories);

            CoreAssembly = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition(CoreNamespace, new Version(1, 0)),
                CoreNamespace, ModuleKind.Dll);

            MainModule = CoreAssembly.MainModule;

            var papyrusObjects = new List<PapyrusAssemblyObject>();

            statusCallback.WriteLine("Parsing Papyrus... This usually takes about 30 seconds.");

            totalFilesToParse = files.Length;

            var asyncParse = files.Select(Parse);
            papyrusObjects.AddRange(await Task.WhenAll(asyncParse.ToArray()));

            statusCallback.Title = "PapyrusDotNet";

            statusCallback.WriteLine("Adding object references... This usually takes about a minute.");
            foreach (var pasObj in papyrusObjects)
                AddedTypeReferences.Add(GetTypeReference(null, pasObj.Name));

            foreach (var pas in papyrusObjects)
                MainModule.Types.Add(TypeDefinitionFromPapyrus(pas));

            statusCallback.WriteLine("Resolving object references... This usually takes about 30 seconds.");
            foreach (var t in MainModule.Types)
            {
                foreach (var f in t.Methods)
                {
                    var typeDefinition = MainModule.Types.FirstOrDefault(ty => ty.FullName == f.ReturnType.FullName);
                    f.ReturnType = GetTypeReference(typeDefinition, f.ReturnType.FullName);
                    foreach (var p in f.Parameters)
                    {
                        var td = MainModule.Types.FirstOrDefault(ty => ty.FullName == p.ParameterType.FullName);
                        if (td != null)
                            /*	// Most likely a System object.							
                                p.ParameterType = GetTypeReference(null, p.ParameterType.FullName);
                            else */
                            p.ParameterType = GetTypeReference(typeDefinition, td.FullName);
                    }
                }
            }

            statusCallback.WriteLine("Importing Papyrus specific attributes...");


            var allAttributesToInclude = Assembly.GetExecutingAssembly().GetTypes().Where(
                t => t.Name.ToLower().EndsWith("attribute")
#warning we should include the PapyrusDotNet.System here as well.
                );

            foreach (var attr in allAttributesToInclude)
            {
                IncludeType(MainModule, attr);
            }

            CoreAssembly.Write(OutputFileName);

            statusCallback.ForegroundColor = ConsoleColor.Green;
            statusCallback.WriteLine(OutputFileName + " successefully generated.");
            statusCallback.ResetColor();
        }


        private void IncludeType(ModuleDefinition mainModule, Type type)
        {
            try
            {
                var reference = mainModule.Import(type);

                var definition = reference.Resolve();

                var newType = new TypeDefinition(CoreNamespace, definition.Name, TypeAttributes.Class);
                newType.IsPublic = true;
                newType.BaseType = definition.BaseType;

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
                    catch { }
                }

                // if (constructor != null && !constructor.HasParameters)
                AddEmptyConstructor(newType);

                foreach (var field in definition.Methods)
                {

                    if (field.IsConstructor && !field.HasParameters) continue;

                    var newField = new MethodDefinition(field.Name, field.Attributes, field.ReturnType);

                    var refer = mainModule.Import(field);

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
            catch { }
        }

        public TypeDefinition TypeDefinitionFromPapyrus(PapyrusAssemblyObject input)
        {
            var newType = new TypeDefinition(CoreNamespace, input.Name, TypeAttributes.Class);


            newType.IsPublic = true;
            // newType.DeclaringType = newType;
            if (!string.IsNullOrEmpty(input.ExtendsName))
            {
                newType.BaseType = new TypeReference(CoreNamespace, input.ExtendsName, MainModule, MainModule);
                // newType.DeclaringType = MainModule.Types.FirstOrDefault(t => t.FullName == newType.BaseType.FullName);
                newType.Scope = MainModule;
            }
            else
            {
                newType.BaseType = MainModule.TypeSystem.Object;
                newType.Scope = MainModule;
            }

            statusCallback.WriteLine("Generating Type '" + CoreNamespace + "." + input.Name + "'...");

            foreach (var prop in input.PropertyTable)
            {
                var typeRef = GetTypeReference(null, prop.Type);
                var pro = new PropertyDefinition(prop.Name, PropertyAttributes.HasDefault, typeRef);
                newType.Properties.Add(pro);
            }

            // newType.AddDefaultConstructor();

            AddEmptyConstructor(newType);

            AddVirtualOnInit(newType);

            foreach (var papyrusAsmState in input.States)
            {
                foreach (var papyrusAsmFunction in papyrusAsmState.Functions)
                {
                    TypeReference typeRef = GetTypeReference(null, papyrusAsmFunction.ReturnType);
                    // var typeRef = MainModule.TypeSystem.Void;



                    var function = new MethodDefinition(papyrusAsmFunction.Name, MethodAttributes.Public, typeRef);
                    function.IsStatic = papyrusAsmFunction.IsStatic;
                    if (function.IsStatic)
                        function.HasThis = false;
                    if (!function.IsStatic && papyrusAsmFunction.Name.StartsWith("On") || papyrusAsmFunction.IsEvent)
                        function.IsVirtual = true;
                    else function.IsVirtual = false;

                    CreateEmptyFunctionBody(ref function);

                    foreach (var par in papyrusAsmFunction.Params)
                    {
                        TypeReference typeRefp = GetTypeReference(null, par.Type);
                        // var typeRefp = MainModule.TypeSystem.Object;

                        var nPar = new ParameterDefinition(par.Name, ParameterAttributes.None, typeRefp);
                        function.Parameters.Add(nPar);
                    }
                    bool skipAdd = false;
                    foreach (var m in newType.Methods)
                    {
                        if (m.Name == function.Name)
                        {
                            if (m.Parameters.Count == function.Parameters.Count)
                            {
                                skipAdd = true;
                                for (int pi = 0; pi < m.Parameters.Count; pi++)
                                {
                                    if (m.Parameters[pi].ParameterType.FullName != function.Parameters[pi].ParameterType.FullName) skipAdd = false;
                                }
                                break;
                            }
                        }
                    }
                    if (!skipAdd)
                        newType.Methods.Add(function);
                }
            }
            return newType;
        }

        private void CreateEmptyFunctionBody(ref MethodDefinition function)
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

            else if (fnl.StartsWith("system.string") || fnl.StartsWith("system.object") || fnl.StartsWith(CoreNamespace.ToLower()))
                function.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));

            else if (fnl.StartsWith("system.int") || fnl.StartsWith("system.bool") || fnl.StartsWith("system.long") || fnl.StartsWith("system.byte") || fnl.StartsWith("system.short"))
            {
                if (fnl.StartsWith("system.long"))
                    function.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I8, 0L));
                else
                    function.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
            }

            else if (fnl.StartsWith("system.float"))
                function.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_R4, 0f));

            else if (fnl.StartsWith("system.double"))
                function.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_R8, 0d));


            function.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private void AddVirtualOnInit(TypeDefinition newType)
        {
            if (!newType.Methods.Any(v => v.Name == "OnInit"))
            {
                var methodAttributes = MethodAttributes.Public;
                var method = new MethodDefinition("OnInit", methodAttributes, MainModule.TypeSystem.Void);
                method.IsVirtual = true;



                method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                newType.Methods.Add(method);
            }
        }

        private TypeReference GetTypeReference(TypeDefinition newType, string fallback = null)
        {
            var typeName = "";
            if (!string.IsNullOrEmpty(fallback))
                typeName = fallback;
            else
                typeName = newType.FullName;

            var ns = GetTypeNamespace(typeName);
            var tn = GetTypeName(typeName);
            var isArray = tn.Contains("[]");

            if (ns == "System")
            {

                var propies = MainModule.TypeSystem.GetType().GetProperties().Where(pr => pr.PropertyType == typeof(TypeReference)).ToList();
                foreach (var propy in propies)
                {
                    var name = propy.Name;
                    if (tn.Replace("[]", "").ToLower() == name.ToLower())
                    {
                        var val = propy.GetValue(MainModule.TypeSystem, null) as TypeReference;
                        return isArray && !val.IsArray ? new ArrayType(val) : val;
                    }
                }
                // fallback
                switch (tn.ToLower())
                {
                    case "none":
                    case "void":
                        return MainModule.TypeSystem.Void;
                    case "byte":
                    case "short":
                    case "int":
                    case "long":
                    case "int8":
                    case "int16":
                    case "int32":
                    case "int64":
                        return isArray ? new ArrayType(MainModule.TypeSystem.Int32) : MainModule.TypeSystem.Int32;
                    case "string":
                        return isArray ? new ArrayType(MainModule.TypeSystem.String) : MainModule.TypeSystem.String;
                    case "float":
                    case "double":
                        return isArray ? new ArrayType(MainModule.TypeSystem.Double) : MainModule.TypeSystem.Double;
                    case "bool":
                    case "boolean":
                        return isArray ? new ArrayType(MainModule.TypeSystem.Boolean) : MainModule.TypeSystem.Boolean;
                    default:
                        return isArray ? new ArrayType(MainModule.TypeSystem.Object) : MainModule.TypeSystem.Object;
                }
            }
            var tnA = tn.Replace("[]", "");
            var existing = AddedTypeReferences.FirstOrDefault(ty => ty.FullName.ToLower() == (ns + "." + tnA).ToLower());
            if (existing == null)
            {
                var hasTypeOf = MainModule.Types.FirstOrDefault(t => t.FullName.ToLower() == (ns + "." + tnA).ToLower());
                if (hasTypeOf != null)
                {
                    var typeRef = new TypeReference(hasTypeOf.Namespace, hasTypeOf.Name, MainModule, MainModule);
                    typeRef.Scope = MainModule;
                    AddedTypeReferences.Add(typeRef);
                    return isArray && !typeRef.IsArray ? new ArrayType(typeRef) : typeRef;
                }
                else
                {
                    if (PreservedTypeNames.Any(n => n.ToLower() == tnA.ToLower()))
                    {
                        tn = PreservedTypeNames.FirstOrDefault(j => j.ToLower() == tnA.ToLower());
                    }
                    var typeRef = new TypeReference(ns, tn, MainModule, MainModule);



                    typeRef.Scope = MainModule;
                    AddedTypeReferences.Add(typeRef);
                    return isArray && !typeRef.IsArray ? new ArrayType(typeRef) : typeRef;
                }

            }

            return isArray && !existing.IsArray ? new ArrayType(existing) : existing;
        }


        public void AddEmptyConstructor(TypeDefinition type)
        {
            var methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var method = new MethodDefinition(".ctor", methodAttributes, MainModule.TypeSystem.Void);

#warning might need to fix this later so that PEVERIFY can verify the outputted library properly.
            // var baseEmptyConstructor = new MethodReference(".ctor", MainModule.TypeSystem.Void, MainModule.TypeSystem.Object);// MainModule.TypeSystem.Object
            // method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            // method.Body.Instructions.Add(Instruction.Create(OpCodes.Call, baseEmptyConstructor));



            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            type.Methods.Add(method);
        }

        private string GetTypeName(string p)
        {
            if (p.Contains('.')) p = p.Split('.').LastOrDefault();
            var pl = p.ToLower();

            /*if (p.EndsWith("[]"))
            {
                pl = pl.Replace("[]", "");
            }*/

            if (pl == "boolean")
                return "bool";
            if (pl == "none")
                return "void";

            if (pl == "float" || pl == "int" || pl == "bool" || pl == "string") return pl;

            return p;
        }

        private string GetTypeNamespace(string p)
        {
            if (p.Contains('.')) p = p.Split('.').LastOrDefault();
            var pl = p.ToLower();

            if (p.EndsWith("[]"))
            {
                pl = pl.Replace("[]", "");
            }

            /* have not added all possible types yet though.. might be a better way of doing it. */
            if (pl == "string" || pl == "int" || pl == "boolean" || pl == "bool" || pl == "none"
                || pl == "void" || pl == "float" || pl == "short" || pl == "char" || pl == "double"
                || pl == "int32" || pl == "integer32" || pl == "long" || pl == "uint")
            {
                return "System";
            }
            return CoreNamespace;
        }

        public async Task<PapyrusAssemblyObject> Parse(string file)
        {
            return await Task.Run(() =>
            {
                PapyrusAssemblyObject obj = null;
                var ext = Path.GetExtension(file);
                if (!string.IsNullOrEmpty(ext))
                {
                    // statusCallback.WriteLine("... " + Path.GetFileNameWithoutExtension(file));
                    if (ext.ToLower().EndsWith("pas"))
                    {
                        obj = papyrusAssemblyParser.ParseAssembly(file);
                    }
                    else
                        obj = papyrusScriptParser.ParseScript(file);
                }
                filesParsed++;
                statusCallback.Title = "PapyrusDotNet - Parsing Papyrus: " + filesParsed + "/" + totalFilesToParse;
                return obj;
            });
        }

        // public static Compiler compiler = new PCompiler.Compiler();
    }
}