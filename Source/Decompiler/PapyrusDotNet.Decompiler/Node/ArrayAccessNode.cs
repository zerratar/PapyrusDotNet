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

using PapyrusDotNet.Decompiler.Interfaces;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Decompiler.Node
{
    public class ArrayAccessNode : BaseNode
    {
        private readonly NodePair array;
        private readonly NodePair index;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ArrayAccessNode" /> class.
        /// </summary>
        /// <param name="instructionOffset">The instruction offset.</param>
        /// <param name="result">The result.</param>
        /// <param name="arrayNode">The field node.</param>
        /// <param name="indexNode">The index node.</param>
        public ArrayAccessNode(int instructionOffset, PapyrusStringTableIndex result, BaseNode arrayNode,
            BaseNode indexNode)
            : base(2, instructionOffset, 0, result)
        {
            array = new NodePair(this, arrayNode);
            index = new NodePair(this, indexNode);
        }

        /// <summary>
        ///     Gets the field.
        /// </summary>
        /// <returns></returns>
        public BaseNode GetArray() => array.SlaveNode;

        /// <summary>
        ///     Gets the index.
        /// </summary>
        /// <returns></returns>
        public BaseNode GetIndex() => index.SlaveNode;

        /// <summary>
        ///     Visits the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public override void Visit(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return "ArrayCreateNode: (" + GetArray() + ")[(" + GetIndex() + ")]";
        }
    }
}