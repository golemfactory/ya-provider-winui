using GolemUI.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GolemUI
{
    /// <summary>
    /// Interaction logic for DashboardSettings.xaml
    /// </summary>
    public partial class DashboardWallet : UserControl
    {
        public DashboardWallet(ViewModel.WalletViewModel model)
        {
            InitializeComponent();
            GlobalApplicationState.Instance.ApplicationStateChanged += OnGlobalApplicationStateChanged;
            this.DataContext = model;           
        }

        public void OnGlobalApplicationStateChanged(object sender, GlobalApplicationStateEventArgs? args)
        {
            if (args != null)
            {
                switch(args.action)
                {
                    case GlobalApplicationStateAction.reloadSettings:
                        break;
                    case GlobalApplicationStateAction.yagnaAppStarting:
                        break;
                    case GlobalApplicationStateAction.yagnaAppStopped:
                        break;
                }
            }
        }

        private void BtnOpenZkSync_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://wallet.zksync.io/");
        }
    }
}
