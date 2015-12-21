using PapyrusDotNet.Common.Utilities;

namespace PapyrusDotNet.Common.Interfaces
{
    public interface IUiRenderer : IUtility
    {
        void EnsureConsoleCursorPosition();
        void ReleaseConsoleCursorPosition();
        void DrawInterface(string message);
        void DrawResult(string message);
        void DrawHotkeys(params Hotkeys[] hotkeys);
        void DrawProgressBarWithInfo(int value, int max);
        void DrawProgressBar(int value, int maxValue);
        void DrawHelp();
    }
}