using GolemUI.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    public partial class UsageDescription : UserControl
    {
        private static readonly DependencyProperty _description = DependencyProperty.Register("Description", typeof(string), typeof(UsageDescription));
        private static readonly DependencyProperty _total = DependencyProperty.Register("Total", typeof(int), typeof(UsageDescription));
        private static readonly DependencyProperty _current = DependencyProperty.Register("Current", typeof(int), typeof(UsageDescription));

        public UsageDescription()
        {
            InitializeComponent();
            this.root.DataContext = this;
        }
        public string Description
        {
            get => (string)GetValue(_description);
            set => SetValue(_description, value);
        }
        public int Total
        {
            get => (int)GetValue(_total);
            set => SetValue(_total, value);
        }
        public int Current
        {
            get => (int)GetValue(_current);
            set => SetValue(_current, value);
        }
    }
}
