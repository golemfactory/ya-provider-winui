using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Model;

namespace GolemUI.Interfaces
{
    public interface IStatusProvider : INotifyPropertyChanged
    {
        DateTime? LastUpdate { get; }

        ICollection<ActivityState> Activities { get; }
    }
}
