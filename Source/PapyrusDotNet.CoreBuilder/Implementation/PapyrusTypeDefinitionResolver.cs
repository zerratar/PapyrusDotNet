#region License

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

#endregion

#region

using Mono.Cecil;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.CoreBuilder.Interfaces;
using PapyrusDotNet.CoreBuilder.Papyrus.Assembly;

#endregion

namespace PapyrusDotNet.CoreBuilder.Implementation
{
    public class PapyrusTypeDefinitionResolver : IPapyrusTypeDefinitionResolver
    {
        private readonly IAssemblyNameResolver assemblyNameResolver;
        private readonly IPapyrusTypeReferenceResolver referenceResolver;
        private readonly IStatusCallbackService statusCallback;
        private IPapyrusCilAssemblyBuilder builder;

        public PapyrusTypeDefinitionResolver(
            IAssemblyNameResolver assemblyNameResolver,
            IPapyrusTypeReferenceResolver referenceResolver,
            IStatusCallbackService statusCallback)
        {
            this.referenceResolver = referenceResolver;
            this.assemblyNameResolver = assemblyNameResolver;
            this.statusCallback = statusCallback;
        }

        public TypeDefinition Resolve(ModuleDefinition mainModule, PapyrusAssemblyObject input)
        {
            var newType = new TypeDefinition(assemblyNameResolver.BaseNamespace, input.Name, TypeAttributes.Class)
            {
                IsPublic = true
            };


            // newType.DeclaringType = newType;
            if (!string.IsNullOrEmpty(input.ExtendsName))
            {
                newType.BaseType = new TypeReference(assemblyNameResolver.BaseNamespace, input.ExtendsName, mainModule,
                    mainModule);
                // newType.DeclaringType = MainModule.Types.FirstOrDefault(t => t.FullName == newType.BaseType.FullName);
                newType.Scope = mainModule;
            }
            else
            {
                newType.BaseType = mainModule.TypeSystem.Object;
                newType.Scope = mainModule;
            }

            statusCallback.WriteLine("Generating Type '" + assemblyNameResolver.BaseNamespace + "." + input.Name +
                                     "'...");

            foreach (var prop in input.PropertyTable)
            {
                var typeRef = referenceResolver.Resolve(mainModule, null, prop.Type);
                var pro = new PropertyDefinition(prop.Name, PropertyAttributes.HasDefault, typeRef);
                newType.Properties.Add(pro);
            }

            // newType.AddDefaultConstructor();

            builder.AddEmptyConstructor(newType);

            builder.AddVirtualOnInit(newType);

            foreach (var papyrusAsmState in input.States)
            {
                foreach (var papyrusAsmFunction in papyrusAsmState.Functions)
                {
                    var typeReference = referenceResolver.Resolve(mainModule, null, papyrusAsmFunction.ReturnType);
                    // var typeRef = MainModule.TypeSystem.Void;

                    var function = new MethodDefinition(papyrusAsmFunction.Name, MethodAttributes.Public, typeReference)
                    {
                        IsStatic = papyrusAsmFunction.IsStatic
                    };

                    if (function.IsStatic)
                        function.HasThis = false;
                    if (!function.IsStatic && papyrusAsmFunction.Name.StartsWith("On") || papyrusAsmFunction.IsEvent)
                        function.IsVirtual = true;
                    else function.IsVirtual = false;

                    builder.CreateEmptyFunctionBody(ref function);

                    foreach (var par in papyrusAsmFunction.Params)
                    {
                        var resolvedTypeReference = referenceResolver.Resolve(mainModule, null, par.Type);
                        // var typeRefp = MainModule.TypeSystem.Object;

                        var nPar = new ParameterDefinition(par.Name, ParameterAttributes.None, resolvedTypeReference);
                        function.Parameters.Add(nPar);
                    }
                    var skipAdd = false;
                    foreach (var m in newType.Methods)
                    {
                        if (m.Name == function.Name)
                        {
                            if (m.Parameters.Count == function.Parameters.Count)
                            {
                                skipAdd = true;
                                for (var pi = 0; pi < m.Parameters.Count; pi++)
                                {
                                    if (m.Parameters[pi].ParameterType.FullName !=
                                        function.Parameters[pi].ParameterType.FullName) skipAdd = false;
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

        public void Initialize(IPapyrusCilAssemblyBuilder builder)
        {
            this.builder = builder;
        }
    }
}