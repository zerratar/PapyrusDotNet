using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Extensions;

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class PexLoader : IPexLoader
    {
        private readonly List<PapyrusAssemblyDefinition> loadedAssemblies = new List<PapyrusAssemblyDefinition>();
        private readonly List<string> loadedAssemblyFolders = new List<string>();
        private readonly Dictionary<string, string> loadedAssemblyNames = new Dictionary<string, string>();
        private Dictionary<string, string> discoveredScriptNames;
        private List<string> discoveredScripts;

        public void LoadPex(string fileName)
        {
            var name = Path.GetFileName(fileName);
            var directoryName = Path.GetDirectoryName(fileName);

            if (name != null && loadedAssemblyNames.ContainsKey(name.ToLower()))
            {
                if (MessageBox.Show("This file has already been loaded.\r\nDo you want to reload it?", "Reload?",
                    MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            var loadedAssembly = PapyrusAssemblyDefinition.ReadAssembly(fileName);
            var loadIndex = -1;
            if (name != null && loadedAssemblyNames.ContainsKey(name.ToLower()))
            {
                loadIndex = Array.IndexOf(loadedAssemblyNames.Values.ToArray(), name.ToLower());
            }

            if (loadIndex == -1)
            {
                loadedAssemblies.Add(loadedAssembly);

                if (name != null) loadedAssemblyNames.Add(name.ToLower(), name.ToLower());
            }
            else
            {
                loadedAssemblies[loadIndex] = loadedAssembly;
            }

            if (!loadedAssemblyFolders.Contains(directoryName))
                loadedAssemblyFolders.Add(directoryName);

            //BuildPexTree(ref PexTree);

            //RaiseCommandsCanExecute();
        }

        public bool EnsureAssemblyLoaded(string value)
        {
            if (value == null) return false;

            var lower = value.ToLower();
            lower = lower.Replace("[]", "");
            // do not try and load any value types
            if (lower == "int" || lower == "string" || lower == "bool" || lower == "none" || lower == "float")
                return false;

            if (discoveredScriptNames == null)
                discoveredScriptNames =
                    new Dictionary<string, string>();

            if (!loadedAssemblyNames.ContainsKey(lower + ".pex"))
            {
                if (discoveredScripts == null)
                {
                    discoveredScripts = loadedAssemblyFolders.SelectMany(
                        i => Directory.GetFiles(i, "*.pex", SearchOption.AllDirectories)).ToList();

                    var items = discoveredScripts.Select(
                        i => new { Name = Path.GetFileNameWithoutExtension(i)?.ToLower(), FullPath = i });

                    items.ForEach(
                        i =>
                        {
                            if (!discoveredScriptNames.ContainsKey(i.Name)) discoveredScriptNames.Add(i.Name, i.FullPath);
                        });
                }
                if (discoveredScriptNames.ContainsKey(lower))
                {
                    var targetScriptFile = discoveredScriptNames[lower];
                    if (targetScriptFile != null)
                    {
                        // Load the script and enforce to reload the tree.
                        LoadPex(targetScriptFile);
                        return true;
                    }
                }
            }
            return false;
        }

        public IReadOnlyDictionary<string, string> GetLoadedAssemblyNames()
        {
            return loadedAssemblyNames;
        }

        public IReadOnlyCollection<PapyrusAssemblyDefinition> GetLoadedAssemblies()
        {
            return loadedAssemblies;
        }

        public IReadOnlyCollection<string> GetLoadedFolders()
        {
            return loadedAssemblyFolders;
        }
    }
}