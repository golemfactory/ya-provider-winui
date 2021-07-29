using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Extensions
{
    // Extension methods must be defined in a static class.
    public static class ProcessExtension
    {
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
    }
}
