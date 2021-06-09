using GolemUI.Interfaces;
using Newtonsoft.Json;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IProcessControler _processController;
        public MainWindow(IProcessControler processController)
        {
            _processController = processController;

            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            var p = new GolemUI.Command.Provider();
            var lst = p.ExeUnitList();
            txtR.Text = JsonConvert.SerializeObject(lst, Formatting.Indented);
            var cfg = p.Config ?? new Command.Config();
            cfg.Subnet = "reqc2";
            p.Config = cfg;

            txtR.Text += JsonConvert.SerializeObject(p.Config, Formatting.Indented);

            foreach (var profile in p.Presets)
            {
                txtR.Text += "\n\n--\n";
                txtR.Text += JsonConvert.SerializeObject(profile, Formatting.Indented);
            }
        }

        private async void btnId_Click(object sender, RoutedEventArgs e)
        {
            await _processController.Init();


            await _processController.Me();


        }
    }
}
