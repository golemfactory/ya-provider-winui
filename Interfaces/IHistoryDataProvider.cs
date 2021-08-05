﻿using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Interfaces
{
    public interface IHistoryDataProvider : INotifyPropertyChanged
    {
        PrettyChartData GetMegaHashHistory();

        public string? ActiveActivityID { get; set; }

    }
}

