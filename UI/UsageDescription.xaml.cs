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
    /// <summary>
    /// Interaction logic for UsageDescription.xaml
    /// </summary>
    public partial class UsageDescription : UserControl
    {
        //private string _text;
        UsageDescriptionViewModel ViewModel;
        private static readonly DependencyProperty _description = DependencyProperty.Register("Description", typeof(string), typeof(UsageDescription)); 
        private static readonly DependencyProperty _total = DependencyProperty.Register("Total", typeof(int), typeof(UsageDescription));
        private static readonly DependencyProperty _current = DependencyProperty.Register("Current", typeof(int), typeof(UsageDescription), new PropertyMetadata(0, new PropertyChangedCallback(OnStepChanged)));

        public UsageDescription()
        {
            
            InitializeComponent();
            this.ViewModel = new UsageDescriptionViewModel(this);
           // this.DataContext = this.ViewModel;  
            

        }

        private static void OnStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var xx = e;

            //ctrl.SetValue(ItemsProperty, ctrl!.Model.Items);
        }

        public string Description
        {
            get
            {
                return (string)GetValue(_description);
                
            }
            set
            {
                SetValue(_description, value);

                this.ViewModel.NotifyChange("Description");
            }
        }
      
        [Bindable(true)]
        public int Total
        {
            get
            {
                int ret=(int)GetValue(_total);
                return ret;
            }
            set
            {
                SetValue(_total, value);
                this.ViewModel.NotifyChange("Total");

            }
        }


        [Bindable(true)]
        public int Current
        {
            get
            {
                var ret =  (int)GetValue(_current);
                return ret;
            }
            set
            {
                SetValue(_current, value);
                this.ViewModel.NotifyChange("Current");

            }
        }
    }
}
