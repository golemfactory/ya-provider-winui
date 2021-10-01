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
        public double? EstimatedEarningsPerSecondGLM { get; }
        public double? EstimatedEarningsPerSecondUSD { get; }

        public string EstimatedEarningsMessage { get; }
    }
}
