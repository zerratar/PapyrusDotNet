using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mono.Cecil;
using PapyrusDotNet.Common;
using PapyrusDotNet.Converters.Clr2Papyrus.Base;
using PapyrusDotNet.Converters.Clr2Papyrus.Implementations;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Classes;
using PapyrusDotNet.PapyrusAssembly.Enums;

namespace PapyrusDotNet.Converters.Clr2Papyrus
{
    public class Clr2PapyrusConverter : Clr2PapyrusConverterBase
    {
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


#warning TODO: THIS!


                pex.HasDebugInfo = true;

                var autoState = new PapyrusStateDefinition();
                var newType = new PapyrusTypeDefinition();

                newType.Name = (PapyrusStringRef)type.Name;
                newType.AutoStateName = (PapyrusStringRef)"";

                if (type.BaseType != null)
                    newType.BaseTypeName = (PapyrusStringRef)type.BaseType.Name;



                // Dummy for now.
                // We want to load these by getting the available attributes used on the type.
                pex.Header.UserflagReferenceHeader.Add("hidden", 0);
                pex.Header.UserflagReferenceHeader.Add("conditional", 1);

                foreach (var prop in type.Properties)
                {
                    var papyrusPropertyDefinition = new PapyrusPropertyDefinition(prop.Name, prop.PropertyType.Name);
                    newType.Properties.Add(papyrusPropertyDefinition);
                }

                foreach (var field in type.Fields)
                {
                    newType.Fields.Add(new PapyrusFieldDefinition(field.Name, field.FieldType.Name));
                }

                autoState.Methods = new Collection<PapyrusMethodDefinition>();
                foreach (var method in type.Methods)
                {
                    var m = new PapyrusMethodDefinition();
                    m.Name = (PapyrusStringRef)method.Name;
                    m.ReturnTypeName = (PapyrusStringRef)method.ReturnType.Name;
                    m.Parameters = new List<PapyrusParameterDefinition>();
                    foreach (var p in method.Parameters)
                    {
                        m.Parameters.Add(new PapyrusParameterDefinition
                        {
                            Name = (PapyrusStringRef)p.Name,
                            TypeName = (PapyrusStringRef)p.ParameterType.Name
                        });
                    }

                    autoState.Methods.Add(m);
                }
                newType.States.Add(autoState);
                pex.Types.Add(newType);


                // pex.Header
                papyrusAssemblies.Add(pex);
            }

            return new PapyrusAssemblyOutput(papyrusAssemblies.ToArray());
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
