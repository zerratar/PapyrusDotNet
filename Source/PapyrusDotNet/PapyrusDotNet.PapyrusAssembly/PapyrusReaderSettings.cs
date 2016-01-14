namespace PapyrusDotNet.PapyrusAssembly
{
    /// <summary>
    /// A class that represents the settings used for the Papyrus Assembly Reader
    /// </summary>
    public class PapyrusReaderSettings
    {
        /// <summary>
        /// Gets the default settings.
        /// </summary>
        public static PapyrusReaderSettings Default => new PapyrusReaderSettings();

        /// <summary>
        /// Gets or sets a value indicating whether method bodies (instructions) should be read or not.
        /// Default is True, this is only used for cases when just the structual data wants to be read.
        /// </summary>
        public bool ReadMethodBodies { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether exceptions should be thrown or not.
        /// </summary>
        public bool ThrowsException { get; set; } = false;
    }
}