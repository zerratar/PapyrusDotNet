namespace PapyrusDotNet.Common.Interfaces
{
    public interface IAssemblyConverter
    {
        IAssemblyOutput Convert(IAssemblyInput input);
    }
}