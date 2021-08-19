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

        SortedDictionary<int, PrettyChartBin> _freePool = new SortedDictionary<int, PrettyChartBin>();
        SortedDictionary<int, PrettyChartBin> _cachedControls = new SortedDictionary<int, PrettyChartBin>();
            



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
        public double TopMargin { get; set; } = 25.0;



        DispatcherTimer _timer = new DispatcherTimer(DispatcherPriority.Render);

        int currentTick = 0;
        long lastTime = 0;
        private bool _isDragStarted = false;
        
        public PrettyChart()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;

            _timer.Interval = TimeSpan.FromSeconds(0.01);
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        public PrettyChartBin GetCachedControl(int idx)
        {
            if (_cachedControls.ContainsKey(idx))
            {
                return _cachedControls[idx];
            }
            else
            {
                if (_freePool.ContainsKey(idx))
                {
                    _cachedControls[idx] = _freePool[idx];
                    _freePool.Remove(idx);
                    return _cachedControls[idx];
                }
                if (_freePool.Count > 10)
                {
                    int firstIdx = _freePool.First().Key;
                    _cachedControls[idx] = _freePool[firstIdx];
                    _cachedControls[idx].Reset();
                    _freePool.Remove(firstIdx);
                    return _cachedControls[idx];
                }
                var newBinControl = new PrettyChartBin();
                cv.Children.Add(newBinControl);
                newBinControl.Visibility = Visibility.Hidden;
                newBinControl.SetHeight(newBinControl.GetMinHeight());
                _cachedControls.Add(idx, newBinControl);
                return newBinControl;
            }
        }


        private void _timer_Tick(object sender, EventArgs e)
        {
            if (AnimationProgress < 1.0)
            {
                AnimationProgress += 1.0 / CurrentFPS * AnimationSpeed;
                CurrentIdx = (TargetIdx - StartIdx) * AnimationProgress + StartIdx;
                CurrentNoBins = (TargetNoBins - StartNoBins) * AnimationProgress + StartNoBins;
            }
            else
            {
                AnimationProgress = 1.0;
                CurrentIdx = TargetIdx;
                StartIdx = TargetIdx;
                CurrentNoBins = TargetNoBins;
                StartNoBins = TargetNoBins;
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

            UpdateBinChart(ChartData);
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

        public void MoveToFreePool(int binIdx)
        {
            if (_cachedControls.ContainsKey(binIdx))
            {
                if (_freePool.ContainsKey(binIdx))
                {
                    throw new Exception("Free pool cannot contain this indes");
                }
                _cachedControls[binIdx].Visibility = Visibility.Hidden;
                _freePool[binIdx] = _cachedControls[binIdx];
                _cachedControls.Remove(binIdx);
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

        

        public double StartIdx { get; set; } = -10.0;
        public double CurrentIdx { get; set; } = -10.0;
        public double TargetIdx { get; set; } = -10.0;

        public double StartNoBins { get; set; } = 21.0;
        public double CurrentNoBins { get; set; } = 21.0;
        public double TargetNoBins { get; set; } = 21.0;


        public void MoveChart(double steps, double zoomSteps, bool animate)
        {
            StartIdx = CurrentIdx;
            TargetIdx = TargetIdx - steps;
            StartNoBins = CurrentNoBins;
            TargetNoBins = TargetNoBins - zoomSteps;

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
                foreach(var binIdx in _cachedControls.Keys.ToList())
                {
                    MoveToFreePool(binIdx);
                }
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
                UpdateBinChart(ChartData);
            }

        }

        void GotoBeginning()
        {
            TargetIdx = 0;
            AnimationProgress = 0;
            UpdateBinChart(ChartData);
        }
        void GotoEnd()
        {
            TargetIdx = ChartData.BinData.BinEntries.Count - TargetNoBins;
            AnimationProgress = 0;
            UpdateBinChart(ChartData);
        }


        void UpdateBinChart(PrettyChartData? newData)
        {
            if (newData != null)
            {
                int entryCount = newData.BinData.BinEntries.Count;

                double maxVal = 0.001;

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

                    double positionX = LeftMargin + (entryNo - newStartIdx) * newFullWidth + BinMargin / 2.0;
                    if (positionX + newFullWidth < -10 || positionX > DrawWidth)
                    {
                        continue;
                    }
                    if (val > maxVal)
                    {
                        maxVal = val;
                    }
                }

                for (int entryNo = 0; entryNo < entryCount; entryNo++)
                {
                    double val = newData.BinData.BinEntries[entryNo].Value;
                    string lbl = newData.BinData.BinEntries[entryNo].Label ?? "empty";

                    double heightWithoutMargins = DrawHeight - TopMargin - BottomMargin;

                    double positionX = LeftMargin + (entryNo - newStartIdx) * newFullWidth + BinMargin / 2.0;
                    if (positionX + newFullWidth < -10 || positionX > DrawWidth)
                    {
                        MoveToFreePool(entryNo);
                        continue;
                    }

                    PrettyChartBin binControl = GetCachedControl(entryNo);

                    //binControl.AnimateEffectiveHeight(), MaxAnimSpeed);

                    double targetHeight = (val / maxVal * (heightWithoutMargins - binControl.GetMinHeight() + binControl.ValuesOffset)) + binControl.GetMinHeight();
                    double currentHeight = binControl.Height;

                    binControl.SetBottomLabelText(lbl);
                    binControl.SetValueLabelText(val.ToString("F2") + newData.Suffix);
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
   
                    binControl.SetHeight(_animationStates[entryNo].StartHeight + _animationStates[entryNo].AnimationT * (_animationStates[entryNo].EndHeight - _animationStates[entryNo].StartHeight));

                    binControl.Visibility = Visibility.Visible;

                    SetPosition(binControl, positionX, TopMargin + heightWithoutMargins - binControl.Height);

                    if (entryNo < CurrentIdx && entryNo > CurrentIdx + CurrentNoBins)
                    {
                        break;
                    }
                }
            }
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

        double startMouseX = 0;
        double currentMouseX = 0;
        private void cv_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragStarted = true;

                startMouseX = e.GetPosition(this.Parent as Canvas).X;
                Mouse.Capture(rectMouseEvents);
            }
            e.Handled = true;
        }

        private void cv_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragStarted)
            {
                currentMouseX = e.GetPosition(this.Parent as Canvas).X;
                MoveChart((currentMouseX - startMouseX) / 10.0, 0, true);
                startMouseX = currentMouseX;
            }
            e.Handled = true;
        }
        private void cv_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragStarted = false;
            Mouse.Capture(null);
            e.Handled = true;
        }

        private void btnGoToBegin_Click(object sender, RoutedEventArgs e)
        {
            GotoBeginning();
        }

        private void btnGoToRight_Click(object sender, RoutedEventArgs e)
        {
            GotoEnd();
        }
    }
}
