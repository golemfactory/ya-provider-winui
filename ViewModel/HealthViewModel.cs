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
using System.Windows.Forms;
using System.Windows.Threading;
using GolemUI.Command;
using Newtonsoft.Json.Linq;
using DataGrid = System.Windows.Controls.DataGrid;

namespace GolemUI.ViewModel
{
    public class HealthViewModel : INotifyPropertyChanged, ISavableLoadableDashboardPage
    {
        private readonly YagnaSrv _srv;

        public ObservableCollection<MetricsEntry> Metrics { get; } = new ObservableCollection<MetricsEntry>(new MetricsEntry[]
        {
            new MetricsEntry("test", "test")
        });

        public ObservableCollection<ProviderOffer> Offers { get; set; } = new ObservableCollection<ProviderOffer>();



        private bool _yagnaHealthVisible = false;
        public bool YagnaHealthVisible 
        { 
            get => _yagnaHealthVisible;
            set
            {
                _yagnaHealthVisible = value;
                NotifyChange();
            }
        }

        private string _yagnaConnectionStatus = "";
        public string YagnaConnectionStatus
        {
            get => _yagnaConnectionStatus;
            set
            {
                _yagnaConnectionStatus = value;
                NotifyChange();
            }
        }

        private bool _providerHealthVisible = false;

        public bool ProviderHealthVisible
        {
            get => _providerHealthVisible;
            set
            {
                _providerHealthVisible = value;
                NotifyChange();
            }
        }

        private ProviderOffer? _selectedProviderOffer;

        public ProviderOffer? SelectedProviderOffer
        {
            get => _selectedProviderOffer;
            set
            {
                _selectedProviderOffer = value;
                NotifyChange();
            }
        }


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

        

        public event PageChangeRequestedEvent? PageChangeRequested;
        public event PropertyChangedEventHandler? PropertyChanged;

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

        public async Task PerformYagnaHealthCheck()
        {
            YagnaHealthVisible = false;
            ProviderHealthVisible = false;

            Metrics.Clear();
            var healthStatus = await _srv.ExecAsync<HealthStatusResponse>("--json", "misc", "check");

            //
            //
            if (healthStatus?.success != true)
            {

            }

            string connectionStatus = "";
            if (healthStatus == null)
            {
                YagnaConnectionStatus = "Error";
                YagnaHealthVisible = true;
                return;
            }
            
            if (healthStatus.value == null)
            {
                YagnaConnectionStatus = healthStatus.error ?? "Unkown problem when getting yagna status";
                YagnaHealthVisible = true;
                return;
            }

            if (healthStatus.value.isNetConnected != null && healthStatus.value.isNetConnected == false)
            {
                YagnaConnectionStatus = "Yagna is not connected to the router";
                YagnaHealthVisible = true;
                return;
            }

            if (healthStatus.value.isNetConnected ?? false)
            {
                var jObject = healthStatus?.value?.metrics as JObject;
                foreach (var property in jObject.Properties())
                {
                    Metrics.Add(new MetricsEntry(property.Name, property.Value.ToString()));
                    //Console.WriteLine("Property: " + property.Name + ": " + property.Value);
                    
                }
                YagnaHealthVisible = true;

                connectionStatus = "Yagna successfully connected to the router";
                NotifyChange("Metrics");
            }

                //if (healthStatus.success == true)
                //{

                //}
                //Console.Error(healthStatus.error.)

            
            using WebClient webClient = new WebClient();
            //token.Register(webClient.CancelAsync);

            var appKey = await _processController.GetAppKey();
            webClient.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {appKey}");
            webClient.BaseAddress = _processController.ServerUri;

            

            YagnaConnectionStatus = connectionStatus;
        }

        public async Task PerformProviderHealthCheck(DataGrid SelectedProviderProperties)
        {
            YagnaHealthVisible = false;
            ProviderHealthVisible = false;

            using WebClient webClient = new WebClient();
            //token.Register(webClient.CancelAsync);

            var appKey = await _processController.GetAppKey();
            webClient.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {appKey}");
            webClient.BaseAddress = _processController.ServerUri;


            var providerOffers = await _processController.GetOffers();
            if (providerOffers != null)
            {
                Offers.Clear();
                foreach (var providerOffer in providerOffers)
                {
                    Console.WriteLine(providerOffer.providerId);

                    SelectedProviderOffer = providerOffer;
                    Offers.Add(providerOffer);
                }


            }



            ProviderHealthVisible = true;


        }


        private void NotifyChange([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
