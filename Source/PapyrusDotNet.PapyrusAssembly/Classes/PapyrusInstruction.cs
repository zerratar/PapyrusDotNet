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

using System.Collections.Generic;

#endregion

namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusInstruction
    {
        public PapyrusInstruction()
        {
            Arguments = new List<PapyrusVariableReference>();
            OperandArguments = new List<PapyrusVariableReference>();
        }

        public int Offset { get; set; }
        public PapyrusOpCodes OpCode { get; set; }
        public List<PapyrusVariableReference> Arguments { get; set; }
        public PapyrusInstruction Previous { get; set; }
        public PapyrusInstruction Next { get; set; }
        public List<PapyrusVariableReference> OperandArguments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instruction is temporarily added or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instruction should be omitted from being written to the output assembly; otherwise, <c>false</c>.
        /// </value>
        internal bool TemporarilyInstruction { get; set; }

        /// <summary>
        /// Gets or sets the instruction operand, not necessary when writing but will be available when reading a papyrus assembly.
        /// hence optional.
        /// </summary>
        public object Operand { get; set; }

        /// <summary>
        /// Gets or sets the sequence point for this instruction.
        /// </summary>
        public PapyrusSequencePoint SequencePoint { get; set; }

        public string GetArg(int index)
        {
            var arg = Arguments[index];
            if (arg == null) return null;
            switch (arg.ValueType)
            {
                case PapyrusPrimitiveType.Reference:
                    return arg.Value?.ToString();
                case PapyrusPrimitiveType.String:
                    {
                        if (!arg.Value.ToString().StartsWith("\""))
                        {
                            return "\"" + arg.Value + "\"";
                        }
                        return arg.Value.ToString();
                    }
                case PapyrusPrimitiveType.Boolean:
                    {
                        if (arg.Value != null)
                        {
                            return arg.Value.Equals(1) ? "true" : "false";
                        }
                    }
                    break;
                case PapyrusPrimitiveType.Integer:
                    if (arg.Value != null)
                    {
                        return ((int)arg.Value).ToString();
                    }
                    break;
                case PapyrusPrimitiveType.Float:
                    if (arg.Value != null)
                    {
                        return ((float)arg.Value).ToString().Replace(",", ".") + "f";
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