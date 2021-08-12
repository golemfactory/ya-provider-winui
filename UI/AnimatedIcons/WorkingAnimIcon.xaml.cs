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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GolemUI.UI.AnimatedIcons
{
    /// <summary>
    /// Interaction logic for WorkingAnimIcon.xaml
    /// </summary>
    public partial class WorkingAnimIcon : UserControl
    {
        private readonly DispatcherTimer _animationTimer;

        DateTime lastTime;
        //0.0 to 1.0
        double _animationPosition = 0.0;

        double _animationLength = 0.7;

        //  double _animationFPS = 60.0;

        public double AnimationFPS
        {
            get { return (double)GetValue(AnimationFPSProperty); }
            set { SetValue(AnimationFPSProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyVarIconX.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AnimationFPSProperty =
            DependencyProperty.Register("AnimationFPS", typeof(double), typeof(WorkingAnimIcon), new UIPropertyMetadata(30.0));


        public double WaveWidth
        {
            get { return (double)GetValue(WaveWidthProperty); }
            set { SetValue(WaveWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyVarIconX.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WaveWidthProperty =
            DependencyProperty.Register("WaveWidth", typeof(double), typeof(WorkingAnimIcon), new UIPropertyMetadata(0.8));

        public int CircleCount
        {
            get { return (int)GetValue(CircleCountProperty); }
            set { SetValue(CircleCountProperty, value); }
        }

        public static readonly DependencyProperty CircleCountProperty =
            DependencyProperty.Register("CircleCount", typeof(int), typeof(WorkingAnimIcon), new UIPropertyMetadata(3));

        public double CircleSize
        {
            get { return (double)GetValue(CircleSizeProperty); }
            set { SetValue(CircleSizeProperty, value); }
        }

        public static readonly DependencyProperty CircleSizeProperty =
            DependencyProperty.Register("CircleSize", typeof(double), typeof(WorkingAnimIcon), new UIPropertyMetadata(5.0));


        public double DelayBetweenWaves
        {
            get { return (double)GetValue(DelayBetweenWavesProperty); }
            set { SetValue(DelayBetweenWavesProperty, value); }
        }

        public static readonly DependencyProperty DelayBetweenWavesProperty =
            DependencyProperty.Register("DelayBetweenWaves", typeof(double), typeof(WorkingAnimIcon), new UIPropertyMetadata(1.0));


        List<Ellipse> _circles = new List<Ellipse>();

        public WorkingAnimIcon()
        {
            InitializeComponent();


            this.DataContext = new DesignWorkingAnimIconModel();

            IsVisibleChanged += OnVisibleChanged;

            _animationTimer = new DispatcherTimer(DispatcherPriority.ContextIdle, Dispatcher);
            _animationTimer.Interval = TimeSpan.FromMilliseconds(1000.0 / AnimationFPS);


        }

        private void Start()
        {
            _animationTimer.Tick += OnAnimationTick;
            _animationTimer.Start();
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        private void Stop()
        {
            _animationTimer.Stop();
            _animationTimer.Tick -= OnAnimationTick;
        }

        private static void SetPosition(DependencyObject ellipse, double x, double y)
        {
            ellipse.SetValue(Canvas.LeftProperty, x);
            ellipse.SetValue(Canvas.TopProperty, y);
        }

        private void UpdateAnimTick()
        {
            //double _ellipseMaxSizeX = cvAnim.ActualWidth / _circles.Count;
            //double _ellipseMaxSizeY = cvAnim.ActualHeight / 2.0;

            if (cvAnim.Children.Count == 0)
            {
                for (int i = 0; i < CircleCount; i++)
                {
                    var ellipse = new Ellipse();
                    ellipse.Width = 20;
                    ellipse.Height = 20;
                    ellipse.VerticalAlignment = VerticalAlignment.Center;
                    ellipse.Fill = Brushes.White;
                    _circles.Add(ellipse);
                    cvAnim.Children.Add(ellipse);
                }
            }

            double _ellipseMaxSize = CircleSize;

            var elapsedTime = DateTime.Now - lastTime;
            lastTime = DateTime.Now;

            var totalElapsed = elapsedTime.TotalMilliseconds;
            totalElapsed = Math.Max(Math.Min(totalElapsed, 200.0), 0.0);
            totalElapsed /= _animationLength;

            _animationPosition += totalElapsed / 1400.0;

            if (_animationPosition >= 1.0)
            {
                _animationPosition = _animationPosition - (1.0 + DelayBetweenWaves); //reset animation state to 0
            }

            double centerWavePos = _animationPosition * 2.0;
            for (int circleNo = 0; circleNo < CircleCount; circleNo++)
            {
                double _waveLength = WaveWidth;
                double xPos = circleNo * (1.0 / (_circles.Count - 1));
                _circles[circleNo].Width = _ellipseMaxSize;
                _circles[circleNo].Height = _ellipseMaxSize;

                double phase = (centerWavePos - xPos) * Math.PI / _waveLength;
                if (phase > 0 && phase < Math.PI)
                {
                    double sin = Math.Sin(phase);
                    SetPosition(_circles[circleNo], xPos * (cvAnim.ActualWidth - _ellipseMaxSize), (cvAnim.ActualHeight - _ellipseMaxSize) - sin * sin * (cvAnim.ActualHeight - _ellipseMaxSize));
                }
                else
                {
                    SetPosition(_circles[circleNo], xPos * (cvAnim.ActualWidth - _ellipseMaxSize), cvAnim.ActualHeight - _ellipseMaxSize);
                }
            }
        }

        private void OnAnimationTick(object? sender, EventArgs e)
        {
            UpdateAnimTick();
        }

        private void OnCanvasLoaded(object sender, RoutedEventArgs e)
        {
            UpdateAnimTick();
        }

        /// <summary>
        /// Handles the unloaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnCanvasUnloaded(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var isVisible = (bool)e.NewValue;

            if (isVisible)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }
    }
}
