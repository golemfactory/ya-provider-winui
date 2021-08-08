using GolemUI.Interfaces;
using GolemUI.UI.CustomControls;
using GolemUI.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.ViewModel
{
    public partial class DashboardViewModel : INotifyPropertyChanged
    {
        public DashboardViewModel(DashboardMain dashboardMain, DashboardSettings dashboardSettings, DashboardSettingsAdv dashboardSettingsAdv, DashboardWallet dashboardWallet)
        {

            PropertyChanged += OnPropertyChanged;

            DashboardMain = dashboardMain;
            DashboardSettings = dashboardSettings;
            DashboardSettingsAdv = dashboardSettingsAdv;
            DashboardWallet = dashboardWallet;

            _pages.Add(DashboardPages.PageDashboardMain, new DashboardPage(DashboardMain, DashboardMain.Model));
            _pages.Add(DashboardPages.PageDashboardSettings, new DashboardPage(DashboardSettings, DashboardSettings.ViewModel));
            _pages.Add(DashboardPages.PageDashboardWallet, new DashboardPage(DashboardWallet, DashboardWallet.Model));
            _pages.Add(DashboardPages.PageDashboardSettingsAdv, new DashboardPage(DashboardSettingsAdv, DashboardSettingsAdv.ViewModel));

            _pages.Values.ToList().ForEach(page => page.PageChangeRequested += PageChangeRequested);

            _pages.Values.ToList().ForEach(page =>
                page.DarkBackgroundRequested += Page_DarkBackgroundRequested);
        }
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


        public Dictionary<DashboardPages, DashboardPage> _pages = new();

        public DashboardMain DashboardMain { get; set; }
        public DashboardSettings DashboardSettings { get; set; }
        public DashboardSettingsAdv DashboardSettingsAdv { get; set; }
        public DashboardWallet DashboardWallet { get; set; }
#if STATISTICS_ENABLED
        public DashboardStatistics DashboardStatistics { get; set; }
#endif

        private DashboardPages _selectedPage;

        public DashboardPages LastPage { get; set; }
        public int SelectedPage
        {
            get
            {
                return _selectedPage switch
                {
                    DashboardPages.PageDashboardMain => 1,
                    DashboardPages.PageDashboardSettings => 2,
                    DashboardPages.PageDashboardSettingsAdv => 2,
                    DashboardPages.PageDashboardWallet => 3,
                    DashboardPages.PageDashboardStatistics => 4,
                    _ => -1,
                };
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
                OnPropertyChanged(nameof(SelectedPage));
            }
        }

        bool _darkBackgroundVisible = false;
        public bool DarkBackgroundVisible
        {
            get => _darkBackgroundVisible; set
            {
                _darkBackgroundVisible = value;
                OnPropertyChanged(nameof(DarkBackgroundVisible));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DashboardViewModel(DashboardSettings dashboardSettings, DashboardMain dashboardMain, DashboardSettingsAdv dashboardSettingsAdv, DashboardWallet dashboardWallet
#if STATISTICS_ENABLED
                ,DashboardStatistics dashboardStatistics
#endif
            )
        {
            PropertyChanged += OnPropertyChanged;

            DashboardMain = dashboardMain;
#if STATISTICS_ENABLED
            DashboardSettings = dashboardSettings;
#endif
            DashboardSettingsAdv = dashboardSettingsAdv;
            DashboardWallet = dashboardWallet;
            DashboardSettings = dashboardSettings;

            _pages.Add(DashboardPages.PageDashboardMain, new DashboardPage(DashboardMain, DashboardMain.Model));
            _pages.Add(DashboardPages.PageDashboardSettings, new DashboardPage(DashboardSettings, DashboardSettings.ViewModel));
            _pages.Add(DashboardPages.PageDashboardWallet, new DashboardPage(DashboardWallet, DashboardWallet.Model));
            _pages.Add(DashboardPages.PageDashboardSettingsAdv, new DashboardPage(DashboardSettingsAdv, DashboardSettingsAdv.ViewModel));

#if STATISTICS_ENABLED
            _pages.Add(DashboardPages.PageDashboardStatistics, new DashboardPage(DashboardStatistics, DashboardStatistics.ViewModel));
#endif

            _pages.Values.ToList().ForEach(page => page.PageChangeRequested += PageChangeRequested);
            _pages.Values.ToList().ForEach(page => page.DarkBackgroundRequested += Page_DarkBackgroundRequested);
        }

        private void Page_DarkBackgroundRequested(bool shouldBackgroundBeVisible)
        {
            DarkBackgroundVisible = shouldBackgroundBeVisible;
        }

        private void OnPropertyChanged(string? propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
