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

using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.PapyrusAssembly;

#endregion

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
                    if (varTypeDef.BaseType.FullName.ToLower().Contains("multicastdelegate") ||
                        varTypeDef.FullName.Contains("/<>"))
                    {
                        locals.Add(local);
                    }
                }
                if (locals.Count > 0)
                    del.DelegateMethodLocalPair.Add(m, locals);
            }

            // Get any compile time generated classes. We want to omit these when creating the Pex files                      
            var nestedTypes = type.NestedTypes;
            var delegates = nestedTypes.Where(n => n.BaseType.FullName.ToLower().Contains("multicastdelegate"));
            foreach (var nt in delegates)
            {
                del.DelegateTypeDefinitions.Add(nt);
                foreach (var m in nt.Methods.Where(mn => mn.Name.StartsWith("<")))
                {
                    AddDelegateMethod(m, del, nt);
                }
            }
            foreach (var nt in nestedTypes.Where(n => n.Name.StartsWith("<>")))
            {
                del.DelegateTypeDefinitions.Add(nt);
                foreach (var m in nt.Methods.Where(mn => mn.Name.StartsWith("<")))
                {
                    AddDelegateMethod(m, del, nt);
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
                if (
                    type.Methods.Any(
                        m2 => !m2.Name.StartsWith("<") && !m2.Name.Contains(">") && m.Name.Contains(m2.Name)))
                {
                    isDelegateMethod = true;
                }
            }
            return isDelegateMethod;
        }

        public string FindDelegateInvokeReference(IDelegatePairDefinition pairDefinitions,
            PapyrusMethodDefinition papyrusMethod)
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

        private void AddDelegateMethod(MethodDefinition m, DelegatePairDefinition del, TypeDefinition nt)
        {
            var locals = new List<VariableReference>();
            foreach (var local in m.Body.Variables)
            {
                var varType = local.VariableType;
                var varTypeDef = varType.Resolve();
                if (varTypeDef.BaseType.FullName.ToLower().Contains("multicastdelegate") ||
                    varTypeDef.FullName.Contains("/<>"))
                {
                    locals.Add(local);
                }
            }
            if (locals.Count > 0)
                del.DelegateMethodLocalPair.Add(m, locals);

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
}