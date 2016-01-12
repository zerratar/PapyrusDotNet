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
    ///     A Constant Value consistent of an object Value and object Identifier.
    ///     This a value node that can contain constant value and a reference.
    ///     The name Constant is for "Value" part of another instruction and not for typically int, float, string, bool values.
    /// </summary>
    public class ConstantNode : BaseNode
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ConstantNode" /> class.
        /// </summary>
        /// <param name="instructionOffset">The instruction offset.</param>
        /// <param name="constant">The value.</param>
        public ConstantNode(int instructionOffset, PapyrusVariableReference constant)
            : base(0, instructionOffset, 0, null)
        {
            Constant = constant;
        }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        public PapyrusVariableReference Constant { get; set; }

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
            var id = Constant.AsStringTableIndex().Identifier;
            if (string.IsNullOrEmpty(id))
                id = Constant.ToString();
            return "ConstantNode: (" + id + ")";
        }
    }
}