using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GolemUI.ViewModel
{
    public class UsageDescriptionViewModel: DependencyObject, INotifyPropertyChanged
    {
        private  string _description;
        private  int _total;
        private  int _current;
        private UsageDescription _parrent;
        public UsageDescriptionViewModel(UsageDescription parrent)
        {
            _parrent = parrent;
        }

        
        public string Description => _parrent.Description;
        public int Total => _parrent.Total;
        public int Current  => _parrent.Current;



        public void NotifyChange(string? propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
