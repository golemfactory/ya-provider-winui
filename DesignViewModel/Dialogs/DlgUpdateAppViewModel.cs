using GolemUI.Interfaces;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GolemUI.DesignViewModel.Dialogs
{
    public class DlgUpdateAppViewModel
    {
        public string UpdateLink => "http://google.com";
        public string CurrentVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        string NewVersion => "2.0.0";

        string AppCodeName => "CandyFlip";
        public string ChangeLog
        {
            get
            {
                if (Changes == null || Changes.Count == 0)
                    return "";
                return "New changes:\n" + String.Join("\n", Changes.Take(5).Select(x => "- " + x).ToArray());
            }
        }
        List<string> Changes = new List<string>() { "change1", "change2", "change3", "change4*" };
        public bool ShouldForceUpdate => false;

        public string NewVersionDisplayString
        {
            get
            {
                if (String.IsNullOrEmpty(AppCodeName))
                    return NewVersion;
                return $"{NewVersion} [{AppCodeName}]";
            }
        }
    }
}
