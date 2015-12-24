using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations
{
    public class DelegateFinder : IDelegateFinder
    {
        public IDelegatePairDefinition FindDelegateTypes(TypeDefinition type)
        {
            var del = new DelegatePairDefinition();

            // Map all local variables that uses the delegates
            foreach (var m in type.Methods)
            {
                var locals = new List<VariableReference>();
                foreach (var local in m.Body.Variables)
                {
                    var varType = local.VariableType;
                    var varTypeDef = varType.Resolve();
                    if (varTypeDef.BaseType.FullName.ToLower().Contains("multicastdelegate"))
                    {
                        locals.Add(local);
                    }
                }
                if (locals.Count > 0)
                    del.DelegateMethodLocalPair.Add(m, locals);
            }

            // Get any compile time generated classes. We want to omit these when creating the Pex files                      
            foreach (var nt in type.NestedTypes.Where(n => n.BaseType.FullName.ToLower().Contains("multicastdelegate")))
            {
                del.DelegateTypeDefinitions.Add(nt);
            }
            foreach (var nt in type.NestedTypes.Where(n => n.Name.StartsWith("<>")))
            {
                del.DelegateTypeDefinitions.Add(nt);
                foreach (var m in nt.Methods.Where(mn => mn.Name.StartsWith("<")))
                {
                    m.IsStatic = false;
                    m.Name = m.Name.Replace("<", "_").Replace(">", "_");
                    del.DelegateMethodDefinitions.Add(m);

                    var fieldDefinitions = new List<FieldDefinition>();
                    foreach (var instruction in m.Body.Instructions)
                    {
                        var op = instruction.Operand;
                        var fieldRef = op as FieldReference;
                        if (fieldRef != null)
                        {
                            foreach (var field in nt.Fields)
                            {
                                if (fieldRef.FullName == field.FullName)
                                {
                                    fieldDefinitions.Add(field);
                                    if (!del.DelegateFields.Contains(field))
                                        del.DelegateFields.Add(field);
                                }
                            }
                        }
                    }
                    if (fieldDefinitions.Count > 0)
                        del.DelegateMethodFieldPair.Add(m, fieldDefinitions);
                }
            }
            // Search for delegate methods inside the current class
            foreach (var m in type.Methods.Where(j => IsDelegateMethod(type, j)))
            {
                m.Name = m.Name.Replace("<", "_").Replace(">", "_");
                m.IsStatic = false;
                del.DelegateMethodDefinitions.Add(m);
            }
            return del;
        }

        public bool IsDelegateMethod(TypeDefinition type, MethodDefinition m)
        {
            var isDelegateMethod = false;
            if (m.Name.StartsWith("<") && m.Name.Contains(">") && m.Name.Contains("_"))
            {
                if (type.Methods.Any(m2 => !m2.Name.StartsWith("<") && !m2.Name.Contains(">") && m.Name.Contains(m2.Name)))
                {
                    isDelegateMethod = true;
                }
            }
            return isDelegateMethod;
        }

        public string FindDelegateInvokeReference(IDelegatePairDefinition pairDefinitions, PapyrusMethodDefinition papyrusMethod)
        {
            // In case this is a delegate inside a delegate...
            // _UtilizeDelegate4_b__0
            var functionName = papyrusMethod.Name.Value;
            var originalName = papyrusMethod.Name.Value;
            if (functionName.StartsWith("_") && functionName.Contains("b_"))
            {
                functionName = functionName.Split('_')[1];
                papyrusMethod.DelegateInvokeCount++;
            }

            var delegateMethod =
                pairDefinitions.DelegateMethodDefinitions.FirstOrDefault(
                    del =>
                        del.Name.Contains("_" + functionName + "_") &&
                        del.Name.EndsWith("_" + papyrusMethod.DelegateInvokeCount));

            if (delegateMethod == null)
            {
                delegateMethod =
                    pairDefinitions.DelegateMethodDefinitions.FirstOrDefault(
                        del =>
                            del.Name.Contains("_" + functionName + "_") && del.Name.Contains("b_") &&
                            del.Name != originalName);
            }
            papyrusMethod.DelegateInvokeCount++;
            return delegateMethod?.Name;
        }
    }
}