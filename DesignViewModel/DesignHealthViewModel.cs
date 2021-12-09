using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Miners;
using GolemUI.Miners.TRex;
using GolemUI.Model;
using Newtonsoft.Json;

namespace GolemUI.DesignViewModel
{
    public class DesignHealthViewModel
    {
        
        public ObservableCollection<MetricsEntry> Metrics { get; } = new ObservableCollection<MetricsEntry>(new MetricsEntry[]
        {
            new MetricsEntry("test", "test")
        });

        public bool YagnaHealthVisible => true;
    }
}
