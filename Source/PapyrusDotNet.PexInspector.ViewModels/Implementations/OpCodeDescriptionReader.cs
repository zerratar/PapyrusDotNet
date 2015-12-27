using System;
using System.Linq;
using System.Xml.Linq;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Extensions;
using PapyrusDotNet.PexInspector.ViewModels.Interfaces;

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

                opcodeToAdd.ForEach(i => result.Instructions.Add(ParseOpCodeDescription(i, ie)));
            }

            return result;
        }

        private static OpCodeDescription ParseOpCodeDescription(string opcode, XElement ie)
        {
            var i = new OpCodeDescription();
            i.OpCode =
                Enum.GetValues(typeof(PapyrusOpCodes))
                    .Cast<PapyrusOpCodes>()
                    .FirstOrDefault(op => op.ToString().ToLower() == opcode.ToLower());

            var args = ie.Element("Arguments")?.Elements("Argument");

            if (args != null)
            {
                foreach (var arg in args)
                {
                    i.Arguments.Add(new OpCodeArgumentDescription
                    {
                        Index = int.Parse(arg.Attribute("Index").Value),
                        Alias = arg.Attribute("Alias")?.Value,
                        Description = arg.Attribute("Description")?.Value,
                        ValueType = ValueTypeFromString(arg.Attribute("ValueType")?.Value),
                        Ref = RefFromString(arg.Attribute("Ref")?.Value)
                    });
                }
            }

            var opargs = ie.Element("OperandArguments")?.Elements("OperandArgument");

            if (opargs != null)
            {
                foreach (var arg in opargs)
                {
                    i.OperandArguments.Add(new OpCodeArgumentDescription
                    {
                        Index = -1,
                        Alias = arg.Attribute("Alias")?.Value,
                        Description = arg.Attribute("Description")?.Value,
                        ValueType = ValueTypeFromString(arg.Attribute("ValueType")?.Value),
                        Ref = RefFromString(arg.Attribute("Ref")?.Value)
                    });
                }
            }

            return i;
        }

        private static OpCodeRef RefFromString(string n)
        {
            if (n == null) return OpCodeRef.None;
            return Enum.GetValues(typeof(OpCodeRef))
                    .Cast<OpCodeRef>()
                    .FirstOrDefault(op => op.ToString().ToLower() == n.ToLower());
        }

        private static OpCodeValueTypes ValueTypeFromString(string n)
        {
            if (n == null) return OpCodeValueTypes.ReferenceOrConstant;
            return Enum.GetValues(typeof(OpCodeValueTypes))
                    .Cast<OpCodeValueTypes>()
                    .FirstOrDefault(op => op.ToString().ToLower() == n.ToLower());
        }
    }
}