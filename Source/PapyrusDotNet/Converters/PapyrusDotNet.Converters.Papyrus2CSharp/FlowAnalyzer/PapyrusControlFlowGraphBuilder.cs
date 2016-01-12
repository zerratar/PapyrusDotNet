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

using System.Collections.Generic;
using System.Linq;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Converters.Papyrus2CSharp.FlowAnalyzer
{
    public class PapyrusControlFlowGraphBuilder
    {
        private readonly Dictionary<PapyrusInstruction, bool> hasIncomingJumps =
            new Dictionary<PapyrusInstruction, bool>();

        private readonly PapyrusMethodDefinition method;
        private readonly PapyrusControlFlowNode entryPoint;
        private readonly List<PapyrusControlFlowNode> nodes = new List<PapyrusControlFlowNode>();
        private readonly PapyrusControlFlowNode regularExit;

        public PapyrusControlFlowGraphBuilder(PapyrusMethodDefinition method)
        {
            this.method = method;
            entryPoint = new PapyrusControlFlowNode(0, 0, PapyrusControlFlowNodeType.EntryPoint);
            nodes.Add(entryPoint);
            regularExit = new PapyrusControlFlowNode(1, -1, PapyrusControlFlowNodeType.RegularExit);
            nodes.Add(regularExit);
        }

        public PapyrusControlFlowGraph Build()
        {
            BuildJumpList();
            CreateNodes();
            CreateRegularControlFlow();
            return new PapyrusControlFlowGraph(nodes.ToArray());
        }

        private PapyrusInstruction GetInstruction(int offset)
        {
            return method.Body.Instructions.FirstOrDefault(i => i.Offset == offset);
        }

        private void BuildJumpList()
        {
            // Step 1
            hasIncomingJumps.Clear();
            foreach (var instruction in method.Body.Instructions)
            {
                hasIncomingJumps.Add(instruction, false);
            }
            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.OpCode == PapyrusOpCodes.Jmpt || instruction.OpCode == PapyrusOpCodes.Jmpf)
                {
                    var destination = instruction.Offset + int.Parse(instruction.GetArg(1));
                    hasIncomingJumps[GetInstruction(destination)] = true;
                }
                else if (instruction.OpCode == PapyrusOpCodes.Jmp)
                {
                    var destination = instruction.Offset + int.Parse(instruction.GetArg(0));
                    hasIncomingJumps[GetInstruction(destination)] = true;
                }
            }
        }


        private void CreateNodes()
        {
            // Step 2: find basic blocks and create nodes for them
            var methodBody = method.Body;
            for (var i = 0; i < methodBody.Instructions.Count; i++)
            {
                var blockStart = methodBody.Instructions[i];
                // try and see how big we can make that block:
                for (; i + 1 < methodBody.Instructions.Count; i++)
                {
                    var inst = methodBody.Instructions[i];
                    if (IsBranch(inst.OpCode))
                        break;
                    if (hasIncomingJumps.ElementAt(i + 1).Value)
                        break;
                    //if (inst.Next != null)
                    //{
                    //    // ensure that blocks never contain instructions from different try blocks
                    //    ExceptionHandler instEH = FindInnermostExceptionHandler(inst.Next.Offset);
                    //    if (instEH != blockStartEH)
                    //        break;
                    //}
                }

                nodes.Add(new PapyrusControlFlowNode(nodes.Count, blockStart, methodBody.Instructions[i]));
            }
        }

        private void CreateRegularControlFlow()
        {
            // Step 3: create edges for the normal flow of control 
            var methodBody = method.Body;
            CreateEdge(entryPoint, methodBody.Instructions[0]);
            foreach (var node in nodes)
            {
                if (node.End != null)
                {
                    // create normal edges from one instruction to the next
                    if (node.End.OpCode == PapyrusOpCodes.Jmpt || node.End.OpCode == PapyrusOpCodes.Jmpf)
                        CreateEdge(node, node.End.Next);

                    // create edges for branch instructions
                    if (node.End.OpCode == PapyrusOpCodes.Jmp)
                    {
                        CreateEdge(node, (PapyrusInstruction) node.End.Operand);
                    }
                    // create edges for return instructions
                    if (node.End.OpCode == PapyrusOpCodes.Return)
                    {
                        CreateEdge(node, regularExit);
                    }
                }
            }
        }

        private void CreateEdge(PapyrusControlFlowNode fromNode, PapyrusInstruction toInstruction)
        {
            CreateEdge(fromNode, nodes.Single(n => n.Start == toInstruction));
        }

        private void CreateEdge(PapyrusControlFlowNode fromNode, PapyrusControlFlowNode toNode)
        {
            var edge = new PapyrusControlFlowEdge(fromNode, toNode);
            fromNode.Outgoing.Add(edge);
            toNode.Incoming.Add(edge);
        }

        private bool IsBranch(PapyrusOpCodes opCode)
        {
            return opCode == PapyrusOpCodes.Jmp || opCode == PapyrusOpCodes.Jmpf || opCode == PapyrusOpCodes.Jmpt;
        }
    }
}