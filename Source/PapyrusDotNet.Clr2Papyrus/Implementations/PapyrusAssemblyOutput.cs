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

using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations
{
    public class PapyrusAssemblyOutput : IAssemblyOutput
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PapyrusAssemblyOutput" /> class.
        /// </summary>
        /// <param name="papyrusAssemblyDefinition">The papyrus assembly definition.</param>
        public PapyrusAssemblyOutput(PapyrusAssemblyDefinition[] papyrusAssemblyDefinition)
        {
            Assemblies = papyrusAssemblyDefinition;
        }

        /// <summary>
        ///     Gets the output papyrus assemblies.
        /// </summary>
        /// <value>
        ///     The assemblies.
        /// </value>
        public PapyrusAssemblyDefinition[] Assemblies { get; }


        public void Save(string output)
        {
            //foreach (var asm in papyrusAssemblyDefinition)
            //{
            //    asm.Write(Path.Combine(output, asm.Types.FirstOrDefault().Name + ".pex"));
            //}
        }
    }
}