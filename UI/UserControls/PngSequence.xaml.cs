using GolemUI.Src;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GolemUI
{
    /// <summary>
    /// Interaction logic for PngSequence.xaml
    /// </summary>
    public partial class PngSequence : UserControl
    {
        readonly DispatcherTimer _updateImageTimer;
        int _currentIndex = 0;
        readonly List<BitmapImage> _images = new List<BitmapImage>();

        public PngSequence()
        {
            InitializeComponent();
            _updateImageTimer = new DispatcherTimer(DispatcherPriority.Render);
            _updateImageTimer.Interval = TimeSpan.FromMilliseconds(30.0);
            _updateImageTimer.Tick += new EventHandler(this.updateImageTimer_Tick);

            // this.DataContext = this;
            Loaded += (sender, args) =>
            {
                try
                {
                    Console.WriteLine("loading sequence : " + SequencePrefix + ", trying to load " + TotalFilesCount + " frames");

                    Enumerable.Range(0, TotalFilesCount).ToList().ForEach(index =>
                    {
                        var bitmap = LoadImageFromResources("/UI/Icons/DefaultStyle/png/Sequences/", SequencePrefix, "png", index, LeadingZerosCount);
                        if (bitmap != null)
                        {
                            _images.Add(bitmap);
                        }
                    });
                    if (_images.Count > 0)
                    {
                        this.image.Source = _images[0];
                    }
                    else
                    {
                        Console.WriteLine("no frames loaded");
                    }


                    if (Active)
                    {
                        _updateImageTimer.Start();
                        image.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        _updateImageTimer.Stop();
                        image.Visibility = Visibility.Hidden;
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("PngSequenceException:\n" + exception.ToString());
                }
            };

        }



        BitmapImage? LoadImageFromResources(String path, String prefix, String extension, int index, int leadingZerosCount)
        {
            String filePath = path + prefix + index.ToString("d" + leadingZerosCount) + "." + extension;
            BitmapImage? result = null;
            try
            {
                result = new BitmapImage(new Uri("pack://application:,,,/ThorgMiner;component" + filePath));
            }
            catch
            {
                return null;
            }

            return result;
        }

        public static readonly DependencyProperty _sequencePrefix = DependencyProperty.Register("SequencePrefix", typeof(string), typeof(PngSequence));
        public static readonly DependencyProperty _fileExtension = DependencyProperty.Register("FileExtension", typeof(string), typeof(PngSequence));
        public static readonly DependencyProperty _totalFilesCount = DependencyProperty.Register("TotalFiles", typeof(int), typeof(PngSequence));
        public static readonly DependencyProperty _leadingZerosCount = DependencyProperty.Register("LeadingZerosCount", typeof(int), typeof(PngSequence));

        public static readonly DependencyProperty _active = DependencyProperty.Register("Active", typeof(bool), typeof(PngSequence), new PropertyMetadata(false, ActiveChangedCallback));
        private static void ActiveChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                Console.WriteLine("Dependency property is now " + e.NewValue);

                PngSequence p = (PngSequence)d;

                if ((bool)e.NewValue)
                {
                    p._updateImageTimer.Start();
                    p.image.Visibility = Visibility.Visible;
                }
                else
                {
                    p._updateImageTimer.Stop();
                    p.image.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception er)
            {
                Console.WriteLine("err while setting PngSequence Active dependency property" + er.ToString());


            }
        }
        public bool Active
        {
            get
            {
                return (bool)GetValue(_active);
            }
            set
            {
                SetValue(_active, value);

            }
        }
        public int LeadingZerosCount
        {
            get
            {
                return (int)GetValue(_leadingZerosCount);
            }
            set
            {
                SetValue(_leadingZerosCount, value);

            }
        }
        public string FileExtension
        {
            get
            {
                return (string)GetValue(_fileExtension);
            }
            set
            {
                SetValue(_fileExtension, value);

            }
        }
        public string SequencePrefix
        {
            get
            {
                return (string)GetValue(_sequencePrefix);
            }
            set
            {
                SetValue(_sequencePrefix, value);

            }
        }
        public int TotalFilesCount
        {
            get
            {
                return (int)GetValue(_totalFilesCount);
            }
            set
            {
                SetValue(_totalFilesCount, value);

            }
        }

        private void updateImageTimer_Tick(object sender, EventArgs e)
        {
            if (_images.Count == 0)
            {
                return;
            }

            this.image.Source = this._images[this._currentIndex];

            _currentIndex = (_currentIndex + 1) % _images.Count;
        }

    }

}
