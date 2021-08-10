using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
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

        /// <summary>
        /// Kill a process, and all of its children, grandchildren, etc.
        /// </summary>
        /// <param name="pid">Process ID.</param>
        private static void KillProcessAndChildren(int pid)
        {
            // Cannot close 'system idle process'.
            if (pid == 0)
            {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
                    ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            Process proc = Process.GetProcessById(pid);
            proc.Kill();
        }


        // This is the extension method.
        // The first parameter takes the "this" modifier
        // and specifies the type for which the method is defined.
        public static void Kill(this Process process, bool entireProcessTree)
        {
            if (entireProcessTree)
            {
                KillProcessAndChildren(process.Id);
            }
            else if (!entireProcessTree)
            {
                process.Kill();
            }
        }

        public static IDisposable WithJob(this Process process, string? name = null)
        {
            var cpm = new ChildProcessManager(name);
            cpm.AddProcess(process);
            return cpm;
        }
    }
}
