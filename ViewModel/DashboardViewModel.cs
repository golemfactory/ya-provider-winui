using GolemUI.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GolemUI.ViewModel
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        public enum DashboardPages
        {
            PageDashboardNone,
            PageDashboardMain,
            PageDashboardSettings,
            PageDashboardSettingsAdv,
            PageDashboardBenchmark,
            PageDashboardWallet,
            PageDashboardStatistics
        }

        public class DashboardPage
        {
            public UserControl View;
            ISavableLoadableDashboardPage? ViewModel = null;
            public bool ShouldAutoLoad = false;
            public bool ShouldAutoSave = false;
            bool ShouldAnimate = true;
            public DashboardPage(UserControl view)
            {
                View = view;
                view.Visibility = Visibility.Hidden;
                view.Opacity = 0;
            }

            public event PageChangeRequestedEvent? PageChangeRequested;
            public event RequestDarkBackgroundEventHandler? DarkBackgroundRequested;
            public DashboardPage(UserControl view, object viewModel)
            {
                View = view;
                if (viewModel is ISavableLoadableDashboardPage model)
                {
                    ViewModel = model;
                    ShouldAutoLoad = true;
                    ShouldAutoSave = true;
                    view.Visibility = Visibility.Hidden;
                    view.Opacity = 0;
                    model.PageChangeRequested += ViewModel_PageChangeRequested;
                }

                if (viewModel is IDialogInvoker dialogInvoker)
                {
                    dialogInvoker.DarkBackgroundRequested += DialogInvoker_DarkBackgroundRequested;
                }
            }

            private void DialogInvoker_DarkBackgroundRequested(bool shouldBackgroundBeVisible)
            {
                DarkBackgroundRequested?.Invoke(shouldBackgroundBeVisible);
            }


            private void ViewModel_PageChangeRequested(DashboardViewModel.DashboardPages page)
            {
                PageChangeRequested?.Invoke(page);
            }

            public void Unmount()
            {
                if (ShouldAutoSave && ViewModel != null)
                {
                    ViewModel.SaveData();
                }
            }

            public void Mount()
            {
                if (ShouldAutoLoad && ViewModel != null)
                {
                    ViewModel.LoadData();
                }
            }
            public void Show()
            {
                if (ShouldAnimate)
                {
                    View.Visibility = Visibility.Visible;
                    ShowSlowly(TimeSpan.FromMilliseconds(250));
                }
                else
                {
                    View.Visibility = Visibility.Visible;
                    View.Opacity = 1.0f;
                }
            }

            public void Hide()
            {
                if (ShouldAnimate)
                {
                    HideSlowly(TimeSpan.FromMilliseconds(300));
                }
                else
                {
                    View.Opacity = 0.0f;
                    View.Visibility = Visibility.Hidden;
                }
            }
            public void Clear()
            {
                View.Visibility = Visibility.Hidden;
                View.Opacity = 0.0f;
            }
            private void ShowSlowly(TimeSpan duration)
            {

                DoubleAnimation animation = new DoubleAnimation(0.0, 1.0, duration);
                DoubleAnimation animation2 = new DoubleAnimation(0.92, 1.0, duration);

                View.RenderTransform = new ScaleTransform(0.9, 0.9, 0.5, 0.5);
                View.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation2);
                View.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation2);
                View.BeginAnimation(UserControl.OpacityProperty, animation);

            }
            private void HideSlowly(TimeSpan duration)
            {
                DoubleAnimation anim1 = new DoubleAnimation(1, 0, duration);
                anim1.Completed += new EventHandler(delegate (object? s, EventArgs ev)
                {
                    View.Visibility = Visibility.Hidden;
                });
                View.BeginAnimation(UserControl.OpacityProperty, anim1);

                /*DoubleAnimation animation = new DoubleAnimation(1.0, 0.0, duration);
                uc.BeginAnimation(UserControl.OpacityProperty, animation);*/
            }
        }


        public Dictionary<DashboardPages, DashboardPage> _pages = new Dictionary<DashboardPages, DashboardPage>();

        public DashboardMain DashboardMain { get; set; }
        public DashboardSettings DashboardSettings { get; set; }
        public DashboardSettingsAdv DashboardSettingsAdv { get; set; }
        public DashboardWallet DashboardWallet { get; set; }
        public DashboardStatistics DashboardStatistics { get; set; }

        private DashboardPages _selectedPage;

        public DashboardPages LastPage { get; set; }
        public int SelectedPage
        {
            get
            {
                switch (_selectedPage)
                {
                    case DashboardPages.PageDashboardMain:
                        return 1;
                    case DashboardPages.PageDashboardSettings:
                        return 2;
                    case DashboardPages.PageDashboardSettingsAdv:
                        return 2;
                    case DashboardPages.PageDashboardWallet:
                        return 3;
                    case DashboardPages.PageDashboardStatistics:
                        return 4;
                    default:
                        return -1;
                }
            }
            set
            {
                if (value == 1)
                {
                    SwitchPage(DashboardPages.PageDashboardMain);
                }
                else if (value == 2)
                {
                    SwitchPage(DashboardPages.PageDashboardSettings);
                }
                else if (value == 3)
                {
                    SwitchPage(DashboardPages.PageDashboardWallet);
                }
                else if (value == 4)
                {
                    SwitchPage(DashboardPages.PageDashboardStatistics);
                }
                else
                {
                    Debug.Fail("No such page");
                }
                OnPropertyChanged("SelectedPage");
            }
        }

        bool _darkBackgroundVisible = false;
        public bool DarkBackgroundVisible
        {
            get => _darkBackgroundVisible; set
            {
                _darkBackgroundVisible = value;
                OnPropertyChanged("DarkBackgroundVisible");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DashboardViewModel(DashboardStatistics dashboardStatistics, DashboardMain dashboardMain, DashboardSettings dashboardSettings, DashboardSettingsAdv dashboardSettingsAdv, DashboardWallet dashboardWallet)
        {
            PropertyChanged += OnPropertyChanged;

            DashboardMain = dashboardMain;
            DashboardSettings = dashboardSettings;
            DashboardSettingsAdv = dashboardSettingsAdv;
            DashboardWallet = dashboardWallet;
            DashboardStatistics = dashboardStatistics;

            _pages.Add(DashboardPages.PageDashboardMain, new DashboardPage(DashboardMain, DashboardMain.Model));
            _pages.Add(DashboardPages.PageDashboardSettings, new DashboardPage(DashboardSettings, DashboardSettings.ViewModel));
            _pages.Add(DashboardPages.PageDashboardWallet, new DashboardPage(DashboardWallet, DashboardWallet.Model));
            _pages.Add(DashboardPages.PageDashboardSettingsAdv, new DashboardPage(DashboardSettingsAdv, DashboardSettingsAdv.ViewModel));
            _pages.Add(DashboardPages.PageDashboardStatistics, new DashboardPage(DashboardStatistics, DashboardStatistics.ViewModel));

            _pages.Values.ToList().ForEach(page => page.PageChangeRequested += PageChangeRequested);

            _pages.Values.ToList().ForEach(page =>
                page.DarkBackgroundRequested += Page_DarkBackgroundRequested);
        }

        private void Page_DarkBackgroundRequested(bool shouldBackgroundBeVisible)
        {
            DarkBackgroundVisible = shouldBackgroundBeVisible;
        }

        private void OnPropertyChanged(string? propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }


        public void SwitchPage(DashboardPages page)
        {
            if (page == _selectedPage) return;

            _pages.ToList().Where(x => x.Key != _selectedPage && x.Key != page).ToList().ForEach(x => x.Value.Clear());

            if (_selectedPage != DashboardPages.PageDashboardNone)
            {
                var lastPage = GetPageDescriptorFromPage(_selectedPage);
                lastPage.Unmount();
                lastPage.Hide();
            }

            var currentPage = GetPageDescriptorFromPage(page);
            currentPage.Mount();
            currentPage.Show();

            LastPage = _selectedPage;

            _selectedPage = page;
        }

        public DashboardPage GetPageDescriptorFromPage(DashboardPages page)
        {
            if (!_pages.ContainsKey(page))
            {
                throw new Exception(String.Format("Requested page not added to _pages. Page: {0}", (int)page));
            }
            return _pages[page];
        }

        public void SwitchPageBack()
        {
            SwitchPage(LastPage);
        }

        private void PageChangeRequested(DashboardPages page)
        {
            SwitchPage(page);
        }

    }
}
