//     This file is part of PapyrusDotNet.
//     But is a port of Champollion, https://github.com/Orvid/Champollion
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
//     Copyright © 2016, Karl Patrik Johansson, zerratar@gmail.com
//     Copyright © 2015, Orvid King
//     Copyright © 2013, Paul-Henry Perrin

#region

using PapyrusDotNet.Decompiler.HelperClasses;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Decompiler.Interfaces
{
    public interface IDecompilerContext
    {
        /// <summary>
        ///     Gets the temporary string table.
        /// </summary>
        PapyrusStringTable TempStringTable { get; }

        /// <summary>
        ///     Gets the source assembly.
        /// </summary>
        /// <returns></returns>
        PapyrusAssemblyDefinition GetSourceAssembly();

        /// <summary>
        ///     Gets the code blocks.
        /// </summary>
        /// <returns></returns>
        Map<int, PapyrusCodeBlock> GetCodeBlocks();

        /// <summary>
        ///     Sets the target method.
        /// </summary>
        /// <param name="method">The method.</param>
        void SetTargetMethod(PapyrusMethodDefinition method);

        /// <summary>
        ///     Gets the target method.
        /// </summary>
        /// <returns></returns>
        PapyrusMethodDefinition GetTargetMethod();

        /// <summary>
        ///     Gets the decompiler.
        /// </summary>
        /// <returns></returns>
        IPapyrusDecompiler GetDecompiler();

        /// <summary>
        ///     Gets the flow analyzer.
        /// </summary>
        /// <returns></returns>
        IFlowAnalyzer GetFlowAnalyzer();

        /// <summary>
        ///     Builds the temporary string table.
        /// </summary>
        void BuildTempStringTable();
    }
}