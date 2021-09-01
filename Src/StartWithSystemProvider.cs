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
    class StartWithSystemProvider : IStartWithWindows
    {
        public bool IsStartWithSystemEnabled()
        {
/*            
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("C:\\Program Files\\TestBetaMiner\\BetaMiner.exe", true);

            object? keyValue = rk.GetValue("TestBetaMiner");
            if (keyValue != null && keyValue is string)
            {
                
                if (keyValue == "")
                {

                }
                

            }*/
            return false;

        }
        public void SetStartWithSystemEnabled(bool enable)
        {
        
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (enable)
                rk.SetValue("TestBetaMiner", @"C:\Program Files\TestBetaMiner\BetaMiner.exe");
            else
                rk.DeleteValue("TestBetaMiner", false);
        }

    }
}
