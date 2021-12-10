﻿using GolemUI.ViewModel;
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
    public partial class DlgAppInfo : Window
    {
        DlgAppInfoViewModel? _model = null;
        public bool ShowHealth { get; set; } = false;

        public DlgAppInfo(DlgAppInfoViewModel model)
        {
            InitializeComponent();
            _model = model;
            this.DataContext = model;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 0;
        }

        private void BtnSendFeedBack_Click(object sender, RoutedEventArgs e)
        {
            _model?.SendFeedback();
            this.DialogResult = true;
            this.Close();
        }



        private void BtGoToSendFeedback_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 1;
        }
        private void BtnTroubleshooting_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                    "This option is intended only for troubleshooting and shows technical details, that you are probably not interested in. Are you sure too continue?", "Are you sure?", MessageBoxButton.YesNo ) ==
                MessageBoxResult.Yes)
            {
                ShowHealth = true;
                this.DialogResult = true;
                this.Close();
            }
        }

    }
}
