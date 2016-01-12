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
    public class ArrayCreateNode : BaseNode
    {
        private readonly NodePair size;
        private readonly PapyrusStringTableIndex type;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ArrayCreateNode" /> class.
        /// </summary>
        /// <param name="instructionOffset">The instruction offset.</param>
        /// <param name="result">The result.</param>
        /// <param name="type">The type.</param>
        /// <param name="size">The size.</param>
        public ArrayCreateNode(int instructionOffset, PapyrusStringTableIndex result, PapyrusStringTableIndex type,
            BaseNode size)
            : base(1, instructionOffset, 0, result)
        {
            this.type = type;
            this.size = new NodePair(this, size);
        }

        /// <summary>
        ///     Gets the type of the array.
        /// </summary>
        /// <returns></returns>
        public PapyrusStringTableIndex GetArrayType() => type;

        /// <summary>
        ///     Gets the index.
        /// </summary>
        /// <returns></returns>
        public BaseNode GetIndex() => size.SlaveNode;

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
            return "ArrayCreateNode: new (" + GetArrayType() + ")[(" + GetIndex() + ")]";
        }
    }
}