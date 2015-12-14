// --------- The following class is part of PapyrusDotNet
// But is heavily inspired by the ControlFlowGraph builder from ilspy which
// can be found here: http://ilspy.net/ and which is why it includes the 
// license from the ControlFlowGraphBuilder class.
// ------------------------------------------------------------
// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.Collections.Generic;
using System.Linq;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.Converters.Papyrus2CSharp.FlowAnalyzer
{
    public class PapyrusControlFlowGraphBuilder
    {
        private readonly PapyrusMethodDefinition method;
        private readonly Dictionary<PapyrusInstruction, bool> hasIncomingJumps = new Dictionary<PapyrusInstruction, bool>();
        private List<PapyrusControlFlowNode> nodes = new List<PapyrusControlFlowNode>();
        private PapyrusControlFlowNode entryPoint;
        private PapyrusControlFlowNode regularExit;

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
                if (instruction.OpCode == PapyrusOpCode.Jmpt || instruction.OpCode == PapyrusOpCode.Jmpf)
                {
                    var destination = instruction.Offset + int.Parse(instruction.GetArg(1));
                    hasIncomingJumps[GetInstruction(destination)] = true;
                }
                else if (instruction.OpCode == PapyrusOpCode.Jmp)
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
            for (int i = 0; i < methodBody.Instructions.Count; i++)
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
            foreach (PapyrusControlFlowNode node in nodes)
            {
                if (node.End != null)
                {
                    // create normal edges from one instruction to the next
                    if (node.End.OpCode == PapyrusOpCode.Jmpt || node.End.OpCode == PapyrusOpCode.Jmpf)
                        CreateEdge(node, node.End.Next);

                    // create edges for branch instructions
                    if (node.End.OpCode == PapyrusOpCode.Jmp)
                    {
                        CreateEdge(node, (PapyrusInstruction)node.End.Operand);

                    }
                    // create edges for return instructions
                    if (node.End.OpCode == PapyrusOpCode.Return)
                    {
                        CreateEdge(node, regularExit);
                    }
                }
            }
        }

        void CreateEdge(PapyrusControlFlowNode fromNode, PapyrusInstruction toInstruction)
        {
            CreateEdge(fromNode, nodes.Single(n => n.Start == toInstruction));
        }

        void CreateEdge(PapyrusControlFlowNode fromNode, PapyrusControlFlowNode toNode)
        {
            PapyrusControlFlowEdge edge = new PapyrusControlFlowEdge(fromNode, toNode);
            fromNode.Outgoing.Add(edge);
            toNode.Incoming.Add(edge);
        }

        private bool IsBranch(PapyrusOpCode opCode)
        {
            return opCode == PapyrusOpCode.Jmp || opCode == PapyrusOpCode.Jmpf || opCode == PapyrusOpCode.Jmpt;
        }
    }
}
