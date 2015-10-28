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

using System.Linq;

namespace PapyrusDotNet.CoreBuilder.Papyrus.Assembly
{
    public class PapyrusAssemblyVariable
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsArray { get; set; }

        public PapyrusAssemblyVariable(string name, string type)
        {
            Name = name;
            Type = type;
            IsArray = type.Contains("[]");

            if (type.Contains('.'))
            {
                var typeName = type.Substring(type.LastIndexOf('.'));
                Type = type.Remove(type.LastIndexOf('.')) + char.ToUpper(typeName[0]) + typeName.Substring(1);
            }
            else
            {
                Type = char.ToUpper(type[0]) + type.Substring(1);
            }
        }
    }
}