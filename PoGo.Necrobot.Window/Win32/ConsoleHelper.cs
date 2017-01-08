using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.Necrobot.Window.Win32
{
    class ConsoleHelper
    {
        [DllImport("Kernel32.dll")]
        public static extern bool AttachConsole(int processId);
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// Allocates a new console for current process.
        /// </summary>
        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();

        /// <summary>
        /// Frees the console.
        /// </summary>
        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();


        public static void HideConsoleWindow()
        {
            IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;

            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, 0); // 0 = SW_HIDE
            }
        }


    }
}
