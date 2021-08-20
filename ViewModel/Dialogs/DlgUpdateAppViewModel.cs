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

namespace GolemUI.ViewModel.Dialogs
{
    public class DlgUpdateAppViewModel
    {
        public string UpdateLink => "http://google.com";
        public string CurrentVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public String NewVersion => "2.0.0";
        public String AppName => "Miner App";
        public String AppCodeName => "[CandyFlip]";
        public String ChangeLog => "-change 1 \nchange 2\n change 3\n change 4";
        public bool ForceUpdate => true;
    }
}
