using System.Collections.Generic;

namespace PapyrusDotNet.PexInspector.ViewModels.Tools
{
    public interface IFindResult
    {
        IEnumerable<FindResultData> Results { get; }
        bool HasResults { get; }
        string SearchText { get; set; }
    }
}