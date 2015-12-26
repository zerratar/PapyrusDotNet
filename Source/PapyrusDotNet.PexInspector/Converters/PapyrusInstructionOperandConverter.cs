using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using PapyrusDotNet.PapyrusAssembly;

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


        private string GetParameterString(List<PapyrusParameterDefinition> parameters, bool includeParameterNames = false)
        {
            var paramDefs = string.Join(", ", parameters.Select(p => p.TypeName.Value +
            (includeParameterNames ? (" " + p.Name.Value) : "")));

            return "(" + paramDefs + ")";
        }
    }
}