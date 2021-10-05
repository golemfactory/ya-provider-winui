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
        private double LabelOffset { get; set; } = 60;
        public double ValuesOffset { get; set; } = 100;

        private bool ValueLabelVisible { get; set; } = true;

        private const double ZERO_VALUE_MARGIN = 2.0;

        public PrettyChartBin()
        {
            InitializeComponent();
            grdMain.RowDefinitions[0].Height = new GridLength(ValuesOffset);
            // grdMain.RowDefinitions[1].Height = new GridLength(BinHeight);
            grdMain.RowDefinitions[2].Height = new GridLength(LabelOffset);
        }

        public void SetLabelVisibility(bool visible)
        {
            tbBottomLabel.Visibility = Visibility.Hidden;
        }

        public void SetLabelCollapsed(bool collapsed)
        {
            LabelOffset = 0;
            tbBottomLabel.Visibility = Visibility.Collapsed;
            grdMain.RowDefinitions[2].Height = new GridLength(0);
        }

        public void SetValueLabelVisibility(bool visible)
        {
            ValueLabelVisible = visible;
        }

        public void SetTargetHeight(double height)
        {
            if (ValueLabelVisible)
            {
                if (height > GetMinHeight() + 70)
                {
                    tbValueLabelInside.Visibility = Visibility.Visible;
                    tbValueLabelOutside.Visibility = Visibility.Hidden;
                }
                else
                {
                    tbValueLabelOutside.Visibility = Visibility.Visible;
                    tbValueLabelInside.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                tbValueLabelOutside.Visibility = Visibility.Hidden;
                tbValueLabelInside.Visibility = Visibility.Hidden;
            }
        }

        public void SetHeight(double height)
        {
            this.Height = height;
        }

        public void SetWidth(double width)
        {
            this.Width = width;
        }

        public void SetValueLabelText(string text)
        {
            tbValueLabelInside.Text = text;
            tbValueLabelOutside.Text = text;
            BinRect.ToolTip = text;
        }

        public void SetBottomLabelText(string text)
        {
            tbBottomLabel.Text = text;
        }

        public void Reset()
        {
            this.Height = GetMinHeight();
        }

        public double GetMinHeight()
        {
            return LabelOffset + ValuesOffset + ZERO_VALUE_MARGIN;
        }
    }
}
