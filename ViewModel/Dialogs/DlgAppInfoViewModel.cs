using GolemUI.Interfaces;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GolemUI.ViewModel.Dialogs
{
    public class DlgAppInfoViewModel : INotifyPropertyChanged
    {

        public string AppVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string? NodeName => _provider?.Config?.NodeName;
        private readonly IProviderConfig _provider;
        public DlgAppInfoViewModel(IProviderConfig provider)
        {

            _provider = provider;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


    }
}
