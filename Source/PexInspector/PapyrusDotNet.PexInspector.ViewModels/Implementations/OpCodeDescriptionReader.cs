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
using System.Linq;
using System.Xml.Linq;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Extensions;
using PapyrusDotNet.PexInspector.ViewModels.Interfaces;

#endregion

namespace PapyrusDotNet.PexInspector.ViewModels.Implementations
{
    public class OpCodeDescriptionReader : IOpCodeDescriptionReader
    {
        public IOpCodeDescriptionDefinition Read(string file)
        {
            var doc = XDocument.Load(file);
            var result = new OpCodeDescriptionDefinition();

            var instructionElements = doc.Root.Elements("Instruction");

            foreach (var ie in instructionElements)
            {
                var opcodes = ie.Attribute("OpCode").Value;
                var opcodeToAdd = opcodes.Split(',');

                opcodeToAdd.ForEach(i => result.Instructions.Add(ParseOpCodeDescription(i, ie, result)));
            }

            return result;
        }

        private static OpCodeDescription ParseOpCodeDescription(string opcode, XElement ie,
            OpCodeDescriptionDefinition definiton)
        {
            var i = new OpCodeDescription();
            i.OpCode =
                Enum.GetValues(typeof (PapyrusOpCodes))
                    .Cast<PapyrusOpCodes>()
                    .FirstOrDefault(op => op.ToString().ToLower() == opcode.ToLower());

            i.Definition = definiton;

            var args = ie.Element("Arguments")?.Elements("Argument");

            if (args != null)
            {
                foreach (var arg in args)
                {
                    var constraints = new OpCodeConstraint[0];
                    var constraintValue = arg.Attribute("Constraint")?.Value;
                    if (constraintValue != null)
                        constraints = constraintValue.Split(',').Select(ConstraintFromString).ToArray();

                    i.Arguments.Add(new OpCodeArgumentDescription
                    {
                        Index = int.Parse(arg.Attribute("Index").Value),
                        Alias = arg.Attribute("Alias")?.Value,
                        Description = arg.Attribute("Description")?.Value,
                        ValueType = ValueTypeFromString(arg.Attribute("ValueType")?.Value),
                        Ref = RefFromString(arg.Attribute("Ref")?.Value),
                        Constraints = constraints
                    });
                }
            }

            var opargs = ie.Element("OperandArguments")?.Elements("OperandArgument");

            if (opargs != null)
            {
                foreach (var arg in opargs)
                {
                    var constraints = new OpCodeConstraint[0];
                    var constraintValue = arg.Attribute("Constraint")?.Value;
                    if (constraintValue != null)
                        constraints = constraintValue.Split(',').Select(ConstraintFromString).ToArray();

                    i.OperandArguments.Add(new OpCodeArgumentDescription
                    {
                        Index = -1,
                        Alias = arg.Attribute("Alias")?.Value,
                        Description = arg.Attribute("Description")?.Value,
                        ValueType = ValueTypeFromString(arg.Attribute("ValueType")?.Value),
                        Ref = RefFromString(arg.Attribute("Ref")?.Value),
                        Constraints = constraints
                    });
                }
            }

            return i;
        }

        private static OpCodeConstraint ConstraintFromString(string n)
        {
            if (n == null) return OpCodeConstraint.NoConstraints;
            return Enum.GetValues(typeof (OpCodeConstraint))
                .Cast<OpCodeConstraint>()
                .FirstOrDefault(op => op.ToString().ToLower() == n.ToLower());
        }

        private static OpCodeRef RefFromString(string n)
        {
            if (n == null) return OpCodeRef.None;
            return Enum.GetValues(typeof (OpCodeRef))
                .Cast<OpCodeRef>()
                .FirstOrDefault(op => op.ToString().ToLower() == n.ToLower());
        }

        private static OpCodeValueTypes ValueTypeFromString(string n)
        {
            if (n == null) return OpCodeValueTypes.ReferenceOrConstant;
            return Enum.GetValues(typeof (OpCodeValueTypes))
                .Cast<OpCodeValueTypes>()
                .FirstOrDefault(op => op.ToString().ToLower() == n.ToLower());
        }
    }
}