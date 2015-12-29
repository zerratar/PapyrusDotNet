using System;
using System.Runtime.InteropServices;

namespace PapyrusDotNet.PexInspector.Implementations
{
    internal class User32
    {
        public const int GWL_STYLE = -16;
        public const int WS_SYSMENU = 0x80000;

        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}