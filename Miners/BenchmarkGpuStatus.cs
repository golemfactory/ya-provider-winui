using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GolemUI.Miners
{
    public enum GpuErrorType
    {
        NotEnoughMemory,
        None
    }

    public class BenchmarkGpuStatus : ICloneable, INotifyPropertyChanged
    {


        public GpuErrorType GpuErrorType { get; set; } = GpuErrorType.None;
        public int GpuNo { get; set; }
        public string? GpuName { get; set; }
        public int? PciExpressLane { get; set; }
        public bool OutOfMemory { get; set; }
        public bool GPUNotFound { get; set; }
        public float BenchmarkSpeed { get; set; }
        public bool LowMemoryMode { get; set; } = false;

        [JsonIgnore]
        public bool BenchmarkSpeedForDifferentThrottling
        {
            get => (BenchmarkSpeed > 0 && BenchmarkDoneForThrottlingLevel != PhoenixPerformanceThrottling);
        }

        public int BenchmarkDoneForThrottlingLevel { get; set; }

        public bool IsDagCreating { get; set; }

        private bool _isEnabledByUser;
        public bool IsEnabledByUser
        {
            get => _isEnabledByUser;
            set
            {

                if (IsEnabledByUser != value)
                {
                    _isEnabledByUser = value;
                    NotifyChange(nameof(IsEnabledByUser));
                }
            }
        }

        public float DagProgress { get; set; }
        public string? GPUVendor { get; set; }
        public string? GPUDetails { get; set; }
        public string? GPUError { get; set; }

        //steps for view presentation (only one state is possible at the time)
        [JsonIgnore]
        public bool IsPreInitialization { get; set; }
        [JsonIgnore]
        public bool IsInitialization { get; set; }
        [JsonIgnore]
        public bool IsEstimation { get; set; }

        public bool IsFinished { get; set; }

        private void NotifyChange([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        //temporary parameters to be removed
        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                if (LowMemoryMode)
                {
                    return GpuNo + ". (4GB mode) " + GpuName;
                }
                return GpuNo + ". " + GpuName;
            }
        }

        [JsonIgnore]
        public string PhoenixPerformanceThrottlingDebug => "(debug: " + PhoenixPerformanceThrottling + ") ";

        [JsonIgnore]
        public int _phoenixPerformanceThrottling { get; set; } = (int)PerformanceThrottlingEnumConverter.Default;
        public int PhoenixPerformanceThrottling
        {
            get { return _phoenixPerformanceThrottling; }
            set
            {
                _phoenixPerformanceThrottling = value;
                NotifyChange("PhoenixPerformanceThrottling");
                NotifyChange("PhoenixPerformanceThrottlingDebug");
            }
        }

        [JsonIgnore]
        public PerformanceThrottlingEnum SelectedMiningMode
        {
            get
            {
                return PerformanceThrottlingEnumConverter.FromInt(PhoenixPerformanceThrottling);
            }
            set
            {
                if (PhoenixPerformanceThrottling != (int)value)
                {
                    PhoenixPerformanceThrottling = (int)value;
                    NotifyChange(nameof(SelectedMiningMode));
                    NotifyChange(nameof(BenchmarkSpeed));
                    NotifyChange(nameof(BenchmarkSpeedForDifferentThrottling));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void SetStepInitialization()
        {
            IsPreInitialization = false;
            IsInitialization = true;
            IsEstimation = false;
            IsFinished = false;
            NotifyChange("");
        }

        public void SetStepEstimation()
        {
            IsPreInitialization = false;
            IsInitialization = false;
            IsEstimation = true;
            IsFinished = false;
            NotifyChange("");
        }

        public void SetStepFinished()
        {
            IsPreInitialization = false;
            IsInitialization = false;
            IsEstimation = false;
            IsFinished = true;
            NotifyChange("");
        }

        public BenchmarkGpuStatus()
        {
        }

        public BenchmarkGpuStatus(int gpuNo, bool isEnabledByUser, int phoenixPerformanceThrottling)
        {
            this.IsEnabledByUser = isEnabledByUser;
            this.PhoenixPerformanceThrottling = phoenixPerformanceThrottling;
            this.GpuNo = gpuNo;
            this.IsPreInitialization = true;
        }

        public object Clone()
        {
            BenchmarkGpuStatus s = new BenchmarkGpuStatus(this.GpuNo, this.IsEnabledByUser, this.PhoenixPerformanceThrottling);
            s.GpuName = this.GpuName;
            s.OutOfMemory = this.OutOfMemory;
            s.BenchmarkDoneForThrottlingLevel = this.BenchmarkDoneForThrottlingLevel;
            s.GPUNotFound = this.GPUNotFound;
            s.BenchmarkSpeed = this.BenchmarkSpeed;
            s.IsDagCreating = this.IsDagCreating;
            s.DagProgress = this.DagProgress;
            s.GPUVendor = this.GPUVendor;
            s.GPUDetails = this.GPUDetails;
            s.PciExpressLane = this.PciExpressLane;
            s.IsEnabledByUser = this.IsEnabledByUser;
            s.PhoenixPerformanceThrottling = this.PhoenixPerformanceThrottling;
            s.GPUError = this.GPUError;
            s.IsPreInitialization = this.IsPreInitialization;
            s.IsInitialization = this.IsInitialization;
            s.IsEstimation = this.IsEstimation;
            s.IsFinished = this.IsFinished;
            s.GpuErrorType = this.GpuErrorType;
            s.LowMemoryMode = this.LowMemoryMode;
            return s;
        }

        [JsonIgnore]
        public bool IsReadyForMining => (IsDagFinished() && BenchmarkSpeed > 0.5 && String.IsNullOrEmpty(GPUError));

        [JsonIgnore]
        public bool IsOperationStopped => (OutOfMemory || GPUNotFound || !String.IsNullOrEmpty(GPUError) || !IsEnabledByUser);

        public bool IsDagFinished()
        {
            if (DagProgress < 1.0f)
            {
                return false;
            }
            if (IsDagCreating)
            {
                return false;
            }
            return true;
        }


    }

}
