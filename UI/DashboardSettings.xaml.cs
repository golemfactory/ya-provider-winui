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
    /// 

    public class SingleGpuDescriptor
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }

        public SingleGpuDescriptor(string name, bool isActive)
        {
            Name = name;
            IsActive = isActive;
        }
    }
    public class SettingsDataContext
    {
        public List<SingleGpuDescriptor> GpuList { get; set; }
        public SettingsDataContext()
        {
            GpuList = new List<SingleGpuDescriptor>();
            GpuList.Add(new SingleGpuDescriptor("1st GPU", false));
            GpuList.Add(new SingleGpuDescriptor("second GPU", true));
            GpuList.Add(new SingleGpuDescriptor("3rd GPU", false));
        }
    }
    public partial class DashboardSettings : UserControl
    {
        public string[] GpuList { get; set; } = { "raz", "dwat" };
        SettingsDataContext ctx = new SettingsDataContext();
        public DashboardSettings()
        {
            InitializeComponent();
            ctx.GpuList.Add(new SingleGpuDescriptor("4th GPU", true));
            
            GlobalApplicationState.Instance.ApplicationStateChanged += OnGlobalApplicationStateChanged;
            GpuList = new[] { "ABC", "DEF" };
            this.DataContext = this.ctx;
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

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
