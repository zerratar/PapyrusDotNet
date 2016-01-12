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
using PapyrusDotNet.Decompiler.HelperClasses;
using PapyrusDotNet.Decompiler.Node;

#endregion

namespace PapyrusDotNet.Decompiler.Extensions
{
    public class WithNode<T> where T : BaseNode
    {
        public Func<T, bool> FilterFunction = arg => true;
        public List<BaseNode> ResultData;
        public Func<T, BaseNode> TransformFunction;

        public WithNode<T> Select(Func<T, bool> filter)
        {
            FilterFunction = filter;
            return this;
        }

        public WithNode<T> Transform(Func<T, BaseNode> transform)
        {
            TransformFunction = transform;
            return this;
        }

        public List<BaseNode> From(BaseNode tree)
        {
            var result = new List<BaseNode>();

            var nodeSelector = new DynamicVisitor();

            nodeSelector.OnVisit((node, visitor) =>
            {
                if (FilterFunction == null || node is T && FilterFunction((T) node))
                {
                    result.Add(node);
                }
                visitor.VisitChildren(node);
            });

            tree.Visit(nodeSelector);

            return result;
        }

        public int On(BaseNode tree)
        {
            var result = 0;

            var nodeSelector = new DynamicVisitor();
            nodeSelector.OnVisit((node, visitor) =>
            {
                visitor.VisitChildren(node);
                if (FilterFunction == null || node is T && FilterFunction((T) node))
                {
                    var transformedNode = TransformFunction((T) node);
                    if (transformedNode != node)
                    {
                        node.Parent.Replace(node, transformedNode);
                    }
                    ++result;
                }
            });
            tree.Visit(nodeSelector);
            return result;
        }
    }

    public static class TransformExtensions
    {
        public static IEnumerable<BaseNode> From<T>(this IEnumerable<T> nodes,
            Func<T, bool> nodeFilter, BaseNode tree) where T : BaseNode
        {
            var selector = new DynamicVisitor();
            var result = new List<BaseNode>();

            selector.OnVisit((node, visitor) =>
            {
                if (node is T && nodeFilter((T) node))
                {
                    result.Add(node);
                }
                visitor.VisitChildren(node);
            });
            tree.Visit(selector);
            return result;
        }

        public static int TransformOn<T>(this IEnumerable<T> nodes,
            Func<T, bool> nodeFilter,
            Func<T, BaseNode> transform, BaseNode tree)
            where T : BaseNode
        {
            var selector = new DynamicVisitor();

            var result = 0;
            selector.OnVisit((node, visitor) =>
            {
                visitor.VisitChildren(node);
                if (node is T && nodeFilter((T) node))
                {
                    var transformed = transform((T) node);
                    if (transformed != node)
                    {
                        var parent = node.Parent;
                        parent.Replace(node, transformed);
                    }
                    result++;
                }
            });

            tree.Visit(selector);

            return result;
        }
    }
}