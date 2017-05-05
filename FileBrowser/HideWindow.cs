using System;
using System.Runtime.InteropServices;

namespace FileBrowser {
    internal static class ShowHideWindow {

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        public static bool Show(bool show) {
            return ShowWindow(GetConsoleWindow(), show ? SW_SHOW : SW_HIDE);
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    }
}