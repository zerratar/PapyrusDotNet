namespace PapyrusDotNet.Common.Interfaces
{
    public interface INamespaceResolver
    {
        string Resolve(string typeName = null);
    }
}