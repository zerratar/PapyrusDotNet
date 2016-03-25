using System.Collections.ObjectModel;

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public interface IPexTreeBuilder
    {
        ObservableCollection<PapyrusViewModel> BuildPexTree(ObservableCollection<PapyrusViewModel> pexTree, PapyrusViewModel target = null);
        bool BuildPexTree(int assemblyIndex, string[] asmnames, out PapyrusViewModel root);
    }
}