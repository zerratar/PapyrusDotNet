using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public interface IMemberDisplayBuilder
    {
        ObservableCollection<Inline> BuildMemberDisplay(object item);
        List<Run> GetParameterRuns(List<PapyrusParameterDefinition> parameters);
    }
}