using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace PapyrusDotNet.Converters.Papyrus2CSharp.FlowAnalyzer
{
    public class PapyrusControlFlowGraph
    {
        private ReadOnlyCollection<PapyrusControlFlowNode> nodes;

        public PapyrusControlFlowNode EntryPoint
        {
            get { return nodes[0]; }
        }

        public PapyrusControlFlowNode RegularExit
        {
            get { return nodes[1]; }
        }

        public ReadOnlyCollection<PapyrusControlFlowNode> Nodes
        {
            get { return nodes; }
        }

        public PapyrusControlFlowGraph(PapyrusControlFlowNode[] nodes)
        {
            this.nodes = new ReadOnlyCollection<PapyrusControlFlowNode>(nodes);
        }

        /// <summary>
        /// Resets "Visited" to false for all nodes in this graph.
        /// </summary>
        public void ResetVisited()
        {
            foreach (PapyrusControlFlowNode node in nodes)
            {
                node.Visited = false;
            }
        }
        /// <summary>
        /// Computes the dominator tree.
        /// </summary>
        public void ComputeDominance(CancellationToken cancellationToken = default(CancellationToken))
        {
            // A Simple, Fast Dominance Algorithm
            // Keith D. Cooper, Timothy J. Harvey and Ken Kennedy

            EntryPoint.ImmediateDominator = EntryPoint;
            bool changed = true;
            while (changed)
            {
                changed = false;
                ResetVisited();

                cancellationToken.ThrowIfCancellationRequested();

                // for all nodes b except the entry point
                EntryPoint.TraversePreOrder(
                    b => b.Successors,
                    b => {
                        if (b != EntryPoint)
                        {
                            PapyrusControlFlowNode newIdom = b.Predecessors.First(block => block.Visited && block != b);
                            // for all other predecessors p of b
                            foreach (PapyrusControlFlowNode p in b.Predecessors)
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
            foreach (PapyrusControlFlowNode node in nodes)
            {
                if (node.ImmediateDominator != null)
                    node.ImmediateDominator.DominatorTreeChildren.Add(node);
            }
        }
        static PapyrusControlFlowNode FindCommonDominator(PapyrusControlFlowNode b1, PapyrusControlFlowNode b2)
        {
            // Here we could use the postorder numbers to get rid of the hashset, see "A Simple, Fast Dominance Algorithm"
            HashSet<PapyrusControlFlowNode> path1 = new HashSet<PapyrusControlFlowNode>();
            while (b1 != null && path1.Add(b1))
                b1 = b1.ImmediateDominator;
            while (b2 != null)
            {
                if (path1.Contains(b2))
                    return b2;
                else
                    b2 = b2.ImmediateDominator;
            }
            throw new Exception("No common dominator found!");
        }

        /// <summary>
		/// Computes dominance frontiers.
		/// This method requires that the dominator tree is already computed!
		/// </summary>
		public void ComputeDominanceFrontier()
        {
            ResetVisited();

            EntryPoint.TraversePostOrder(
                b => b.DominatorTreeChildren,
                n => {
                    //logger.WriteLine("Calculating dominance frontier for " + n.Name);
                    n.DominanceFrontier = new HashSet<PapyrusControlFlowNode>();
                    // DF_local computation
                    foreach (PapyrusControlFlowNode succ in n.Successors)
                    {
                        if (succ.ImmediateDominator != n)
                        {
                            //logger.WriteLine("  local: " + succ.Name);
                            n.DominanceFrontier.Add(succ);
                        }
                    }
                    // DF_up computation
                    foreach (PapyrusControlFlowNode child in n.DominatorTreeChildren)
                    {
                        foreach (PapyrusControlFlowNode p in child.DominanceFrontier)
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