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
    /// Interaction logic for WelcomeNodeName.xaml
    /// </summary>
    public partial class WelcomeNodeName : UserControl
    {
        public WelcomeNodeName()
        {
            InitializeComponent();

            LocalSettings s = SettingsLoader.LoadSettingsFromFileOrDefault();
            tbNodeName.Text = s.NodeName;
        }

        private bool validateName()
        {
            if (String.IsNullOrEmpty(tbNodeName.Text))
            {
                return false;
            }
            return true;
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (validateName())
            {
                LocalSettings s = SettingsLoader.LoadSettingsFromFileOrDefault();
                s.NodeName = tbNodeName.Text;
                SettingsLoader.SaveSettingsToFile(s);
                GlobalApplicationState.Instance.NotifyApplicationStateChanged(this, GlobalApplicationStateAction.reloadSettings);
                GlobalApplicationState.Instance.Dashboard?.SwitchPage(DashboardPages.PageDashboardBenchmark);
            }
        }

        private void btnCheckAddress(object sender, RoutedEventArgs e)
        {
            NameGen gen = new NameGen();

            tbNodeName.Text = gen.GenerateElvenName() + "-" + gen.GenerateElvenName();

        }
    }
}
