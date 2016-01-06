using System.Collections.Generic;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.PexInspector.ViewModels.Tools
{
    public class FindResult : IFindResult
    {
        public IEnumerable<FindResultData> Results => usageRepresentaitons;
        public bool HasResults => usageRepresentaitons.Count > 0;
        public string SearchText { get; set; }

        private readonly List<FindResultData> usageRepresentaitons;

        public FindResult()
        {
            usageRepresentaitons = new List<FindResultData>();
        }

        public void AddResult(PapyrusTypeDefinition type, PapyrusStateDefinition state, PapyrusMethodDefinition method, PapyrusInstruction instruction, string search, string resultRepresentation)
        {
            SearchText = search;
            usageRepresentaitons.Add(new FindResultData()
            {
                Type = type,
                State = state,
                Method = method,
                Instruction = instruction,
                Text = resultRepresentation,
                SearchText = search
            });
        }
    }
}