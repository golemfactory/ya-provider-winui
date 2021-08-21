using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BetaMiner.Model;

namespace BetaMiner.Interfaces
{
    public interface IStatusProvider : INotifyPropertyChanged
    {
        DateTime? LastUpdate { get; }

        ICollection<ActivityState> Activities { get; }
    }
}
