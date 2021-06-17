using GolemUI.Claymore;
using GolemUI.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GolemUI.Command
{


    public class ClaymoreBenchmark
    {

        private static Mutex mut = new Mutex();

        public LogLineHandler? LineHandler { get; set; }

        string _claymore_working_dir = @"plugins\claymore";
        string _claymore_exe_path = @"plugins\claymore\EthDcrMiner64.exe";

        public string BenchmarkError = "";

        public volatile bool GPUNotFound;
        public volatile bool OutOfMemory;
        public volatile bool BenchmarkFinished;
        public volatile float BenchmarkProgress;
        public volatile float BenchmarkSpeed;

        private object _sync = new object();

        public string? GPUVendor { get; set; }

        private string? _unsafeGpuDetails;

        //private ClaymoreLiveStatus _liveStatus = new ClaymoreLiveStatus();
        private ClaymoreParser _claymoreParser = new ClaymoreParser(isBenchmark:true);
        public ClaymoreParser ClaymoreParser { get { return _claymoreParser; } }

        public string? GPUDetails
        {
            get {
                lock (_sync)
                {
                    //return copy to be more thread safe
                    if (_unsafeGpuDetails == null)
                    {
                        return null;
                    }
                    return new String(_unsafeGpuDetails);
                }
            }
            set
            {
                lock (_sync)
                {
                    _unsafeGpuDetails = value;
                }
            }
        }



        Process? _claymoreProcess;

        int? _gpuNo;
        public ClaymoreBenchmark(int? gpuNo)
        {
            _gpuNo = gpuNo;

        }

        public void Stop()
        {
            if (_claymoreProcess != null)
            {
                _claymoreProcess.Kill(entireProcessTree: true);
                _claymoreProcess = null;
            }
        }

        public bool RunBenchmark()
        {
            BenchmarkError = "";
            BenchmarkFinished = false;
            GPUNotFound = false;

            this.BenchmarkProgress = 0.0f;

            var startInfo = new ProcessStartInfo
            {
                FileName = this._claymore_exe_path,
                WorkingDirectory = this._claymore_working_dir,
                //Arguments = null,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            

            List<string> arguments = new List<string>();

            //Enable benchmark mode:
            arguments.Add("-benchmark");
            //Set GPU number to test:
            if (this._gpuNo != null && this._gpuNo.ToString() != null)
            {
                string? s = this._gpuNo.ToString();
                if (s != null)
                {
                    arguments.AddRange(new string[] { "-di", s});
                }
            }

            foreach (var arg in arguments)
            {
                if (arg == null)
                {
                    throw new ArgumentNullException();
                }
                startInfo.ArgumentList.Add(arg);
            }

            _claymoreProcess = new Process
            {
                StartInfo = startInfo
            };

            try
            {
                _claymoreProcess.Start();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                BenchmarkError = $"Process {this._claymore_exe_path} cannot be run, check antivirus settings";
                return false;
            }
            _claymoreProcess.OutputDataReceived += OnOutputDataRecv;
            _claymoreProcess.ErrorDataReceived += OnErrorDataRecv;
            _claymoreProcess.BeginErrorReadLine();
            _claymoreProcess.BeginOutputReadLine();

            var t = new Thread(() =>
            {
                while (!_claymoreProcess.WaitForExit(300))
                {
                    //do stuff waiting for claymore process
                }
                this.BenchmarkFinished = true;
            });
            this.BenchmarkProgress = 0.1f;

            return true;
        }

        void OnOutputDataRecv(object sender, DataReceivedEventArgs e)
        {
            string? lineText = e.Data;
            //output contains spelling error avaiable instead of available, checking for boths:
            if (lineText == null)
                return;

            _claymoreParser.ParseLine(lineText);

            /*
            if (lineText.Contains("No avaiable GPUs for mining", StringComparison.InvariantCultureIgnoreCase) 
                || lineText.Contains("No avaiable GPUs for mining", StringComparison.InvariantCultureIgnoreCase))
            {
                //there should be no need of closing claymore process. It should close automatically.
                //_claymoreProcess.Kill();

                GPUNotFound = true;
                BenchmarkFinished = true;
            }



            //parse once only at the start of the benchmark
            if (this._unsafeGpuDetails == null && lineText.StartsWith("GPU1:", StringComparison.InvariantCultureIgnoreCase))
            {
                


                bool nVidiaGpuFound = false;
                bool amdGpuFound = false;
                if (lineText.Contains("NVIDIA", StringComparison.InvariantCultureIgnoreCase) ||
                    lineText.Contains("GeForce", StringComparison.InvariantCultureIgnoreCase)
                    )
                {
                    this.GPUVendor = "nVidia";
                    nVidiaGpuFound = true;
                }
                if (nVidiaGpuFound && lineText.Contains(":"))
                {
                    //todo - what happens when details contains :
                    this.GPUDetails = lineText.Split(":")[1].Trim();
                }
                if (lineText.Contains("RADEON", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.GPUVendor = "AMD";
                    amdGpuFound = true;
                }
                if ((nVidiaGpuFound || amdGpuFound) && lineText.Contains(":"))
                {
                    //todo - what happens when details contains :
                    this.GPUDetails = lineText.Split(":")[1].Trim();
                }
            }

            if (lineText.StartsWith("GPU1: Allocating DAG"))
            {
                this.BenchmarkProgress = 0.2f;
            }
            if (lineText.StartsWith("GPU1: Generating DAG"))
            {
                this.BenchmarkProgress = 0.25f;
            }
            if (lineText.StartsWith("GPU1: DAG") )
            {
                var splits = lineText.Split(" ");

                foreach (var split in splits)
                {
                    if (split.Contains("%"))
                    {
                        string percentDag = split.Trim(new char[] { ' ', '%' });
                        double res = 0;
                        if (double.TryParse(percentDag, out res))
                        {
                            this.BenchmarkProgress = 0.3f + (float)((res / 100.0) * (0.8 - 0.4));
                        }
                        else
                        {
                            //handle error somehow??
                        }
                    }
                }
            }
            if (lineText.StartsWith("Eth speed", StringComparison.InvariantCultureIgnoreCase))
            {
                if (this.BenchmarkProgress < 0.8f)
                {
                    this.BenchmarkProgress = 0.8f;
                }
                var splits = lineText.Split(" ");
                double val;
                if (splits.Length > 2 && double.TryParse(splits[2], out val))
                {
                    double s = val;

                    this.BenchmarkSpeed = (float)val;
                    this.BenchmarkProgress += 0.05f;

                }
            }

            if (this.BenchmarkProgress >= 1.0f)
            {
                this.Stop();
                this.BenchmarkProgress = 1.0f;
                this.BenchmarkFinished = true;
            }

            if (lineText.Contains("out of memory", StringComparison.InvariantCultureIgnoreCase))
            {
                OutOfMemory = true;
            }

            if (LineHandler != null)
            {
                LineHandler("claymore", e.Data);
            }
            */
        }

        void OnErrorDataRecv(object sender, DataReceivedEventArgs e)
        {
            if (LineHandler != null && e.Data != null)
            {
                LineHandler("claymore", e.Data);
            }
        }


    }
}
