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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using PapyrusDotNet.Common.Utilities;
using PapyrusDotNet.Decompiler.Interfaces;
using PapyrusDotNet.Decompiler.Node;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Decompiler
{
    public class SourceBuilder
    {
        private readonly StringBuilder src = new StringBuilder();
        private int indent;
        private int indentIndex;

        public int Indent
        {
            get { return indent; }
            set
            {
                indent = value;
                indentIndex = indent;
                src.Append(StringUtility.Indent(indent, "", false));
            }
        }

        public void AppendLine()
        {
            src.AppendLine();
            src.Append(StringUtility.Indent(Indent, "", false));
        }

        public void AppendLine(string s)
        {
            src.AppendLine(s);
            src.Append(StringUtility.Indent(Indent, "", false));
        }

        public void Append(string s)
        {
            src.Append(s);
        }

        public override string ToString() => src.ToString();
    }

    public class PapyrusCodeGenerator : ICodeGenerator
    {
        private List<ICodeResultError> errors;
        private bool hasErrors;
        private int indent;
        private SourceBuilder source = new SourceBuilder();

        public ICodeResult Generate(BaseNode flowTree)
        {
            errors = new List<ICodeResultError>();
            source = new SourceBuilder();
            hasErrors = false;
            indent = 0;

            flowTree.Visit(this);

            Result = true;

            return new PapyrusCodeResult(source.ToString(), hasErrors, errors);
        }

        public string GetMethodSourceCode(ICodeResult result, PapyrusMethodDefinition method)
        {
            var sb = new StringBuilder();

            var methodBody = result.DecompiledSourceCode;
            var isNone = method.ReturnTypeName.Value.ToLower() == "none";
            var name = method.Name?.Value;
            if (string.IsNullOrEmpty(name))
            {
                if (method.IsSetter) name = "Set";
                if (method.IsSetter) name = "Get";
            }
            var returnValue = "";
            var fDesc = "Function";
            if (method.IsEvent)
                fDesc = "Event";
            else if (!isNone)
            {
                returnValue = method.ReturnTypeName.Value + " ";
            }

            var last = "";
            if (method.IsNative)
                last += " Native";
            if (method.IsGlobal)
                last += " Global";

            var parameters = GetParameterString(method.Parameters);

            sb.AppendLine(returnValue + fDesc + " " + name + parameters + last);

            if (!string.IsNullOrEmpty(method.Documentation?.Value))
                sb.AppendLine("{ " + method.Documentation?.Value + " }");

            sb.AppendLine(IndentedLines(1, methodBody.Trim('\n', '\r')));

            if (!method.IsNative)
                sb.AppendLine("End" + fDesc);

            return sb.ToString().Trim('\n');
        }

        public bool Result { get; private set; }

        public void Visit(ScopeNode node)
        {
            var notFirst = false;
            foreach (var statement in node.Children)
            {
                if (notFirst) source.AppendLine();
                notFirst = true;
                statement.Visit(this);
            }
            if (node.Parent == null)
                source.AppendLine();
        }

        public void Visit(BinaryOperatorNode node)
        {
            var parenOnLeft = node.GetPrecedence() < node.GetLeft().GetPrecedence();
            var parenOnRight = node.GetPrecedence() < node.GetRight().GetPrecedence();
            if (parenOnLeft) source.Append("(");
            node.GetLeft().Visit(this);
            if (parenOnLeft) source.Append(")");

            source.Append(" " + node.GetOperator() + " ");
            if (parenOnRight) source.Append("(");
            if (node.GetOperator() == "is" && node.GetRight() is IdentifierStringNode)
            {
                source.Append((node.GetRight() as IdentifierStringNode).GetIdentifier());
            }
            else
            {
                node.GetRight().Visit(this);
            }
            if (parenOnRight) source.Append(")");
        }

        public void Visit(UnaryOperatorNode node)
        {
            var paren = node.GetPrecedence() < node.GetValue().GetPrecedence();
            source.Append(node.GetOperator());
            if (paren) source.Append("(");
            node.GetValue().Visit(this);
            if (paren) source.Append(")");
        }

        public void Visit(AssignNode node)
        {
            node.GetDestination().Visit(this);
            source.Append(" = ");
            node.GetValue().Visit(this);
        }

        public void Visit(AssignOperatorNode node)
        {
            node.GetDestination().Visit(this);
            source.Append(" " + node.GetOperator() + " ");
            node.GetValue().Visit(this);
        }

        public void Visit(CopyNode node)
        {
            node.GetValue().Visit(this);
        }

        public void Visit(CastNode node)
        {
            var paren = node.GetPrecedence() < node.GetValue().GetPrecedence() || node.GetValue() is CastNode;
            if (paren) source.Append("(");
            node.GetValue().Visit(this);
            if (paren) source.Append(")");
            source.Append(" as " + node.GetCastType().Identifier);
        }

        public void Visit(CallMethodNode node)
        {
            var paren = node.GetPrecedence() < node.GetObject().GetPrecedence();
            if (paren) source.Append("(");
            if (node.GetObject() is IdentifierStringNode)
            {
                source.Append((node.GetObject() as IdentifierStringNode).GetIdentifier());
            }
            else
            {
                node.GetObject().Visit(this);
            }
            if (paren) source.Append(")");

            source.Append("." + node.GetMethod().Identifier + "(");
            node.GetParameters().Visit(this);
            source.Append(")");
        }

        public void Visit(ParamsNode node)
        {
            var notFirst = false;
            foreach (var param in node.Children)
            {
                if (notFirst) source.Append(", ");
                notFirst = true;
                param.Visit(this);
            }
        }

        public void Visit(ReturnNode node)
        {
            source.Append("return");
            var val = node.GetValue();
            if (val != null)
            {
                source.Append(" ");
                val.Visit(this);
            }
        }

        public void Visit(PropertyAccessNode node)
        {
            var paren = node.GetPrecedence() < node.GetObject().GetPrecedence();
            if (paren) source.Append("(");
            node.GetObject().Visit(this);
            if (paren) source.Append(")");
            source.Append("." + node.GetProperty().Identifier);
        }

        public void Visit(StructCreateNode node)
        {
            source.Append("new " + node.GetStructType().Identifier + "()");
        }

        public void Visit(ArrayCreateNode node)
        {
            var type = node.GetArrayType().Identifier.Replace("[]", "");
            source.Append("new " + type + "[");
            node.GetIndex().Visit(this);
            source.Append("]");
        }

        public void Visit(ArrayLengthNode node)
        {
            var paren = node.GetPrecedence() < node.GetArray().GetPrecedence();
            if (paren) source.Append("(");
            node.GetArray().Visit(this);
            if (paren) source.Append(")");
            source.Append(".length");
        }

        public void Visit(ArrayAccessNode node)
        {
            var paren = node.GetPrecedence() < node.GetArray().GetPrecedence();
            if (paren) source.Append("(");
            node.GetArray().Visit(this);
            if (paren) source.Append(")");
            source.Append("[");
            node.GetIndex().Visit(this);
            source.Append("]");
        }

        public void Visit(ConstantNode node)
        {
            source.Append(node.Constant.GetStringRepresentation());
        }

        public void Visit(IdentifierStringNode node)
        {
            if (node.GetIdentifier().ToLower() == "self")
                source.Append("Self");
            else
                source.Append(node.GetIdentifier());
        }

        public void Visit(WhileNode node)
        {
            source.Append("While (");
            node.GetCondition().Visit(this);
            source.Append(")");
            source.Indent++;
            source.AppendLine();
            node.GetBody().Visit(this);
            source.Indent--;
            source.AppendLine();
            source.Append("EndWhile");
        }

        public void Visit(IfElseNode node)
        {
            source.Append("If (");
            node.GetCondition().Visit(this);
            source.AppendLine(")");
            source.Indent++;
            node.GetBody().Visit(this);
            source.Indent--;
            source.AppendLine();
            var lastBody = node.GetBody();
            foreach (var childNode in node.GetElseIf().Children)
            {
                var elseIf = childNode as IfElseNode;
                source.Append("ElseIf (");
                elseIf.GetCondition().Visit(this);
                source.AppendLine(")");
                source.Indent++;
                elseIf.GetBody().Visit(this);
                source.Indent--;
                source.AppendLine();
                lastBody = elseIf.GetBody();
            }
            if (node.GetElse().Size != 0)
            {
                source.Append("Else");
                source.Indent++;
                source.AppendLine();
                node.GetElse().Visit(this);
                source.Indent--;
                source.AppendLine();
            }
            source.Append("EndIf");
        }

        public void Visit(DeclareNode node)
        {
            source.Append(node.GetDeclareType().Identifier + " ");
            node.GetObject().Visit(this);
        }

        private string IndentedLines(int indentLevel, string methodBody)
        {
            var lines = methodBody.Split('\n');
            return string.Join("", lines.Select(l => StringUtility.Indent(indentLevel, l, false)));
        }

        private string GetParameterString(List<PapyrusParameterDefinition> parameters, bool includeParameterNames = true)
        {
            var paramDefs = string.Join(", ", parameters.Select(p => p.TypeName.Value +
                                                                     (includeParameterNames ? " " + p.Name.Value : "")));

            return "(" + paramDefs + ")";
        }
    }
}