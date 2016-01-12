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
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.PexInspector.Converters
{
    public class PapyrusInstructionDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var i = value as PapyrusInstruction;
            if (i == null)
                return Binding.DoNothing;

            var instructionParams = string.Join(" ", i.Arguments.Select(GetArgumentValue));
            var instructionObjectParams = string.Join(" ", i.OperandArguments.Select(GetArgumentValue));

            var output = "L_" + i.Offset.ToString("0000") + ": " + i.OpCode + " " + instructionParams + " " +
                         instructionObjectParams;
            if (i.OpCode == PapyrusOpCodes.Jmp)
                output += " (Offset: L_" + (i.Offset + int.Parse(GetArgumentValue(i.Arguments[0]))).ToString("0000") +
                          ")";
            if (i.OpCode == PapyrusOpCodes.Jmpf || i.OpCode == PapyrusOpCodes.Jmpt)
                output += " (Offset: L_" + (i.Offset + int.Parse(GetArgumentValue(i.Arguments[1]))).ToString("0000") +
                          ")";
            return output;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }


        private string GetArgumentValue(PapyrusVariableReference arg)
        {
            if (arg == null) return null;
            switch (arg.Type)
            {
                case PapyrusPrimitiveType.Reference:
                    return arg.Value?.ToString();
                case PapyrusPrimitiveType.String:
                    return "\"" + arg.Value + "\"";
                case PapyrusPrimitiveType.Boolean:
                {
                    if (arg.Value != null)
                    {
                        return arg.Value.Equals(true) || arg.Value.Equals(1) ? "true" : "false";
                    }
                }
                    break;
                case PapyrusPrimitiveType.Integer:
                    if (arg.Value != null)
                    {
                        return arg.Value.ToString();
                    }
                    break;
                case PapyrusPrimitiveType.Float:
                    if (arg.Value != null)
                    {
                        return arg.Value.ToString().Replace(",", ".") + "f";
                    }
                    break;
            }

            if (arg.Name != null)
            {
                return (string) arg.Name;
            }
            return null;
        }
    }
}