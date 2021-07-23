using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Src
{
    public enum CpuCountMode { Cores, Threads };
    public static class CpuInfo
    {

        public static int GetCpuCount(CpuCountMode mode)
        {

            int count = 0;
            try
            {
                if (mode == CpuCountMode.Cores)
                {
                    foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
                    {
                        count += int.Parse(item["NumberOfCores"].ToString());
                    }
                }
                else if (mode == CpuCountMode.Threads)
                {
                    foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem").Get())
                    {
                        count += int.Parse(item["NumberOfLogicalProcessors"].ToString());
                    }
                }
            }
            catch
            {
                return 0;
            }


            return count;
        }
    }
}
