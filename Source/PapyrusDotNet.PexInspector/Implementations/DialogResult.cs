using PapyrusDotNet.PexInspector.ViewModels.Interfaces;

namespace PapyrusDotNet.PexInspector.Implementations
{
    public class DialogResult : IDialogResult
    {
        public object Result { get; set; }
        public DialogResult(object resultValue)
        {
            Result = resultValue;
        }
    }
}