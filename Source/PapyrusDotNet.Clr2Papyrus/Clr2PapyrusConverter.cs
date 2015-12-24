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
using Mono.Cecil;
using PapyrusDotNet.Common;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.Common.Utilities;
using PapyrusDotNet.Converters.Clr2Papyrus.Base;
using PapyrusDotNet.Converters.Clr2Papyrus.Enums;
using PapyrusDotNet.Converters.Clr2Papyrus.Exceptions;
using PapyrusDotNet.Converters.Clr2Papyrus.Implementations;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Extensions;

#endregion

namespace PapyrusDotNet.Converters.Clr2Papyrus
{
    public class Clr2PapyrusConverter : Clr2PapyrusConverterBase
    {
        private readonly IClr2PapyrusInstructionProcessor instructionProcessor;
        private readonly PapyrusCompilerOptions processorOptions;
        private List<MethodDefinition> propertyMethods = new List<MethodDefinition>();
        private MethodDefinition constructor;
        private IPapyrusAttributeReader attributeReader;
        private readonly IPropertyAnalyzer propertyAnalyzer = new PropertyAnalyzer();
        private List<PapyrusAssemblyDefinition> papyrusAssemblies = new List<PapyrusAssemblyDefinition>();
        private List<TypeDefinition> EnumDefinitions = new List<TypeDefinition>();
        private TypeDefinition activeClrType;

        private readonly DelegateFinder delegateFinder;
        private IDelegatePairDefinition delegatePairDefinition;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Clr2PapyrusConverter" /> class.
        /// </summary>
        /// <param name="instructionProcessor">The instruction processor.</param>
        /// <param name="processorOptions"></param>
        public Clr2PapyrusConverter(IClr2PapyrusInstructionProcessor instructionProcessor, PapyrusCompilerOptions processorOptions)
        {
            attributeReader = new PapyrusAttributeReader(new PapyrusValueTypeConverter());
            this.instructionProcessor = instructionProcessor;
            this.processorOptions = processorOptions;
            delegateFinder = new DelegateFinder();
        }

        /// <summary>
        ///     Converts the assembly.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        protected override PapyrusAssemblyOutput ConvertAssembly(ClrAssemblyInput input)
        {
            var clr = input.Assembly;
            var mainModule = clr.MainModule;

            papyrusAssemblies.Clear();

            propertyMethods = new List<MethodDefinition>();

            try
            {
                var papyrusAssemblyToTypeDefinition = new Dictionary<PapyrusAssemblyDefinition, TypeDefinition>();

                // Find all delegate types and methods and create references to them.                                
                delegatePairDefinition = delegateFinder.FindDelegateTypes(mainModule);

                // Keep track on the enum types so we can verify any parameter, variables, etc, and change the type into integers.
                ResolveEnumDefinitions(mainModule);

                foreach (var type in mainModule.Types)
                {
                    // We will skip this one for now
                    // as it will not really provide us with any necessary information at this early stage.
                    if (type.Name == "<Module>") continue;

                    activeClrType = type;

                    var pex = PapyrusAssemblyDefinition.CreateAssembly(input.TargetPapyrusVersion);

                    SetHeaderInfo(input, pex, type);

                    CreateType(pex, type, processorOptions);

                    papyrusAssemblies.Add(pex);

                    papyrusAssemblyToTypeDefinition.Add(pex, type);
                }

                // After the type has been processed
                // we will create the methods and then create the debug info as we do not have any info regarding the methods until those have been created ;-)

                foreach (var pex in papyrusAssemblies)
                {
                    foreach (var t in pex.Types)
                    {
                        var type = papyrusAssemblyToTypeDefinition[pex];

                        CreateMethods(papyrusAssemblies, type, t, pex, processorOptions)
                            .ForEach(t.States.FirstOrDefault().Methods.Add);

                        CreateDebugInfo(pex, t, type);
                    }
                }
            }
            catch (ProhibitedCodingBehaviourException exc)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Error: Prohibited use of " + exc.OpCode.GetValueOrDefault().Code + " in " +
                                  exc.Method.FullName + " at " + exc.Offset);
            }
            catch (Exception exc)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Error: Unhandled Exception - " + exc);
            }
            Console.ResetColor();
            return new PapyrusAssemblyOutput(papyrusAssemblies.ToArray());
        }

        private void ResolveEnumDefinitions(ModuleDefinition mainModule)
        {
            foreach (var type in mainModule.Types)
            {
                if (type.IsEnum)
                {
                    EnumDefinitions.Add(type);
                    continue;
                }

                foreach (var nestedType in type.NestedTypes.Where(nestedType => nestedType.IsEnum))
                {
                    EnumDefinitions.Add(nestedType);
                }
            }
        }


        private PapyrusTypeDefinition CreateType(PapyrusAssemblyDefinition pex, TypeDefinition type, PapyrusCompilerOptions options, bool isStruct = false)
        {
            var papyrusType = new PapyrusTypeDefinition(pex, isStruct);


            if (isStruct)
            {
                papyrusType.IsStruct = true;
                papyrusType.IsClass = false;
            }

            papyrusType.Name = type.Name.Ref(pex);
            papyrusType.AutoStateName = "".Ref(pex);
            papyrusType.Documentation = "".Ref(pex);
            papyrusType.BaseTypeName = type.BaseType != null
                ? Utility.GetPapyrusBaseType(type.BaseType).Ref(pex)
                : "".Ref(pex);

            UpdateUserFlags(type, pex);

            // Create Fields
            CreateFields(type, pex).ForEach(papyrusType.Fields.Add);

            // Create Properties
            CreateProperties(papyrusAssemblies, type, papyrusType, pex).ForEach(papyrusType.Properties.Add);

            // Create Structs
            foreach (var nestedType in type.NestedTypes)
            {
                // Make sure we don't add any delegate classes, as those are not supported as is
                if (delegatePairDefinition.DelegateTypeDefinitions.Contains(nestedType)) continue;
                // We do not want to include any Enums either :-)
                if (EnumDefinitions.Contains(nestedType)) continue;
                papyrusType.NestedTypes.Add(CreateStruct(nestedType, pex, options));
            }

            if (!isStruct)
            {
                var autoState = new PapyrusStateDefinition(papyrusType)
                {
                    Name = "".Ref(pex)
                };
                // -- Do not create the methods until all types has been parsed. excluding getters and setters
                // CreateMethods(type, papyrusType, pex, options).ForEach(autoState.Methods.Add);
            }
            return papyrusType;
        }

        private PapyrusTypeDefinition CreateStruct(TypeDefinition structType,
            PapyrusAssemblyDefinition asm, PapyrusCompilerOptions options)
        {
            return CreateType(asm, structType, options, true);
        }

        private void CreateDebugInfo(PapyrusAssemblyDefinition pex, PapyrusTypeDefinition papyrusType, TypeDefinition type)
        {
            var debug = pex.DebugInfo;
            debug.DebugTime = UnixTimeConverterUtility.Convert(DateTime.Now);

            if (pex.VersionTarget == PapyrusVersionTargets.Fallout4)
            {
                foreach (var t in papyrusType.NestedTypes)
                {
                    var structInfo = new PapyrusStructDescription();
                    structInfo.Name = papyrusType.Name;
                    structInfo.DeclaringTypeName = type.Name.Ref(pex);

                    foreach (var f in t.Fields)
                        structInfo.FieldNames.Add(f.Name);

                    debug.StructDescriptions.Add(structInfo);
                }
            }

            foreach (var s in papyrusType.States)
            {

                foreach (var method in s.Methods)
                {
                    var m = new PapyrusMethodDecription();
                    m.Name = method.Name;
                    m.DeclaringTypeName = type.Name.Ref(pex);
                    m.StateName = "".Ref(pex);

                    if (method.Name.Value.ToLower().StartsWith("get_"))
                    {
                        m.MethodType = PapyrusMethodTypes.Getter;
                    }
                    else if (method.Name.Value.ToLower().StartsWith("set_"))
                    {
                        m.MethodType = PapyrusMethodTypes.Setter;
                    }
                    else
                    {
                        m.MethodType = PapyrusMethodTypes.Method;
                    }

                    method.Body.Instructions.ForEach(i =>
                    {
                        if (i.SequencePoint != null) m.BodyLineNumbers.Add((short)i.SequencePoint.StartLine);
                    });

                    debug.MethodDescriptions.Add(m);
                }
            }

            var stateProperties = new PapyrusStatePropertyDescriptions();
            stateProperties.GroupDocumentation = "".Ref(pex);
            stateProperties.GroupName = "".Ref(pex);
            stateProperties.ObjectName = type.Name.Ref(pex);

            foreach (var prop in type.Properties)
            {
                // TODO: This                
                stateProperties.PropertyNames.Add(prop.Name.Ref(pex));
            }
            debug.PropertyDescriptions.Add(stateProperties);
        }

        private void UpdateUserFlags(TypeDefinition type, PapyrusAssemblyDefinition pex)
        {
            var props = attributeReader.ReadPapyrusAttributes(type);
            pex.Header.UserflagReferenceHeader.Add("hidden", (byte)(props.IsHidden ? 1 : 0));
            pex.Header.UserflagReferenceHeader.Add("conditional", (byte)(props.IsConditional ? 1 : 0));
        }

        private List<PapyrusMethodDefinition> CreateMethods(IEnumerable<PapyrusAssemblyDefinition> papyrusAssemblyCollection, TypeDefinition type, PapyrusTypeDefinition papyrusType, PapyrusAssemblyDefinition pex, PapyrusCompilerOptions options)
        {
            var methods = new List<PapyrusMethodDefinition>();

            foreach (var method in delegatePairDefinition.DelegateMethodDefinitions)
            {
                methods.Add(CreatePapyrusMethodDefinition(papyrusAssemblyCollection, pex, papyrusType, method, delegatePairDefinition, options));
            }

            foreach (var method in type.Methods.OrderByDescending(m => m.IsConstructor))
            {
                if (propertyMethods.Contains(method)) continue;

                methods.Add(CreatePapyrusMethodDefinition(papyrusAssemblyCollection, pex, papyrusType, method, delegatePairDefinition, options));
            }
            return methods;
        }

        private List<PapyrusPropertyDefinition> CreateProperties(IEnumerable<PapyrusAssemblyDefinition> papyrusAssemblyCollection, TypeDefinition type, PapyrusTypeDefinition papyrusType, PapyrusAssemblyDefinition pex)
        {
            var propList = new List<PapyrusPropertyDefinition>();
            foreach (var prop in type.Properties)
            {
                var properties = attributeReader.ReadPapyrusAttributes(prop);
                var propertyTypeName = Utility.GetPapyrusReturnType(prop.PropertyType);

                if (EnumDefinitions.Any(i => i.FullName == prop.PropertyType.FullName))
                    propertyTypeName = "Int";

                var papyrusPropertyDefinition = new PapyrusPropertyDefinition(pex, prop.Name,
                    propertyTypeName)
                {
                    Documentation = "".Ref(pex),
                    Userflags = properties.UserFlagsValue
                };

                var result = propertyAnalyzer.Analyze(prop);

                if (result.IsAutoVar)
                {
                    papyrusPropertyDefinition.IsAuto = true;
                    papyrusPropertyDefinition.AutoName = result.AutoVarName;
                }
                else
                {

                    if (prop.SetMethod != null)
                    {
                        papyrusPropertyDefinition.HasSetter = true;
                        papyrusPropertyDefinition.SetMethod = CreatePapyrusMethodDefinition(papyrusAssemblyCollection, pex, papyrusType, prop.SetMethod,
                            delegatePairDefinition,
                            processorOptions);
                        propertyMethods.Add(prop.SetMethod);
                    }

                    if (prop.GetMethod != null)
                    {
                        papyrusPropertyDefinition.HasGetter = true;
                        papyrusPropertyDefinition.GetMethod = CreatePapyrusMethodDefinition(papyrusAssemblyCollection, pex, papyrusType, prop.GetMethod,
                            delegatePairDefinition,
                            processorOptions);
                        propertyMethods.Add(prop.GetMethod);
                    }

                    if (prop.SetMethod == null && prop.GetMethod == null)
                    {
                        papyrusPropertyDefinition.IsAuto = true;
                        papyrusPropertyDefinition.AutoName =
                            papyrusType.Fields.FirstOrDefault(f => f.Name.Value.Contains("_" + prop.Name + "_") && f.Name.Value.EndsWith("_BackingField")).Name.Value;
                    }
                }
                propList.Add(papyrusPropertyDefinition);
            }
            return propList;
        }

        private IEnumerable<PapyrusFieldDefinition> CreateFields(TypeDefinition type, PapyrusAssemblyDefinition pex)
        {
            var fields = new List<PapyrusFieldDefinition>();
            foreach (var field in type.Fields)
            {
                var papyrusFriendlyName = "::" + field.Name.Replace('<', '_').Replace('>', '_'); // Only for the VariableReference

                var properties = attributeReader.ReadPapyrusAttributes(field);
                var fieldType = Utility.GetPapyrusReturnType(field.FieldType, type);

                if (EnumDefinitions.Any(i => i.FullName == field.FieldType.FullName))
                    fieldType = "Int";

                var nameRef = papyrusFriendlyName.Ref(pex);
                var papyrusFieldDefinition = new PapyrusFieldDefinition(pex, field.Name,
                    fieldType)
                {
                    FieldVariable = new PapyrusVariableReference()
                    {
                        Name = nameRef,
                        TypeName = fieldType.Ref(pex),
                        Value = nameRef.Value,
                        ValueType = PapyrusPrimitiveType.Reference
                    },
                    UserFlags = properties.UserFlagsValue
                };
                fields.Add(papyrusFieldDefinition);
            }
            return fields;
        }

        private PapyrusMethodDefinition CreatePapyrusMethodDefinition(IEnumerable<PapyrusAssemblyDefinition> papyrusAssemblyCollection, PapyrusAssemblyDefinition asm,
            PapyrusTypeDefinition papyrusType,
            MethodDefinition method, IDelegatePairDefinition delegatePairDef, PapyrusCompilerOptions options)
        {
            if (method.IsConstructor)
            {
                // Replace: .ctor with __ctor
                method.Name = method.Name.Replace(".", "__");
                constructor = method;
            }

            var m = new PapyrusMethodDefinition(asm);
            m.Documentation = "".Ref(asm);
            m.UserFlags = attributeReader.ReadPapyrusAttributes(method).UserFlagsValue;
            m.IsGlobal = method.IsStatic;
            m.IsNative = method.CustomAttributes.Any(i => i.AttributeType.Name.Equals("NativeAttribute"));
            m.Name = method.Name.Ref(asm);
            var papyrusReturnType = Utility.GetPapyrusReturnType(method.ReturnType, activeClrType);

            if (EnumDefinitions.Any(m2 => m2.FullName == method.ReturnType.FullName))
            {
                papyrusReturnType = "Int";
            }

            m.ReturnTypeName = papyrusReturnType.Ref(asm); // method.ReturnType.Name
            m.Parameters = new List<PapyrusParameterDefinition>();
            foreach (var p in method.Parameters)
            {
                // TODO: Add support for delegate as parameter

                var paramTypeName = Utility.GetPapyrusReturnType(p.ParameterType, activeClrType, true);

                // Replace enum types into Integer
                if (EnumDefinitions.Any(i => i.FullName == p.ParameterType.FullName))
                    paramTypeName = "Int";

                m.Parameters.Add(new PapyrusParameterDefinition
                {
                    Name = p.Name.Ref(asm),
                    TypeName = paramTypeName.Ref(asm)
                });
            }

            var clrVariables = method.Body.Variables;

            int varNum = 0;
            foreach (var clrVar in clrVariables)
            {

                var delegateVars = delegatePairDef.DelegateMethodLocalPair.Where(d => d.Key == method).SelectMany(d => d.Value);

                if (delegateVars.Any(d => "V_" + d.Index == "V_" + clrVar.Index))
                {
                    // This local variable is pointing to a delegate
                    // and since we are removing all Delegate types, this wont work. So we have to change the type into something else.
                    // in this case, we are changing it into a Int
                    var varName = (!string.IsNullOrEmpty(clrVar.Name) ? clrVar.Name : clrVar.ToString()).Ref(asm);

                    var delegateInvokeRef = delegateFinder.FindDelegateInvokeReference(delegatePairDefinition, m);

                    m.Body.Variables.Add(new PapyrusVariableReference(varName, "Int".Ref(asm))
                    {
                        IsDelegateReference = true,
                        DelegateInvokeReference = delegateInvokeRef,
                        Value = varName.Value,
                        ValueType = PapyrusPrimitiveType.Reference
                    });

                }
                else
                {
                    var varName = (!string.IsNullOrEmpty(clrVar.Name) ? clrVar.Name : clrVar.ToString()).Ref(asm);
                    var variableTypeName = Utility.GetPapyrusReturnType(clrVar.VariableType.FullName);

                    // If its an enum, we want to change the type into a Int
                    if (EnumDefinitions.Any(i => i.FullName == clrVar.VariableType.FullName))
                        variableTypeName = "Int";

                    m.Body.Variables.Add(new PapyrusVariableReference(varName, variableTypeName.Ref(asm))
                    {
                        Value = varName.Value,
                        ValueType = PapyrusPrimitiveType.Reference
                    });
                }
                varNum++;
            }

            if (method.HasBody)
            {
                ProcessInstructions(papyrusAssemblyCollection, delegatePairDef, method, asm, papyrusType, m, options);

                m.Body.Instructions.RecalculateOffsets();
            }

            return m;
        }

        private void ProcessInstructions(IEnumerable<PapyrusAssemblyDefinition> papyrusAssemblyCollection, IDelegatePairDefinition delegatePairDef, MethodDefinition method, PapyrusAssemblyDefinition asm, PapyrusTypeDefinition papyrusType, PapyrusMethodDefinition m, PapyrusCompilerOptions options)
        {
            var papyrusInstructions =
                instructionProcessor.ProcessInstructions(papyrusAssemblyCollection, delegatePairDef, asm, papyrusType, m, method, method.Body, method.Body.Instructions, options);

            if (method.Name.ToLower() == "oninit")
            {
                List<PapyrusInstruction> structGets;
                var ip = instructionProcessor as Clr2PapyrusInstructionProcessor; // TODO: Going against solid here just because im to damn tired, which I ended up breaking in lots of places.
                m.Body.Instructions.Insert(0,
                    ip.CallInstructionProcessor.CreatePapyrusCallInstruction(PapyrusOpCodes.Callmethod, constructor, "self",
                        "::nonevar", new List<object>(), out structGets));
            }

            m.Body.Instructions.AddRange(papyrusInstructions);
        }

        private void SetHeaderInfo(ClrAssemblyInput input, PapyrusAssemblyDefinition pex, TypeDefinition type)
        {
            pex.Header.HeaderIdentifier = input.TargetPapyrusVersion == PapyrusVersionTargets.Fallout4
                ? PapyrusHeader.Fallout4PapyrusHeaderIdentifier
                : PapyrusHeader.SkyrimPapyrusHeaderIdentifier;

            pex.Header.SourceHeader.Version = input.TargetPapyrusVersion == PapyrusVersionTargets.Fallout4
                ? PapyrusHeader.Fallout4PapyrusVersion
                : PapyrusHeader.SkyrimPapyrusVersion;
            pex.Header.SourceHeader.Source = "PapyrusDotNet" + type.Name + ".psc";

            pex.Header.SourceHeader.User = Environment.UserName;
            pex.Header.SourceHeader.Computer = Environment.MachineName;
            pex.Header.SourceHeader.GameId = (short)input.TargetPapyrusVersion;
            pex.Header.SourceHeader.CompileTime = UnixTimeConverterUtility.Convert(DateTime.Now);
            pex.Header.SourceHeader.ModifyTime = UnixTimeConverterUtility.Convert(DateTime.Now);
        }
    }
}