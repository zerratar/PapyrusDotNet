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

using PapyrusDotNet.Common;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.CoreBuilder.Implementation;
using PapyrusDotNet.CoreBuilder.Interfaces;
using PapyrusDotNet.CoreBuilder.Papyrus.Assembly;
using PapyrusDotNet.CoreBuilder.Papyrus.Script;

#endregion

namespace PapyrusDotNet.CoreBuilder
{
    public class PapyrusCilAssemblyBuilder : PapyrusCilAssemblyBuilderBase
    {
        public PapyrusCilAssemblyBuilder()
        {
            AssemblyNameResolver = new CoreAssemblyNameResolver();

            var statusCallBack = new ConsoleStatusCallbackService();
            var nameResolver = new DictionaryPapyrusNameResolver(statusCallBack);
            var namespaceResolver = new PapyrusNamespaceResolver(AssemblyNameResolver);
            var typeNameResolver = new PapyrusTypeNameResolver();

            PapyrusScriptParser = new PapyrusScriptParser(nameResolver);
            PapyrusAssemblyParser = new PapyrusAssemblyParser(nameResolver);

            TypeReferenceResolver = new PapyrusTypeReferenceResolver(namespaceResolver, typeNameResolver);
            TypeDefinitionResolver = new PapyrusTypeDefinitionResolver(AssemblyNameResolver, TypeReferenceResolver,
                statusCallBack);

            TypeDefinitionResolver.Initialize(this);
            TypeReferenceResolver.Initialize(this);
        }

        public PapyrusCilAssemblyBuilder(IPapyrusScriptParser scriptParser, IPapyrusAssemblyParser assemblyParser,
            IPapyrusTypeDefinitionResolver typeDefinitionResolver, IPapyrusTypeReferenceResolver typeReferenceResolver,
            IAssemblyNameResolver nameResolver, IStatusCallbackService callback)
            : base(scriptParser, assemblyParser, typeDefinitionResolver, typeReferenceResolver, nameResolver, callback)
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
    }
}