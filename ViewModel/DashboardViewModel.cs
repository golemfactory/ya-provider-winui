using GolemUI.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.ViewModel
{
    public class DashboardViewModel : INotifyPropertyChanged
    {


        public Dictionary<DashboardPages, DashboardPage> _pages = new Dictionary<DashboardPages, DashboardPage>();

        public DashboardMain DashboardMain { get; set; }
        public DashboardSettings DashboardSettings { get; set; }
        public DashboardAdvancedSettings DashboardAdvancedSettings { get; set; }
        public DashboardSettingsAdv DashboardSettingsAdv { get; set; }
        public DashboardWallet DashboardWallet { get; set; }

        private DashboardPages _selectedPage;

        public DashboardPages LastPage { get; set; }
        public int SelectedPage {
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
                    case DashboardPages.PageDashboardDetails:
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
                if (value == 2)
                {
                    SwitchPage(DashboardPages.PageDashboardSettings);
                }
                if (value == 3)
                {
                    SwitchPage(DashboardPages.PageDashboardWallet);
                }
                OnPropertyChanged("SelectedPage");
            }


    }

        public event PropertyChangedEventHandler PropertyChanged;

        public DashboardViewModel(DashboardMain dashboardMain, DashboardSettings dashboardSettings, DashboardSettingsAdv dashboardSettingsAdv, DashboardWallet dashboardWallet)
        {
            PropertyChanged += OnPropertyChanged;
            _selectedPage = DashboardPages.PageDashboardMain;

            DashboardMain = dashboardMain;
            DashboardSettings = dashboardSettings;
            DashboardSettingsAdv = dashboardSettingsAdv;
            DashboardWallet = dashboardWallet;

            _pages.Add(DashboardPages.PageDashboardMain, new DashboardPage(DashboardMain, DashboardMain.Model));
            _pages.Add(DashboardPages.PageDashboardSettings, new DashboardPage(DashboardSettings, DashboardSettings.ViewModel));
            _pages.Add(DashboardPages.PageDashboardWallet, new DashboardPage(DashboardWallet));
            _pages.Add(DashboardPages.PageDashboardSettingsAdv, new DashboardPage(DashboardSettingsAdv, DashboardSettingsAdv.ViewModel));

            _pages.Values.ToList().ForEach(page => page.PageChangeRequested += PageChangeRequested);
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

            var lastPage = GetPageDescriptorFromPage(_selectedPage);
            lastPage.Unmount();
            lastPage.Hide();

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
