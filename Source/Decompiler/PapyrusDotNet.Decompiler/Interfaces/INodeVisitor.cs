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

using PapyrusDotNet.Decompiler.Node;

#endregion

namespace PapyrusDotNet.Decompiler.Interfaces
{
    public interface INodeVisitor : IVisitor
    {
        bool Result { get; }

        void Visit(ScopeNode node);
        void Visit(BinaryOperatorNode node);
        void Visit(UnaryOperatorNode node);
        void Visit(AssignNode node);
        void Visit(AssignOperatorNode node);
        void Visit(CopyNode node);
        void Visit(CastNode node);
        void Visit(CallMethodNode node);
        void Visit(ParamsNode node);
        void Visit(ReturnNode node);
        void Visit(PropertyAccessNode node);
        void Visit(StructCreateNode node);
        void Visit(ArrayCreateNode node);
        void Visit(ArrayLengthNode node);
        void Visit(ArrayAccessNode node);
        void Visit(ConstantNode node);
        void Visit(IdentifierStringNode node);
        void Visit(WhileNode node);
        void Visit(IfElseNode node);
        void Visit(DeclareNode node);
    }
}