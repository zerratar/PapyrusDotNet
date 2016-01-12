//     This file is part of PapyrusDotNet.
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
//     Copyright 2016, Karl Patrik Johansson, zerratar@gmail.com

#region

using System;
using System.Collections.Generic;
using System.Linq;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Converters.Papyrus2CSharp.FlowAnalyzer
{
    public class PapyrusControlFlowNode
    {
        /// <summary>
        ///     List of children in the dominator tree.
        /// </summary>
        public readonly List<PapyrusControlFlowNode> DominatorTreeChildren = new List<PapyrusControlFlowNode>();

        /// <summary>
        ///     The dominance frontier of this node.
        ///     This is the set of nodes for which this node dominates a predecessor, but which are not strictly dominated by this
        ///     node.
        /// </summary>
        /// <remarks>
        ///     b.DominanceFrontier = { y in CFG; (exists p in predecessors(y): b dominates p) and not (b strictly dominates y)}
        /// </remarks>
        public HashSet<PapyrusControlFlowNode> DominanceFrontier;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PapyrusControlFlowNode" /> class.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        public PapyrusControlFlowNode(int count, PapyrusInstruction start, PapyrusInstruction end)
        {
            BlockIndex = count;
            Start = start;
            End = end;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PapyrusControlFlowNode" /> class.
        /// </summary>
        /// <param name="blockIndex">Index of the block.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="nodeType">Type of the node.</param>
        public PapyrusControlFlowNode(int blockIndex, int offset, PapyrusControlFlowNodeType nodeType)
        {
            BlockIndex = blockIndex;
            Offset = offset;
            NodeType = nodeType;
        }

        public PapyrusControlFlowNodeType NodeType { get; set; }

        /// <summary>
        ///     Gets or sets the instructor offset.
        /// </summary>
        /// <value>
        ///     The offset.
        /// </value>
        public int Offset { get; set; }

        /// <summary>
        ///     Gets or sets the index of the block.
        /// </summary>
        /// <value>
        ///     The index of the block.
        /// </value>
        public int BlockIndex { get; set; }

        /// <summary>
        ///     Gets whether this node is reachable. Requires that dominance is computed!
        /// </summary>
        public bool IsReachable
        {
            get { return ImmediateDominator != null || NodeType == PapyrusControlFlowNodeType.EntryPoint; }
        }

        /// <summary>
        ///     Gets the immediate dominator (the parent in the dominator tree).
        ///     Null if dominance has not been calculated; or if the node is unreachable.
        /// </summary>
        public PapyrusControlFlowNode ImmediateDominator { get; internal set; }

        public List<PapyrusControlFlowEdge> Incoming { get; set; } = new List<PapyrusControlFlowEdge>();
        public List<PapyrusControlFlowEdge> Outgoing { get; set; } = new List<PapyrusControlFlowEdge>();
        public PapyrusInstruction Start { get; set; }
        public PapyrusInstruction End { get; set; }
        public bool Visited { get; set; }


        /// <summary>
        ///     Gets all predecessors (=sources of incoming edges)
        /// </summary>
        public IEnumerable<PapyrusControlFlowNode> Predecessors
        {
            get { return Incoming.Select(e => e.Source); }
        }

        /// <summary>
        ///     Gets all successors (=targets of outgoing edges)
        /// </summary>
        public IEnumerable<PapyrusControlFlowNode> Successors
        {
            get { return Outgoing.Select(e => e.Target); }
        }

        /// <summary>
        ///     Gets all instructions in this node.
        ///     Returns an empty list for special nodes that don't have any instructions.
        /// </summary>
        public IEnumerable<PapyrusInstruction> Instructions
        {
            get
            {
                var inst = Start;
                if (inst != null)
                {
                    yield return inst;
                    while (inst != End)
                    {
                        inst = inst.Next;
                        yield return inst;
                    }
                }
            }
        }

        public void TraversePreOrder(Func<PapyrusControlFlowNode, IEnumerable<PapyrusControlFlowNode>> children,
            Action<PapyrusControlFlowNode> visitAction)
        {
            if (Visited)
                return;
            Visited = true;
            visitAction(this);
            foreach (var t in children(this))
                t.TraversePreOrder(children, visitAction);
        }

        public void TraversePostOrder(Func<PapyrusControlFlowNode, IEnumerable<PapyrusControlFlowNode>> children,
            Action<PapyrusControlFlowNode> visitAction)
        {
            if (Visited)
                return;
            Visited = true;
            foreach (var t in children(this))
                t.TraversePostOrder(children, visitAction);
            visitAction(this);
        }

        /// <summary>
        ///     Gets whether <c>this</c> dominates <paramref name="node" />.
        /// </summary>
        public bool Dominates(PapyrusControlFlowNode node)
        {
            // TODO: this can be made O(1) by numbering the dominator tree
            var tmp = node;
            while (tmp != null)
            {
                if (tmp == this)
                    return true;
                tmp = tmp.ImmediateDominator;
            }
            return false;
        }
    }
}