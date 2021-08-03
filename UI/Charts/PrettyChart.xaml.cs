using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class PrettyChart : UserControl
    {
        enum BinCompatibilityResult
        {
            Compatible,
            MoveDataLeft,
            Recreate,
        }

        public PrettyChart()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;
        }

        public PrettyChartData ChartData
        {
            get { return (PrettyChartData)GetValue(_chartData); }
            set { SetValue(_chartData, value); }
        }

        public static readonly DependencyProperty _chartData =
            DependencyProperty.Register("ChartData", typeof(PrettyChartData), typeof(PrettyChart), new UIPropertyMetadata(new PrettyChartData(), StaticPropertyChangedCallback));


        public double BinMargin { get; set; } = 5.0;
        public double BottomMargin { get; set; } = 5.0;
        public double LeftMargin { get; set; } = 5.0;
        public double RightMargin { get; set; } = 5.0;
        public double TopMargin { get; set; } = 5.0;

        public static void StaticPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is PrettyChart prettyChart)
            {
                prettyChart.PropertyChangedCallback(e);
            }
        }
        public void PropertyChangedCallback(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == _chartData)
            {
                UpdateBinChart((PrettyChartData?)e.NewValue, (PrettyChartData?)e.OldValue, animate:true);
            }
        }


        BinCompatibilityResult CheckBinCompatibility(PrettyChartData newData, PrettyChartData? oldData)
        {
            if (oldData == null || newData == null)
            {
                return BinCompatibilityResult.Recreate;
            }
            if (newData.BinData.BinEntries.Count != oldData.BinData.BinEntries.Count)
            {
                return BinCompatibilityResult.Recreate;
            }

            return BinCompatibilityResult.Compatible;
        }

        private double DrawWidth => cv.ActualWidth > 0 ? cv.ActualWidth : 100.0;

        private double DrawHeight => cv.ActualHeight > 0 ? cv.ActualHeight : 100.0;

        public void OnSizeChanged(object sender, System.EventArgs e)
        {
            UpdateBinChart(ChartData, null, animate:false);
        }


        void RecreateBins(PrettyChartData? cd)
        {
            cv.Children.Clear();

            if (cd == null)
            {
                return;
            }

            int numBins = cd.BinData.BinEntries.Count;
            double newFullWidth = (DrawWidth - LeftMargin - RightMargin) / numBins;
            double widthWithoutMargins = newFullWidth - BinMargin;


            for (int entryNo = 0; entryNo < numBins; entryNo++)
            {
                double val = cd.BinData.BinEntries[entryNo].Value;

                PointCollection myPointCollection = new PointCollection();
                myPointCollection.Add(new Point(0, 0));
                myPointCollection.Add(new Point(0, 1));
                myPointCollection.Add(new Point(1, 1));
                myPointCollection.Add(new Point(1, 0));

                Polygon myPolygon = new Polygon();
                myPolygon.Points = myPointCollection;
                myPolygon.Fill = Brushes.White;
                myPolygon.Opacity = 0.6;
                myPolygon.Stretch = Stretch.Fill;
                myPolygon.Stroke = Brushes.Gray;
                myPolygon.StrokeThickness = 1;
                myPolygon.Width = widthWithoutMargins;
                
                SetPosition(myPolygon, LeftMargin + entryNo * newFullWidth + BinMargin / 2.0, BottomMargin);

                cv.Children.Add(myPolygon);
            }
        }



        void UpdateBinChart(PrettyChartData? newData, PrettyChartData? oldData, bool animate)
        {
            var cd = newData;

            double speedChange = 0.4;

            var res = CheckBinCompatibility(newData, oldData);
            if (res == BinCompatibilityResult.Recreate)
            {
                RecreateBins(cd);
            }

            if (cd == null)
            {
                return;
            }

            double maxVal = cd.BinData.GetMaxValue(-1.0);

            double heightWithoutMargins = DrawHeight - TopMargin - BottomMargin;

            Storyboard? myStoryboard = null;
            if (animate)
            {
                myStoryboard = new Storyboard();
            }

            int entryCount = cd.BinData.BinEntries.Count;
            for (int entryNo = 0; entryNo < entryCount; entryNo++)
            {
                double val = cd.BinData.BinEntries[entryNo].Value;
                double? valOld = null;
                if (res == BinCompatibilityResult.Compatible)
                {
                    valOld = oldData.BinData.BinEntries[entryNo].Value;
                }

                if (cv.Children[entryNo] is Polygon p)
                {
                    if (animate)
                    {
                        DoubleAnimation myDoubleAnimation = new DoubleAnimation();
                        myDoubleAnimation.From = 0;
                        if (valOld != null)
                        {
                            myDoubleAnimation.From = valOld / maxVal * heightWithoutMargins;
                        }
                        myDoubleAnimation.To = val / maxVal * heightWithoutMargins;
                        myDoubleAnimation.BeginTime = TimeSpan.FromSeconds((double)entryNo / (double)entryCount * speedChange);
                        myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(val / maxVal * speedChange));
                        Storyboard.SetTarget(myDoubleAnimation, p);
                        Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(Polygon.HeightProperty));
                        myStoryboard.Children.Add(myDoubleAnimation);
                    }
                    else
                    {
                        p.Height = val / maxVal * heightWithoutMargins;
                    }
                }
            }
            myStoryboard?.Begin(this);
        }

        private static void SetPosition(DependencyObject obj, double? x, double? y)
        {
            if (x != null)
            {
                obj.SetValue(Canvas.LeftProperty, x);
            }
            if (y != null)
            {
                obj.SetValue(Canvas.TopProperty, y);
            }
        }
    }
}
