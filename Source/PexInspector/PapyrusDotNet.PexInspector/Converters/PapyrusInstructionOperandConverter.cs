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
//     Copyright 2016, Karl Patrik Johansson, zerratar@gmail.com

#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.PexInspector.Converters
{
    public class PapyrusInstructionOperandConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var instruction = value as PapyrusInstruction;
            if (instruction != null)
            {
                return "-> (" + instruction.Offset + ") " + instruction.OpCode;
            }

            var method = value as PapyrusMethodDefinition;
            if (method != null)
            {
                return method.ReturnTypeName.Value + " " + method.Name.Value + GetParameterString(method.Parameters);
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }


        private string GetParameterString(List<PapyrusParameterDefinition> parameters,
            bool includeParameterNames = false)
        {
            var paramDefs = string.Join(", ", parameters.Select(p => p.TypeName.Value +
                                                                     (includeParameterNames ? " " + p.Name.Value : "")));

            return "(" + paramDefs + ")";
        }
    }
}