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

#endregion

namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusVariableReference : PapyrusMemberReference
    {
        private readonly PapyrusStringTableIndex internal_stringTableIndex;
        private object value;

        public PapyrusVariableReference()
        {
        }

        public PapyrusVariableReference(PapyrusStringTableIndex value, bool isReference)
        {
            internal_stringTableIndex = value;
            Type = isReference ? PapyrusPrimitiveType.Reference : PapyrusPrimitiveType.String;
            Name = new PapyrusStringRef(value);
        }

        public PapyrusVariableReference(PapyrusStringRef name, PapyrusPrimitiveType type)
        {
            Type = type;
            Name = name;
        }

        public PapyrusVariableReference(PapyrusStringRef name, PapyrusStringRef variableTypeName)
        {
            Name = name;
            TypeName = variableTypeName;
        }

        public PapyrusStringRef Name { get; set; }
        public PapyrusStringRef TypeName { get; set; }

        public object Value
        {
            get { return value; }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    if (Name != null && string.IsNullOrEmpty(Name?.Value) && value != null &&
                        Type == PapyrusPrimitiveType.Reference)
                    {
                        Name.Value = value.ToString();
                    }
                }
            }
        }

        public PapyrusPrimitiveType Type { get; set; }
        public bool IsDelegateReference { get; set; }
        public string DelegateInvokeReference { get; set; }

        public string GetStringRepresentation()
        {
            switch (Type)
            {
                case PapyrusPrimitiveType.None:
                    return "NONE";
                case PapyrusPrimitiveType.Reference:
                    return Value?.ToString() ?? Name?.Value;
                case PapyrusPrimitiveType.String:
                {
                    if (!Value.ToString().StartsWith("\""))
                    {
                        return "\"" + Value + "\"";
                    }
                    return Value.ToString();
                }
                case PapyrusPrimitiveType.Boolean:
                {
                    if (Value != null)
                    {
                        return Value.Equals(1) ? "true" : "false";
                    }
                }
                    break;
                case PapyrusPrimitiveType.Integer:
                    if (Value != null)
                    {
                        return ((int) Value).ToString();
                    }
                    break;
                case PapyrusPrimitiveType.Float:
                    if (Value != null)
                    {
                        return ((float) Value).ToString().Replace(",", ".") + "f";
                    }
                    break;
            }

            if (Name != null)
            {
                return (string) Name;
            }
            return null;
        }

        public override string ToString()
        {
            var name = Value + "";
            var type = Type.ToString();

            if (TypeName != null && !string.IsNullOrEmpty(TypeName.Value))
            {
                type = TypeName.Value;
            }

            if (Name != null && !string.IsNullOrEmpty(Name.Value))
            {
                name = Name.Value;
            }

            return name + " : " + type;
        }

        public PapyrusStringTableIndex AsStringTableIndex()
        {
            if (internal_stringTableIndex != null) return internal_stringTableIndex;

            if (Name != null && string.IsNullOrEmpty(Name.Value) && Value != null)
            {
                if (Type == PapyrusPrimitiveType.Reference)
                {
                    return Name.GetStringTable().Add(Value.ToString());
                }
            }
            return Name.AsTableIndex();
        }
    }
}