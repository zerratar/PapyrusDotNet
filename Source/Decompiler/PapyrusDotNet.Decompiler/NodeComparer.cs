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

using PapyrusDotNet.Decompiler.HelperClasses;
using PapyrusDotNet.Decompiler.Interfaces;
using PapyrusDotNet.Decompiler.Node;

#endregion

namespace PapyrusDotNet.Decompiler
{
    public class NodeComparer : INodeVisitor
    {
        private readonly BaseNode reference;
        private readonly DynamicVisitor visitor;

        public NodeComparer(BaseNode reference)
        {
            this.reference = reference;
            Result = false;
            visitor = new DynamicVisitor();
            visitor.Common((n, v) => { });
        }

        public bool Result { get; set; }

        public void Visit(ScopeNode node)
        {
            visitor.OnVisit((n, v) => Result = IsSameChildren(n, node));
            reference.Visit(visitor);
        }

        public void Visit(BinaryOperatorNode node)
        {
            visitor.OnVisit((n, v) =>
            {
                var r = n as BinaryOperatorNode;
                if (r != null && r.GetOperator() == node.GetOperator())
                    Result = IsSameChildren(r, node);
            });
            reference.Visit(visitor);
        }

        public void Visit(UnaryOperatorNode node)
        {
            visitor.OnVisit((n, v) =>
            {
                var r = n as UnaryOperatorNode;
                if (r != null && r.GetOperator() == node.GetOperator())
                    Result = IsSameChildren(r, node);
            });
            reference.Visit(visitor);
        }

        public void Visit(AssignNode node)
        {
            visitor.OnVisit((n, v) => Result = IsSameChildren(n, node));
            reference.Visit(visitor);
        }

        public void Visit(AssignOperatorNode node)
        {
            visitor.OnVisit((n, v) => Result = IsSameChildren(n, node));
            reference.Visit(visitor);
        }

        public void Visit(CopyNode node)
        {
            visitor.OnVisit((n, v) => Result = IsSameChildren(n, node));
            reference.Visit(visitor);
        }

        public void Visit(CastNode node)
        {
            visitor.OnVisit((n, v) =>
            {
                var r = n as CastNode;
                if (r != null && r.GetCastType() == node.GetCastType())
                    Result = IsSameChildren(r, node);
            });
            reference.Visit(visitor);
        }

        public void Visit(CallMethodNode node)
        {
            visitor.OnVisit((n, v) =>
            {
                var r = n as CallMethodNode;
                if (r != null && r.GetMethod() == node.GetMethod())
                    Result = IsSameChildren(r, node);
            });
            reference.Visit(visitor);
        }

        public void Visit(ParamsNode node)
        {
            visitor.OnVisit((n, v) => Result = IsSameChildren(n, node));
            reference.Visit(visitor);
        }

        public void Visit(ReturnNode node)
        {
            visitor.OnVisit((n, v) => Result = IsSameChildren(n, node));
            reference.Visit(visitor);
        }

        public void Visit(PropertyAccessNode node)
        {
            visitor.OnVisit((n, v) =>
            {
                var r = n as PropertyAccessNode;
                if (r != null && r.GetProperty() == node.GetProperty())
                    Result = IsSameChildren(r, node);
            });
            reference.Visit(visitor);
        }

        public void Visit(StructCreateNode node)
        {
            visitor.OnVisit((n, v) => Result = (n as StructCreateNode)?.GetStructType() == node.GetStructType());
            reference.Visit(visitor);
        }

        public void Visit(ArrayCreateNode node)
        {
            visitor.OnVisit((n, v) =>
            {
                var r = n as ArrayCreateNode;
                if (r != null && r.GetArrayType() == node.GetArrayType())
                    Result = IsSameChildren(r, node);
            });
            reference.Visit(visitor);
        }

        public void Visit(ArrayLengthNode node)
        {
            visitor.OnVisit((n, v) => Result = IsSameChildren(n, node));
            reference.Visit(visitor);
        }

        public void Visit(ArrayAccessNode node)
        {
            visitor.OnVisit((n, v) => Result = IsSameChildren(n, node));
            reference.Visit(visitor);
        }

        public void Visit(ConstantNode node)
        {
            visitor.OnVisit((n, v) => Result = (n as ConstantNode)?.Constant == node.Constant);
            reference.Visit(visitor);
        }

        public void Visit(IdentifierStringNode node)
        {
            visitor.OnVisit((n, v) => Result = (n as IdentifierStringNode)?.GetIdentifier() == node.GetIdentifier());
            reference.Visit(visitor);
        }

        public void Visit(WhileNode node)
        {
            visitor.OnVisit((n, v) => Result = IsSameChildren(n, node));
            reference.Visit(visitor);
        }

        public void Visit(IfElseNode node)
        {
            visitor.OnVisit((n, v) => Result = IsSameChildren(n, node));
            reference.Visit(visitor);
        }

        public void Visit(DeclareNode node)
        {
            visitor.OnVisit(
                (n, v) =>
                    Result = (n as DeclareNode)?.GetDeclareType() == node.GetDeclareType() && IsSameChildren(n, node));
            reference.Visit(visitor);
        }

        public static bool IsSameTree<T, T2>(T left, T2 right) where T : BaseNode where T2 : BaseNode
        {
            var nodeComparer = new NodeComparer(left);
            right.Visit(nodeComparer);
            return nodeComparer.Result;
        }

        public static bool IsSameChildren(BaseNode nref, BaseNode node)
        {
            if (nref.Size == node.Size)
            {
                for (var i = 0; i < nref.Size; ++i)
                {
                    if (nref[i] != node[i] && !IsSameTree(nref[i], node[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}