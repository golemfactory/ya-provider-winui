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
        public void SetStartWithSystemEnabled(bool enable)
        {
            if (enable)
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                object? keyValue = rk.GetValue("TestBetaMiner");
                string targetVal = System.Reflection.Assembly.GetEntryAssembly().Location;
                if (keyValue != null && keyValue is string && keyValue.ToString() == targetVal)
                {
                    //already set
                }
                else
                {
                    rk.SetValue("TestBetaMiner", targetVal);
                }
            }
            if (!enable)
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                rk.DeleteValue("TestBetaMiner", false);
            }


        }

    }
}
