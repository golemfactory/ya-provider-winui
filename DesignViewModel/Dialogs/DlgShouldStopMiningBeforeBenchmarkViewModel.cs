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
    public class DlgShouldStopMiningBeforeBenchmarkViewModel
    {
        public bool ShouldAutoRestartMining => true;
        public bool RememberMyPreference => true;
    }
}
