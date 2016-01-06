using System.Collections.Generic;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.PexInspector.ViewModels.Tools
{
    public class PapyrusReferenceFinder : IPapyrusReferenceFinder
    {
        private readonly IList<PapyrusAssemblyDefinition> haystack;

        /// <summary>
        /// Initializes a new instance of the <see cref="PapyrusReferenceFinder"/> class.
        /// </summary>
        /// <param name="haystack">The haystack.</param>
        public PapyrusReferenceFinder(IList<PapyrusAssemblyDefinition> haystack)
        {
            this.haystack = haystack;
        }

        /// <summary>
        /// Finds the type references.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        public IFindResult FindTypeReference(string typeName)
        {
            var result = new FindResult();
            result.SearchText = typeName;
            foreach (var asm in haystack)
            {
                foreach (var t in asm.Types)
                {
                    if (t.BaseTypeName != null
                        && !string.IsNullOrEmpty(t.BaseTypeName.Value)
                        && t.BaseTypeName.Value.ToLower() == typeName.ToLower())
                    {
                        // result.AddResult();
                        result.AddResult(t, null, null, null, typeName, null);
                    }
                }
            }
            return result;
        }
    }
}