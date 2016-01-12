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
    ///     A Binary Operator that expects a destination and two constant targets using
    ///     the +, -, *, %, / operators.
    /// </summary>
    /// <example>
    ///     var myValue = 2515 + 58282;
    ///     var myOtherValue = 222 * aVar;
    /// </example>
    /// <remarks>IMUL, FMUL, FADD, IADD, IDIV, FDIV, IMOD, ISUB, FSUB, STRCAT</remarks>
    public class BinaryOperatorNode : BaseNode
    {
        private readonly NodePair left;
        private readonly string op;
        private readonly NodePair right;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BinaryOperatorNode" /> class.
        /// </summary>
        /// <param name="instructionOffset">The instruction offset.</param>
        /// <param name="precedence">The precedence.</param>
        /// <param name="result">The result.</param>
        /// <param name="left">The left.</param>
        /// <param name="op">The op.</param>
        /// <param name="right">The right.</param>
        public BinaryOperatorNode(int instructionOffset, int precedence, PapyrusStringTableIndex result, BaseNode left,
            string op, BaseNode right)
            : base(2, instructionOffset, precedence, result)
        {
            this.left = new NodePair(this, left);
            this.right = new NodePair(this, right);
            this.op = op;
        }

        /// <summary>
        ///     Gets the operator.
        /// </summary>
        /// <returns></returns>
        public string GetOperator() => op;

        /// <summary>
        ///     Gets the left.
        /// </summary>
        /// <returns></returns>
        public BaseNode GetLeft() => left.SlaveNode;

        /// <summary>
        ///     Gets the right.
        /// </summary>
        /// <returns></returns>
        public BaseNode GetRight() => right.SlaveNode;

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
            return "BinaryOpNode: (" + GetLeft() + ") " + GetOperator() + " (" + GetRight() + ")";
        }
    }
}