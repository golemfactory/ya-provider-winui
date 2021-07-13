using GolemUI.Interfaces;
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
        private readonly IPaymentService _paymentService;
        public ViewModel.WalletViewModel Model => (DataContext as ViewModel.WalletViewModel)!;
        public DashboardWallet(ViewModel.WalletViewModel model, IPaymentService paymentService)
        {
            _paymentService = paymentService;
            InitializeComponent();
            DataContext = model;
        }

        private void BtnOpenZkSync_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://wallet.zksync.io/");
        }

        private void BtnWithdraw_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void BtnEditWalletAddress_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new UI.DlgEditAddress(Model.EditModel);
            dlg.Owner = Window.GetWindow(this);
            if (dlg.ShowDialog() ?? false)
            {
                
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Model.WalletAddress);
        }
    }
}
