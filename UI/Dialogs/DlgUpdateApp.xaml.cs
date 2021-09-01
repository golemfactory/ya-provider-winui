
using GolemUI.ViewModel;
using GolemUI.ViewModel.Dialogs;
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
using System.Windows.Shapes;

namespace GolemUI.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for DlgEditAddress.xaml
    /// </summary>
    public partial class DlgUpdateApp : Window
    {

        public DlgUpdateApp(DlgUpdateAppViewModel model)
        {
            InitializeComponent();
            DataContext = model;
        }

        public DlgUpdateAppViewModel Model
        {
            get
            {
                DlgUpdateAppViewModel? result = DataContext as DlgUpdateAppViewModel;
                if (result == null)
                {
                    throw new Exception("DataContext as DlgUpdateAppViewModel cast failed");
                }
                return result;
            }
        }



        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {

            this.DialogResult = false;
            this.Close();
        }



        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            if (Model.ShouldForceUpdate)
                System.Windows.Application.Current.Shutdown();
            else
                this.Close();
        }

        private void BtnCloseApp_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }


        private void BtnGoToDownloadPage_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Model.UpdateLink);
        }

        private void BtnContinueUsingApp_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System && e.SystemKey == Key.F4)
            {
                e.Handled = true;
            }
        }
    }
}
