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
    /// <summary>
    /// </summary>
    public class PropertyAccessNode : BaseNode
    {
        private readonly NodePair objectNode;
        private readonly PapyrusStringTableIndex property;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyAccessNode" /> class.
        /// </summary>
        /// <param name="instructionOffset">The instruction offset.</param>
        /// <param name="result">The result.</param>
        /// <param name="objectNode">The object node.</param>
        /// <param name="property">The property.</param>
        public PropertyAccessNode(int instructionOffset, PapyrusStringTableIndex result, BaseNode objectNode,
            PapyrusStringTableIndex property)
            : base(1, instructionOffset, 0, result)
        {
            this.property = property;
            this.objectNode = new NodePair(this, objectNode);
        }

        /// <summary>
        ///     Gets the property.
        /// </summary>
        /// <returns></returns>
        public PapyrusStringTableIndex GetProperty() => property;

        /// <summary>
        ///     Gets the object.
        /// </summary>
        /// <returns></returns>
        public BaseNode GetObject() => objectNode.SlaveNode;

        /// <summary>
        ///     Visits the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public override void Visit(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}