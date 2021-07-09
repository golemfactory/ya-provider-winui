using GolemUI.Settings;
using System;
using System.Collections.Generic;
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

namespace GolemUI
{
    /// <summary>
    /// Interaction logic for DashboardSettings.xaml
    /// </summary>
    public partial class DashboardWallet : UserControl
    {


        public DashboardWallet(Interfaces.IPriceProvider priceProv)
        {
            InitializeComponent();

            GlobalApplicationState.Instance.ApplicationStateChanged += OnGlobalApplicationStateChanged;
            this.DataContext = new ViewModel.WalletViewModel(priceProv);
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


    }
}
