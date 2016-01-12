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
    /// <summary>
    ///     Assign Node for whenever an assignment is expected but is not explicitly made.
    /// </summary>
    /// <example>
    ///     result = MethodCall();
    /// </example>
    public class AssignNode : BaseNode
    {
        private readonly NodePair destination;
        private readonly NodePair value;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AssignNode" /> class.
        /// </summary>
        /// <param name="instructionOffset">The instruction offset.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="value">The value.</param>
        public AssignNode(int instructionOffset, BaseNode destination, BaseNode value)
            : base(2, instructionOffset, 10)
        {
            this.value = new NodePair(this, value);
            this.destination = new NodePair(this, destination);
        }

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

        /// <summary>
        ///     Sets the destination.
        /// </summary>
        /// <param name="node">The node.</param>
        public void SetDestination(BaseNode node) => destination.SetSlave(node);

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
            return "AssignNode: (" + GetDestination() + ") = (" + GetValue() + ")";
        }
    }
}