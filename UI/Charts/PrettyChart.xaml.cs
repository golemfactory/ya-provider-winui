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


        public PrettyChartData ChartData
        {
            get { return (PrettyChartData)GetValue(_chartData); }
            set { SetValue(_chartData, value); }
        }

        public static readonly DependencyProperty _chartData =
            DependencyProperty.Register("ChartData", typeof(PrettyChartData), typeof(PrettyChart), new UIPropertyMetadata(new PrettyChartData(), StaticPropertyChangedCallback));


        public double AnimationProgress
        {
            get { return (double)GetValue(_animationProgress); }
            set { SetValue(_animationProgress, value); }
        }

        public static readonly DependencyProperty _animationProgress =
            DependencyProperty.Register("AnimationProgress", typeof(double), typeof(PrettyChart), new UIPropertyMetadata(0.0, StaticPropertyChangedCallback));


        public double BinMargin { get; set; } = 5.0;
        public double BottomMargin { get; set; } = 5.0;
        public double LeftMargin { get; set; } = 5.0;
        public double RightMargin { get; set; } = 5.0;
        public double TopMargin { get; set; } = 5.0;

        Storyboard? MainStoryboard { get; set; } = null;


        List<PrettyChartBin> BinControlsList { get; set; } = new List<PrettyChartBin>();
        int CurrentMaxBins { get; set; } = 100;



        public PrettyChart()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;

            for (int i = 0; i < CurrentMaxBins; i++)
            {
                var newBinControl = new PrettyChartBin();
                cv.Children.Add(newBinControl);

                string binControlName = IndexToBinName(i);
                newBinControl.Name = binControlName;
                newBinControl.Visibility = Visibility.Hidden;
                NameScope.GetNameScope(this).RegisterName(binControlName, newBinControl);
                BinControlsList.Add(newBinControl);
            }
        }

        private string IndexToBinName(int idx)
        {
            string binControlName = "Bin_" + idx.ToString();
            return binControlName;
        }


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
            if (e.Property == _animationProgress)
            {
                double animationValue = (double)e.NewValue;
                CurrentIdx = (TargetIdx - StartIdx) * animationValue + StartIdx;
                CurrentNoBins = (TargetNoBins - StartNoBins) * animationValue + StartNoBins;
                if (animationValue >= 1.0)
                {
                    StartIdx = TargetIdx;
                    StartNoBins = CurrentNoBins;
                }

                UpdateBinChart(ChartData, null, animate: false);
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

                        var bin = new PrettyChartBin();


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


        public double StartIdx { get; set; } = 2;
        public double StartNoBins { get; set; } = 5;
        public double CurrentIdx { get; set; } = 2;
        public double CurrentNoBins { get; set; } = 5;
        public double TargetNoBins { get; set; } = 5;
        public double TargetIdx { get; set; } = 2;


        public void MoveChart(int steps, int zoomSteps, bool animate)
        {
            StartIdx = CurrentIdx;
            TargetIdx = TargetIdx + steps;
            StartNoBins = CurrentNoBins;
            TargetNoBins = TargetNoBins + zoomSteps;

            if (MainStoryboard != null)
            {
                //cancel storyboard
                MainStoryboard.Completed -= MainStoryBoardFinished;
                MainStoryboard.Stop();
                MainStoryboard.Remove();
            }

            Storyboard? myStoryboard = null;
            if (animate)
            {
                myStoryboard = new Storyboard();
                MainStoryboard = myStoryboard;
                {
                    DoubleAnimation anim = new DoubleAnimation();
                    anim.From = 0.0f;
                    anim.To = 1.0f;
                    anim.Duration = new Duration(TimeSpan.FromSeconds(MaxAnimSpeed));
                    Storyboard.SetTarget(anim, this);
                    Storyboard.SetTargetProperty(anim, new PropertyPath("AnimationProgress"));
                    MainStoryboard.Children.Add(anim);
                }
                MainStoryboard.Begin();
            }
            else
            {
                UpdateBinChart(ChartData, null, animate: true, steps);
            }

        }

        public void MoveRight(int steps, bool animate)
        {
            MoveChart(steps, 0, animate);
        }
        public void MoveLeft(int steps, bool animate)
        {
            MoveChart(-steps, 0, animate);
        }
        public void ZoomIn(int steps, bool animate)
        {
            MoveChart(0, steps, animate);
        }
        public void ZoomOut(int steps, bool animate)
        {
            MoveChart(0, -steps, animate);
        }


        void UpdateBinChart(PrettyChartData? newData, PrettyChartData? oldData, bool animate, int movement = 0, int zoom = 0)
        {
            if (newData != null && newData.NoAnimate)
            {
                animate = false;
            }




            if (newData != null)
            {
                int entryCount = newData.BinData.BinEntries.Count;

                double maxVal = newData.BinData.GetMaxValue(0.001);

                double newNumBins = CurrentNoBins;
                double newStartIdx = CurrentIdx;
                double newFullWidth = (DrawWidth - LeftMargin - RightMargin) / newNumBins;
                double newWidthWithoutMargins = newFullWidth - BinMargin;

                if (newWidthWithoutMargins <= 1)
                {
                    newWidthWithoutMargins = 1;
                }



                for (int entryNo = 0; entryNo < entryCount; entryNo++)
                {
                    double val = newData.BinData.BinEntries[entryNo].Value;
                    string lbl = newData.BinData.BinEntries[entryNo].Label;

                    double heightWithoutMargins = DrawHeight - TopMargin - BottomMargin;

                    string binName = IndexToBinName(entryNo);
                    PrettyChartBin binControl = (PrettyChartBin)cv.FindName(binName);

                    binControl.Height = heightWithoutMargins;
                    binControl.AnimateEffectiveHeight(val / maxVal * (heightWithoutMargins - binControl.GetMinHeight()), MaxAnimSpeed);
                    binControl.SetBottomLabelText(lbl);
                    binControl.SetValueLabelText(val.ToString("F2"));
                    binControl.Width = newWidthWithoutMargins;


                    SetPosition(binControl, LeftMargin + (entryNo - newStartIdx) * newFullWidth + BinMargin / 2.0, TopMargin + heightWithoutMargins - binControl.GetTotalHeight());
                    /*else
                    {
                        {
                            DoubleAnimation anim = new DoubleAnimation();

                            double idealWidthCandidate = oldWidthWithoutMargins;
                            double existingWidth = (double)binControl.GetValue(Canvas.WidthProperty);
                            if (!double.IsNaN(existingWidth))
                            {
                                idealWidthCandidate = existingWidth;
                            }

                            anim.From = idealWidthCandidate;
                            anim.To = newWidthWithoutMargins;
                            anim.Duration = new Duration(TimeSpan.FromSeconds(MaxAnimSpeed));
                            Storyboard.SetTarget(anim, binControl);
                            Storyboard.SetTargetProperty(anim, new PropertyPath(Polygon.WidthProperty));
                            myStoryboard.Children.Add(anim);
                        }

                        {
                            DoubleAnimation anim = new DoubleAnimation();

                            double idealLeftCandidate = LeftMargin + (entryNo - oldStartIdx) * oldFullWidth + BinMargin / 2.0;
                            double existingLeft = (double)binControl.GetValue(Canvas.LeftProperty); 

                            if (!double.IsNaN(existingLeft))
                            {
                                idealLeftCandidate = existingLeft;
                            }

                            anim.From = idealLeftCandidate;
                            anim.To = LeftMargin + (entryNo - newStartIdx) * newFullWidth + BinMargin / 2.0;
                            anim.Duration = new Duration(TimeSpan.FromSeconds(MaxAnimSpeed));
                            Storyboard.SetTarget(anim, binControl);
                            Storyboard.SetTargetProperty(anim, new PropertyPath(Canvas.LeftProperty));
                            myStoryboard.Children.Add(anim);
                        }

                        binControl.SetEffectiveHeight(val / maxVal * heightWithoutMargins);
                        binControl.SetBottomLabelText(lbl);
                        binControl.SetValueLabelText(val.ToString("F2"));
                        SetPosition(binControl, null, heightWithoutMargins - binControl.GetTotalHeight());
                    }*/

                    binControl.Visibility = Visibility.Visible;
                    if (entryNo < CurrentIdx && entryNo > CurrentIdx + CurrentNoBins)
                    {
                        break;
                    }
                }
            }
           /* if (myStoryboard != null)
            {
                myStoryboard.Completed += MainStoryBoardFinished;
            }

            myStoryboard?.Begin(this); */
            /*
            if (newData != null && newData.NoAnimate)
            {
                animate = false;
            }

            if (MainStoryboard != null)
            {
                MainStoryboard.Stop();
            }

            Storyboard? myStoryboard = null;
            if (animate)
            {
                myStoryboard = new Storyboard();
            }
            MainStoryboard = myStoryboard;

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
            myStoryboard?.Begin(this);*/
        }

        public void MainStoryBoardFinished(object sender, EventArgs e)
        {
            Debug.WriteLine("storyboard completed");
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
