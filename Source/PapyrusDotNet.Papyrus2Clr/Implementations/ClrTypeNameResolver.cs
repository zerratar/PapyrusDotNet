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

#region

using System.Linq;
using PapyrusDotNet.Common.Interfaces;

#endregion

namespace PapyrusDotNet.Converters.Papyrus2Clr.Implementations
{
    public class ClrTypeNameResolver : ITypeNameResolver
    {
        public string Resolve(string typeName)
        {
            if (typeName.Contains('.')) typeName = typeName.Split('.').LastOrDefault();
            if (typeName != null)
            {
                var typeNameLower = typeName.ToLower();

                /*if (p.EndsWith("[]"))
            {
                pl = pl.Replace("[]", "");
            }*/

                if (typeNameLower == "boolean")
                    return "bool";
                if (typeNameLower == "none")
                    return "void";

                if (typeNameLower == "float" || typeNameLower == "int" || typeNameLower == "bool" ||
                    typeNameLower == "string")
                    return typeNameLower;
            }

            return typeName;
        }
    }
}