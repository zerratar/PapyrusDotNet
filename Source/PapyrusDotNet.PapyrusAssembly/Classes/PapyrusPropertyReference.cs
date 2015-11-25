using PapyrusDotNet.PapyrusAssembly.Enums;

namespace PapyrusDotNet.PapyrusAssembly.Classes
{
    public class PapyrusPropertyReference
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public PapyrusPrimitiveType ValueType { get; set; }

        public PapyrusPropertyReference()
        {
        }

        public PapyrusPropertyReference(string name, object value, PapyrusPrimitiveType valueType)
        {
            Name = name;
            Value = value;
            ValueType = valueType;
        }
    }
}