namespace PapyrusDotNet.PapyrusAssembly.Parser
{
    public class PapyrusAsmValue
    {
        /// <summary>
        /// Gets or sets the table reference.
        /// </summary>
        public PapyrusStringTableIndex TableReference { get; private set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Resolves the table reference.
        /// </summary>
        /// <returns></returns>
        public PapyrusStringTableIndex ResolveTableReference()
        {
            TableReference = null;

            return TableReference;
        }
    }
}