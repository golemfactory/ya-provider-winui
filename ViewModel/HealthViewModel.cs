using GolemUI.Interfaces;
using GolemUI.Model;
using GolemUI.UI.Charts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using GolemUI.Command;
using Newtonsoft.Json.Linq;

namespace GolemUI.ViewModel
{
    public class HealthViewModel : INotifyPropertyChanged, ISavableLoadableDashboardPage
    {
        private readonly DispatcherTimer _timer;
        YagnaSrv _srv;

        public ObservableCollection<MetricsEntry> Metrics { get; } = new ObservableCollection<MetricsEntry>(new MetricsEntry[]
        {
            new MetricsEntry("test", "test")
        });

        public bool YagnaHealthVisible { get; set; } = false;

        private readonly ProcessController _processController;

        public HealthViewModel(YagnaSrv srv, IProcessController processController)
        {
            _srv = srv;
            _processController = processController as ProcessController; //temporary hack to ease development. TODO fix
 
        }

        private void HealthViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void HistoryDataProvider_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyChange("ChartData1");
        }

        public PrettyChartDataHistogram ChartData1 { get; set; }


        public event PageChangeRequestedEvent? PageChangeRequested;
        public event PropertyChangedEventHandler PropertyChanged;

        Random _rand = new Random();

        public void LoadData()
        {
        }

        public void SaveData()
        {
            //throw new NotImplementedException();
        }

        //public PrettyChartData RandomData()
        // {
        /*
        var chartData = new PrettyChartData();
        chartData.NoAnimate = false;

        var binData = chartData.BinData;
        for (int i = 0; i < 10 + _rand.Next(10); i++)
        {
            var bin = new PrettyChartBinEntry();
            bin.Value = _rand.NextDouble() * 30;
            bin.Label = $"{i}";

            chartData.BinData.BinEntries.Add(bin);
        }


        return chartData;
        */

        // }

        public async void PerformHealthCheck()
        {
            Metrics.Clear();
            var healthStatus = await _srv.ExecAsync<HealthStatusResponse>("--json", "misc", "check");

            //
            //
            if (healthStatus?.success != true)
            {

            }
            if (healthStatus?.value?.isNetConnected ?? false)
            {
                var jObject = healthStatus?.value?.metrics as JObject;
                foreach (var property in jObject.Properties())
                {
                    Metrics.Add(new MetricsEntry(property.Name, property.Value.ToString()));
                    //Console.WriteLine("Property: " + property.Name + ": " + property.Value);
                    
                }
                NotifyChange("Metrics");
            }
            //if (healthStatus.success == true)
            //{

            //}
            //Console.Error(healthStatus.error.)

            YagnaHealthVisible = true;
            NotifyChange("YagnaHealthVisible");

            using WebClient webClient = new WebClient();
            //token.Register(webClient.CancelAsync);

            var appKey = await _processController.GetAppKey();
            webClient.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {appKey}");
            webClient.BaseAddress = _processController.ServerUri;

            
            var stream = await _processController.GetOffers() ?? "";
            Console.WriteLine(stream);

        }

        private void NotifyChange([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
