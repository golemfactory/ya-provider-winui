using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Model
{
    public class NavBar : INotifyPropertyChanged
    {
        [Bindable(true)]
        public int Step
        {
            get => _step;
            set
            {
                _step = value;
                UpdateItems();
                OnPropertyChanged();
            }
        }

        public ObservableCollection<NavBarItem> Items
        {
            get => _items;
            set
            {
                _items = new ObservableCollection<NavBarItem>(value);
                UpdateItems();
                OnPropertyChanged();
            }
        }

        private void UpdateItems()
        {

        }

        public NavBar()
        {
            _items = new ObservableCollection<NavBarItem>();
            _items.CollectionChanged += OnItemsChanged;
        }

        private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateItems();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private ObservableCollection<NavBarItem> _items;

        private int _step;
    }
}
