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
        private DashboardPages _selectedPage;
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
                    _selectedPage = DashboardPages.PageDashboardMain;
                }
                if (value == 2)
                {
                    _selectedPage = DashboardPages.PageDashboardSettings;
                }
                if (value == 3)
                {
                    _selectedPage = DashboardPages.PageDashboardWallet;
                }
                OnPropertyChanged("SelectedPage");
            } 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DashboardViewModel()
        {
            PropertyChanged += OnPropertyChanged;
            _selectedPage = DashboardPages.PageDashboardMain;
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

    }
}
