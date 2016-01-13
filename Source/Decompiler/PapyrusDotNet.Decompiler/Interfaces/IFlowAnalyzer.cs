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

using PapyrusDotNet.Decompiler.Node;

#endregion

namespace PapyrusDotNet.Decompiler.Interfaces
{
    public interface IFlowAnalyzer
    {
        /// <summary>
        ///     Finializes the tree by declaring variables and cleaning up conditional statements and more.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <returns></returns>
        BaseNode Finalize(BaseNode tree);

        /// <summary>
        ///     Builds the flow tree.
        /// </summary>
        /// <returns></returns>
        BaseNode RebuildControlFlow(int startBlock, int endBlock);

        /// <summary>
        ///     Sets the decompiler context.
        /// </summary>
        /// <param name="context">The context.</param>
        void SetContext(IDecompilerContext context);

        /// <summary>
        ///     Builds the blocks.
        /// </summary>
        void CreateFlowBlocks();

        /// <summary>
        ///     Rebuilds the expressions.
        /// </summary>
        void RebuildExpressionsInBlocks();

        /// <summary>
        /// Rebuils the boolean operators.
        /// </summary>
        /// <param name="startBlock">The start block.</param>
        /// <param name="endBlock">The end block.</param>
        /// <param name="depth">The depth.</param>
        void RebuildBooleanOperators(int startBlock, int endBlock, int depth = 0);

        /// <summary>
        ///     Builds the type index, mapping a table reference to its type.
        /// </summary>
        void FindVarTypes();

        /// <summary>
        /// Maps the long lived temporary variables.
        /// </summary>
        void MapLongLivedTempVariables();

        /// <summary>
        /// Assigns the long lived temporary variables.
        /// </summary>
        void AssignLongLivedTempVariables();
    }
}