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

        class AnimationState
        {
            private double Delay { get; set; }
            private double AnimationT { get; set; } = 1.0;
            private double StartVal { get; set; } = 0.0;
            public double TargetVal { get; private set; } = 0.0;
            public double AnimationSpeed { get; set; } = 1.0;

            public bool IsAnimationFinished { get => AnimationT >= 1.0; }

            public void Reset(double delay = 0.0)
            {
                Delay = delay;
                AnimationT = -delay;
            }

            public void Finish()
            {
                AnimationT = 1.0;
                StartVal = TargetVal;
            }

            public void Tick(double elapsedSeconds)
            {
                double step = elapsedSeconds * AnimationSpeed;
                double animT = AnimationT;
                if (animT < 1.0)
                {
                    animT += step;
                }
                if (animT >= 1.0)
                {
                    animT = 1.0;
                }
                AnimationT = animT;
            }

            public double CurrentValue
            {
                get
                {
                    if (AnimationT <= 0.0)
                    {
                        return StartVal;
                    }
                    return StartVal + AnimationT * (TargetVal - StartVal);
                }
            }

            public void ChangeTargetAndResetIfNeeded(double targetVal)
            {
                if (targetVal != TargetVal)
                {
                    StartVal = CurrentValue;
                    TargetVal = targetVal;
                    AnimationT = -Delay;
                }
            }

            public AnimationState(double startVal, double targetVal, double animationSpeed, double delay)
            {
                Delay = delay;
                StartVal = startVal;
                TargetVal = targetVal;
                AnimationSpeed = animationSpeed;
                AnimationT = -delay;
            }
        }

        Dictionary<int, AnimationState> _animationStates = new Dictionary<int, AnimationState>();

        SortedDictionary<int, PrettyChartBin> _freePool = new SortedDictionary<int, PrettyChartBin>();
        SortedDictionary<int, PrettyChartBin> _cachedControls = new SortedDictionary<int, PrettyChartBin>();




        public PrettyChartDataHistogram ChartData
        {
            get { return (PrettyChartDataHistogram)GetValue(_chartData); }
            set { SetValue(_chartData, value); }
        }

        public static readonly DependencyProperty _chartData =
            DependencyProperty.Register("ChartData", typeof(PrettyChartDataHistogram), typeof(PrettyChart), new UIPropertyMetadata(new PrettyChartDataHistogram(), StaticPropertyChangedCallback));

        public double CurrentFPS = 0.0;


        const double BIN_ANIMATION_SPEED = 1.2;
        const double BIN_ANIMATION_DELAY = 0.5;
        const double START_IDX_ANIMATION_SPEED = 1.2;
        const double ZOOM_IN_IDX_ANIMATION_SPEED = 1.8;
        const double MAX_VAL_ANIMATION_SPEED = 1.8;
        const double MAX_VAL_ANIMATION_DELAY = 0.5;

        public double BinMargin { get; set; } = 5.0;
        public double BottomMargin { get; set; } = 5.0;
        public double LeftMargin { get; set; } = 5.0;
        public double RightMargin { get; set; } = 5.0;
        public double TopMargin { get; set; } = 25.0;

        public string Title { get; set; } = "Default title";


        Stopwatch _sw = Stopwatch.StartNew();
        DispatcherTimer _timer = new DispatcherTimer(DispatcherPriority.Render);

        private bool _isDragStarted = false;

        public PrettyChart()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;

            _timer.Interval = TimeSpan.FromSeconds(0.01);
            _timer.Tick += _timer_Tick;
        }

        private bool TimerActivated
        {
            get
            {
                return _timer.IsEnabled;
            }
            set
            {
                if (value == true)
                {
                    if (!_timer.IsEnabled)
                    {
                        _lastTick = _sw.ElapsedTicks;
                        _timer.Start();
                        txTimerStatus.Text = "Timer activated";
                    }
                }
                else
                {
                    if (_timer.IsEnabled)
                    {
                        _timer.Stop();
                        txTimerStatus.Text = "Timer stopped";
                    }
                }
            }
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
                //newBinControl.SetLabelCollapsed(true);
                //newBinControl.SetValueLabelVisibility(false);
                cv.Children.Add(newBinControl);
                newBinControl.Visibility = Visibility.Hidden;
                newBinControl.SetHeight(newBinControl.GetMinHeight());
                _cachedControls.Add(idx, newBinControl);
                return newBinControl;
            }
        }




        private void ComputeCurrentFPS(double elapsedSec)
        {
            const int GetFPSEveryTick = 14;
            if (_currentTick % GetFPSEveryTick == 0)
            {
                long currentMs = _sw.ElapsedMilliseconds;
                if (elapsedSec > 0)
                {
                    CurrentFPS = 1.0 / elapsedSec;
                }
                FPS.Text = CurrentFPS.ToString("F2");
            }
        }


        int _currentTick = 0;
        long _lastTick = 0;
        private void _timer_Tick(object sender, EventArgs? e)
        {
            tbTitle.Text = Title;

            long newTick = _sw.ElapsedTicks;
            double elapsedSec = (newTick - _lastTick) / (double)Stopwatch.Frequency;
            _lastTick = newTick;
            _currentTick += 1;

            //AnimationProgressTick();
            //AnimateChartPosition(out bool chartPositionAnimationFinished);
            StartIdxAnimState.Tick(elapsedSec);
            NoBinsAnimState.Tick(elapsedSec);
            MaxValAnimState.Tick(elapsedSec);

            ComputeCurrentFPS(elapsedSec);

            UpdateBinChart(ChartData.HistData, elapsedSec, out bool binChartAnimationsFinished);
            bool allAnimationsFinished = binChartAnimationsFinished && StartIdxAnimState.IsAnimationFinished && NoBinsAnimState.IsAnimationFinished && MaxValAnimState.IsAnimationFinished;

            if (allAnimationsFinished)
            {
                TimerActivated = false;
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
                if (e.NewValue != e.OldValue)
                {
                    PrettyChartDataHistogram newChartData = (PrettyChartDataHistogram)e.NewValue;
                    newChartData.HistData.OnBinEntryAdded += OnBinEntryAdded;
                    newChartData.HistData.OnBinEntryUpdated += OnBinEntryUpdated;
                    newChartData.OnBinTimeSizeChanged += OnBinTimeSizeChanged;

                    ResetChartSettings(newChartData);
                    GotoEnd();
                    TimerActivated = true;
                }
            }

        }

        public void OnBinTimeSizeChanged()
        {
            //            ResetChartSettings(ChartData);
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

        public bool WasAlignedToRightBeforeAdded()
        {
            double targetNoBins = NoBinsAnimState.TargetVal;
            if (Math.Abs(StartIdxAnimState.TargetVal - ChartData.HistData.BinData.BinEntries.Count + targetNoBins + 1) < 1.1)
            {
                return true;
            }
            return false;
        }

        public void OnBinEntryUpdated(object sender, int binIdx, double oldValue, double newValue)
        {
            TimerActivated = true;
        }

        public void OnBinEntryAdded(object sender, double newValue)
        {
            if (WasAlignedToRightBeforeAdded())
            {
                this.MoveChart(-1, 0, true);
            }
            else
            {
                TimerActivated = true;
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
            TimerActivated = true;
        }

        AnimationState StartIdxAnimState = new AnimationState(startVal: -10.0, targetVal: -10.0, animationSpeed: START_IDX_ANIMATION_SPEED, delay: 0.0);
        AnimationState NoBinsAnimState = new AnimationState(startVal: 21.0, targetVal: 21.0, animationSpeed: ZOOM_IN_IDX_ANIMATION_SPEED, delay: 0.0);
        AnimationState MaxValAnimState = new AnimationState(startVal: 1.0, targetVal: 1.0, animationSpeed: MAX_VAL_ANIMATION_SPEED, delay: MAX_VAL_ANIMATION_DELAY);


        public void MoveChart(double steps, double zoomSteps, bool animate)
        {
            if (steps != 0.0)
            {
                StartIdxAnimState.ChangeTargetAndResetIfNeeded(StartIdxAnimState.TargetVal - steps);
            }
            if (zoomSteps != 0.0)
            {
                NoBinsAnimState.ChangeTargetAndResetIfNeeded(NoBinsAnimState.TargetVal + zoomSteps);
            }

            if (!animate)
            {
                StartIdxAnimState.Finish();
                NoBinsAnimState.Finish();
            }
            TimerActivated = true;
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


        void ResetChartSettings(PrettyChartDataHistogram? newData)
        {
            if (newData != null)
            {
                foreach (var binIdx in _cachedControls.Keys.ToList())
                {
                    MoveToFreePool(binIdx);
                }
                int entryCount = newData.HistData.BinData.BinEntries.Count;

                NoBinsAnimState.ChangeTargetAndResetIfNeeded(21);
                NoBinsAnimState.Finish();

                //StartIdxAnimState.ChangeTargetAndResetIfNeeded(-20);
                //StartIdxAnimState.Finish();
                TimerActivated = true;
                _timer_Tick(this, null);
            }

        }

        void JumpPage(double direction)
        {
            double targetNoBins = NoBinsAnimState.TargetVal;
            double targetStartPosX = StartIdxAnimState.TargetVal - targetNoBins * direction;

            if (direction > 0.0)
            {
                if (targetStartPosX < 0)
                {
                    targetStartPosX = 0;
                }
            }
            else
            {
                if (ChartData.HistData.BinData.BinEntries.Count - targetNoBins < targetStartPosX)
                {
                    targetStartPosX = ChartData.HistData.BinData.BinEntries.Count - targetNoBins;
                }
            }

            StartIdxAnimState.ChangeTargetAndResetIfNeeded(targetStartPosX);
            TimerActivated = true;
        }

        void GotoBeginning()
        {
            StartIdxAnimState.ChangeTargetAndResetIfNeeded(0.0);
            TimerActivated = true;
        }

        void GotoEnd()
        {
            double targetNoBins = NoBinsAnimState.TargetVal;
            StartIdxAnimState.ChangeTargetAndResetIfNeeded(ChartData.HistData.BinData.BinEntries.Count - targetNoBins);
            TimerActivated = true;
        }



        void UpdateBinChart(PrettyChartData? newData, double elapsedSeconds, out bool allAnimationFinished)
        {
            allAnimationFinished = true;
            if (newData != null)
            {
                int entryCount = newData.BinData.BinEntries.Count;

                double newNumBins = NoBinsAnimState.CurrentValue;
                double newStartIdx = StartIdxAnimState.CurrentValue;

                double newFullWidth = (DrawWidth - LeftMargin - RightMargin) / newNumBins;
                double newWidthWithoutMargins = newFullWidth - BinMargin;

                if (newWidthWithoutMargins <= 1)
                {
                    newWidthWithoutMargins = 1;
                }

                double maxVal = 0.001;
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
                MaxValAnimState.ChangeTargetAndResetIfNeeded(maxVal);

                double currentMaxVal = MaxValAnimState.CurrentValue;
                //double targetMaxVal = MaxValAnimState.TargetVal;

                for (int entryNo = 0; entryNo < entryCount; entryNo++)
                {
                    double val = newData.BinData.BinEntries[entryNo].Value;
                    string lbl = newData.BinData.BinEntries[entryNo].Label ?? "empty";

                    double heightWithoutMargins = DrawHeight - TopMargin - BottomMargin;

                    if (!_animationStates.ContainsKey(entryNo))
                    {
                        _animationStates[entryNo] = new AnimationState(startVal: 0.0, targetVal: val, animationSpeed: BIN_ANIMATION_SPEED, delay: BIN_ANIMATION_DELAY);
                        _animationStates[entryNo].Reset(delay: 1.0);
                    }
                    AnimationState animState = _animationStates[entryNo];
                    animState.ChangeTargetAndResetIfNeeded(val);

                    animState.Tick(elapsedSeconds);


                    double positionX = LeftMargin + (entryNo - newStartIdx) * newFullWidth + BinMargin / 2.0;
                    if (positionX + newFullWidth < -10 || positionX > DrawWidth)
                    {
                        MoveToFreePool(entryNo);
                        continue;
                    }

                    PrettyChartBin binControl = GetCachedControl(entryNo);

                    //binControl.AnimateEffectiveHeight(), MaxAnimSpeed);

                    if (!animState.IsAnimationFinished)
                    {
                        allAnimationFinished = false;
                    }

                    double binHeight = (animState.CurrentValue / currentMaxVal * (heightWithoutMargins - binControl.GetMinHeight() + binControl.ValuesOffset)) + binControl.GetMinHeight();
                    double binTargetHeight = (animState.TargetVal / currentMaxVal * (heightWithoutMargins - binControl.GetMinHeight() + binControl.ValuesOffset)) + binControl.GetMinHeight();

                    binControl.SetBottomLabelText(lbl);
                    binControl.SetValueLabelText(val.ToString("F2") + newData.Suffix);
                    binControl.Width = newWidthWithoutMargins;

                    binControl.SetHeight(binHeight);
                    binControl.SetTargetHeight(binTargetHeight);

                    binControl.Visibility = Visibility.Visible;

                    SetPosition(binControl, positionX, TopMargin + heightWithoutMargins - binControl.Height);
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
                e.Handled = true;
            }
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

        private void btnPageNext_Click(object sender, RoutedEventArgs e)
        {
            JumpPage(direction: -1.0);
        }

        private void btnPageBack_Click(object sender, RoutedEventArgs e)
        {
            JumpPage(direction: 1.0);
        }
    }
}
