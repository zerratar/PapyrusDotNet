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
    public class WhileNode : BaseNode
    {
        private readonly NodePair body;
        private readonly NodePair condition;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WhileNode" /> class.
        /// </summary>
        /// <param name="instructionOffset">The instruction offset.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="body">The body.</param>
        public WhileNode(int instructionOffset, BaseNode condition, BaseNode body)
            : base(2, instructionOffset, 10)
        {
            this.condition = new NodePair(this, condition);
            this.body = new NodePair(this, body);
        }

        /// <summary>
        ///     Gets the condition.
        /// </summary>
        /// <returns></returns>
        public BaseNode GetCondition() => condition.SlaveNode;

        /// <summary>
        ///     Gets the body.
        /// </summary>
        /// <returns></returns>
        public BaseNode GetBody() => body.SlaveNode;

        /// <summary>
        ///     Computes the instruction bounds.
        /// </summary>
        public override void ComputeInstructionBounds()
        {
            base.ComputeInstructionBounds();

            if (GetCondition().GetBegin() == -1 && GetCondition().GetEnd() == -1 && GetBody().Size != 0)
            {
                begin = GetBody().GetBegin() - 1;
                end = begin;
            }
            else
            {
                begin = GetCondition().GetBegin();
                end = GetCondition().GetEnd() + 1;
            }
        }

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