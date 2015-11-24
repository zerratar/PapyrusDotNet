using System;
using System.Collections.Generic;
using Mono.Cecil;
using PapyrusDotNet.Common;
using PapyrusDotNet.Converters.Clr2Papyrus.Base;
using PapyrusDotNet.Converters.Clr2Papyrus.Implementations;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Classes;
using PapyrusDotNet.PapyrusAssembly.Enums;

namespace PapyrusDotNet.Converters.Clr2Papyrus
{
    public class Clr2PapyrusConverter : ClrToPapyrusConverterBase
    {
        protected override PapyrusAssemblyOutput ConvertAssembly(ClrAssemblyInput input)
        {
            var clr = input.Assembly;
            var mainModule = clr.MainModule;

            var papyrusAssemblies = new List<PapyrusAssemblyDefinition>();
            foreach (var type in mainModule.Types)
            {
                var pex = PapyrusAssemblyDefinition.CreateAssembly(input.TargetPapyrusVersion);

                SetHeaderInfo(input, pex, type);

#warning TODO: THIS!

                // Dummy for now.
                // We want to load these by getting the available attributes used on the type.
                pex.Header.UserflagReferenceHeader.Add("hidden", 0);
                pex.Header.UserflagReferenceHeader.Add("conditional", 0);




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
