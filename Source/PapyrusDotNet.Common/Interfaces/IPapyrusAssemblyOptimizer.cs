namespace PapyrusDotNet.Common.Interfaces
{
    public interface IPapyrusAssemblyOptimizer : IUtility
    {
        string OptimizeLabels();
        string RemoveUnusedLabels();
        string RemoveUnnecessaryLabels();
    }
}