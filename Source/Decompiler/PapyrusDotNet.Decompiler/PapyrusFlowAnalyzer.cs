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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PapyrusDotNet.Decompiler.Extensions;
using PapyrusDotNet.Decompiler.HelperClasses;
using PapyrusDotNet.Decompiler.Interfaces;
using PapyrusDotNet.Decompiler.Node;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Decompiler
{
    public class PapyrusFlowAnalyzer : IFlowAnalyzer
    {
        private IDecompilerContext context;
        private readonly Map<int, PapyrusStringTableIndex> varTypes = new Map<int, PapyrusStringTableIndex>();

        /// <summary>
        ///     Sets the decompiler context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void SetContext(IDecompilerContext context)
        {
            this.context = context;
        }

        /// <summary>
        ///     Builds the blocks.
        /// </summary>
        public void CreateFlowBlocks()
        {
            var tempTable = context.TempStringTable;
            var method = context.GetTargetMethod();
            var blocks = context.GetCodeBlocks();
            var instructions = method.Body.Instructions;
            var full = new PapyrusCodeBlock(0, instructions.Count - 1);
            full.Next = instructions.Count;


            blocks[full.Begin] = full;
            blocks[instructions.Count] = new PapyrusCodeBlock(instructions.Count, PapyrusCodeBlock.END);
            var ip = 0;
            foreach (var ins in instructions)
            {
                var block = FindBlockForInstruction(ip);
                switch (ins.OpCode)
                {
                    case PapyrusOpCodes.Jmp:
                        {
                            Assert.AreEqual(1, ins.Arguments.Count);
                            Assert.AreEqual(PapyrusPrimitiveType.Integer, ins.Arguments[0].Type);

                            var target = ip + int.Parse(ins.GetArg(0));

                            // Unconditional jump
                            // Split the block at the jump and set the next block to the target of the jump.

                            var nextBlock = blocks.Find(ip + 1);
                            if (nextBlock == null)
                            {
                                var nblock = blocks[block].Split(ip + 1);
                                blocks[nblock.Begin] = nblock;
                            }
                            blocks[block].Next = target;
                            var tarBlock = blocks.Find(target);
                            if (tarBlock == null)
                            {
                                var cblock = blocks[FindBlockForInstruction(target)];
                                var tblock = cblock.Split(target);
                                blocks[tblock.Begin] = tblock;
                            }
                            break;
                        }
                    case PapyrusOpCodes.Jmpf:
                    case PapyrusOpCodes.Jmpt:
                        {
                            Assert.AreEqual(2, ins.Arguments.Count);
                            Assert.IsTrue(ins.Arguments[0].Type == PapyrusPrimitiveType.Reference ||
                                          ins.Arguments[0].Type == PapyrusPrimitiveType.Boolean);
                            Assert.AreEqual(PapyrusPrimitiveType.Integer, ins.Arguments[1].Type);

                            // Conditional jump
                            // The block is split at the jump.
                            // The block condition is set to the condition of the jump,
                            // The true and false block are set to the target of the jump, and the instruction following the jump.
                            // The true or false order is decided from the kind of jump (jmpf/jmpt).
                            var target = ip + int.Parse(ins.GetArg(1));
                            var nextBlock = blocks.Find(ip + 1);
                            if (nextBlock == null)
                            {
                                var nblock = blocks[block].Split(ip + 1);
                                blocks[nblock.Begin] = nblock;
                            }

                            var tarBlock = blocks.Find(target);
                            if (tarBlock == null)
                            {
                                var cblock = blocks[FindBlockForInstruction(target)];
                                var tblock = cblock.Split(target);
                                blocks[tblock.Begin] = tblock;
                            }

                            PapyrusStringTableIndex condition = null;

                            if (ins.Arguments[0].Type == PapyrusPrimitiveType.Reference)
                            {
                                condition = ins.GetArgTableIndex(0);
                            }
                            else
                            {
                                if (ins.GetBoolArg(0))
                                {
                                    condition = tempTable["true"];
                                }
                                else
                                {
                                    condition = tempTable["false"];
                                }
                            }
                            if (ins.OpCode == PapyrusOpCodes.Jmpf)
                            {
                                blocks[block].SetCondition(condition, ip + 1, target);
                            }
                            else
                            {
                                blocks[block].SetCondition(condition, target, ip + 1);
                            }
                            break;
                        }
                }
                ++ip;
            }

            foreach (var block in blocks)
            {
                CreateNodesForBlock(context, block.Key);
            }
        }

        /// <summary>
        ///     Rebuilds the expressions.
        /// </summary>
        public void RebuildExpressionsInBlocks()
        {
            var blocks =
                context.GetCodeBlocks();
            foreach (var block in blocks)
            {
                var bnode = block.Value;
                var scope = bnode.Scope;

                RebuildExpression(scope);
            }
        }

        /// <summary>
        ///     Builds the flow tree.
        /// </summary>
        /// <returns></returns>
        public BaseNode RebuildControlFlow(int startBlock, int endBlock)
        {
            if (endBlock < startBlock) throw new Exception("Decompilation failed");

            var blocks = context.GetCodeBlocks();
            var begin = blocks.Find(startBlock);
            var end = blocks.Find(endBlock);
            var it = begin;

            var result = new ScopeNode();

            while (it != null && it != end)
            {
                var advance = 1;
                var current = it.Begin;
                var source = it;
                if (source.IsConditional())
                {
                    var exit = source.OnFalse;
                    var beforeExit = FindBlockForInstruction(exit - 1);
                    if (beforeExit == PapyrusCodeBlock.END)
                    {
                        throw new Exception("Decompilation failed");
                    }

                    // The last block is an unconditional jump to the current block
                    // This is a while.
                    var lastBlock = blocks[beforeExit];
                    if (!lastBlock.IsConditional() && lastBlock.Next == current)
                    {
                        var whileStartBlock = source.OnTrue;
                        var whileEndBlock = source.OnFalse;

                        var whileCondition = new ConstantNode(-1, new PapyrusVariableReference(source.Condition, true));

                        result.MergeChildren(source.Scope);

                        // rebuild the statements in the while loop.
                        var whileBody = RebuildControlFlow(whileStartBlock, whileEndBlock);
                        result.Adopt(new WhileNode(-1, whileCondition, whileBody));

                        advance = 0;
                        it = blocks.Find(whileEndBlock);
                    }
                    else if (!lastBlock.IsConditional())
                    {
                        // The last block exists to the false block
                        // This is a simple if
                        if (lastBlock.Next == exit)
                        {
                            // simple if
                            var ifStartBlock = source.OnTrue;
                            var ifEndBlock = source.OnFalse;

                            var ifCondition = new ConstantNode(-1, new PapyrusVariableReference(source.Condition, true));

                            result.MergeChildren(source.Scope);

                            var ifBody = RebuildControlFlow(ifStartBlock, ifEndBlock);
                            result.Adopt(new IfElseNode(-1, ifCondition, ifBody, null));

                            advance = 0;
                            it = blocks.Find(ifEndBlock);
                        }
                        else
                        {
                            // this is a if-else statement
                            var ifStartBlock = source.OnTrue;
                            var elseStartBlock = source.OnFalse;
                            var endElseBlock = lastBlock.Next;

                            var ifCondition = new ConstantNode(-1, new PapyrusVariableReference(source.Condition, true));

                            result.MergeChildren(source.Scope);

                            var ifBody = RebuildControlFlow(ifStartBlock, elseStartBlock);
                            var elseBody = RebuildControlFlow(elseStartBlock, endElseBlock);

                            result.Adopt(new IfElseNode(-1, ifCondition, ifBody, elseBody));

                            advance = 0;
                            it = blocks.Find(endElseBlock);
                        }
                    }
                }
                else
                {
                    //On unconditional jump, merge the current block statements to the result scope.
                    result.MergeChildren(source.Scope);
                }

                if (advance == 1)
                {
                    it = blocks.GetNext(it);
                }
            }

            RebuildExpression(result);
            return result;
        }

        /// <summary>
        ///     Rebuilds the boolean operators.
        /// </summary>
        public void RebuildBooleanOperators(int startBlock, int endBlock)
        {
            var blocks = context.GetCodeBlocks();
            var begin = blocks.Find(startBlock);
            var end = blocks.Find(endBlock);
            var it = begin;
            while (it != null && it != end)
            {
                var source = it;
                var advance = 1;
                // Process only conditional block with at least one statement.
                if (source.IsConditional() && source.Scope.Size != 0)
                {
                    // Ensure that the last statement computes the value of the condition variable.
                    var lastItem = source.Scope.Back();
                    var lastResult = lastItem.GetResult();
                    if (source.Condition == lastResult)
                    {
                        // If the true block is the block directly following this one, it is a potential and
                        var maybeAnd = source.OnTrue == source.End + 1;
                        // If the false block is the block directly following this one, it is a potential or
                        var maybeOr = source.OnFalse == source.End + 1;

                        Assert.IsFalse(maybeAnd && maybeOr);

                        if (maybeAnd)
                        {
                            RebuildBooleanOperators(source.OnTrue, source.OnFalse);
                            var onTrue = blocks[source.OnTrue];
                            var onFalse = blocks[source.OnFalse];
                            if (onTrue.Scope.Size == 1)
                            {
                                if (onTrue.Scope.Back().GetResult() == source.Condition)
                                {
                                    // Create the && operator
                                    var left = source.Scope.Back();
                                    source.Scope.Children.Remove(left);

                                    var right = onTrue.Scope.Front();
                                    onTrue.Scope.Children.Remove(right);

                                    var andOperator = new BinaryOperatorNode(-1, 7, source.Condition, left, "&&", right);
                                    source.Scope.Adopt(andOperator);

                                    // Remove the true block now that the expression is rebuild
                                    blocks.Erase(onTrue.Begin);

                                    source.Scope.MergeChildren(onFalse.Scope);

                                    RebuildExpression(source.Scope);

                                    source.End = onFalse.End;
                                    source.SetCondition(onFalse.Condition, onFalse.OnTrue, onFalse.OnFalse);
                                    blocks.Erase(onFalse.Begin);

                                    advance = 0;
                                }
                            }
                            it = blocks.Find(source.Begin);
                        }
                        else if (maybeOr)
                        {
                            RebuildBooleanOperators(source.OnFalse, source.OnTrue);
                            var onTrue = blocks[source.OnTrue];
                            var onFalse = blocks[source.OnFalse];

                            if (onFalse.Scope.Size == 1 && onFalse.Scope.Back().GetResult() == source.Condition)
                            {
                                var left = source.Scope.Back();
                                source.Scope.Children.Remove(left);

                                var right = onFalse.Scope.Front();
                                onFalse.Scope.Children.Remove(right);

                                var orOperator = new BinaryOperatorNode(-1, 8, source.Condition, left, "||", right);
                                source.Scope.Adopt(orOperator);

                                blocks.Erase(onFalse.Begin);

                                source.Scope.MergeChildren(onTrue.Scope);
                                RebuildExpression(source.Scope);
                                source.End = onTrue.End;
                                source.SetCondition(onTrue.Condition, onTrue.OnTrue, onTrue.OnFalse);
                                blocks.Erase(onTrue.Begin);

                                advance = 0;
                            }

                            it = blocks.Find(source.Begin);
                            advance = 0;
                        }
                    }
                }
                if (advance == 1)
                {
                    it = blocks.GetNext(it);
                }
            }
        }

        /// <summary>
        ///     Builds the type index, mapping a table reference to its type.
        /// </summary>
        public void FindVarTypes()
        {
            var asm = context.GetSourceAssembly();
            var method = context.GetTargetMethod();
            var obj = asm.Types.First();
            foreach (var t in obj.Fields)
                varTypes[t.Name.Index] = asm.CreateStringTableIndex(t.TypeName);
            foreach (var t in method.Parameters)
                varTypes[t.Name.Index] = t.TypeName.AsTableIndex();
            foreach (var t in method.GetVariables())
                varTypes[t.Name.Index] = t.TypeName.AsTableIndex();
        }

        /// <summary>
        ///     Finalizes the tree by declaring variables and cleaning up conditional statements and more.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public BaseNode Finalize(BaseNode tree)
        {
            DeclareVariables(tree);

            CleanUpTree(tree);

            return tree;
        }

        private void CreateNodesForBlock(IDecompilerContext ctx, int block)
        {
            var blocks = ctx.GetCodeBlocks();
            var code = blocks[block];
            var method = ctx.GetTargetMethod();
            var tempTable = context.TempStringTable;
            var instructions = method.Body.Instructions;
            var scope = code.Scope;

            if (code.Begin < instructions.Count)
            {
                for (var ip = code.Begin; ip <= code.End; ++ip)
                {
                    var ins = instructions[ip];
                    var args = ins.Arguments;
                    var varargs = ins.OperandArguments;
                    BaseNode node = null;
                    if (ins.TemporarilyInstruction) continue;


                    switch (ins.OpCode)
                    {
                        case PapyrusOpCodes.Jmp:
                        case PapyrusOpCodes.Jmpf:
                        case PapyrusOpCodes.Jmpt:
                        case PapyrusOpCodes.Nop:
                            break;
                        case PapyrusOpCodes.Fadd:
                        case PapyrusOpCodes.Iadd:
                        case PapyrusOpCodes.Strcat:
                            {
                                node = new BinaryOperatorNode(ip, 5, ins.GetArgTableIndex(0), FromValue(ip, args[1]), "+",
                                    FromValue(ip, args[2]));
                                break;
                            }
                        case PapyrusOpCodes.Isub:
                        case PapyrusOpCodes.Fsub:
                            {
                                node = new BinaryOperatorNode(ip, 5, ins.GetArgTableIndex(0), FromValue(ip, args[1]), "-",
                                    FromValue(ip, args[2]));
                                break;
                            }
                        case PapyrusOpCodes.Imul:
                        case PapyrusOpCodes.Fmul:
                            {
                                node = new BinaryOperatorNode(ip, 4, ins.GetArgTableIndex(0), FromValue(ip, args[1]), "*",
                                    FromValue(ip, args[2]));
                                break;
                            }
                        case PapyrusOpCodes.Idiv:
                        case PapyrusOpCodes.Fdiv:
                            {
                                node = new BinaryOperatorNode(ip, 4, ins.GetArgTableIndex(0), FromValue(ip, args[1]), "/",
                                    FromValue(ip, args[2]));
                                break;
                            }
                        case PapyrusOpCodes.Imod:
                            {
                                node = new BinaryOperatorNode(ip, 4, ins.GetArgTableIndex(0), FromValue(ip, args[1]), "%",
                                    FromValue(ip, args[2]));
                                break;
                            }
                        case PapyrusOpCodes.Not:
                            {
                                node = new UnaryOperatorNode(ip, 3, ins.GetArgTableIndex(0), "!", FromValue(ip, args[1]));
                                break;
                            }
                        case PapyrusOpCodes.Fneg:
                        case PapyrusOpCodes.Ineg:
                            {
                                node = new UnaryOperatorNode(ip, 3, ins.GetArgTableIndex(0), "-", FromValue(ip, args[1]));
                                break;
                            }
                        case PapyrusOpCodes.Assign:
                            {
                                var idx = ins.GetArgTableIndex(0);
                                node = new CopyNode(ip, idx, FromValue(ip, args[1]));
                                break;
                            }
                        case PapyrusOpCodes.Cast:
                            {
                                if (args[1].Type == PapyrusPrimitiveType.None)
                                {
                                    node = new CopyNode(ip, ins.GetArgTableIndex(0), FromValue(ip, args[1]));
                                }
                                else if (args[1].Type != PapyrusPrimitiveType.Reference ||
                                         (TypeOfVar(ins.GetArgTableIndex(0)) != TypeOfVar(ins.GetArgTableIndex(1))
                                          && ins.GetArgTableIndex(1).Identifier.ToLower() != "none"))
                                {
                                    node = new CastNode(ip, ins.GetArgTableIndex(0), FromValue(ip, args[1]),
                                        TypeOfVar(ins.GetArgTableIndex(0)));
                                }
                                else
                                {
                                    node = new CopyNode(ip, ins.GetArgTableIndex(0), FromValue(ip, args[1]));
                                }

                                break;
                            }
                        case PapyrusOpCodes.CmpEq:
                            {
                                node = new BinaryOperatorNode(ip, 5, ins.GetArgTableIndex(0), FromValue(ip, args[1]), "==",
                                    FromValue(ip, args[2]));
                                break;
                            }
                        case PapyrusOpCodes.CmpLt:
                            {
                                node = new BinaryOperatorNode(ip, 5, ins.GetArgTableIndex(0), FromValue(ip, args[1]), "<",
                                    FromValue(ip, args[2]));
                                break;
                            }
                        case PapyrusOpCodes.CmpLte:
                            {
                                node = new BinaryOperatorNode(ip, 5, ins.GetArgTableIndex(0), FromValue(ip, args[1]), "<=",
                                    FromValue(ip, args[2]));
                                break;
                            }
                        case PapyrusOpCodes.CmpGt:
                            {
                                node = new BinaryOperatorNode(ip, 5, ins.GetArgTableIndex(0), FromValue(ip, args[1]), ">",
                                    FromValue(ip, args[2]));
                                break;
                            }
                        case PapyrusOpCodes.CmpGte:
                            {
                                node = new BinaryOperatorNode(ip, 5, ins.GetArgTableIndex(0), FromValue(ip, args[1]), ">=",
                                    FromValue(ip, args[2]));
                                break;
                            }

                        case PapyrusOpCodes.Callmethod:
                            {
                                var callNode = new CallMethodNode(ip, ins.GetArgTableIndex(2), FromValue(ip, args[1]),
                                    ins.GetArgTableIndex(0));
                                var p = callNode.GetParameters();
                                foreach (var varg in varargs)
                                {
                                    p.Adopt(FromValue(ip, varg));
                                }
                                node = callNode;
                                break;
                            }
                        case PapyrusOpCodes.Callstatic:
                            {
                                var callNode = new CallMethodNode(ip, ins.GetArgTableIndex(2), FromValue(ip, args[0]),
                                    ins.GetArgTableIndex(1));

                                var p = callNode.GetParameters();
                                foreach (var varg in varargs)
                                {
                                    p.Adopt(FromValue(ip, varg));
                                }
                                node = callNode;
                                break;
                            }
                        case PapyrusOpCodes.Callparent:
                            {
                                var callNode = new CallMethodNode(ip, ins.GetArgTableIndex(1),
                                    new IdentifierStringNode(ip, "Parent"),
                                    ins.GetArgTableIndex(0));

                                var p = callNode.GetParameters();
                                foreach (var varg in varargs)
                                {
                                    p.Adopt(FromValue(ip, varg));
                                }
                                node = callNode;
                                break;
                            }
                        case PapyrusOpCodes.Return:
                            {
                                if (method.ReturnTypeName.Value.ToLower() == "none")
                                {
                                    node = new ReturnNode(ip, null);
                                }
                                else
                                {
                                    node = new ReturnNode(ip, FromValue(ip, args[0]));
                                }
                                break;
                            }
                        case PapyrusOpCodes.PropGet:
                            {
                                node = new PropertyAccessNode(ip, ins.GetArgTableIndex(2), FromValue(ip, args[1]),
                                    ins.GetArgTableIndex(0));
                                break;
                            }
                        case PapyrusOpCodes.PropSet:
                            {
                                node = new PropertyAccessNode(ip, new PapyrusStringTableIndex(), FromValue(ip, args[1]),
                                    ins.GetArgTableIndex(0));
                                node = new AssignNode(ip, node, FromValue(ip, args[2]));
                                break;
                            }
                        case PapyrusOpCodes.ArrayCreate:
                            {
                                node = new ArrayCreateNode(ip, ins.GetArgTableIndex(0), TypeOfVar(ins.GetArgTableIndex(0)),
                                    FromValue(ip, args[1]));
                                break;
                            }
                        case PapyrusOpCodes.ArrayLength:
                            {
                                node = new ArrayLengthNode(ip, ins.GetArgTableIndex(0), FromValue(ip, args[1]));
                                break;
                            }
                        case PapyrusOpCodes.ArrayGetElement:
                            {
                                node = new ArrayAccessNode(ip, ins.GetArgTableIndex(0), FromValue(ip, args[1]),
                                    FromValue(ip, args[2]));
                                break;
                            }
                        case PapyrusOpCodes.ArraySetElement:
                            {
                                node = new ArrayAccessNode(ip, new PapyrusStringTableIndex(), FromValue(ip, args[0]),
                                    FromValue(ip, args[1]));
                                node = new AssignNode(ip, node, FromValue(ip, args[2]));
                                break;
                            }
                        case PapyrusOpCodes.ArrayFindElement:
                            {
                                var callNode = new CallMethodNode(ip, ins.GetArgTableIndex(1), FromValue(ip, args[0]),
                                    tempTable["find"]);
                                var pNode = callNode.GetParameters();
                                pNode.Adopt(FromValue(ip, args[2]));
                                pNode.Adopt(FromValue(ip, args[3]));
                                node = callNode;
                                break;
                            }
                        case PapyrusOpCodes.ArrayFindLastElement:
                            {
                                var callNode = new CallMethodNode(ip, ins.GetArgTableIndex(1), FromValue(ip, args[0]),
                                    tempTable["rfind"]);
                                var pNode = callNode.GetParameters();
                                pNode.Adopt(FromValue(ip, args[2]));
                                pNode.Adopt(FromValue(ip, args[3]));
                                node = callNode;
                                break;
                            }
                        case PapyrusOpCodes.Is:
                            {
                                node = new BinaryOperatorNode(ip, 0, ins.GetArgTableIndex(0), FromValue(ip, args[1]), "is",
                                    FromValue(ip, args[2]));
                                break;
                            }
                        case PapyrusOpCodes.StructCreate:
                            {
                                node = new StructCreateNode(ip, ins.GetArgTableIndex(0), TypeOfVar(ins.GetArgTableIndex(0)));
                                break;
                            }
                        case PapyrusOpCodes.StructGet:
                            {
                                node = new PropertyAccessNode(ip, ins.GetArgTableIndex(0), FromValue(ip, args[1]),
                                    ins.GetArgTableIndex(2));
                                break;
                            }
                        case PapyrusOpCodes.StructSet:
                            {
                                node = new PropertyAccessNode(ip, new PapyrusStringTableIndex(), FromValue(ip, args[0]),
                                    ins.GetArgTableIndex(1));
                                node = new AssignNode(ip, node, FromValue(ip, args[2]));
                                break;
                            }
                        case PapyrusOpCodes.ArrayFindStruct:
                            {
                                var callNode = new CallMethodNode(ip, ins.GetArgTableIndex(1), FromValue(ip, args[0]),
                                    tempTable["find"]);
                                var pNode = callNode.GetParameters();
                                pNode.Adopt(FromValue(ip, args[2]));
                                pNode.Adopt(FromValue(ip, args[3]));
                                pNode.Adopt(FromValue(ip, args[4]));
                                node = callNode;
                                break;
                            }
                        case PapyrusOpCodes.ArrayFindLastStruct:
                            {
                                var callNode = new CallMethodNode(ip, ins.GetArgTableIndex(1), FromValue(ip, args[0]),
                                    tempTable["rfind"]);
                                var pNode = callNode.GetParameters();
                                pNode.Adopt(FromValue(ip, args[2]));
                                pNode.Adopt(FromValue(ip, args[3]));
                                pNode.Adopt(FromValue(ip, args[4]));
                                node = callNode;
                                break;
                            }
                        case PapyrusOpCodes.ArrayAddElements:
                            {
                                var callNode = new CallMethodNode(ip, new PapyrusStringTableIndex(), FromValue(ip, args[0]),
                                    tempTable["add"]);
                                var pNode = callNode.GetParameters();
                                pNode.Adopt(FromValue(ip, args[1]));
                                pNode.Adopt(FromValue(ip, args[2]));
                                node = callNode;
                                break;
                            }
                        case PapyrusOpCodes.ArrayInsertElement:
                            {
                                var callNode = new CallMethodNode(ip, new PapyrusStringTableIndex(), FromValue(ip, args[0]),
                                    tempTable["insert"]);
                                var pNode = callNode.GetParameters();
                                pNode.Adopt(FromValue(ip, args[1]));
                                pNode.Adopt(FromValue(ip, args[2]));
                                node = callNode;
                                break;
                            }
                        case PapyrusOpCodes.ArrayRemoveLastElement:
                            {
                                node = new CallMethodNode(ip, new PapyrusStringTableIndex(), FromValue(ip, args[0]),
                                    tempTable["removelast"]);
                                break;
                            }
                        case PapyrusOpCodes.ArrayRemoveElements:
                            {
                                var callNode = new CallMethodNode(ip, new PapyrusStringTableIndex(), FromValue(ip, args[0]),
                                    tempTable["remove"]);
                                var pNode = callNode.GetParameters();
                                pNode.Adopt(FromValue(ip, args[1]));
                                pNode.Adopt(FromValue(ip, args[2]));
                                node = callNode;
                                break;
                            }
                        case PapyrusOpCodes.ArrayClearElements:
                            {
                                node = new CallMethodNode(ip, new PapyrusStringTableIndex(), FromValue(ip, args[0]),
                                    tempTable["clear"]);
                                break;
                            }
                    }
                    if (node != null)
                    {
                        scope.Adopt(CheckAssign(node));
                    }
                }
            }
        }

        private PapyrusStringTableIndex TypeOfVar(PapyrusStringTableIndex variable)
        {
            var type = varTypes.Find(variable.TableIndex);
            if (type == null)
            {
                return PapyrusStringTableIndex.Undefined;
            }
            if (!type.IsValid())
                throw new InvalidOperationException(
                    "The found string table index item was expected to be valid, but was not.");
            return type;
        }

        private BaseNode CheckAssign(BaseNode expr)
        {
            Assert.IsNotNull(expr);
            var result = expr.GetResult();
            if (result.IsValid() && !IsTempVar(result))
            {
                return new AssignNode(expr.GetBegin(),
                    new ConstantNode(expr.GetBegin(), new PapyrusVariableReference(result, true)), expr);
            }
            return expr;
        }

        private bool IsTempVar(PapyrusStringTableIndex variable)
        {
            var name = variable.Identifier.ToLower();
            return name.Length > 6 && name.StartsWith("::temp") && !name.EndsWith("_var") || name == "::nonevar";
        }

        private bool IsMangledVar(PapyrusStringTableIndex variable)
        {
            var name = variable.Identifier.ToLower();
            return name.StartsWith("::mangled_");
        }


        private PapyrusStringTableIndex ToIdentifier(PapyrusVariableReference value)
        {
            if (value.Type != PapyrusPrimitiveType.Reference)
                throw new InvalidOperationException("Unable to grab the identifier of a non-reference value.");
            var asm =
                context.GetSourceAssembly();

            return asm.StringTable[value.Value.ToString()];
        }

        private BaseNode FromValue(int ip, PapyrusVariableReference value)
        {
            return new ConstantNode(ip, value);
        }

        /// <summary>
        ///     Finds the block for instruction.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <returns></returns>
        private int FindBlockForInstruction(int ip)
        {
            foreach (var cb in context.GetCodeBlocks())
            {
                var block = cb.Value;
                if (block.Begin <= ip && ip <= block.End) return block.Begin;
            }
            return PapyrusCodeBlock.END;
        }

        private void RebuildExpression(BaseNode scope)
        {
            var blocks = context.GetCodeBlocks();
            var it = scope.First();
            var lastInScope = scope.Last();
            while (it != lastInScope)
            {
                var nextIt = scope.Children.Next(it);
                var expressionGeneration = it;
                if (!expressionGeneration.IsFinal() && /*nextIt != lastInScope*/ nextIt != null) // FAILS HERE! 
                {
                    var expressionUse = nextIt;

                    // Check if an identifier in expressionUse references the result of expressionGeneration
                    // If so, perform a replacement
                    // At this steps of the decompilation, there should be only one replacement.
                    var modified = new WithNode<ConstantNode>()
                        .Select(n =>
                            n.Constant.Type == PapyrusPrimitiveType.Reference &&
                            n.Constant.AsStringTableIndex() == expressionGeneration.GetResult())
                        .Transform(n => expressionGeneration)
                        .On(expressionUse);

                    if (modified == 0)
                    {
                        it = scope.Children.Next(it);
                    }
                    else if (modified == 1)
                    {
                        it = scope[0];
                    }
                    else
                    {
                        throw new Exception("Decompilation Failed.");
                    }
                }
                else
                {
                    it = scope.Children.Next(it);
                }
            }
        }

        private void DeclareVariables(BaseNode tree)
        {
            var m = context.GetTargetMethod();
            foreach (var local in m.GetVariables())
            {
                // do nothing on temp variables
                if (!IsTempVar(local.AsStringTableIndex()))
                {
                    var scope = FindScopeForVariable(local.AsStringTableIndex(), tree);

                    Assert.IsNotNull(scope);

                    var declare = new DeclareNode(-1,
                        new ConstantNode(-1, new PapyrusVariableReference(local.Name.AsTableIndex(), true)),
                        local.TypeName.AsTableIndex());

                    var assignments = scope.Children.Where(n => n is AssignNode).Cast<AssignNode>()
                        .From(n =>
                        {
                            var dest = n.GetDestination() as ConstantNode;
                            if (dest != null)
                            {
                                return dest.Constant.Type == PapyrusPrimitiveType.Reference &&
                                       dest.Constant.AsStringTableIndex() == local.AsStringTableIndex();
                            }
                            return false;
                        }, scope).ToList();

                    // The first assignment is in the upper level scope
                    if (assignments.Any() && assignments.First().Parent == scope)
                    {
                        // Declare and assign at the same time
                        var assign = assignments.First() as AssignNode;
                        assign.SetDestination(declare);
                    }
                    else
                    {
                        // Declare at the top of the scope
                        scope.Children.Insert(0, declare);
                    }
                }
            }
        }

        private BaseNode FindScopeForVariable(PapyrusStringTableIndex variable, BaseNode scope)
        {
            var result = scope;

            var references = new WithNode<ConstantNode>()
                .Select(
                    n =>
                        n.Constant.Type == PapyrusPrimitiveType.Reference && n.Constant.AsStringTableIndex() == variable)
                .From(scope).ToList();

            if (references.Count > 0)
            {
                var commonScopes = new List<BaseNode>();
                var initial = references.First();
                references.RemoveAt(0);
                while (initial != null)
                {
                    // Find the scope hierarchy for this node by goin up,
                    // collecting scope as we go.
                    if (initial is ScopeNode)
                    {
                        commonScopes.Insert(0, initial);
                    }
                    initial = initial.Parent;
                }

                foreach (var r in references)
                {
                    var _ref = r;
                    var it = commonScopes.LastOrDefault();
                    // Exit if the scope was found in the current hierarchy
                    while (_ref != null && it == commonScopes.LastOrDefault())
                    {
                        // Find the enclosing scope of the reference.
                        while (_ref != null && !(_ref is ScopeNode))
                        {
                            _ref = _ref.Parent;
                        }
                        if (_ref != null)
                        {
                            var item = commonScopes.FirstOrDefault(i => i == _ref);
                            if (item != null)
                                it = item;
                            _ref = _ref.Parent;
                        }
                    }

                    // At least the initial scope should be common to all reference
                    Assert.AreNotEqual(null, it);

                    // If the found scope is not the last of the hierarchy, we delete from this scope to the end.
                    if (commonScopes.Next(it) != commonScopes.LastOrDefault())
                    {
                        commonScopes.Remove(commonScopes.Next(it));
                    }
                }
                // At least the initial scope should be common to all reference
                Assert.AreNotEqual(0, commonScopes.Count);
                // The result scope is the last from the hierarchy.
                result = commonScopes.LastOrDefault();
            }
            return result;
        }


        /// <summary>
        ///     Cleans up tree.
        /// </summary>
        /// <param name="tree">The tree.</param>
        private void CleanUpTree(BaseNode tree)
        {
            tree.ComputeInstructionBounds();

            // Remove the copy node, which was used to assign to temporary variables.
            new WithNode<CopyNode>()
                .Transform(n => n.GetValue())
                .On(tree);

            // Remove casting a variable to its own type as they are useless.
            new WithNode<CastNode>()
                .Select(n =>
                {
                    var value = (n.GetValue() as ConstantNode)?.Constant;
                    if (value?.Type == PapyrusPrimitiveType.Reference)
                        return TypeOfVar(value.AsStringTableIndex()) == n.GetCastType();
                    return false;
                })
                .Transform(n => n.GetValue())
                .On(tree);

            // Remove casting none as Something as they are invalid.
            new WithNode<CastNode>()
                .Select(n =>
                {
                    var value = (n.GetValue() as ConstantNode)?.Constant;
                    return value?.Type == PapyrusPrimitiveType.None;
                })
                .Transform(n => n.GetValue())
                .On(tree);

            // Replace the identifiers name index with a string value, unmangling names and property autovar
            var updated = 0;
            new WithNode<ConstantNode>()
                .Select(n => n.Constant.Type == PapyrusPrimitiveType.Reference)
                .Transform(n =>
                {
                    updated++;
                    return new IdentifierStringNode(n.GetBegin(), GetVarName(n.Constant.AsStringTableIndex()));
                })
                .On(tree);

            // Apply ! operator on == comparison
            new WithNode<UnaryOperatorNode>()
                .Select(n =>
                {
                    if (n.GetOperator() == "!" && n.GetValue() is BinaryOperatorNode)
                    {
                        var op = n.GetValue() as BinaryOperatorNode;
                        return op.GetOperator() == "==";
                    }
                    return false;
                })
                .Transform(n =>
                {
                    var op = n.GetValue() as BinaryOperatorNode;
                    var result = new BinaryOperatorNode(op.GetBegin(), op.GetPrecedence(), op.GetResult(), op.GetLeft(),
                        "!=", op.GetRight());
                    result.IncludeInstruction(n.GetEnd());
                    return result;
                })
                .On(tree);

            // Rebuild ElseIf structures
            new WithNode<IfElseNode>()
                .Select(n =>
                {
                    var elseNode = n.GetElse();
                    return elseNode.Size == 1 && elseNode[0] is IfElseNode;
                })
                .Transform(node =>
                {
                    var childIfNode = node.GetElse()[0] as IfElseNode;

                    node.SetElse(childIfNode.GetElse());
                    childIfNode.SetElse(new ScopeNode());


                    node.GetElseIf().Adopt(childIfNode);
                    node.GetElseIf().MergeChildren(childIfNode.GetElseIf());
                    return node;
                })
                .On(tree);

            // Make empty IfElse bodies that contains an "else" with a body have its condition changed
            // by adding a unaryoperator: NOT.
            // EX:
            // if(something)
            //  ; empty
            // else
            //  ; body here
            // endif
            new WithNode<IfElseNode>()
                .Select(n =>
                {

                    var elseNode = n.GetElse();
                    var elseIfNode = n.GetElseIf();
                    var bodyNode = n.GetBody();
                    return bodyNode.Size == 0 &&
                           elseNode.Size > 0 &&
                           elseIfNode.Size == 0; // First item should be the if/else. While parent is a scope.
                })
                .Transform(node =>
                {
                    var eN = node.GetElse(); // Else Body

                    node.SetBody(eN);

                    node.SetCondition(new UnaryOperatorNode(node.GetBegin(), 3, node.GetResult(), "!", node.GetCondition()));

                    node.SetElse(new ScopeNode());
                    return node;
                })
                .On(tree);

            // Extract assign operator ( x = x + 1 => x += 1)
            new WithNode<AssignNode>()
                .Select(n =>
                {
                    var destination = n.GetDestination();
                    var binaryOp = n.GetValue() as BinaryOperatorNode;
                    if (binaryOp != null)
                    {
                        if (binaryOp.GetOperator() != "||" && binaryOp.GetOperator() != "&&"
                            && !(destination is PropertyAccessNode) && !(destination is ArrayAccessNode))
                        {
                            var left = binaryOp.GetLeft();
                            return NodeComparer.IsSameTree(destination, left);
                        }
                    }
                    return false;
                })
                .Transform(n =>
                {
                    var binaryOp = n.GetValue() as BinaryOperatorNode;
                    return new AssignOperatorNode(n.GetBegin(), n.GetDestination(), binaryOp.GetOperator() + "=",
                        binaryOp.GetRight());
                })
                .On(tree);

            tree.ComputeInstructionBounds();
        }

        private string GetVarName(PapyrusStringTableIndex variable)
        {
            var name = variable.Identifier;
            if (name.StartsWith("::mangled_"))
                name = name.Substring(10);
            if (name.StartsWith("::"))
                name = name.Substring(2);
            if (name.EndsWith("_var"))
                name = name.Remove(name.Length - 4);
            return name;
        }

        //}
        //        "'. The expected condition was not met and therefor the decompilation threw this exception.");

        //    if (!condition) throw new InvalidProgramException("Assert Failed in: '" + methodName + 
        //{

        //public static void Assert(bool condition, [CallerMemberName] string methodName = null)
    }
}