using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Implementations
{
    public class PapyrusStructFieldReference : PapyrusFieldDefinition
    {
        public object StructSource { get; set; }
        public PapyrusVariableReference StructVariable { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PapyrusStructFieldReference"/> class.
        /// </summary>
        /// <param name="declaringAssembly">The assembly.</param>
        /// <param name="declaringType">(OPTIONAL) The declaring typedefinition. You can pass NULL</param>
        public PapyrusStructFieldReference(PapyrusAssemblyDefinition declaringAssembly, PapyrusTypeDefinition declaringType) : base(declaringAssembly, declaringType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PapyrusStructFieldReference"/> class.
        /// </summary>
        /// <param name="declaringAssembly">The assembly.</param>
        /// <param name="declaringType">(OPTIONAL) The declaring typedefinition. You can pass NULL</param>
        /// <param name="name">The name.</param>
        /// <param name="typeName">Name of the type.</param>
        public PapyrusStructFieldReference(PapyrusAssemblyDefinition declaringAssembly, PapyrusTypeDefinition declaringType, string name, string typeName) : base(declaringAssembly, declaringType, name, typeName)
        {
        }

        public override string ToString()
        {
            return "StructRef: " + StructSource + " -> " + StructVariable;
        }
    }
}