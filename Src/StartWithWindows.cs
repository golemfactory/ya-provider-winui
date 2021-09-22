using GolemUI.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GolemUI.Src
{
    class StartWithWindows : IStartWithWindows
    {
        public void SetStartWithWindows(bool startWithWindows)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (startWithWindows)
            {
                object? keyValue = rk.GetValue("ThorgMiner");
                string targetVal = System.Reflection.Assembly.GetEntryAssembly().Location;
                if (keyValue?.ToString() == targetVal)
                {
                    //already set
                }
                else
                {
                    rk.SetValue("ThorgMiner", targetVal);
                }
            }
            else
            {
                rk.DeleteValue("ThorgMiner", false);
            }
            rk.Close();
        }

    }
}
