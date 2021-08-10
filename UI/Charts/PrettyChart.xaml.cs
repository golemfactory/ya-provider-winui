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
                UpdateBinChart((PrettyChartData?)e.NewValue, (PrettyChartData?)e.OldValue, animate: true);
            }
        }


        BinCompatibilityResult CheckBinCompatibility(PrettyChartData? newData, PrettyChartData? oldData)
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
            UpdateBinChart(ChartData, null, animate: false);
        }

        double MaxAnimSpeed = 0.4;


        void RecreateBins(PrettyChartData? newData, PrettyChartData? oldData, Storyboard? myStoryboard)
        {
            if (newData == null)
            {
                return;
            }

            int numBins = newData.BinData.BinEntries.Count;
            double newFullWidth = (DrawWidth - LeftMargin - RightMargin) / numBins;
            double widthWithoutMargins = newFullWidth - BinMargin;

            if (widthWithoutMargins <= 1)
            {
                widthWithoutMargins = 1;
            }

            double oldFullWidth = newFullWidth;
            int oldNumBins = numBins;
            double oldWidthWithoutMargins = widthWithoutMargins;
            if (oldData != null)
            {
                oldNumBins = Math.Max(oldData.BinData.BinEntries.Count, 1);
                oldFullWidth = (DrawWidth - LeftMargin - RightMargin) / oldNumBins;
                oldWidthWithoutMargins = oldFullWidth - BinMargin;
            }

            for (int entryNo = 0; ; entryNo++)
            {
                string polyName = $"poly_no_{entryNo}";
                string textName = $"text_no_{entryNo}";
                string text2Name = $"text2_no_{entryNo}";
                Polygon? pol = null;
                TextBlock? lbl = null;
                TextBlock? val = null;

                object? obj = cv.FindName(polyName);
                object? obj2 = cv.FindName(textName);
                object? obj3 = cv.FindName(text2Name);
                if (obj?.GetType() == typeof(Polygon))
                {
                    pol = (Polygon)obj;
                    lbl = (TextBlock)obj2;
                    val = (TextBlock)obj3;
                }
                else
                {
                    if (entryNo < numBins)
                    {

                        PointCollection myPointCollection = new PointCollection();
                        myPointCollection.Add(new Point(0, 0));
                        myPointCollection.Add(new Point(0, 1));
                        myPointCollection.Add(new Point(1, 1));
                        myPointCollection.Add(new Point(1, 0));

                        pol = new Polygon();
                        pol.Points = myPointCollection;
                        pol.Fill = Brushes.White;
                        pol.Opacity = 0.6;
                        pol.Stretch = Stretch.Fill;
                        pol.Stroke = Brushes.Gray;
                        pol.StrokeThickness = 1;

                        pol.Name = polyName;


                        cv.Children.Add(pol);

                        var tb = new TextBlock();
                        tb.Name = textName;
                        tb.Text = "test";
                        tb.FontSize = 12;
                        var tg = new TransformGroup();
                        tg.Children.Add(new ScaleTransform(1.0, -1.0));
                        tg.Children.Add(new RotateTransform(90));

                        tb.LayoutTransform = tg;
                        cv.Children.Add(tb);

                        var tb2 = new TextBlock();
                        tb2.Name = text2Name;
                        tb2.Text = "val";
                        tb2.FontSize = 14;
                        var tg2 = new TransformGroup();
                        tg2.Children.Add(new ScaleTransform(1.0, -1.0));
                        tg2.Children.Add(new RotateTransform(70));
                        tb2.LayoutTransform = tg2;
                        cv.Children.Add(tb2);

                        NameScope.GetNameScope(this).RegisterName(polyName, pol);
                        NameScope.GetNameScope(this).RegisterName(textName, tb);
                        NameScope.GetNameScope(this).RegisterName(text2Name, tb2);
                        lbl = tb;
                        val = tb2;
                    }
                    else
                    {
                        return;
                    }
                }
                if (entryNo < numBins)
                {
                    if (myStoryboard == null)
                    {
                        pol.Width = widthWithoutMargins;
                        SetPosition(pol, LeftMargin + entryNo * newFullWidth + BinMargin / 2.0, BottomMargin);
                        SetPosition(lbl, LeftMargin + entryNo * newFullWidth + BinMargin / 2.0, BottomMargin - 20);
                        SetPosition(val, LeftMargin + entryNo * newFullWidth + BinMargin / 2.0, BottomMargin + 10);
                    }
                    else
                    {
                        {
                            DoubleAnimation anim = new DoubleAnimation();
                            anim.From = oldWidthWithoutMargins;
                            anim.To = widthWithoutMargins;
                            anim.Duration = new Duration(TimeSpan.FromSeconds(MaxAnimSpeed));
                            Storyboard.SetTarget(anim, pol);
                            Storyboard.SetTargetProperty(anim, new PropertyPath(Polygon.WidthProperty));
                            myStoryboard.Children.Add(anim);
                        }

                        {
                            DoubleAnimation anim = new DoubleAnimation();
                            anim.From = LeftMargin + entryNo * oldFullWidth + BinMargin / 2.0;
                            anim.To = LeftMargin + entryNo * newFullWidth + BinMargin / 2.0;
                            anim.Duration = new Duration(TimeSpan.FromSeconds(MaxAnimSpeed));
                            Storyboard.SetTarget(anim, pol);
                            Storyboard.SetTargetProperty(anim, new PropertyPath(Canvas.LeftProperty));
                            myStoryboard.Children.Add(anim);
                        }

                        {
                            DoubleAnimation anim = new DoubleAnimation();
                            anim.From = LeftMargin + entryNo * oldFullWidth + BinMargin / 2.0 + 3;
                            anim.To = LeftMargin + entryNo * newFullWidth + BinMargin / 2.0 + 3;
                            anim.Duration = new Duration(TimeSpan.FromSeconds(MaxAnimSpeed));
                            Storyboard.SetTarget(anim, lbl);
                            Storyboard.SetTargetProperty(anim, new PropertyPath(Canvas.LeftProperty));
                            myStoryboard.Children.Add(anim);
                        }

                        {
                            DoubleAnimation anim = new DoubleAnimation();
                            anim.From = LeftMargin + entryNo * oldFullWidth + BinMargin / 2.0;
                            anim.To = LeftMargin + entryNo * newFullWidth + BinMargin / 2.0;
                            anim.Duration = new Duration(TimeSpan.FromSeconds(MaxAnimSpeed));
                            Storyboard.SetTarget(anim, val);
                            Storyboard.SetTargetProperty(anim, new PropertyPath(Canvas.LeftProperty));
                            myStoryboard.Children.Add(anim);
                        }


                        SetPosition(pol, null, BottomMargin);
                        SetPosition(lbl, null, BottomMargin - 20);
                        SetPosition(val, null, BottomMargin + 40);
                    }
                }
                else
                {
                    cv.Children.Remove(pol);
                    NameScope.GetNameScope(this).UnregisterName(pol.Name);
                    cv.Children.Remove(lbl);
                    NameScope.GetNameScope(this).UnregisterName(lbl.Name);
                    cv.Children.Remove(val);
                    NameScope.GetNameScope(this).UnregisterName(val.Name);
                }
            }
        }



        void UpdateBinChart(PrettyChartData? newData, PrettyChartData? oldData, bool animate)
        {
            if (newData != null && newData.NoAnimate)
            {
                animate = false;
            }

            Storyboard? myStoryboard = null;
            if (animate)
            {
                myStoryboard = new Storyboard();
            }

            var res = CheckBinCompatibility(newData, oldData);
            if (res == BinCompatibilityResult.Recreate)
            {
                RecreateBins(newData, oldData, myStoryboard);
            }

            if (newData == null)
            {
                return;
            }

            double maxVal = newData.BinData.GetMaxValue(0.001);
            double? oldMaxVal = oldData?.BinData.GetMaxValue(0.001);

            double heightWithoutMargins = DrawHeight - TopMargin - BottomMargin;



            int entryCount = newData.BinData.BinEntries.Count;
            for (int entryNo = 0; entryNo < entryCount; entryNo++)
            {
                double val = newData.BinData.BinEntries[entryNo].Value;
                double? valOld = null;
                if (oldData != null && entryNo < oldData.BinData.BinEntries.Count)
                {
                    valOld = oldData.BinData.BinEntries[entryNo].Value;
                }


                string polyName = $"poly_no_{entryNo}";
                string textName = $"text_no_{entryNo}";
                string text2Name = $"text2_no_{entryNo}";


                Polygon p = (Polygon)cv.FindName(polyName);
                TextBlock lbl = (TextBlock)cv.FindName(textName);
                TextBlock tbVal = (TextBlock)cv.FindName(text2Name);

                lbl.Text = newData.BinData.BinEntries[entryNo].Label;
                tbVal.Text = val.ToString("F2");
                if (animate)
                {
                    {
                        DoubleAnimation anim = new DoubleAnimation();
                        anim.From = 0;
                        if (valOld != null && oldMaxVal != null && oldMaxVal > 0.0)
                        {
                            anim.From = valOld / oldMaxVal * heightWithoutMargins;
                        }
                        anim.To = val / maxVal * heightWithoutMargins;
                        anim.BeginTime = TimeSpan.FromSeconds((double)entryNo / (double)entryCount * MaxAnimSpeed);
                        anim.Duration = new Duration(TimeSpan.FromSeconds(val / maxVal * MaxAnimSpeed));
                        Storyboard.SetTarget(anim, p);
                        Storyboard.SetTargetProperty(anim, new PropertyPath(Polygon.HeightProperty));
                        myStoryboard?.Children.Add(anim);
                    }
                    {
                        DoubleAnimation anim = new DoubleAnimation();
                        anim.From = 0;
                        if (valOld != null && oldMaxVal != null && oldMaxVal > 0.0)
                        {
                            anim.From = valOld / oldMaxVal * heightWithoutMargins + 6;
                        }
                        anim.To = val / maxVal * heightWithoutMargins + 6;
                        anim.BeginTime = TimeSpan.FromSeconds((double)entryNo / (double)entryCount * MaxAnimSpeed);
                        anim.Duration = new Duration(TimeSpan.FromSeconds(val / maxVal * MaxAnimSpeed));
                        Storyboard.SetTarget(anim, tbVal);
                        Storyboard.SetTargetProperty(anim, new PropertyPath(Canvas.TopProperty));
                        myStoryboard?.Children.Add(anim);
                    }

                }
                else
                {
                    SetPosition(tbVal, null, val / maxVal * heightWithoutMargins + 6);
                    p.Height = val / maxVal * heightWithoutMargins;
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
