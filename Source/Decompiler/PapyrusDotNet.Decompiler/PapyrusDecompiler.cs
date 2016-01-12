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

using System;
using System.Collections.Generic;
using PapyrusDotNet.Decompiler.Interfaces;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Decompiler
{
    public class PapyrusDecompiler : IPapyrusDecompiler
    {
        private readonly PapyrusAssemblyDefinition asm;
        private readonly ICodeGenerator codeGenerator;
        private readonly IFlowAnalyzer flowAnalyzer;
        private IDecompilerContext decompilerContext;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PapyrusDecompiler" /> class.
        /// </summary>
        /// <param name="asm">The asm.</param>
        public PapyrusDecompiler(PapyrusAssemblyDefinition asm)
        {
            this.asm = asm;
            flowAnalyzer = new PapyrusFlowAnalyzer();
            codeGenerator = new PapyrusCodeGenerator();
        }

        /// <summary>
        ///     Gets the current context or creates a new one if none exists.
        /// </summary>
        /// <returns></returns>
        public IDecompilerContext CreateContext()
        {
            if (decompilerContext == null)
            {
                decompilerContext = new DecompilerContext(this, flowAnalyzer, asm);
                decompilerContext.BuildTempStringTable();
            }
            return decompilerContext;
        }

        /// <summary>
        ///     Decompiles the papyrus assembly available in the context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public IPapyrusDecompilerResult Decompile(IDecompilerContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Merges the results.
        /// </summary>
        /// <param name="methodResults">The results.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IPapyrusDecompilerResult MergeResults(IEnumerable<IPapyrusDecompilerResult> methodResults)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Decompiles the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="method">The method.</param>
        /// <returns></returns>
        public IPapyrusDecompilerResult Decompile(IDecompilerContext context, PapyrusMethodDefinition method)
        {
            context.SetTargetMethod(method);


            flowAnalyzer.FindVarTypes();

            flowAnalyzer.CreateFlowBlocks();

            flowAnalyzer.RebuildExpressionsInBlocks();

            flowAnalyzer.RebuildBooleanOperators(0, method.Body.Instructions.Count);

            var tree = flowAnalyzer.RebuildControlFlow(0, method.Body.Instructions.Count);

            tree = flowAnalyzer.Finalize(tree);

            var result = codeGenerator.Generate(tree);

            var sourceCode = codeGenerator.GetMethodSourceCode(result, method);


            return new PapyrusDecompilerResult(sourceCode, null);
        }
    }
}