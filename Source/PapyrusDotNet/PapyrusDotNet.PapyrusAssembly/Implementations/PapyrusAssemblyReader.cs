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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PapyrusDotNet.PapyrusAssembly.Extensions;
using PapyrusDotNet.PapyrusAssembly.Interfaces;
using PapyrusDotNet.PapyrusAssembly.IO;

#endregion

namespace PapyrusDotNet.PapyrusAssembly.Implementations
{
    internal class PapyrusAssemblyReader : IPapyrusAssemblyReader, IDisposable
    {
        private readonly PapyrusAssemblyDefinition assembly;
        private readonly PexReader pexReader;

        private bool isDisposed;

        public PapyrusAssemblyReader(PapyrusAssemblyDefinition assembly, string pex, bool throwsException = false)
        {
            this.assembly = assembly;
            ThrowsExceptions = throwsException;
            pexReader = new PexReader(assembly, pex, throwsException);
        }

        public bool IsCorrupted => pexReader.IsCorrupted;
        public bool ThrowsExceptions { get; }

        public void Dispose()
        {
            Dispose(true);
        }

        public PapyrusAssemblyDefinition Read()
        {
            assembly.Header = ReadHeader(assembly);
            try
            {
                if (assembly.HasDebugInfo)
                {
                    assembly.DebugInfo = ReadDebugInfo(assembly);
                }

                //if (asm.Header.VersionTarget == PapyrusVersionTargets.Skyrim)
                //{
                //    throw new NotImplementedException(
                //        "Reading and Writing Skyrim Papyrus (v3.2) has not yet been implemented. Please use an older version of PapyrusDotNet for this.");
                //}

                ReadHeaderUserflags(assembly);

                assembly.Types = ReadTypeDefinitions(assembly);
            }
            catch (Exception exc)
            {
                pexReader.IsCorrupted = true;
                if (ThrowsExceptions)
                    throw exc;
            }
            return assembly;
        }

        private void ReadHeaderUserflags(PapyrusAssemblyDefinition asm)
        {
            var userflagCount = pexReader.ReadInt16();

            for (var userflagIndex = 0; userflagIndex < userflagCount; userflagIndex++)
            {
                var stringValue = pexReader.ReadString();
                var flagValue = pexReader.ReadByte();
                asm.Header.UserflagReferenceHeader.Add(stringValue.Ref(asm), flagValue);
            }
        }

        public PapyrusHeader ReadHeader(PapyrusAssemblyDefinition asm)
        {
            var header = new PapyrusHeader(asm);
            header.HeaderIdentifier = pexReader.ReadUInt32();
            switch (header.HeaderIdentifier)
            {
                case PapyrusHeader.Fallout4PapyrusHeaderIdentifier:
                    asm.VersionTarget = PapyrusVersionTargets.Fallout4;
                    pexReader.SetVersionTarget(PapyrusVersionTargets.Fallout4);
                    break;
                default:
                    asm.VersionTarget = PapyrusVersionTargets.Skyrim;
                    pexReader.SetVersionTarget(PapyrusVersionTargets.Skyrim);
                    break;
            }
            ReadHeader(header);

            ReadStringTable(asm);

            var hasDebugInfoByte = pexReader.ReadByte();

            asm.HasDebugInfo = hasDebugInfoByte == 1;

            return header;
        }

        private void ReadStringTable(PapyrusAssemblyDefinition asm)
        {
            var stringTableLength = pexReader.ReadInt16();

            var stringTable = new PapyrusStringTable();

            for (var i = 0; i < stringTableLength; i++)
            {
                stringTable.Add(pexReader.ReadString());
            }

            asm.StringTable = stringTable;

            pexReader.SetStringTable(stringTable);
        }

        public PapyrusTypeDebugInfo ReadDebugInfo(PapyrusAssemblyDefinition asm)
        {
            var debugTable = new PapyrusTypeDebugInfo();

            var debugTime = pexReader.ReadInt64();
            debugTable.DebugTime = debugTime;

            var functionCount = pexReader.ReadInt16();

            var methodDescriptions = new List<PapyrusMethodDecription>();
            for (var i = 0; i < functionCount; i++)
            {
                var dbgfunc = new PapyrusMethodDecription();
                dbgfunc.DeclaringTypeName = pexReader.ReadStringRef();
                dbgfunc.StateName = pexReader.ReadStringRef();
                dbgfunc.Name = pexReader.ReadStringRef();
                dbgfunc.MethodType = (PapyrusMethodTypes)pexReader.ReadByte();
                dbgfunc.BodyLineNumbers = new List<short>();
                var lineNumberCount = pexReader.ReadInt16();
                for (var j = 0; j < lineNumberCount; j++)
                {
                    dbgfunc.BodyLineNumbers.Add(pexReader.ReadInt16());
                }
                methodDescriptions.Add(dbgfunc);
            }
            debugTable.MethodDescriptions = methodDescriptions;

            if (asm.VersionTarget == PapyrusVersionTargets.Fallout4)
            {
                var propertyGroupCount = pexReader.ReadInt16();
                var propertyDescriptions = new List<PapyrusStatePropertyDescriptions>();
                for (var i = 0; i < propertyGroupCount; i++)
                {
                    var groupInfo = new PapyrusStatePropertyDescriptions();
                    groupInfo.ObjectName = pexReader.ReadStringRef();
                    groupInfo.GroupName = pexReader.ReadStringRef();
                    groupInfo.GroupDocumentation = pexReader.ReadStringRef();
                    groupInfo.Userflags = pexReader.ReadInt32();
                    var propertyNameCount = pexReader.ReadInt16();
                    for (var j = 0; j < propertyNameCount; j++)
                    {
                        groupInfo.PropertyNames.Add(pexReader.ReadStringRef());
                    }
                    propertyDescriptions.Add(groupInfo);
                }
                debugTable.PropertyDescriptions = propertyDescriptions;

                var structureOrderCount = pexReader.ReadInt16();

                var structDescriptions = new List<PapyrusStructDescription>();
                for (var i = 0; i < structureOrderCount; i++)
                {
                    var structDescription = new PapyrusStructDescription();
                    structDescription.DeclaringTypeName = pexReader.ReadStringRef();
                    structDescription.Name = pexReader.ReadStringRef();
                    var fieldCount = pexReader.ReadInt16();
                    for (var j = 0; j < fieldCount; j++)
                    {
                        structDescription.FieldNames.Add(pexReader.ReadStringRef());
                    }
                    structDescriptions.Add(structDescription);
                }
                debugTable.StructDescriptions = structDescriptions;
            }
            return debugTable;
        }

        public Collection<PapyrusTypeDefinition> ReadTypeDefinitions(PapyrusAssemblyDefinition asm)
        {
            var types = new Collection<PapyrusTypeDefinition>();

            var classCount = pexReader.ReadInt16();
            for (var j = 0; j < classCount; j++)
            {
                var typeDef = new PapyrusTypeDefinition(asm);
                typeDef.IsClass = true;

                if (asm.VersionTarget == PapyrusVersionTargets.Fallout4)
                {
                    ReadTypeInfo(asm, typeDef);

                    ReadStructs(asm, typeDef);

                    ReadFields(asm, typeDef);

                    ReadProperties(asm, typeDef);

                    ReadStates(asm, typeDef);
                }
                else
                {
                    typeDef.Name = pexReader.ReadStringRef();
                    typeDef.Size = pexReader.ReadInt32();
                    // pexReader.DEBUGGING = true
                    typeDef.BaseTypeName = pexReader.ReadStringRef();
                    typeDef.Documentation = pexReader.ReadStringRef();
                    typeDef.UserFlags = pexReader.ReadInt32();
                    typeDef.AutoStateName = pexReader.ReadStringRef();

                    ReadFields(asm, typeDef);

                    ReadProperties(asm, typeDef);

                    ReadStates(asm, typeDef);
                }
                types.Add(typeDef);
            }
            return types;
        }

        private void ReadTypeInfo(PapyrusAssemblyDefinition asm, PapyrusTypeDefinition typeDef)
        {
            typeDef.Name = pexReader.ReadStringRef();
            typeDef.Size = pexReader.ReadInt32();
            typeDef.BaseTypeName = pexReader.ReadStringRef();
            typeDef.Documentation = pexReader.ReadStringRef();
            typeDef.Flags = pexReader.ReadByte();
            typeDef.UserFlags = pexReader.ReadInt32();
            typeDef.AutoStateName = pexReader.ReadStringRef();
        }

        private void ReadStructs(PapyrusAssemblyDefinition asm, PapyrusTypeDefinition typeDef)
        {
            var structCount = pexReader.ReadInt16();
            for (var i = 0; i < structCount; i++)
            {
                var structDef = new PapyrusTypeDefinition(asm, true);
                structDef.IsStruct = true;
                structDef.Name = pexReader.ReadStringRef();

                var variableCount = pexReader.ReadInt16();
                for (var l = 0; l < variableCount; l++)
                {
                    structDef.Fields.Add(ReadDocumentedField(asm, typeDef));
                }
                typeDef.NestedTypes.Add(structDef);
            }
        }

        private void ReadStates(PapyrusAssemblyDefinition asm, PapyrusTypeDefinition typeDef)
        {
            var stateCount = pexReader.ReadInt16();

            for (var i = 0; i < stateCount; i++)
            {
                var state = new PapyrusStateDefinition(typeDef);
                state.Name = pexReader.ReadStringRef();
                var methodCount = pexReader.ReadInt16();
                for (var k = 0; k < methodCount; k++)
                {
                    var name = pexReader.ReadString();
                    var method = ReadMethod(asm);

                    method.DeclaringState = state;
                    method.Name = new PapyrusStringRef(asm, name);
                    if (method.Name.Value.ToLower().StartsWith("on"))
                    {
                        // For now, lets assume that all functions with the name starting with "On" is an event.
                        method.IsEvent = true;
                    }
                    state.Methods.Add(method);
                }
                // typeDef.States.Add(state);
            }

            UpdateOperands(typeDef.States);
        }

        private void UpdateOperands(Collection<PapyrusStateDefinition> states)
        {
            // Sets the operand to its proper target

            var allMethods = states
                .SelectMany(s => s.Methods).ToList();

            var props = assembly.Types.First().Properties;
            foreach (var p in props)
            {
                if (p.SetMethod != null)
                {
                    foreach (var inst in p.SetMethod.Body.Instructions)
                    {
                        inst.Method = p.SetMethod;
                        assembly.Types.First().UpdateOperand(inst, p.SetMethod.Body.Instructions);
                    }
                }
                if (p.GetMethod != null)
                {
                    foreach (var inst in p.GetMethod.Body.Instructions)
                    {
                        inst.Method = p.GetMethod;
                        assembly.Types.First().UpdateOperand(inst, p.GetMethod.Body.Instructions);
                    }
                }
            }
            foreach (var state in states)
            {
                foreach (var m in state.Methods)
                {
                    foreach (var inst in m.Body.Instructions)
                    {
                        inst.Method = m;
                        state.DeclaringType.UpdateOperand(inst, m.Body.Instructions);
                    }
                }
            }
        }

        private void ReadProperties(PapyrusAssemblyDefinition asm, PapyrusTypeDefinition typeDef)
        {
            var propDefs = new Collection<PapyrusPropertyDefinition>();
            var propertyCount = pexReader.ReadInt16();
            for (var i = 0; i < propertyCount; i++)
            {
                var prop = new PapyrusPropertyDefinition(asm);
                prop.Name = pexReader.ReadStringRef();
                prop.TypeName = pexReader.ReadStringRef();
                prop.Documentation = pexReader.ReadStringRef();
                prop.Userflags = pexReader.ReadInt32();
                prop.Flags = pexReader.ReadByte();
                if (prop.IsAuto)
                {
                    prop.AutoName = pexReader.ReadString();
                }
                else
                {
                    if (prop.HasGetter)
                    {
                        prop.GetMethod = ReadMethod(asm);
                        prop.GetMethod.IsGetter = true;
                        prop.GetMethod.PropName = prop.Name.Value;
                    }
                    if (prop.HasSetter)
                    {
                        prop.SetMethod = ReadMethod(asm);
                        prop.SetMethod.IsSetter = true;
                        prop.SetMethod.PropName = prop.Name.Value;
                    }
                }
                propDefs.Add(prop);
            }
            typeDef.Properties = propDefs;
        }

        private void ReadFields(PapyrusAssemblyDefinition asm, PapyrusTypeDefinition typeDef)
        {
            var fieldCount = pexReader.ReadInt16();
            for (var i = 0; i < fieldCount; i++)
            {
                typeDef.Fields.Add(ReadFieldDefinition(asm, typeDef));
            }
        }

        public PapyrusMethodDefinition ReadMethod(PapyrusAssemblyDefinition asm)
        {
            var method = new PapyrusMethodDefinition(asm);
            method.ReturnTypeName = pexReader.ReadStringRef();
            method.Documentation = pexReader.ReadStringRef();
            method.UserFlags = pexReader.ReadInt32();
            method.Flags = pexReader.ReadByte();

            var parameterCount = pexReader.ReadInt16();
            for (var i = 0; i < parameterCount; i++)
            {
                method.Parameters.Add(ReadParameter(asm));
            }

            var localVariableCount = pexReader.ReadInt16();
            for (var i = 0; i < localVariableCount; i++)
            {
                method.Body.Variables.Add(ReadVariable(asm));
            }

            var instructionCount = pexReader.ReadInt16();
            for (var i = 0; i < instructionCount; i++)
            {
                var instruction = ReadInstruction(asm);
                instruction.Offset = i;
                method.Body.Instructions.Add(instruction);
            }

            for (var i = 0; i < instructionCount; i++)
            {
                var instruction = method.Body.Instructions[i];
                if (i > 0)
                {
                    var previous = method.Body.Instructions[i - 1];
                    previous.Next = instruction;
                    instruction.Previous = previous;
                }
                if (i < instructionCount - 1)
                {
                    var next = method.Body.Instructions[i + 1];
                    instruction.Next = next;
                    next.Previous = instruction;
                }
            }

            //var last = method.Body.Instructions.LastOrDefault();
            //if (last != null)
            //{
            //    if (last.OpCode != PapyrusOpCodes.Return)
            //    {
            //        var ret = new PapyrusInstruction()
            //        {
            //            OpCode = PapyrusOpCodes.Return,
            //            Previous = last,
            //            Offset = last.Offset + 1,
            //            TemporarilyInstruction = true,
            //            Arguments = new List<PapyrusVariableReference>(),
            //            OperandArguments = new List<PapyrusVariableReference>()
            //        };
            //        last.Next = ret;
            //        if (IsJump(last.OpCode))
            //        {
            //            last.Operand = ret;
            //        }
            //        ret.Operand = null;
            //        method.Body.Instructions.Add(ret);
            //    }
            //}

            return method;
        }

        private bool IsJump(PapyrusOpCodes opCode)
        {
            return opCode == PapyrusOpCodes.Jmp || opCode == PapyrusOpCodes.Jmpf || opCode == PapyrusOpCodes.Jmpt;
        }

        private PapyrusInstruction ReadInstruction(PapyrusAssemblyDefinition asm)
        {
            var instruction = new PapyrusInstruction();

            instruction.OpCode = (PapyrusOpCodes)pexReader.ReadByte();

            var desc = PapyrusInstructionOpCodeDescription.FromOpCode(instruction.OpCode);

            var references = new List<PapyrusVariableReference>();
            var instructionParamSize = desc.ArgumentCount;
            for (var p = 0; p < instructionParamSize; p++)
            {
                references.Add(ReadValueReference(asm));
            }

            if (desc.HasOperandArguments)
            {
                var typeRef = ReadValueReference(asm);
                if (typeRef.Type == PapyrusPrimitiveType.Integer)
                {
                    var argCount = (int)typeRef.Value;
                    for (var i = 0; i < argCount; i++)
                    {
                        instruction.OperandArguments.Add(ReadValueReference(asm));
                    }
                }
            }

            instruction.Arguments = references;
            return instruction;
        }

        public PapyrusVariableReference ReadVariable(PapyrusAssemblyDefinition asm)
        {
            return new PapyrusVariableReference(pexReader.ReadStringRef(), pexReader.ReadStringRef());
        }

        public PapyrusParameterDefinition ReadParameter(PapyrusAssemblyDefinition asm)
        {
            return new PapyrusParameterDefinition
            {
                Name = pexReader.ReadStringRef(),
                TypeName = pexReader.ReadStringRef()
            };
        }

        private void ReadHeader(PapyrusHeader header)
        {
            var majorVersion = pexReader.ReadByte();
            var minorVersion = pexReader.ReadByte();
            var gameId = pexReader.ReadInt16();
            var compileTime = pexReader.ReadInt64();

            //if (true)
            //{
            //    var data =
            //    pexReader.ReadChars(100);
            //}

            var source = pexReader.ReadString();
            var user = pexReader.ReadString();
            var computer = pexReader.ReadString();

            header.SourceHeader = new PapyrusSourceHeader(
                majorVersion, minorVersion,
                gameId, compileTime, source, user, computer
                );
        }

        private PapyrusFieldDefinition ReadDocumentedField(PapyrusAssemblyDefinition asm,
            PapyrusTypeDefinition declaringType)
        {
            var sfd = ReadFieldDefinition(asm, declaringType);
            sfd.Documentation = pexReader.ReadString();
            return sfd;
        }

        private PapyrusFieldDefinition ReadFieldDefinition(PapyrusAssemblyDefinition asm,
            PapyrusTypeDefinition declaringType)
        {
            var fd = new PapyrusFieldDefinition(asm, declaringType);
            // Field Definition

            fd.Name = pexReader.ReadStringRef();
            fd.TypeName = pexReader.ReadString();
            fd.UserFlags = pexReader.ReadInt32();
            {
                // Type Reference
                fd.DefaultValue = ReadValueReference(asm, fd.TypeName);
            }
            fd.Flags = pexReader.ReadByte(); //== 1;
            return fd;
        }

        private PapyrusVariableReference ReadValueReference(PapyrusAssemblyDefinition asm, string name = null)
        {
            var tr = new PapyrusVariableReference(new PapyrusStringRef(asm, name),
                (PapyrusPrimitiveType)pexReader.ReadByte());

            switch (tr.Type)
            {
                case PapyrusPrimitiveType.Reference:
                    tr.Value = pexReader.ReadString();
                    break;
                case PapyrusPrimitiveType.String:
                    tr.Value = pexReader.ReadString();
                    break;
                case PapyrusPrimitiveType.Boolean:
                    tr.Value = pexReader.ReadByte();
                    break;
                case PapyrusPrimitiveType.Float:
                    tr.Value = pexReader.ReadSingle();
                    break;
                case PapyrusPrimitiveType.Integer:
                    tr.Value = pexReader.ReadInt32();
                    break;

                default:
                case PapyrusPrimitiveType.None:
                    break;
            }
            return tr;
        }

        ~PapyrusAssemblyReader()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;
            if (disposing)
            {
                pexReader.Dispose();
            }
            isDisposed = true;
        }
    }
}