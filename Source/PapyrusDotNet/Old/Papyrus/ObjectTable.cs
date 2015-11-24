#region License

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
//     Copyright 2015, Karl Patrik Johansson, zerratar@gmail.com

#endregion

#region

using System.Collections.Generic;
using PapyrusDotNet.Common.Papyrus;

#endregion

namespace PapyrusDotNet.Old.Papyrus
{
    public class ObjectTable
    {
        public ObjectTable()
        {
            Info = new FieldAttributes();
            VariableTable = new List<VariableReference>();
            PropertyTable = new List<VariableReference>();
            StateTable = new List<ObjectState>();
        }

        public string Name { get; set; }

        public string BaseType { get; set; }

        public FieldAttributes Info { get; set; }

        public string AutoState { get; set; }

        public List<VariableReference> VariableTable { get; set; }

        public List<VariableReference> PropertyTable { get; set; }

        public List<ObjectState> StateTable { get; set; }
    }
}