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
using GolemUI.Model;
using GolemUI.Utils;
using System.Windows;

namespace GolemUI.ViewModel
{
    public partial class DashboardViewModel : INotifyPropertyChanged
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

        private readonly IRemoteSettingsProvider _remoteSettingsProvider;

        public DashboardViewModel(DashboardSettings dashboardSettings, DashboardMain dashboardMain, DashboardSettingsAdv dashboardSettingsAdv, DashboardWallet dashboardWallet, IRemoteSettingsProvider remoteSettingsProvider
#if STATISTICS_ENABLED
                ,DashboardStatistics dashboardStatistics
#endif
            )
        {
            _remoteSettingsProvider = remoteSettingsProvider;
            _remoteSettingsProvider.OnRemoteSettingsUpdated += RemoteSettingsUpdatedEventHandler;

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
            OnPropertyChanged(nameof(SelectedPage));
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

        public static FileVersionInfo GetCurrentVersionInfo()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi;
        }

        public string CurrentVersionString
        {
            get => GetCurrentVersionInfo().FileVersion;
        }

        public string VersionInfo
        {
            get
            {
                string version = CurrentVersionString;
                if (!_canRun)
                {
                    return $"Critical update: {_latestVersion}";
                }
                if (_upToDate == true)
                {
                    return $"Version: {version}";
                }
                if (_upToDate == false)
                {
                    return $"Update available: {_latestVersion}";
                }
                return $"Version: {version}";
            }
        }

        private bool? _upToDate = null;
        private bool _canRun = true;
        public string _latestVersion = "unknown";
        public string _changelog = "No changes found";
        public string _latestVersionCodename = "";
        public string _latestVersionDownloadLink = "";
        private bool _firstUpdate = true;


        public void RemoteSettingsUpdatedEventHandler(RemoteSettings rs)
        {
            bool forceUpdate = false;
            bool suggestUpdate = false;

            string currentVersionString = GetCurrentVersionInfo().FileVersion;
            if (VersionUtil.CompareVersions(rs.LastSupportedVersion, currentVersionString, out int cr1))
            {
                //Last supported version greater than current version:
                if (cr1 > 0)
                {
                    forceUpdate = true;
                }
            }
            if (VersionUtil.CompareVersions(rs.LatestVersion, currentVersionString, out int cr2))
            {
                //Latest version greater than current version:
                if (cr2 > 0)
                {
                    suggestUpdate = true;
                }
            }

            if (forceUpdate && suggestUpdate)
            {
                //force update user    
                _upToDate = false;
                _canRun = false;
            }
            else if (suggestUpdate)
            {
                //suggest update user
                _upToDate = false;
                _canRun = true;
            }
            else
            {
                //Version check failed or the version the same
                _upToDate = true;
                _canRun = true;
            }
            _latestVersion = rs.LatestVersion ?? "unknown";
            _changelog = rs.ChangeLog ?? "No changes found";
            _latestVersionCodename = rs.LatestVersionCodename ?? "";
            _latestVersionDownloadLink = rs.LatestVersionDownloadLink ?? "";

            if (_firstUpdate && _canRun == false)
            {
                _firstUpdate = false;
                App app = (App)Application.Current;
                app.ShowUpdateDialog();
            }
            _firstUpdate = false;

            OnPropertyChanged("VersionInfo");
        }

        public void ShowVersionDialog(Window owner)
        {
            List<string> changelog = new List<string>();
            foreach (string change in _changelog.Split('|'))
            {
                string changeNew = change.Trim();
                if (!String.IsNullOrEmpty(changeNew))
                {
                    changelog.Add(changeNew);
                }
            }
            var dlg = new UI.Dialogs.DlgUpdateApp(new ViewModel.Dialogs.DlgUpdateAppViewModel(_latestVersionDownloadLink, CurrentVersionString, _latestVersion, _latestVersionCodename, changelog, !_canRun));
            dlg.Owner = owner;
            dlg?.ShowDialog();
        }
    }
}
