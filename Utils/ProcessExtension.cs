using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Utils
{
    public static class ProcessExtension
    {
        const int CTRL_C_EVENT = 0;
        [DllImport("kernel32.dll")]
        private static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate? HandlerRoutine, bool Add);

        private delegate Boolean ConsoleCtrlDelegate(uint CtrlType);

        public static bool StopWithCtrlC(this Process process, int miliseconds)
        {
            if (AttachConsole((uint)process.Id))
            {
                SetConsoleCtrlHandler(null, true);
                try
                {
                    if (!GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0))
                    {
                        return false;
                    }
                    return process.WaitForExit(miliseconds);
                }
                finally
                {
                    SetConsoleCtrlHandler(null, false);
                }
            }
            return false;
        }

        public static async Task<bool> StopWithCtrlCAsync(this Process process, int miliseconds)
        {
            return await Task.Run(() => process.StopWithCtrlC(miliseconds));
        }
    }
}
