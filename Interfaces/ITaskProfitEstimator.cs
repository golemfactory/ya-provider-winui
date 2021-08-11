using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Interfaces
{
    public interface ITaskProfitEstimator : INotifyPropertyChanged
    {
        public double? EstimatedEarningsPerSecond { get; }

        public string EstimatedEarningsMessage { get; }
    }
}
