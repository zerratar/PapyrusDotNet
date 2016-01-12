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

using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;

#endregion

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations
{
    public class PropertyAnalyzer : IPropertyAnalyzer
    {
        public IPropertyAnalyzerResult Analyze(PropertyDefinition property)
        {
            var isAutoVar = true;
            var autoVarName = "::" + property.Name + "_var";

            if (property.SetMethod != null)
            {
                // If thsi is going to be a autovar we expect it to be: 
                // ldarg.0
                // ldarg.1
                // stfld <backingField>
                // ret
                var instructions =
                    property.SetMethod.Body.Instructions.Where(i => i.OpCode.Code != Code.Nop).ToArray();
                if (instructions.Length == 4)
                {
                    var ldarg0 = instructions[0];
                    var ldarg1 = instructions[1];
                    var stfld = instructions[2];
                    var ret = instructions[3];

                    if (ldarg0.OpCode.Code != Code.Ldarg_0 || ldarg1.OpCode.Code != Code.Ldarg_1 ||
                        stfld.OpCode.Code != Code.Stfld || ret.OpCode.Code != Code.Ret)
                        isAutoVar = false;

                    if (isAutoVar)
                    {
                        var field = stfld.Operand as FieldDefinition;
                        autoVarName = field.Name.Replace("<", "_").Replace(">", "_");
                    }
                }
                else isAutoVar = false;
            }

            if (property.GetMethod != null)
            {
                // ldarg.0
                // ldfld <backingField>
                // ret

                var instructions =
                    property.GetMethod.Body.Instructions.Where(i => i.OpCode.Code != Code.Nop).ToArray();

                if (instructions.Length == 3)
                {
                    var ldarg0 = instructions[0];
                    var ldfld = instructions[1];
                    var ret = instructions[2];

                    if (ldarg0.OpCode.Code != Code.Ldarg_0 || ldfld.OpCode.Code != Code.Ldfld ||
                        ret.OpCode.Code != Code.Ret)
                        isAutoVar = false;

                    if (isAutoVar)
                    {
                        var field = ldfld.Operand as FieldDefinition;
                        autoVarName = field.Name.Replace("<", "_").Replace(">", "_");
                    }
                }
                else isAutoVar = false;
            }

            return new PropertyAnalyzerResult(isAutoVar, autoVarName);
        }
    }
}