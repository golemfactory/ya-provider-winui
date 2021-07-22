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
    /// Interaction logic for UsageDescription.xaml
    /// </summary>
    public partial class UsageDescription : UserControl
    {
        //private string _text;
        public UsageDescription()
        {
            InitializeComponent();
            Description = "";
        }
        public static readonly DependencyProperty _description = DependencyProperty.Register("Description", typeof(string), typeof(UsageDescription));

        public string Description
        {
            get
            {
                return (string)GetValue(_description);
            }
            set
            {
                SetValue(_description, value);

            }
        }
        public static readonly DependencyProperty _total = DependencyProperty.Register("Total", typeof(int), typeof(UsageDescription));

        public int Total
        {
            get
            {
                return (int)GetValue(_total);
            }
            set
            {
                SetValue(_total, value);

            }
        }

        public static readonly DependencyProperty _current = DependencyProperty.Register("Current", typeof(int), typeof(UsageDescription));

        public int Current
        {
            get
            {
                return (int)GetValue(_current);
            }
            set
            {
                SetValue(_current, value);

            }
        }
    }
}
