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
        private double LabelOffset { get; set; } = 50;
        private double ValuesOffset { get; set; } = 50;
        private double BinHeight { get; set; } = 200;
        private string LabelText { get; set; }

        Storyboard MainStoryboard { get; set; }


        public double AnimationProgress
        {
            get { return (double)GetValue(_animationProgress); }
            set { SetValue(_animationProgress, value); }
        }

        public static readonly DependencyProperty _animationProgress =
            DependencyProperty.Register("AnimationProgress", typeof(double), typeof(PrettyChartBin), new UIPropertyMetadata(0.0, StaticPropertyChangedCallback));

        public static void StaticPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is PrettyChartBin prettyChartBin)
            {
                prettyChartBin.PropertyChangedCallback(e);
            }
        }
        public void PropertyChangedCallback(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == _animationProgress)
            {
                double animationValue = (double)e.NewValue;
                this.SetEffectiveHeight(BinHeight * animationValue);
            }
        }

        public PrettyChartBin()
        {
            InitializeComponent();
            grdMain.RowDefinitions[0].Height = new GridLength(ValuesOffset);
            grdMain.RowDefinitions[1].Height = new GridLength(BinHeight);
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

        public void AnimateEffectiveHeight(double height, double MaxAnimSpeed)
        {
            BinHeight = height;
            grdMain.RowDefinitions[1].Height = new GridLength(height);
        }


        public void SetEffectiveHeight(double height)
        {
            BinHeight = height;
            grdMain.RowDefinitions[1].Height = new GridLength(height);
        }

        public double GetEffectiveHeight()
        {
            return BinHeight;
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
