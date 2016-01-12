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

using System.Linq;
using PapyrusDotNet.Decompiler.Extensions;
using PapyrusDotNet.Decompiler.Node;

#endregion

namespace PapyrusDotNet.Decompiler.HelperClasses
{
    public class Visitor : IVisitor
    {
        public virtual void VisitChildren(BaseNode node)
        {
            var skipNext = false;
            var originalChildren =
                node.Children.ToList(); // The list may change during the enumeration

            foreach (var child in originalChildren)
            {
                if (skipNext)
                {
                    skipNext = false;
                    continue;
                }
                if (child != null)
                {
                    child.Visit(this);

                    // if the list has been modified, skip the next item if it no longer exists.
                    if (!node.Children.Contains(originalChildren.Next(child)))
                        skipNext = true;
                }
            }
        }

        public virtual void Visit(BaseNode node)
        {
            VisitChildren(node);
        }
    }
}