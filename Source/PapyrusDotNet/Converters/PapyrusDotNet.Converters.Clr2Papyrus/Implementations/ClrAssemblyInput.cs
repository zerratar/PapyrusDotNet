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
//     Copyright 2016, Karl Patrik Johansson, zerratar@gmail.com

#region

using Mono.Cecil;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations
{
    public class ClrAssemblyInput : IAssemblyInput
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ClrAssemblyInput" /> class.
        /// </summary>
        /// <param name="asm">The asm.</param>
        /// <param name="targetPapyrusVersion">The target papyrus version.</param>
        public ClrAssemblyInput(AssemblyDefinition asm, PapyrusVersionTargets targetPapyrusVersion)
        {
            Assembly = asm;
            TargetPapyrusVersion = targetPapyrusVersion;
        }

        /// <summary>
        ///     Gets the assembly.
        /// </summary>
        /// <value>
        ///     The assembly.
        /// </value>
        public AssemblyDefinition Assembly { get; }

        /// <summary>
        ///     Gets the target papyrus version.
        /// </summary>
        /// <value>
        ///     The target papyrus version.
        /// </value>
        public PapyrusVersionTargets TargetPapyrusVersion { get; }
    }
}