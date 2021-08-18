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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GolemUI.UI.Charts
{
    /// <summary>
    /// Interaction logic for PrettyChartBin.xaml
    /// </summary>
    public partial class PrettyChartBin : UserControl
    {
        private double LabelOffset { get; set; } = 80;
        private double ValuesOffset { get; set; } = 60;
        private double BinHeight { get; set; } = 200;

        public double StartHeight { get; set; } = 300;
        public double CurrentHeight { get; set; } = 300;
        public double TargetHeight { get; set; } = 300;


        public PrettyChartBin()
        {
            InitializeComponent();
            grdMain.RowDefinitions[0].Height = new GridLength(ValuesOffset);
           // grdMain.RowDefinitions[1].Height = new GridLength(BinHeight);
            grdMain.RowDefinitions[2].Height = new GridLength(LabelOffset);
        }

        public void SetValueLabelText(string text)
        {
            tbValueLabel.Text = text;
        }

        public void SetBottomLabelText(string text)
        {
            tbBottomLabel.Text = text;
        }



        public double GetTotalHeight()
        {
            return LabelOffset + ValuesOffset + BinHeight;
        }

        public double GetMinHeight()
        {
            return LabelOffset + ValuesOffset;
        }

    }
}
