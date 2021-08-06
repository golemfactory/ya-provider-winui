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
    /// Interaction logic for DashboardStatusControl.xaml
    /// </summary>
    public partial class DashboardStatusControl : UserControl
    {
        public DashboardStatusControl()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty _status = DependencyProperty.Register("Status", typeof(DashboardStatusEnum), typeof(DashboardStatusControl));

        public DashboardStatusEnum Status
        {
            get
            {
                return (DashboardStatusEnum)GetValue(_status);
            }
            set
            {
                SetValue(_status, value);

            }
        }
    }

}
