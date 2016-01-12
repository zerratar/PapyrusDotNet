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

namespace PapyrusDotNet.Decompiler.Node
{
    /// <summary>
    ///     A Wrapper Class to contain and refer two nodes by Master and Slave.
    /// </summary>
    public class NodePair
    {
        private readonly int childIndex;
        private BaseNode slaveNode;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NodePair" /> class.
        /// </summary>
        /// <param name="master">The master.</param>
        /// <param name="slave">The slave.</param>
        public NodePair(BaseNode master, BaseNode slave)
        {
            MasterNode = master;

            childIndex = master.GetChildCount(i => i != null);

            master.SetChild(childIndex, slave);
        }

        /// <summary>
        ///     Gets or sets the master node.
        /// </summary>
        public BaseNode MasterNode { get; set; }

        /// <summary>
        ///     Gets or sets the slave node.
        /// </summary>
        public BaseNode SlaveNode => MasterNode[childIndex];

        public override string ToString()
        {
            return SlaveNode.ToString();
        }

        public void SetSlave(BaseNode elseNode)
        {
            MasterNode.SetChild(childIndex, elseNode);
        }
    }
}