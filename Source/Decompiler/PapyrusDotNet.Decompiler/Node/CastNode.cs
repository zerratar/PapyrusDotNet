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
    public class CastNode : BaseNode
    {
        private readonly PapyrusStringTableIndex type;
        private readonly NodePair value;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CastNode" /> class.
        /// </summary>
        /// <param name="instructionOffset">The instruction offset.</param>
        /// <param name="result">The result.</param>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        public CastNode(int instructionOffset, PapyrusStringTableIndex result, BaseNode value,
            PapyrusStringTableIndex type)
            : base(1, instructionOffset, 1, result)
        {
            this.type = type;
            this.value = new NodePair(this, value);
        }

        /// <summary>
        ///     Gets the type of the cast.
        /// </summary>
        /// <returns></returns>
        public PapyrusStringTableIndex GetCastType() => type;

        /// <summary>
        ///     Gets the value.
        /// </summary>
        /// <returns></returns>
        public BaseNode GetValue() => value.SlaveNode;

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
            return "CastNode: (" + GetValue() + ") as " + GetCastType().Identifier;
        }
    }
}