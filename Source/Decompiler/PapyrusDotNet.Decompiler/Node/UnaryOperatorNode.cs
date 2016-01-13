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
    ///     A Unary Operator that expects a destination and one constant target using
    ///     the +, -, !, ~, ++, --, and cast operators.
    /// </summary>
    /// <example>
    ///     var myValue++;
    ///     var myOtherValue = -25;
    /// </example>
    /// <remarks>INEG, FNEG, NOT, CAST</remarks>
    public class UnaryOperatorNode : BaseNode
    {
        private readonly string op;
        private readonly NodePair value;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnaryOperatorNode" /> class.
        /// </summary>
        /// <param name="instructionOffset">The instruction offset.</param>
        /// <param name="precedence">The precedence.</param>
        /// <param name="result">The result.</param>
        /// <param name="op">The unary operator</param>
        /// <param name="value">The value.</param>
        public UnaryOperatorNode(int instructionOffset, int precedence, PapyrusStringTableIndex result, string op,
            BaseNode value)
            : base(1, instructionOffset, precedence, result)
        {
            this.op = op;
            this.value = new NodePair(this, value);
        }

        /// <summary>
        ///     Gets the operator.
        /// </summary>
        /// <returns></returns>
        public string GetOperator() => op;

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

        public void SetValue(BaseNode node) => value.SetSlave(node);
    }
}