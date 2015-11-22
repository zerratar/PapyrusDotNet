#region License

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

#endregion

#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PapyrusDotNet.PapyrusAssembly.Classes;
using PapyrusDotNet.PapyrusAssembly.Enums;
using PapyrusDotNet.PapyrusAssembly.Interfaces;
using PapyrusDotNet.PapyrusAssembly.IO;
using PapyrusDotNet.PapyrusAssembly.Structs;

#endregion

namespace PapyrusDotNet.PapyrusAssembly.Implementations
{
    internal class PapyrusAssemblyReader : IPapyrusAssemblyReader, IDisposable
    {
        private readonly PexReader pexReader;

        private bool isDisposed;

        public bool IsCorrupted => pexReader.IsCorrupted;

        public PapyrusAssemblyReader(string pex)
        {
            pexReader = new PexReader(pex);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public PapyrusAssemblyDefinition Read()
        {
            var asm = new PapyrusAssemblyDefinition();
            asm.Header = ReadHeader(asm);

            if (asm.Header.HasDescriptionTable)
            {
                asm.DescriptionTable = ReadDescriptionTable(asm);
            }

            if (asm.Header.VersionTarget == PapyrusVersionTargets.Skyrim)
            {
                throw new NotImplementedException("Reading and Writing Skyrim Papyrus (v3.2) has not yet been implemented. Please use an older version of PapyrusDotNet for this.");
            }

            ReadHeaderUserflags(asm);

            asm.Types = ReadTypeDefinitions(asm);

            return asm;
        }

        private void ReadHeaderUserflags(PapyrusAssemblyDefinition asm)
        {
            var userflagCount = pexReader.ReadInt16();// asm.Header.VersionTarget == PapyrusVersionTargets.Fallout4
                                                      //? pexReader.ReadInt16()
                                                      //: pexReader.ReadByte();

            for (var userflagIndex = 0; userflagIndex < userflagCount; userflagIndex++)
            {
                var stringValue = pexReader.ReadString();
                var flagValue = pexReader.ReadByte();
                asm.Header.UserflagReferenceHeader.Add(stringValue, flagValue);
            }
        }

        public PapyrusHeader ReadHeader(PapyrusAssemblyDefinition asm)
        {
            var header = new PapyrusHeader();
            header.HeaderIdentifier = pexReader.ReadUInt32();
            switch (header.HeaderIdentifier)
            {
                case PapyrusHeader.Fallout4PapyrusHeaderIdentifier:
                    header.VersionTarget = PapyrusVersionTargets.Fallout4;
                    ReadFallout4Header(header);
                    break;
                default:
                    header.VersionTarget = PapyrusVersionTargets.Skyrim;
                    ReadSkyrimHeader(header);
                    break;
            }

            ReadStringTable();

            if (header.VersionTarget == PapyrusVersionTargets.Fallout4)
                header.HasDescriptionTable = pexReader.ReadByte() == 1;

            return header;
        }

        private void ReadStringTable()
        {
            var stringTableLength = pexReader.ReadInt16();
            var stringTable = new List<string>();
            for (var i = 0; i < stringTableLength; i++)
            {
                stringTable.Add(pexReader.ReadString());
            }
            pexReader.SetStringTable(stringTable);
        }

        public PapyrusTypeDescriptionTable ReadDescriptionTable(PapyrusAssemblyDefinition asm)
        {
            var descriptionTable = new PapyrusTypeDescriptionTable();

            var debugTime = pexReader.ReadInt64();
            descriptionTable.DescriptionTime = debugTime;
            var functionCount = pexReader.ReadInt16();

            var methodDescriptions = new List<PapyrusMethodDecription>();
            for (var i = 0; i < functionCount; i++)
            {
                var dbgfunc = new PapyrusMethodDecription();
                dbgfunc.DeclaringTypeName = pexReader.ReadString();
                dbgfunc.StateName = pexReader.ReadString();
                dbgfunc.Name = pexReader.ReadString();
                dbgfunc.MethodType = (PapyrusMethodTypes)pexReader.ReadByte();
                dbgfunc.BodyLineNumbers = new List<int>();
                var lineNumberCount = pexReader.ReadInt16();
                for (var j = 0; j < lineNumberCount; j++)
                {
                    dbgfunc.BodyLineNumbers.Add(pexReader.ReadInt16());
                }
                methodDescriptions.Add(dbgfunc);
            }

            descriptionTable.MethodDescriptions = methodDescriptions;

            var propertyGroupCount = pexReader.ReadInt16();
            var propertyDescriptions = new List<PapyrusPropertyDescriptions>();
            for (var i = 0; i < propertyGroupCount; i++)
            {
                var groupInfo = new PapyrusPropertyDescriptions();
                groupInfo.ObjectName = pexReader.ReadString();
                groupInfo.GroupName = pexReader.ReadString();
                groupInfo.GroupDocumentation = pexReader.ReadString();
                groupInfo.Userflags = pexReader.ReadInt32();
                var propertyNameCount = pexReader.ReadInt16();
                for (var j = 0; j < propertyNameCount; j++)
                {
                    groupInfo.PropertyNames.Add(pexReader.ReadString());
                }
                propertyDescriptions.Add(groupInfo);
            }
            descriptionTable.PropertyDescriptions = propertyDescriptions;

            var structureOrderCount = pexReader.ReadInt16();

            var structDescriptions = new List<PapyrusStructDescription>();
            for (var i = 0; i < structureOrderCount; i++)
            {
                var structDescription = new PapyrusStructDescription();
                structDescription.ObjectName = pexReader.ReadString();
                structDescription.OrderName = pexReader.ReadString();
                var fieldCount = pexReader.ReadInt16();
                for (var j = 0; j < fieldCount; j++)
                {

                    structDescription.FieldNames.Add(pexReader.ReadString());
                }
                structDescriptions.Add(structDescription);
            }
            descriptionTable.StructDescriptions = structDescriptions;

            return descriptionTable;
        }

        public Collection<PapyrusTypeDefinition> ReadTypeDefinitions(PapyrusAssemblyDefinition asm)
        {
            var types = new Collection<PapyrusTypeDefinition>();

            var classCount = pexReader.ReadInt16();
            for (var j = 0; j < classCount; j++)
            {
                var typeDef = new PapyrusTypeDefinition();
                typeDef.IsClass = true;
                typeDef.Name = pexReader.ReadString();
                typeDef.Size = pexReader.ReadInt32();
                typeDef.BaseClass = pexReader.ReadString();
                typeDef.ConstFlag = pexReader.ReadByte();
                typeDef.Documentation = pexReader.ReadString();
                typeDef.UserFlags = pexReader.ReadInt32();
                typeDef.AutoStateName = pexReader.ReadString();

                try
                {

                    if (asm.Header.VersionTarget == PapyrusVersionTargets.Fallout4)
                    {
                        var structCount = pexReader.ReadInt16();
                        for (var i = 0; i < structCount; i++)
                        {
                            var structDef = new PapyrusTypeDefinition();
                            structDef.IsStruct = true;
                            structDef.Name = pexReader.ReadString();

                            var variableCount = pexReader.ReadInt16();
                            for (var l = 0; l < variableCount; l++)
                            {
                                structDef.Fields.Add(ReadDocumentedField());
                            }
                            typeDef.NestedTypes.Add(structDef);
                        }
                    }

                    var fieldCount = pexReader.ReadInt16();
                    for (var i = 0; i < fieldCount; i++)
                    {
                        typeDef.Fields.Add(ReadFieldDefinition());
                    }
                    var propDefs = new Collection<PapyrusPropertyDefinition>();
                    var propertyCount = pexReader.ReadInt16();
                    for (var i = 0; i < propertyCount; i++)
                    {
                        var prop = new PapyrusPropertyDefinition();
                        prop.Name = pexReader.ReadString();
                        prop.TypeName = pexReader.ReadString();
                        prop.Documentation = pexReader.ReadString();
                        prop.Userflags = pexReader.ReadInt32();
                        prop.Flags = pexReader.ReadByte();
                        if (prop.IsAuto)
                        {
                            // if isAuto then the getter and setter is not defined
                            prop.AutoName = pexReader.ReadString();
                        }
                        else
                        {
                            if (prop.HasGetter)
                            {
                                prop.GetMethod = ReadMethod(asm);
                            }
                            if (prop.HasSetter)
                            {
                                prop.SetMethod = ReadMethod(asm);
                            }
                        }
                        propDefs.Add(prop);
                    }
                    typeDef.Properties = propDefs;

                    var stateCount = pexReader.ReadInt16();

                    for (var i = 0; i < stateCount; i++)
                    {
                        var state = new PapyrusStateDefinition();
                        state.Name = pexReader.ReadString();
                        var methodCount = pexReader.ReadInt16();
                        for (var k = 0; k < methodCount; k++)
                        {
                            var name = pexReader.ReadString();
                            var method = ReadMethod(asm);
                            method.Name = name;
                            state.Methods.Add(method);
                        }
                        typeDef.States.Add(state);
                    }
                }
                catch
                {
                    pexReader.IsCorrupted = true;
                }

                types.Add(typeDef);
            }
            return types;
        }

        public PapyrusMethodDefinition ReadMethod(PapyrusAssemblyDefinition asm)
        {
            var method = new PapyrusMethodDefinition();
            method.ReturnTypeName = pexReader.ReadString();
            method.Documentation = pexReader.ReadString();
            method.UserFlags = pexReader.ReadInt32();
            method.Flags = pexReader.ReadByte();

            var parameterCount = pexReader.ReadInt16();
            for (var i = 0; i < parameterCount; i++)
            {
                method.Parameters.Add(ReadParameter());
            }

            var localVariableCount = pexReader.ReadInt16();
            for (var i = 0; i < localVariableCount; i++)
            {
                method.Body.Variables.Add(ReadVariable());
            }

            var instructionCount = pexReader.ReadInt16();
            for (var i = 0; i < instructionCount; i++)
            {
                method.Body.Instructions.Add(ReadInstruction());
            }
            return method;
        }

        private PapyrusInstruction ReadInstruction()
        {
            var instruction = new PapyrusInstruction();

            instruction.OpCode = (PapyrusInstructionOpCodes)pexReader.ReadByte();

            var desc = PapyrusInstructionOpCodeDescription.FromOpCode(instruction.OpCode);

            var references = new List<PapyrusTypeReference>();
            var instructionParamSize = desc.ParamSize;
            for (var p = 0; p < instructionParamSize; p++)
            {
                references.Add(ReadTypeReference());
            }

            if (desc.HasVariableArguments)
            {
                var typeRef = ReadTypeReference(); // 
                if (typeRef.ValueType == PapyrusPrimitiveType.Integer)
                {
                    var argCount = (int)typeRef.Value;
                    for (var i = 0; i < argCount; i++)
                    {
                        instruction.VariableArguments.Add(ReadTypeReference());
                    }
                }
            }

            instruction.Operand = references;
            return instruction;
        }

        public PapyrusVariableDefinition ReadVariable()
        {
            return new PapyrusVariableDefinition
            {
                Name = pexReader.ReadString(),
                TypeName = pexReader.ReadString()
            };
        }

        public PapyrusParameterDefinition ReadParameter()
        {
            return new PapyrusParameterDefinition
            {
                Name = pexReader.ReadString(),
                TypeName = pexReader.ReadString()
            };
        }

        private void ReadFallout4Header(PapyrusHeader header)
        {
            pexReader.SetVersionTarget(PapyrusVersionTargets.Fallout4);

            var majorVersion = pexReader.ReadByte();
            var minorVersion = pexReader.ReadByte();
            var gameId = pexReader.ReadInt16();
            var compileTime = pexReader.ReadInt64();
            var source = pexReader.ReadString();
            var user = pexReader.ReadString();
            var computer = pexReader.ReadString();

            header.SourceHeader = new PapyrusSourceHeader(
                majorVersion, minorVersion,
                gameId, compileTime, source, user, computer
            );
        }


        private void ReadSkyrimHeader(PapyrusHeader header)
        {
            pexReader.SetVersionTarget(PapyrusVersionTargets.Skyrim);

            var majorVersion = pexReader.ReadByte();
            var minorVersion = pexReader.ReadByte();
            var gameId = pexReader.ReadInt16();
            var compileTime = pexReader.ReadInt64();

            if (pexReader.ReadByte() != 0) throw new Exception("Invalid Header Format");

            var computer = pexReader.ReadString();
            var source = pexReader.ReadString();
            var user = pexReader.ReadString();

            header.SourceHeader = new PapyrusSourceHeader(
                majorVersion, minorVersion,
                gameId, compileTime, source, user, computer
            );
        }

        private PapyrusFieldDefinition ReadDocumentedField()
        {
            var sfd = ReadFieldDefinition();
            sfd.Documentation = pexReader.ReadString();
            return sfd;
        }

        private PapyrusFieldDefinition ReadFieldDefinition()
        {
            var fd = new PapyrusFieldDefinition();
            // Field Definition

            fd.Name = pexReader.ReadString();
            fd.TypeName = pexReader.ReadString();
            fd.UserFlags = pexReader.ReadInt32();
            {
                // Type Reference
                fd.FieldType = ReadTypeReference(fd.TypeName);
            }
            fd.IsConst = pexReader.ReadByte() == 1;
            return fd;
        }

        private PapyrusTypeReference ReadTypeReference(string name = null)
        {
            var tr = new PapyrusTypeReference();
            tr.Name = name;
            tr.ValueType = (PapyrusPrimitiveType)pexReader.ReadByte();
            switch (tr.ValueType)
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