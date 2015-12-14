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
using PapyrusDotNet.Converters.Clr2Papyrus.Base;
using PapyrusDotNet.Converters.Clr2Papyrus.Enums;
using PapyrusDotNet.Converters.Clr2Papyrus.Exceptions;
using PapyrusDotNet.Converters.Clr2Papyrus.Implementations;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Extensions;

#endregion

namespace PapyrusDotNet.Converters.Clr2Papyrus
{
    public class Clr2PapyrusConverter : Clr2PapyrusConverterBase
    {
        private readonly IClr2PapyrusInstructionProcessor instructionProcessor;
        private readonly PapyrusCompilerOptions processorOptions;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Clr2PapyrusConverter" /> class.
        /// </summary>
        /// <param name="instructionProcessor">The instruction processor.</param>
        /// <param name="processorOptions"></param>
        public Clr2PapyrusConverter(IClr2PapyrusInstructionProcessor instructionProcessor, PapyrusCompilerOptions processorOptions)
        {
            this.instructionProcessor = instructionProcessor;
            this.processorOptions = processorOptions;
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
            var papyrusAssemblies = new List<PapyrusAssemblyDefinition>();

            try
            {
                foreach (var type in mainModule.Types)
                {
                    // We will skip this one for now
                    // as it will not really provide us with any necessary information at this early stage.
                    if (type.Name == "<Module>") continue;

                    var pex = PapyrusAssemblyDefinition.CreateAssembly(input.TargetPapyrusVersion);

                    SetHeaderInfo(input, pex, type);

                    CreateType(pex, type, processorOptions);

                    CreateDebugInfo(pex, type);

                    // pex.Header
                    papyrusAssemblies.Add(pex);
                }
            }
            catch (ProhibitedCodingBehaviourException exc)
            {
                Console.WriteLine("Error: Prohibited use of " + exc.OpCode.GetValueOrDefault().Code + " in " + exc.Method.FullName + " at " + exc.Offset);
            }
            return new PapyrusAssemblyOutput(papyrusAssemblies.ToArray());
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

            // Create Properties
            CreateProperties(type, pex).ForEach(papyrusType.Properties.Add);

            // Create Fields
            CreateFields(type, pex).ForEach(papyrusType.Fields.Add);

            // Create Structs
            foreach (var nestedType in type.NestedTypes)
            {
                papyrusType.NestedTypes.Add(CreateStruct(nestedType, pex, options));
            }
            if (!isStruct)
            {
                var autoState = new PapyrusStateDefinition(papyrusType)
                {
                    Name = "".Ref(pex)
                };
                // Create Methods
                CreateMethods(type, papyrusType, pex, options).ForEach(autoState.Methods.Add);
            }
            return papyrusType;
        }

        private PapyrusTypeDefinition CreateStruct(TypeDefinition structType,
            PapyrusAssemblyDefinition asm, PapyrusCompilerOptions options)
        {
            return CreateType(asm, structType, options, true);
        }

        private void CreateDebugInfo(PapyrusAssemblyDefinition pex, TypeDefinition type)
        {
            var debug = pex.DebugInfo;
            debug.DebugTime = Utility.ConvertToTimestamp(DateTime.Now);

            if (pex.VersionTarget == PapyrusVersionTargets.Fallout4)
            {
                foreach (var t in type.NestedTypes)
                {
                    var structInfo = new PapyrusStructDescription();
                    structInfo.Name = t.Name.Ref(pex);
                    structInfo.DeclaringTypeName = type.Name.Ref(pex);

                    foreach (var f in t.Fields)
                        structInfo.FieldNames.Add(f.Name.Ref(pex));

                    debug.StructDescriptions.Add(structInfo);
                }
            }
            foreach (var method in type.Methods)
            {
                var m = new PapyrusMethodDecription();
                m.Name = method.Name.Ref(pex);
                m.DeclaringTypeName = type.Name.Ref(pex);
                m.StateName = "".Ref(pex);

                if (method.IsGetter)
                {
                    m.MethodType = PapyrusMethodTypes.Getter;
                }
                else if (method.IsSetter)
                {
                    m.MethodType = PapyrusMethodTypes.Setter;
                }
                else
                {
                    m.MethodType = PapyrusMethodTypes.Method;
                }
                //TODO: m.BodyLineNumbers
                debug.MethodDescriptions.Add(m);
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
            var props = Utility.GetFlagsAndProperties(type);
            pex.Header.UserflagReferenceHeader.Add("hidden", (byte)(props.IsHidden ? 1 : 0));
            pex.Header.UserflagReferenceHeader.Add("conditional", (byte)(props.IsConditional ? 1 : 0));
        }

        private List<PapyrusMethodDefinition> CreateMethods(TypeDefinition type, PapyrusTypeDefinition papyrusType, PapyrusAssemblyDefinition pex, PapyrusCompilerOptions options)
        {
            var methods = new List<PapyrusMethodDefinition>();
            foreach (var method in type.Methods)
            {
                methods.Add(CreatePapyrusMethodDefinition(pex, papyrusType, method, options));
            }
            return methods;
        }

        private List<PapyrusPropertyDefinition> CreateProperties(TypeDefinition type, PapyrusAssemblyDefinition pex)
        {
            var propList = new List<PapyrusPropertyDefinition>();
            foreach (var prop in type.Properties)
            {
                var properties = Utility.GetFlagsAndProperties(prop);
                var papyrusPropertyDefinition = new PapyrusPropertyDefinition(pex, prop.Name,
                    Utility.GetPapyrusReturnType(prop.PropertyType))
                {
                    Documentation = "".Ref(pex),
                    Userflags = properties.UserFlagsValue
                };

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

                var properties = Utility.GetFlagsAndProperties(field);
                var fieldType = Utility.GetPapyrusReturnType(field.FieldType);
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

        private PapyrusMethodDefinition CreatePapyrusMethodDefinition(PapyrusAssemblyDefinition asm,
            PapyrusTypeDefinition papyrusType,
            MethodDefinition method, PapyrusCompilerOptions options)
        {
            var m = new PapyrusMethodDefinition(asm);
            m.Documentation = "".Ref(asm);
            m.UserFlags = Utility.GetFlagsAndProperties(method).UserFlagsValue;
            m.IsGlobal = method.IsStatic;
            m.IsNative = method.CustomAttributes.Any(i => i.AttributeType.Name.Equals("NativeAttribute"));
            m.Name = method.Name.Ref(asm);
            m.ReturnTypeName = Utility.GetPapyrusReturnType(method.ReturnType).Ref(asm); // method.ReturnType.Name
            m.Parameters = new List<PapyrusParameterDefinition>();
            foreach (var p in method.Parameters)
            {
                m.Parameters.Add(new PapyrusParameterDefinition
                {
                    Name = p.Name.Ref(asm),
                    TypeName = Utility.GetPapyrusReturnType(p.ParameterType, true).Ref(asm)
                });
            }

            var clrVariables = method.Body.Variables;

            foreach (var clrVar in clrVariables)
            {
                var varName = (!string.IsNullOrEmpty(clrVar.Name)
                    ? clrVar.Name
                    : clrVar.ToString()).Ref(asm);
                m.Body.Variables.Add(
                        new PapyrusVariableReference(varName,
                            Utility.GetPapyrusReturnType(clrVar.VariableType.FullName)
                            .Ref(asm)
                        )
                        {
                            Value = varName.Value,
                            ValueType = PapyrusPrimitiveType.Reference
                        }
                    );
            }

            if (method.HasBody)
            {
                ProcessInstructions(method, asm, papyrusType, m, options);

                m.Body.Instructions.RecalculateOffsets();
            }

            return m;
        }

        private void ProcessInstructions(MethodDefinition method, PapyrusAssemblyDefinition asm, PapyrusTypeDefinition papyrusType, PapyrusMethodDefinition m, PapyrusCompilerOptions options)
        {
            var papyrusInstructions =
                instructionProcessor.ProcessInstructions(asm, papyrusType, m, method, method.Body, method.Body.Instructions, options);

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
            pex.Header.SourceHeader.CompileTime = Utility.ConvertToTimestamp(DateTime.Now);
            pex.Header.SourceHeader.ModifyTime = Utility.ConvertToTimestamp(DateTime.Now);
        }
    }
}