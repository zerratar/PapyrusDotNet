using System.Collections.Generic;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public interface IPexLoader
    {
        void LoadPex(string targetScriptFile);
        IReadOnlyDictionary<string, string> GetLoadedAssemblyNames();
        IReadOnlyCollection<PapyrusAssemblyDefinition> GetLoadedAssemblies();
        IReadOnlyCollection<string> GetLoadedFolders();
        bool EnsureAssemblyLoaded(string value);
    }
}