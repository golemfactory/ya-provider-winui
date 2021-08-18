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
using System.Windows.Threading;

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

        class BinAnimationState
        {
            public double DelayT { get; set; }
            public double AnimationT { get; set; }
            public double StartHeight { get; set; }
            public double EndHeight { get; set; }
        }

        Dictionary<int, BinAnimationState> _animationStates = new Dictionary<int, BinAnimationState>(); 
            



        public PrettyChartData ChartData
        {
            get { return (PrettyChartData)GetValue(_chartData); }
            set { SetValue(_chartData, value); }
        }

        public static readonly DependencyProperty _chartData =
            DependencyProperty.Register("ChartData", typeof(PrettyChartData), typeof(PrettyChart), new UIPropertyMetadata(new PrettyChartData(), StaticPropertyChangedCallback));

        public double CurrentFPS = 0.0;

        Stopwatch sw = Stopwatch.StartNew();

        public double AnimationProgress = 1.0;
        public double AnimationSpeed = 2.0;

        /*
        public double AnimationProgress
        {
            get { return (double)GetValue(_animationProgress); }
            set { SetValue(_animationProgress, value); }
        }

        
        public static readonly DependencyProperty _animationProgress =
            DependencyProperty.Register("AnimationProgress", typeof(double), typeof(PrettyChart), new UIPropertyMetadata(0.0, StaticPropertyChangedCallback));
        */

        public double AnimationValueProgress
        {
            get { return (double)GetValue(_animationValueProgress); }
            set { SetValue(_animationValueProgress, value); }
        }

        public static readonly DependencyProperty _animationValueProgress =
            DependencyProperty.Register("AnimationValueProgress", typeof(double), typeof(PrettyChart), new UIPropertyMetadata(0.0, StaticPropertyChangedCallback));

        public double BinMargin { get; set; } = 5.0;
        public double BottomMargin { get; set; } = 5.0;
        public double LeftMargin { get; set; } = 5.0;
        public double RightMargin { get; set; } = 5.0;
        public double TopMargin { get; set; } = 5.0;

        Storyboard? MainStoryboard { get; set; } = null;


        List<PrettyChartBin> BinControlsList { get; set; } = new List<PrettyChartBin>();
        int CurrentMaxBins { get; set; } = 100;


        DispatcherTimer _timer = new DispatcherTimer(DispatcherPriority.Render);

        int currentTick = 0;
        long lastTime = 0;
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
                newBinControl.Height = newBinControl.GetMinHeight();
                NameScope.GetNameScope(this).RegisterName(binControlName, newBinControl);
                BinControlsList.Add(newBinControl);
            }
            _timer.Interval = TimeSpan.FromSeconds(0.01);
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (AnimationProgress < 1.0)
            {
                AnimationProgress += 1.0 / CurrentFPS * AnimationSpeed;
                double animationValue = AnimationProgress;
                CurrentIdx = (TargetIdx - StartIdx) * animationValue + StartIdx;
                CurrentNoBins = (TargetNoBins - StartNoBins) * animationValue + StartNoBins;
                if (animationValue >= 1.0)
                {
                    StartIdx = TargetIdx;
                    StartNoBins = CurrentNoBins;
                }
            }
            else
            {
                AnimationProgress = 1.0;
            }
            const int GetFPSEveryTick = 14;
            if (currentTick % GetFPSEveryTick == 0)
            {
                long currentMs = sw.ElapsedMilliseconds;
                if (currentMs - lastTime > 0)
                {
                    CurrentFPS = 1.0 / (currentMs - lastTime) * 1000.0 * GetFPSEveryTick;
                }
                FPS.Text = CurrentFPS.ToString("F2");
                lastTime = sw.ElapsedMilliseconds;
            }

            UpdateBinChart(ChartData, AnimationProgress);
            currentTick += 1;
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
                if (e.NewValue != e.OldValue)
                {
                    PrettyChartData newChartData = (PrettyChartData)e.NewValue;
                    newChartData.OnBinEntryAdded += OnBinEntryAdded;
                    newChartData.OnBinEntryUpdated += OnBinEntryUpdated;

                    ResetChartSettings(newChartData);
                    UpdateBinChart(newChartData);
                }
            }

        }

        public void OnBinEntryUpdated(object sender, int binIdx, double oldValue, double newValue)
        {

        }
        public void OnBinEntryAdded(object sender, double newValue)
        {
            this.MoveChart(-1, 0, true);
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
            UpdateBinChart(ChartData);
        }

        double MaxAnimSpeed = 0.4;


        public double StartIdx { get; set; } = -10.0;
        public double CurrentIdx { get; set; } = -10.0;
        public double TargetIdx { get; set; } = -10.0;

        public double StartNoBins { get; set; } = 11.0;
        public double CurrentNoBins { get; set; } = 11.0;
        public double TargetNoBins { get; set; } = 11.0;


        public void MoveChart(int steps, int zoomSteps, bool animate)
        {
            StartIdx = CurrentIdx;
            TargetIdx = TargetIdx - steps;
            StartNoBins = CurrentNoBins;
            TargetNoBins = TargetNoBins - zoomSteps;

            if (MainStoryboard != null)
            {
                //cancel storyboard
                MainStoryboard.Completed -= MainStoryBoardFinished;
                MainStoryboard.Stop();
                MainStoryboard.Remove();
            }

            if (animate)
            {
                AnimationProgress = 0.0f;
            }
            else
            {
                UpdateBinChart(ChartData);
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

        
        void ResetChartSettings(PrettyChartData? newData)
        {
            if (newData != null)
            {
                int entryCount = newData.BinData.BinEntries.Count;
                CurrentNoBins = StartNoBins = TargetNoBins = 10;
                if (entryCount > 10)
                {
                    StartIdx = TargetIdx = CurrentIdx = 0;
                }
                else
                {
                    StartIdx = TargetIdx = CurrentIdx = CurrentNoBins - StartIdx;
                }


            }

        }


        void UpdateBinChart(PrettyChartData? newData, double animationHeightT = 1.0)
        {
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
                    string lbl = newData.BinData.BinEntries[entryNo].Label ?? "empty";

                    double heightWithoutMargins = DrawHeight - TopMargin - BottomMargin;

                    string binName = IndexToBinName(entryNo);
                    PrettyChartBin binControl = (PrettyChartBin)cv.FindName(binName);

                    //binControl.AnimateEffectiveHeight(), MaxAnimSpeed);
                    
                    double targetHeight = (val / maxVal * (heightWithoutMargins - binControl.GetMinHeight())) + binControl.GetMinHeight();
                    double currentHeight = binControl.Height;
                    double animstep = 10;
                    double nextStepHeight;
                    if (targetHeight > currentHeight + animstep)
                    {
                        nextStepHeight = currentHeight + animstep;
                    }
                    else if (targetHeight < currentHeight - animstep)
                    {
                        nextStepHeight = currentHeight - animstep;
                    }
                    else
                    {
                        nextStepHeight = targetHeight;
                    }
                    binControl.SetBottomLabelText(lbl);
                    binControl.SetValueLabelText(val.ToString("F2"));
                    binControl.Width = newWidthWithoutMargins;
                    //binControl.Height = nextStepHeight;

                    if (!_animationStates.ContainsKey(entryNo))
                    {
                        _animationStates[entryNo] = new BinAnimationState() { AnimationT = 0.0, StartHeight = binControl.GetMinHeight(), EndHeight = binControl.GetMinHeight() };
                    }
                    _animationStates[entryNo].AnimationT += 0.05;

                    if (_animationStates[entryNo].AnimationT > 1.0)
                    {
                        _animationStates[entryNo].AnimationT = 1.0;
                    }
                    if (targetHeight != _animationStates[entryNo].EndHeight)
                    {
                        _animationStates[entryNo].EndHeight = targetHeight;
                        _animationStates[entryNo].StartHeight = binControl.Height;
                        _animationStates[entryNo].AnimationT = 0.0;
                    }
                    binControl.Height = _animationStates[entryNo].StartHeight + _animationStates[entryNo].AnimationT * (_animationStates[entryNo].EndHeight - _animationStates[entryNo].StartHeight);


                    SetPosition(binControl, LeftMargin + (entryNo - newStartIdx) * newFullWidth + BinMargin / 2.0, TopMargin + heightWithoutMargins - binControl.Height);
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
