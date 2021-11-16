
using GolemUI.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using GolemUI.Miners;
using GolemUI.Src;
using GolemUI.Utils;
using Microsoft.Extensions.Logging;
using MinerAppName = GolemUI.Miners.MinerAppName;
using GolemUI.Command;

namespace GolemUI.Miners
{

    public enum ProblemWithExeFile { Timeout, Antivirus, FileMissing, None };
    public delegate void OnProblemsWithExeFileEventHander(ProblemWithExeFile problem);


    public class MinerBenchmark
    {
        public event OnProblemsWithExeFileEventHander? ProblemWithExe;

        private static Mutex mut = new Mutex();

        public LogLineHandler? LineHandler { get; set; }

        public string BenchmarkError = "";

        public volatile bool GPUNotFound;
        public volatile bool OutOfMemory;
        public volatile bool BenchmarkFinished;
        public volatile bool PreBenchmarkFinished;

        public volatile bool RunningPreRecorded;
        public volatile float BenchmarkProgress;
        public volatile float BenchmarkSpeed;

        private object _sync = new object();
        private readonly ILogger _logger;

        public string? GPUVendor { get; set; }

        private string? _unsafeGpuDetails;

        //private BenchmarkLiveStatus _liveStatus = new BenchmarkLiveStatus();
        private IMinerParser _minerParserBenchmark;
        private IMinerParser _minerParserPreBenchmark;

        PhoenixImitateBenchmarkFromFile? _imitate;

        private readonly DispatcherTimer _timer;

        public IMinerParser MinerParserBenchmark { get { return _minerParserBenchmark; } }
        public IMinerParser MinerParserPreBenchmark { get { return _minerParserPreBenchmark; } }

        public string? GPUDetails
        {
            get
            {
                lock (_sync)
                {
                    //return copy to be more thread safe
                    if (_unsafeGpuDetails == null)
                    {
                        return null;
                    }
                    return new String(_unsafeGpuDetails?.ToCharArray());
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



        Process? _minerProcess;

        public MinerBenchmark(IMinerApp minerApp, int totalPhoenixReportsNeeded, ILogger logger)
        {
            _logger = logger;
            _minerParserBenchmark = minerApp.CreateParserForBenchmark();
            _minerParserPreBenchmark = minerApp.CreateParserForPreBenchmark();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(10);
            _timer.Tick += OnUpdateMinerTick;
            _timer.Start();
        }

        private async void OnUpdateMinerTick(object sender, EventArgs e)
        {
            try
            {
                await _minerParserBenchmark.TimerBasedUpdateTick();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error OnUpdateMinerTick: {ex.Message}");
            }
        }

        public void Stop()
        {
            _timer.Stop();
            if (_minerProcess != null)
            {
                if (!_minerProcess.HasExited)
                {
                    _minerProcess.Kill(true);
                }
                _minerProcess = null;
            }
            _minerParserPreBenchmark.SetFinished();
            _minerParserBenchmark.SetFinished();
            if (_imitate != null)
            {
                _imitate.Stop();
            }
        }


        public bool RunBenchmarkRecording(string brFile, bool isPreBenchmark)
        {
            _imitate = new PhoenixImitateBenchmarkFromFile();

            if (!File.Exists(brFile))
            {
                return false;
            }
            if (isPreBenchmark)
            {
            }

            string readText = File.ReadAllText(brFile);

            _imitate.PrepareLines(readText);
            if (isPreBenchmark)
            {
                _minerParserPreBenchmark.BeforeParsing(enableRecording: false);
                _imitate.OutputDataReceived += OnOutputDataPreRecv;
                _imitate.OutputErrorReceived += OnErrorDataPreRecv;
                _imitate.OnExited += OnPreBenchmarkExit;
            }
            else
            {
                _minerParserBenchmark.BeforeParsing(enableRecording: false);
                _imitate.OutputDataReceived += OnOutputDataRecv;
                _imitate.OutputErrorReceived += OnErrorDataRecv;
                _imitate.OnExited += OnBenchmarkExit;
            }


            _imitate.Run();
            return true;
        }


        public bool RunPreBenchmark(IMinerApp minerApp, MinerAppConfiguration minerAppConfiguration)
        {
            if (!System.IO.File.Exists(minerApp.ExePath))
            {
                ProblemWithExe?.Invoke(ProblemWithExeFile.FileMissing);
                return false;
            }


            BenchmarkError = "";
            BenchmarkFinished = false;
            GPUNotFound = false;


            this.BenchmarkProgress = 0.0f;

            var startInfo = new ProcessStartInfo
            {
                FileName = minerApp.ExePath,
                WorkingDirectory = minerApp.WorkingDir,
                //Arguments = null,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            List<string> arguments = new List<string>();

            //Enable benchmark mode:

            string preBenchmarkParams = minerApp.GetPreBenchmarkParams(minerAppConfiguration);
            arguments.AddRange(preBenchmarkParams.Split(' '));

            foreach (var arg in arguments)
            {
                if (arg == null)
                {
                    throw new ArgumentNullException();
                }
                startInfo.Arguments += arg + " ";
                //startInfo.ArgumentList.Add(arg);
            }

            _minerProcess = new Process
            {
                StartInfo = startInfo
            };

            try
            {
                _minerProcess.Start();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                BenchmarkError = $"Miner failed to run, \ncheck antivirus settings";
                _minerProcess = null;
                ProblemWithExe?.Invoke(ProblemWithExeFile.Antivirus);
                return false;

            }
            _minerParserPreBenchmark.BeforeParsing(enableRecording: true);
            _minerProcess.OutputDataReceived += OnOutputDataPreRecv;
            _minerProcess.ErrorDataReceived += OnErrorDataPreRecv;
            _minerProcess.Exited += OnPreBenchmarkExit;
            _minerProcess.EnableRaisingEvents = true;
            _minerProcess.BeginErrorReadLine();
            _minerProcess.BeginOutputReadLine();

            return true;
        }


        public bool RunBenchmark(IMinerApp minerApp, MinerAppConfiguration minerAppConfiguration)
        {
            if (!System.IO.File.Exists(minerApp.ExePath))
            {
                ProblemWithExe?.Invoke(ProblemWithExeFile.FileMissing);
                return false;
            }
            BenchmarkError = "";
            BenchmarkFinished = false;
            GPUNotFound = false;

            this.BenchmarkProgress = 0.0f;

            var startInfo = new ProcessStartInfo
            {
                FileName = minerApp.ExePath,
                WorkingDirectory = minerApp.WorkingDir,
                //Arguments = null,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            List<string> arguments = new List<string>();

            //Enable benchmark mode:


            arguments.AddRange(minerApp.GetBenchmarkParams(minerAppConfiguration).Split(' '));


            foreach (var arg in arguments)
            {
                if (arg == null)
                {
                    throw new ArgumentNullException();
                }
                startInfo.Arguments += arg + " ";
                //startInfo.ArgumentList.Add(arg);
            }

            _minerProcess = new Process
            {
                StartInfo = startInfo
            };

            try
            {
                _minerProcess.Start();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                BenchmarkError = $"Miner failed to run, \ncheck antivirus settings";
                _minerProcess = null;

                ProblemWithExe?.Invoke(ProblemWithExeFile.Antivirus);

                return false;

            }
            _minerParserBenchmark.BeforeParsing(enableRecording: true);
            _minerProcess.OutputDataReceived += OnOutputDataRecv;
            _minerProcess.ErrorDataReceived += OnErrorDataRecv;
            _minerProcess.Exited += OnBenchmarkExit;
            _minerProcess.EnableRaisingEvents = true;
            _minerProcess.BeginErrorReadLine();
            _minerProcess.BeginOutputReadLine();
            _timer.Start();

            /*
            var t = new Thread(() =>
            {
                while (_minerProcess.WaitForExit(300))
                {
                    //do stuff waiting for phoenix process
                }
                this.BenchmarkFinished = true;
            });*/
            this.BenchmarkProgress = 0.1f;
            //t.Start();
            return true;
        }

        void OnPreBenchmarkExit(object? sender, EventArgs e)
        {

            _minerParserPreBenchmark.SetFinished();
        }

        void OnBenchmarkExit(object? sender, EventArgs e)
        {
            _timer.Stop();
            _minerParserBenchmark.SetFinished();
        }

        void OnOutputDataRecv(object sender, DataReceivedEventArgs e)
        {

            string? lineText = e.Data;
            //output contains spelling error avaiable instead of available, checking for boths:
            if (lineText == null)
                return;
            _logger?.LogInformation("OUTPUT: {0}", lineText);
            _minerParserBenchmark.ParseLine(lineText);
        }

        void OnOutputDataPreRecv(object sender, DataReceivedEventArgs e)
        {
            string? lineText = e.Data;
            //output contains spelling error avaiable instead of available, checking for boths:
            if (lineText == null)
                return;
            _logger?.LogInformation("PREOUT: {0}", lineText);
            _minerParserPreBenchmark.ParseLine(lineText);
        }

        void OnErrorDataPreRecv(object sender, DataReceivedEventArgs e)
        {
            if (LineHandler != null && e.Data != null)
            {
                LineHandler("phoenix", e.Data);
            }
        }

        void OnErrorDataRecv(object sender, DataReceivedEventArgs e)
        {
            if (LineHandler != null && e.Data != null)
            {
                LineHandler("phoenix", e.Data);
            }
        }


    }


    /// <summary>
    /// This class is for debug purposes only, it's not used in production code
    /// </summary>
    public class PhoenixImitateBenchmarkFromFile
    {
        struct ImitateBenchmarkEntry
        {
            public int millis;
            public string line;
        }

        public DataReceivedEventHandler? OutputDataReceived;
        public DataReceivedEventHandler? OutputErrorReceived;
        public EventHandler? OnExited;
        bool _requestStop = false;

        List<ImitateBenchmarkEntry> _entries = new List<ImitateBenchmarkEntry>();


        //public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);

        public void PrepareLines(string input)
        {
            string[] lines = input.Split('\n');

            _entries.Clear();
            foreach (var line in lines)
            {
                var parsedStrs = line.Split(new char[] { ':' }, 2);

                if (parsedStrs.Length < 2)
                {
                    continue;
                }
                int millis;
                if (!int.TryParse(parsedStrs[0], out millis))
                {
                    continue;
                }
                string outputLine = parsedStrs[1].Trim();

                if (!String.IsNullOrEmpty(outputLine))
                {
                    var entry = new ImitateBenchmarkEntry();
                    entry.millis = millis;
                    entry.line = outputLine;
                    _entries.Add(entry);
                }
            }
        }

        public void Stop()
        {
            _requestStop = true;
        }

        private DataReceivedEventArgs CreateMockDataReceivedEventArgs(string TestData)
        {
            if (String.IsNullOrEmpty(TestData))
                throw new ArgumentException("Data is null or empty.", "Data");

            DataReceivedEventArgs MockEventArgs =
                (DataReceivedEventArgs)System.Runtime.Serialization.FormatterServices
                 .GetUninitializedObject(typeof(DataReceivedEventArgs));

            FieldInfo[] EventFields = typeof(DataReceivedEventArgs)
                .GetFields(
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.DeclaredOnly);

            if (EventFields.Count() > 0)
            {
                EventFields[0].SetValue(MockEventArgs, TestData);
            }
            else
            {
                throw new ApplicationException(
                    "Failed to find _data field!");
            }

            return MockEventArgs;
        }

        public void Run()
        {
            var t = new Thread(() =>
            {
                int lastMillis = 0;
                for (int i = 0; i < _entries.Count; i++)
                {
                    var entry = _entries[i];

                    if (OutputDataReceived != null)
                    {
                        //this is dirty implementation only for replaying data for debug reasons
                        var args = CreateMockDataReceivedEventArgs(entry.line);
                        int sleepFor = entry.millis - lastMillis;
                        lastMillis = entry.millis;

                        if (sleepFor < 0)
                        {
                            sleepFor = 0;
                        }
                        if (sleepFor > 5000)
                        {
                            sleepFor = 5000;
                        }
                        Thread.Sleep(sleepFor / 2);
                        OutputDataReceived(this, args);
                    }
                    if (_requestStop)
                    {
                        break;
                    }
                }
                if (OnExited != null)
                {
                    OnExited(this, new EventArgs());
                }
            });
            t.Start();
        }



    }
}