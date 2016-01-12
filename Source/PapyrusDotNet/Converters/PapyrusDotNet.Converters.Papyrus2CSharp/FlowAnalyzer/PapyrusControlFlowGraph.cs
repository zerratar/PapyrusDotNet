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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

#endregion

namespace PapyrusDotNet.Converters.Papyrus2CSharp.FlowAnalyzer
{
    public class PapyrusControlFlowGraph
    {
        public PapyrusControlFlowGraph(PapyrusControlFlowNode[] nodes)
        {
            Nodes = new ReadOnlyCollection<PapyrusControlFlowNode>(nodes);
        }

        public PapyrusControlFlowNode EntryPoint
        {
            get { return Nodes[0]; }
        }

        public PapyrusControlFlowNode RegularExit
        {
            get { return Nodes[1]; }
        }

        public ReadOnlyCollection<PapyrusControlFlowNode> Nodes { get; }

        /// <summary>
        ///     Resets "Visited" to false for all nodes in this graph.
        /// </summary>
        public void ResetVisited()
        {
            foreach (var node in Nodes)
            {
                node.Visited = false;
            }
        }

        /// <summary>
        ///     Computes the dominator tree.
        /// </summary>
        public void ComputeDominance(CancellationToken cancellationToken = default(CancellationToken))
        {
            // A Simple, Fast Dominance Algorithm
            // Keith D. Cooper, Timothy J. Harvey and Ken Kennedy

            EntryPoint.ImmediateDominator = EntryPoint;
            var changed = true;
            while (changed)
            {
                changed = false;
                ResetVisited();

                cancellationToken.ThrowIfCancellationRequested();

                // for all nodes b except the entry point
                EntryPoint.TraversePreOrder(
                    b => b.Successors,
                    b =>
                    {
                        if (b != EntryPoint)
                        {
                            var newIdom = b.Predecessors.First(block => block.Visited && block != b);
                            // for all other predecessors p of b
                            foreach (var p in b.Predecessors)
                            {
                                if (p != b && p.ImmediateDominator != null)
                                {
                                    newIdom = FindCommonDominator(p, newIdom);
                                }
                            }
                            if (b.ImmediateDominator != newIdom)
                            {
                                b.ImmediateDominator = newIdom;
                                changed = true;
                            }
                        }
                    });
            }
            EntryPoint.ImmediateDominator = null;
            foreach (var node in Nodes)
            {
                if (node.ImmediateDominator != null)
                    node.ImmediateDominator.DominatorTreeChildren.Add(node);
            }
        }

        private static PapyrusControlFlowNode FindCommonDominator(PapyrusControlFlowNode b1, PapyrusControlFlowNode b2)
        {
            // Here we could use the postorder numbers to get rid of the hashset, see "A Simple, Fast Dominance Algorithm"
            var path1 = new HashSet<PapyrusControlFlowNode>();
            while (b1 != null && path1.Add(b1))
                b1 = b1.ImmediateDominator;
            while (b2 != null)
            {
                if (path1.Contains(b2))
                    return b2;
                b2 = b2.ImmediateDominator;
            }
            throw new Exception("No common dominator found!");
        }

        /// <summary>
        ///     Computes dominance frontiers.
        ///     This method requires that the dominator tree is already computed!
        /// </summary>
        public void ComputeDominanceFrontier()
        {
            ResetVisited();

            EntryPoint.TraversePostOrder(
                b => b.DominatorTreeChildren,
                n =>
                {
                    //logger.WriteLine("Calculating dominance frontier for " + n.Name);
                    n.DominanceFrontier = new HashSet<PapyrusControlFlowNode>();
                    // DF_local computation
                    foreach (var succ in n.Successors)
                    {
                        if (succ.ImmediateDominator != n)
                        {
                            //logger.WriteLine("  local: " + succ.Name);
                            n.DominanceFrontier.Add(succ);
                        }
                    }
                    // DF_up computation
                    foreach (var child in n.DominatorTreeChildren)
                    {
                        foreach (var p in child.DominanceFrontier)
                        {
                            if (p.ImmediateDominator != n)
                            {
                                //logger.WriteLine("  DF_up: " + p.Name + " (child=" + child.Name);
                                n.DominanceFrontier.Add(p);
                            }
                        }
                    }
                });
        }
    }
}