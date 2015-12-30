using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.PexInspector.ViewModels.Extensions
{
    public static class PapyrusOpCodeExtensions
    {
        //public static string GetArgumentsDescription(this PapyrusOpCodes code)
        //{
        //    var noArgs = "No arguments expected.";

        //    switch (code)
        //    {
        //        case PapyrusOpCodes.PropSet:
        //            return "[0] Constant String: Property Name\r\n[1] Reference or Constant: Location\r\n[2] Reference or Constant: Value";
        //        case PapyrusOpCodes.PropGet:
        //            return "[0] Constant String: Property Name\r\n[1] Reference or Constant: Location\r\n[2] Reference: Destination";
        //        case PapyrusOpCodes.Assign:
        //            return "[0] Reference or Constant: Value\r\n[1] Reference: Destination";
        //        case PapyrusOpCodes.Callstatic:
        //            return "[0] Reference or Constant: Location/Class\r\n[1] Constant String: Method Name\r\n[2] Reference: Destination";
        //        case PapyrusOpCodes.Callmethod:
        //            return "[0] Constant String: Method Name\r\n[1] Reference or Constant: Location/Class\r\n[2] Reference: Destination";
        //        case PapyrusOpCodes.Return:
        //            return "[0] Reference or Constant: value\r\n[1] (Expects ::nonevar if void)";
        //        case PapyrusOpCodes.CmpEq:
        //        case PapyrusOpCodes.CmpGte:
        //        case PapyrusOpCodes.CmpGt:
        //        case PapyrusOpCodes.CmpLt:
        //        case PapyrusOpCodes.CmpLte:
        //            return "[0] Reference: Destination\r\n[1] Reference or Constant: Value\r\n[2]  Reference or Constant: Value";
        //        case PapyrusOpCodes.Jmp:
        //            return "[0] Constant Integer: offset from current position";
        //        case PapyrusOpCodes.Jmpf:
        //            return "[0] Reference Bool: Conditional\r\n[1] Constant Integer: offset from current position";
        //        case PapyrusOpCodes.Jmpt:
        //            return "[0] Reference Bool: Conditional\r\n[1] Constant Integer: offset from current position";
        //    }

        //    return "This OpCode has no arguments or the arguments has not yet been documented.";
        //}

        public static string GetOperandArgumentsDescription(this PapyrusOpCodes code)
        {
            var noArgs = "No operand arguments expected.";
            switch (code)
            {
                case PapyrusOpCodes.Callparent:
                case PapyrusOpCodes.Callstatic:
                case PapyrusOpCodes.Callmethod:
                    return "Method Parameters can be either Constant or Reference. One parameter per row.";
                default:
                    return noArgs;
            }
        }

        public static string GetDescription(this PapyrusOpCodes code)
        {
            switch (code)
            {
                case PapyrusOpCodes.Iadd:
                    return
                        "Integer Add math operator, takes two references or constant values and adds them together and then assigns the result to the destination variable.";
                case PapyrusOpCodes.Fadd:
                    return
                        "Float Add math operator, takes two references or constant values and adds them together and then assigns the result to the destination variable.";
                case PapyrusOpCodes.Return:
                    return
                        "Terminate the method and returns a value. If the method is a void (none) it should return a ::nonevar.";
                case PapyrusOpCodes.Callparent:
                    return
                        "Invokes a parent method from inside a state by its name, using a location reference, set of arguments and assigns the result to the destination variable.";
                case PapyrusOpCodes.Callstatic:
                    return
                        "Invokes a static method by its name, using a location reference, set of arguments and assigns the result to the destination variable.";
                case PapyrusOpCodes.Callmethod:
                    return
                        "Invokes a instance method by its name, using a location reference, set of arguments and assigns the result to the destination variable.";
                case PapyrusOpCodes.Cast:
                    return "Casts variable, parameter or reference into the same type as the target reference.";
                case PapyrusOpCodes.ArrayCreate:
                    return
                        "Creates a new array instance of a target type and size and  then assigns the destination variable with the new array.";
                case PapyrusOpCodes.ArrayLength:
                    return "Gets the length of the target array and assigns the destination variable with the count.";
                case PapyrusOpCodes.Assign:
                    return "Assign a field, variable or parameter with the target value.";
                case PapyrusOpCodes.Jmp:
                    return "Jumps to the target instruction by an offset from the current position.";
                case PapyrusOpCodes.Jmpf:
                    return "If the condition is false then jump to the target instruction by an offset from the current position.";
                case PapyrusOpCodes.Jmpt:
                    return "If the condition is true then jump to the target instruction by an offset from the current position.";
            }
            return "This OpCode has not yet been documented.";
        }
    }
}
