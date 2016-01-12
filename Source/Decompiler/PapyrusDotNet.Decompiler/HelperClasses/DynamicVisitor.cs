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
using PapyrusDotNet.Decompiler.Node;

#endregion

namespace PapyrusDotNet.Decompiler.HelperClasses
{
    public class DynamicVisitor : Visitor
    {
        public Action<BaseNode, Visitor> onCommonAction;
        private Action<BaseNode, Visitor> onVisitAction;

        public DynamicVisitor Common(Action<BaseNode, Visitor> onCommonAction)
        {
            this.onCommonAction = onCommonAction;
            return this;
        }

        public override void Visit(BaseNode node)
        {
            if (onVisitAction != null)
                onVisitAction(node, this);
            else if (onCommonAction != null)
                onCommonAction(node, this);
            else base.Visit(node);
            //base.Visit(node);
        }

        public void OnVisit(Action<BaseNode, Visitor> action)
        {
            onVisitAction = action;
        }
    }
}