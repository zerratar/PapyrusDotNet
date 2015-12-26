using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using PapyrusDotNet.PapyrusAssembly;

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

            var output = "L_" + i.Offset.ToString("0000") + ": " + i.OpCode + " " + instructionParams + " " + instructionObjectParams;
            if (i.OpCode == PapyrusOpCodes.Jmp)
                output += " (Offset: L_" + (i.Offset + int.Parse(GetArgumentValue(i.Arguments[0]))).ToString("0000") + ")";
            if (i.OpCode == PapyrusOpCodes.Jmpf || i.OpCode == PapyrusOpCodes.Jmpt)
                output += " (Offset: L_" + (i.Offset + int.Parse(GetArgumentValue(i.Arguments[1]))).ToString("0000") + ")";
            return output;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }


        private string GetArgumentValue(PapyrusVariableReference arg)
        {
            if (arg == null) return null;
            switch (arg.ValueType)
            {
                case PapyrusPrimitiveType.Reference:
                    return arg.Value?.ToString();
                case PapyrusPrimitiveType.String:
                    return "\"" + arg.Value + "\"";
                case PapyrusPrimitiveType.Boolean:
                    {
                        if (arg.Value != null)
                        {
                            return (arg.Value.Equals(true) || arg.Value.Equals(1)) ? "true" : "false";
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
                return (string)arg.Name;
            }
            return null;
        }
    }
}
