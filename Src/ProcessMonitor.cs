﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Management;

namespace BetaMiner
{
    public class ProcessMonitor
    {
        public ProcessMonitor()
        {

        }

        public static void GetProcessList(out Process[] yagnaProcesses, out Process[] providerProcesses, out Process[] claymoreProcesses)
        {
            yagnaProcesses = Process.GetProcessesByName("yagna");
            providerProcesses = Process.GetProcessesByName("ya-provider");
            claymoreProcesses = Process.GetProcessesByName("EthDcrMiner64");
        }



    }
}
