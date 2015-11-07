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
using Mono.Cecil;
using PapyrusDotNet.CoreBuilder.Interfaces;

namespace PapyrusDotNet.CoreBuilder.Implementation
{
    public class CoreAssemblyNameResolver : IAssemblyNameResolver
    {
        private AssemblyNameDefinition assemblyNameDefinition;

        /// <summary>
        ///     Resolve the input typeName and returns an appropiate AssemblyNameDefinition
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns>Matching AssemblyNameDefinition</returns>
        public AssemblyNameDefinition Resolve(string typeName)
        {
            return assemblyNameDefinition ??
                   (assemblyNameDefinition = new AssemblyNameDefinition("PapyrusDotNet.Core", new Version(1, 0)));
        }

        /// <summary>
        ///     The target output library filename
        /// </summary>
        public string OutputLibraryFilename => assemblyNameDefinition.Name + ".dll";

        /// <summary>
        ///     The target base namespace to be used
        /// </summary>
        public string BaseNamespace => assemblyNameDefinition.Name;
    }
}