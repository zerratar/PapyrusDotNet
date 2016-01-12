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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic.CompilerServices;
using PapyrusDotNet.Decompiler.HelperClasses;
using PapyrusDotNet.Decompiler.Interfaces;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Decompiler.Node
{
    public class BaseNode : IVisitable
    {
        private readonly int childCount;
        private readonly bool fixedSize;
        private readonly int instructionOffset;
        private readonly int precedence;
        private readonly PapyrusStringTableIndex result;
        protected int begin;
        protected int end;
        private readonly int expectedFixedSize;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseNode" /> class.
        /// </summary>
        /// <param name="childCount">The child count.</param>
        /// <param name="instructionOffset">The instruction offset.</param>
        /// <param name="precedence">The precedence.</param>
        /// <param name="result">The result.</param>
        public BaseNode(int childCount, int instructionOffset, int precedence, PapyrusStringTableIndex result = null)
        {
            if (result == null) result = new PapyrusStringTableIndex();
            fixedSize = childCount != 0;
            expectedFixedSize = childCount;
            if (fixedSize)
                Children = new List<BaseNode>(childCount);
            else Children = new List<BaseNode>();
            this.childCount = childCount;
            this.instructionOffset = instructionOffset;
            begin = instructionOffset;
            end = instructionOffset;
            this.precedence = precedence;
            this.result = result;
        }

        public int Size => Children.Count;

        /// <summary>
        ///     Gets the children.
        /// </summary>
        public List<BaseNode> Children { get; }

        /// <summary>
        ///     Gets the parent.
        /// </summary>
        public BaseNode Parent { get; private set; }

        /// <summary>
        ///     Gets or sets the <see cref="PapyrusDotNet.Decompiler.Node.BaseNode" /> at the specified index.
        /// </summary>
        /// <value>
        ///     The <see cref="PapyrusDotNet.Decompiler.Node.BaseNode" />.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public BaseNode this[int index]
        {
            get
            {
                EnsureChildSize(index);
                return Children[index];
            }
            set
            {
                EnsureChildSize(index);
                Children[index] = value;
            }
        }

        /// <summary>
        ///     Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public void Visit(BaseNode visitor)
        {
            visitor.Visit(this);
        }

        public BaseNode First() => Children.FirstOrDefault();

        public BaseNode Last() => Children.LastOrDefault();

        public int GetChildCount(Func<BaseNode, bool> predicate = null)
        {
            if (predicate != null)
                return Children.Count(predicate);
            return Children.Count;
        }

        /// <summary>
        ///     Gets the begin.
        /// </summary>
        /// <returns></returns>
        public int GetBegin() => begin;

        /// <summary>
        ///     Gets the end.
        /// </summary>
        /// <returns></returns>
        public int GetEnd() => end;

        /// <summary>
        ///     Gets the precedence.
        /// </summary>
        /// <returns></returns>
        public int GetPrecedence() => precedence;

        /// <summary>
        ///     Gets the result.
        /// </summary>
        /// <returns></returns>
        public PapyrusStringTableIndex GetResult() => result;

        /// <summary>
        ///     Visits the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public void Visit(Visitor visitor)
        {
            visitor.Visit(this);
        }

        public virtual void Visit(INodeVisitor visitor)
        {
        }

        /// <summary>
        ///     Merges the children from the source Node and removes all the children from the source node.
        /// </summary>
        /// <param name="source">The source.</param>
        public void MergeChildren(BaseNode source)
        {
            foreach (var c in source.Children)
            {
                c.Parent = this;
                Children.Add(c);
            }
            source.Children.Clear();
        }

        /// <summary>
        ///     Adopts the specified node and makes this instance as the new parent.
        /// </summary>
        /// <param name="node">The node.</param>
        public BaseNode Adopt(BaseNode node)
        {
            var oldParent = node.Parent;
            if (oldParent != null)
                oldParent.Children.Remove(node);

            Children.Add(node);
            node.Parent = this;
            return this;
        }

        /// <summary>
        ///     Replaces the old node with the new old.
        /// </summary>
        /// <param name="oldNode">The old node.</param>
        /// <param name="newNode">The new node.</param>
        public void Replace(BaseNode oldNode, BaseNode newNode)
        {
            if (oldNode.Parent != this)
                throw new InvalidOperationException("The '" + nameof(oldNode) + "' is not a child of this node.");
            if (oldNode == newNode)
                throw new InvalidOperationException("The '" + nameof(newNode) +
                                                    "' is the same node as you're trying to replace.");
            var parent = newNode.Parent;
            if (parent != null)
            {
                if (parent.fixedSize)
                {
                    var i = parent.Children.IndexOf(newNode);
                    parent.Children.Remove(newNode);
                    parent.Children.Insert(i, null);
                }
                else
                    parent.Children.Remove(newNode);
            }

            var childIndex = Children.IndexOf(oldNode);
            oldNode.Parent = null;
            newNode.Parent = this;
            Children.RemoveAt(childIndex);
            Children.Insert(childIndex, newNode);
        }

        /// <summary>
        ///     Sets the child.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="child">The child.</param>
        public void SetChild(int index, BaseNode child)
        {
            if (child != null)
            {
                if (child.Parent != null && child.Parent.fixedSize)
                {
                    var i = child.Parent.Children.IndexOf(child);
                    child.Parent.Children.Remove(child);
                    child.Parent.Children.Insert(i, null);
                }
                else
                    child.Parent?.Children.Remove(child);
                if (this[index] != null)
                    this[index].Parent = null;
                this[index] = child;
                child.Parent = this;
            }
            else
            {
                this[index] = null;
            }
        }

        /// <summary>
        ///     Determines whether this instance is final.
        /// </summary>
        /// <returns></returns>
        public bool IsFinal()
        {
            if (result.IsValid() && !result.IsUndefined())
            {
                var id = result.Identifier.ToLower();
                return !id.StartsWith("::temp") && StringType.StrCmp(id, "::nonevar", true) != 0;
            }
            return true;
        }

        private void EnsureChildSize(int index)
        {
            if (fixedSize && index < expectedFixedSize && Children.Count < expectedFixedSize)
            {
                Children.Add(null);
            }
        }

        /// <summary>
        ///     Computes the instruction bounds.
        /// </summary>
        public virtual void ComputeInstructionBounds()
        {
            foreach (var child in Children)
            {
                if (child != null)
                {
                    child.ComputeInstructionBounds();

                    if (begin == -1) begin = child.begin;
                    else if (child.begin != -1)
                        begin = Math.Min(begin, child.begin);

                    if (end == -1) end = child.end;
                    else if (child.end != -1)
                        end = Math.Max(end, child.end);
                }
            }
        }

        public void IncludeInstruction(int ip)
        {
            if (begin == -1)
                end = begin = ip;
            else if (ip < begin)
                begin = ip;
            else if (ip > end)
                end = ip;
        }


        public void Recursive(Action<BaseNode> nodeAction)
        {
            if (nodeAction == null) return;

            nodeAction(this);

            foreach (var c in Children)
            {
                c.Recursive(nodeAction);
            }
        }
    }
}