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

using System.Collections.Generic;
using PapyrusDotNet.Decompiler.HelperClasses;
using PapyrusDotNet.Decompiler.Interfaces;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Decompiler
{
    public class DecompilerContext : IDecompilerContext
    {
        private readonly PapyrusAssemblyDefinition asm;
        private readonly IPapyrusDecompiler decompiler;
        private readonly IFlowAnalyzer flowAnalyzer;
        private Map<int, PapyrusCodeBlock> codeBlocks;
        private PapyrusMethodDefinition targetMethod;
        private List<string> longLivedTempVars;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DecompilerContext" /> class.
        /// </summary>
        /// <param name="decompiler">The decompiler.</param>
        /// <param name="flowAnalyzer"></param>
        /// <param name="asm">The asm.</param>
        public DecompilerContext(IPapyrusDecompiler decompiler, IFlowAnalyzer flowAnalyzer,
            PapyrusAssemblyDefinition asm)
        {
            this.decompiler = decompiler;
            this.flowAnalyzer = flowAnalyzer;
            this.asm = asm;
            TempStringTable = new PapyrusStringTable();
            codeBlocks = new Map<int, PapyrusCodeBlock>();
            flowAnalyzer.SetContext(this);
        }

        /// <summary>
        ///     Gets the source assembly.
        /// </summary>
        /// <returns></returns>
        public PapyrusAssemblyDefinition GetSourceAssembly()
        {
            return asm;
        }

        /// <summary>
        ///     Gets the code blocks.
        /// </summary>
        /// <returns></returns>
        public Map<int, PapyrusCodeBlock> GetCodeBlocks()
        {
            return codeBlocks;
        }

        /// <summary>
        ///     Gets the target method.
        /// </summary>
        /// <returns></returns>
        public PapyrusMethodDefinition GetTargetMethod()
        {
            return targetMethod;
        }

        /// <summary>
        ///     Gets the decompiler.
        /// </summary>
        /// <returns></returns>
        public IPapyrusDecompiler GetDecompiler()
        {
            return decompiler;
        }

        /// <summary>
        ///     Gets the flow analyzer.
        /// </summary>
        /// <returns></returns>
        public IFlowAnalyzer GetFlowAnalyzer()
        {
            return flowAnalyzer;
        }

        /// <summary>
        ///     Gets the temporary string table.
        /// </summary>
        public PapyrusStringTable TempStringTable { get; private set; }

        /// <summary>
        ///     Builds the temporary string table.
        /// </summary>
        public void BuildTempStringTable()
        {
            TempStringTable = new PapyrusStringTable();
            TempStringTable.Add("true");
            TempStringTable.Add("false");

            TempStringTable.Add("find");
            TempStringTable.Add("rfind");
            TempStringTable.Add("add");
            TempStringTable.Add("insert");
            TempStringTable.Add("removelast");
            TempStringTable.Add("remove");
            TempStringTable.Add("clear");
        }

        /// <summary>
        /// Gets the long lived temporary variables.
        /// </summary>
        /// <returns></returns>
        public List<string> GetLongLivedTempVariables()
        {
            return longLivedTempVars;
        }

        /// <summary>
        /// Sets the long lived temporary variables.
        /// </summary>
        /// <param name="list">The list.</param>
        public void SetLongLivedTempVariables(List<string> list)
        {
            longLivedTempVars = list;
        }

        /// <summary>
        ///     Sets the target method.
        /// </summary>
        /// <param name="method">The method.</param>
        public void SetTargetMethod(PapyrusMethodDefinition method)
        {
            targetMethod = method;
            longLivedTempVars = new List<string>();
            codeBlocks = new Map<int, PapyrusCodeBlock>();
        }
    }
}