namespace PapyrusDotNet.Common
{
    public class PapyrusVariableReference
    {
        public PapyrusVariableReference(string name, string type)
        {
            // TODO: Complete member initialization
            this.Name = name;
            this.TypeName = type;
        }
        public PapyrusVariableReference(string name, string type, string definition)
        {
            // TODO: Complete member initialization
            this.Name = name;
            this.TypeName = type;
            this.Definition = definition;
        }

        public string Definition { get; set; }

        public object Value { get; set; }

        public string Name { get; set; }

        public string TypeName { get; set; }

        public string AutoVarName { get; set; }

        public PapyrusFieldProperties Properties { get; set; }
    }
}