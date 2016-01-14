namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusMergerSettings
    {
        /// <summary>
        /// Gets the default merger settings.
        /// </summary>
        public static PapyrusMergerSettings Default => new PapyrusMergerSettings();
        
        /// <summary>
        /// Gets or sets a value indicating whether conflicting methods should be overwritten or not.
        /// </summary>
        public bool OverwriteConflictingMethods { get; set; } = false;
    }
}