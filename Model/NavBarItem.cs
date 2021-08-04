using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GolemUI.Model
{
    public class NavBarItems : ObservableCollection<NavBarItem>
    {
        public NavBarItems() : base()
        {

        }

        public void UpdateItems(int step)
        {
            int prevStepIndex = -1;
            var _items = this;
            for (var i = 0; i < _items.Count; ++i)
            {
                var item = _items[i];
                item.Index = i + 1;
                item.IsLast = (i + 1) == _items.Count;
                var stepIndex = item.StepIndex ?? prevStepIndex + 1;
                var nextStepIndex = ((i + 1) < _items.Count ? _items[i + 1].StepIndex : null) ?? stepIndex + 1;

                if (stepIndex <= step && step < nextStepIndex)
                {
                    item.Status = NavBarItem.ItemStatus.Active;
                }
                else if (stepIndex < step)
                {
                    item.Status = NavBarItem.ItemStatus.Realized;
                }
                else
                {
                    item.Status = NavBarItem.ItemStatus.Inactive;
                }
                prevStepIndex = stepIndex;
            }
        }
    }

    public class NavBarItem : INotifyPropertyChanged
    {
        public enum ItemStatus
        {
            Inactive = 0,
            Realized = 1,
            Active = 2
        }

        private int? _index;
        public int? Index
        {
            get => _index;
            set
            {
                _index = value;
                OnPropertyChanged();
            }
        }

        public string? StepName { get; set; }

        public int? StepIndex { get; set; }

        public bool IsLast { get; set; }

        private ItemStatus _status;

        public SolidColorBrush HoverColor => _status == ItemStatus.Realized ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0FB2AB")) : new SolidColorBrush(Colors.Gray);
        public ItemStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
                OnPropertyChanged("Status");
                OnPropertyChanged("HoverColor");
            }
        }


        public NavBarItem()
        {
            _status = ItemStatus.Inactive;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
