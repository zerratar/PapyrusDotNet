/*
    This file is part of PapyrusDotNet.

    PapyrusDotNet is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    PapyrusDotNet is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with PapyrusDotNet.  If not, see <http://www.gnu.org/licenses/>.
	
	Copyright 2015, Karl Patrik Johansson, zerratar@gmail.com
 */

namespace PapyrusDotNet.Common.Papyrus
{
    public class VariableReference
    {
        public VariableReference(string name, string type)
        {
            // TODO: Complete member initialization
            Name = name;
            TypeName = type;
        }

        public VariableReference(string name, string type, string definition)
        {
            // TODO: Complete member initialization
            Name = name;
            TypeName = type;
            Definition = definition;
        }

        public string DelegateInvokeReference { get; set; }

        public bool IsDelegateInstance { get; set; }

        public string Definition { get; set; }

        public object Value { get; set; }

        public string Name { get; set; }

        public string TypeName { get; set; }

        public string AutoVarName { get; set; }

        public FieldAttributes Attributes { get; set; }
    }
}