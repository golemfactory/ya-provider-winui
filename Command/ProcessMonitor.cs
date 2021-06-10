using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Management;
using System.Diagnostics;

namespace GolemUI
{
    public class ProcessMonitor
    {
        public ProcessMonitor()
        {

        }

        public static void GetProcessList(out Process[] yagnaProcesses, out Process[] providerProcesses)
        {
            yagnaProcesses = Process.GetProcessesByName("yagna");
            providerProcesses = Process.GetProcessesByName("ya-provider");
        }



    }
}
