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

#endregion

namespace PapyrusDotNet.Decompiler.Node
{
    public class AssignOperatorNode : BaseNode
    {
        private readonly NodePair destination;
        private readonly string op;
        private readonly NodePair value;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AssignNode" /> class.
        /// </summary>
        /// <param name="instructionOffset">The instruction offset.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="op">Operator</param>
        /// <param name="expr">The expression.</param>
        public AssignOperatorNode(int instructionOffset, BaseNode destination, string op, BaseNode expr)
            : base(2, instructionOffset, 10)
        {
            this.op = op;
            value = new NodePair(this, expr);
            this.destination = new NodePair(this, destination);
        }

        /// <summary>
        ///     Visits the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public override void Visit(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <summary>
        ///     Gets the operator.
        /// </summary>
        /// <returns></returns>
        public string GetOperator() => op;

        /// <summary>
        ///     Gets the destination.
        /// </summary>
        /// <returns></returns>
        public BaseNode GetDestination() => destination.SlaveNode;

        /// <summary>
        ///     Gets the value.
        /// </summary>
        /// <returns></returns>
        public BaseNode GetValue() => value.SlaveNode;
    }
}