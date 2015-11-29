using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mono.Cecil;
using PapyrusDotNet.Common;
using PapyrusDotNet.Converters.Clr2Papyrus.Base;
using PapyrusDotNet.Converters.Clr2Papyrus.Implementations;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Classes;
using PapyrusDotNet.PapyrusAssembly.Enums;
using PapyrusDotNet.PapyrusAssembly.Extensions;

namespace PapyrusDotNet.Converters.Clr2Papyrus
{
    public class Clr2PapyrusConverter : Clr2PapyrusConverterBase
    {
        private readonly IClr2PapyrusInstructionProcessor instructionProcessor;

        public Clr2PapyrusConverter(IClr2PapyrusInstructionProcessor instructionProcessor)
        {
            this.instructionProcessor = instructionProcessor;
        }

        /// <summary>
        /// Converts the assembly.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        protected override PapyrusAssemblyOutput ConvertAssembly(ClrAssemblyInput input)
        {
            var clr = input.Assembly;
            var mainModule = clr.MainModule;

            var papyrusAssemblies = new List<PapyrusAssemblyDefinition>();
            foreach (var type in mainModule.Types)
            {
                // We will skip this one for now
                // as it will not really provide us with any necessary information at this early stage.
                if (type.Name == "<Module>") continue;

                var pex = PapyrusAssemblyDefinition.CreateAssembly(input.TargetPapyrusVersion);

                SetHeaderInfo(input, pex, type);

                CreateDebugInfo(pex, type);

                CreateType(pex, type);

                // pex.Header
                papyrusAssemblies.Add(pex);
            }

            return new PapyrusAssemblyOutput(papyrusAssemblies.ToArray());
        }

        private void CreateType(PapyrusAssemblyDefinition pex, TypeDefinition type)
        {
            var newType = new PapyrusTypeDefinition(pex);
            var autoState = new PapyrusStateDefinition(newType);

            newType.Name = type.Name.Ref(pex);
            newType.AutoStateName = "".Ref(pex);

            newType.BaseTypeName = type.BaseType != null
                ? Utility.GetPapyrusBaseType(type.BaseType).Ref(pex)
                : "".Ref(pex);

            UpdateUserFlags(type, pex);

            // Create Properties
            CreateProperties(type, pex).ForEach(newType.Properties.Add);

            // Create Properties
            CreateFields(type, pex).ForEach(newType.Fields.Add);

            // Create Methods
            CreateMethods(type, pex).ForEach(autoState.Methods.Add);
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

            foreach (var prop in type.Properties)
            {
                // For Each State, add the following thingy
                //
                //property.
                //debug.PropertyDescriptions.Add(property);
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

        private List<PapyrusMethodDefinition> CreateMethods(TypeDefinition type, PapyrusAssemblyDefinition pex)
        {
            var methods = new List<PapyrusMethodDefinition>();
            foreach (var method in type.Methods)
            {
                methods.Add(CreatePapyrusMethodDefinition(pex, method));
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
                var properties = Utility.GetFlagsAndProperties(field);
                var papyrusFieldDefinition = new PapyrusFieldDefinition(pex, field.Name,
                    Utility.GetPapyrusReturnType(field.FieldType))
                {
                    UserFlags = properties.UserFlagsValue
                };
                fields.Add(papyrusFieldDefinition);
            }
            return fields;
        }

        private PapyrusMethodDefinition CreatePapyrusMethodDefinition(PapyrusAssemblyDefinition asm, MethodDefinition method)
        {
            var m = new PapyrusMethodDefinition(asm);
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

            if (method.HasBody)
            {
                ProcessInstructions(method, m);
            }

            return m;
        }

        private void ProcessInstructions(MethodDefinition method, PapyrusMethodDefinition m)
        {
            foreach (var instruction in method.Body.Instructions)
            {

            }
            var papyrusInstructions =
                instructionProcessor.ProcessInstructions(method, method.Body, method.Body.Instructions);

            m.Body.Instructions.AddRange(papyrusInstructions);
        }

        private static void SetHeaderInfo(ClrAssemblyInput input, PapyrusAssemblyDefinition pex, TypeDefinition type)
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
