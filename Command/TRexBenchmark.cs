using GolemUI.TRex;
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
using GolemUI.Utils;
using Microsoft.Extensions.Logging;

namespace GolemUI.Command
{



    public class TRexBenchmark
    {
        public event OnProblemsWithExeFileEventHander? ProblemWithExe;

        private static Mutex mut = new Mutex();

        public LogLineHandler? LineHandler { get; set; }

        string _trex_working_dir = @"plugins\t-rex";
        string _trex_exe_path = @"plugins\t-rex\t-rex.exe";

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
        private TRexParser _trexParserBenchmark;
        private TRexParser _trexParserPreBenchmark;

        TRexImitateBenchmarkFromFile? _imitate;

        public TRexParser TRexParserBenchmark { get { return _trexParserBenchmark; } }
        public TRexParser TRexParserPreBenchmark { get { return _trexParserPreBenchmark; } }

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



        Process? _trexProcess;

        public TRexBenchmark(int totalTRexReportsNeeded, ILogger logger)
        {
            _logger = logger;
            _trexParserBenchmark = new TRexParser(isBenchmark: true, isPreBenchmark: false, totalTRexReportsNeeded, logger);
            _trexParserPreBenchmark = new TRexParser(isBenchmark: true, isPreBenchmark: true, totalTRexReportsNeeded, logger);

        }



        public void Stop()
        {

            if (_trexProcess != null)
            {
                if (!_trexProcess.HasExited)
                {
                    _trexProcess.Kill(true);
                }
                _trexProcess = null;
            }
            _trexParserPreBenchmark.SetFinished();
            _trexParserBenchmark.SetFinished();
            if (_imitate != null)
            {
                _imitate.Stop();
            }
        }


        public bool RunBenchmarkRecording(string brFile, bool isPreBenchmark)
        {
            _imitate = new TRexImitateBenchmarkFromFile();

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
                _trexParserPreBenchmark.BeforeParsing(enableRecording: false);
                _imitate.OutputDataReceived += OnOutputDataPreRecv;
                _imitate.OutputErrorReceived += OnErrorDataPreRecv;
                _imitate.OnExited += OnPreBenchmarkExit;
            }
            else
            {
                _trexParserBenchmark.BeforeParsing(enableRecording: false);
                _imitate.OutputDataReceived += OnOutputDataRecv;
                _imitate.OutputErrorReceived += OnErrorDataRecv;
                _imitate.OnExited += OnBenchmarkExit;
            }


            _imitate.Run();
            return true;
        }


        public bool RunPreBenchmark()
        {
            if (!System.IO.File.Exists(_trex_exe_path))
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
                FileName = this._trex_exe_path,
                WorkingDirectory = this._trex_working_dir,
                //Arguments = null,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            List<string> arguments = new List<string>();

            //Enable benchmark mode:
            arguments.AddRange("--algo ethash --benchmark --benchmark-epoch 450".Split(' '));

            foreach (var arg in arguments)
            {
                if (arg == null)
                {
                    throw new ArgumentNullException();
                }
                startInfo.Arguments += arg + " ";
                //startInfo.ArgumentList.Add(arg);
            }

            _trexProcess = new Process
            {
                StartInfo = startInfo
            };

            try
            {
                _trexProcess.Start();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                BenchmarkError = $"Miner failed to run, \ncheck antivirus settings";
                _trexProcess = null;
                ProblemWithExe?.Invoke(ProblemWithExeFile.Antivirus);
                return false;

            }
            _trexParserPreBenchmark.BeforeParsing(enableRecording: true);
            _trexProcess.OutputDataReceived += OnOutputDataPreRecv;
            _trexProcess.ErrorDataReceived += OnErrorDataPreRecv;
            _trexProcess.Exited += OnPreBenchmarkExit;
            _trexProcess.EnableRaisingEvents = true;
            _trexProcess.BeginErrorReadLine();
            _trexProcess.BeginOutputReadLine();

            return true;
        }


        public bool RunBenchmark(string cards, string niceness, string pool, string ethereumAddress, string nodeName)
        {
            if (!System.IO.File.Exists(_trex_exe_path))
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
                FileName = this._trex_exe_path,
                WorkingDirectory = this._trex_working_dir,
                //Arguments = null,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            List<string> arguments = new List<string>();

            //Enable benchmark mode:


            string args_text = $"--no-watchdog --algo ethash -o {pool} -u {ethereumAddress} -p x -w \"benchmark:0x0/{nodeName}:{ethereumAddress}/0\"";

            arguments.AddRange(args_text.Split(' '));
            //arguments.AddRange($"-wd 0 -r -1 -epool {pool} -ewal {ethereumAddress} -eworker \"benchmark:0x0/{nodeName}:{ethereumAddress}/0\" -clnew 1 -clKernel 0".Split(' '));


            if (!string.IsNullOrEmpty(cards))
            {
                arguments.Add("-gpus");
                arguments.Add(cards);
            }
            if (!string.IsNullOrEmpty(niceness))
            {
                arguments.Add("-li");
                arguments.Add(niceness);
            }

            foreach (var arg in arguments)
            {
                if (arg == null)
                {
                    throw new ArgumentNullException();
                }
                startInfo.Arguments += arg + " ";
                //startInfo.ArgumentList.Add(arg);
            }

            _trexProcess = new Process
            {
                StartInfo = startInfo
            };

            try
            {
                _trexProcess.Start();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                BenchmarkError = $"Miner failed to run, \ncheck antivirus settings";
                _trexProcess = null;

                ProblemWithExe?.Invoke(ProblemWithExeFile.Antivirus);

                return false;

            }
            _trexParserBenchmark.BeforeParsing(enableRecording: true);
            _trexProcess.OutputDataReceived += OnOutputDataRecv;
            _trexProcess.ErrorDataReceived += OnErrorDataRecv;
            _trexProcess.Exited += OnBenchmarkExit;
            _trexProcess.EnableRaisingEvents = true;
            _trexProcess.BeginErrorReadLine();
            _trexProcess.BeginOutputReadLine();

            /*
            var t = new Thread(() =>
            {
                while (_trexProcess.WaitForExit(300))
                {
                    //do stuff waiting for trex process
                }
                this.BenchmarkFinished = true;
            });*/
            this.BenchmarkProgress = 0.1f;
            //t.Start();
            return true;
        }

        void OnPreBenchmarkExit(object? sender, EventArgs e)
        {
            _trexParserPreBenchmark.SetFinished();
        }

        void OnBenchmarkExit(object? sender, EventArgs e)
        {
            _trexParserBenchmark.SetFinished();
        }

        void OnOutputDataRecv(object sender, DataReceivedEventArgs e)
        {

            string? lineText = e.Data;
            //output contains spelling error avaiable instead of available, checking for boths:
            if (lineText == null)
                return;
            _logger?.LogInformation("OUTPUT: {0}", lineText);
            _trexParserBenchmark.ParseLine(lineText);
        }

        void OnOutputDataPreRecv(object sender, DataReceivedEventArgs e)
        {
            string? lineText = e.Data;
            //output contains spelling error avaiable instead of available, checking for boths:
            if (lineText == null)
                return;
            _logger?.LogInformation("PREOUT: {0}", lineText);
            _trexParserPreBenchmark.ParseLine(lineText);
        }

        void OnErrorDataPreRecv(object sender, DataReceivedEventArgs e)
        {
            if (LineHandler != null && e.Data != null)
            {
                LineHandler("trex", e.Data);
            }
        }

        void OnErrorDataRecv(object sender, DataReceivedEventArgs e)
        {
            if (LineHandler != null && e.Data != null)
            {
                LineHandler("trex", e.Data);
            }
        }


    }


    struct ImitateBenchmarkEntry
    {
        public int millis;
        public string line;
    }

    /// <summary>
    /// This class is for debug purposes only, it's not used in production code
    /// </summary>
    public class TRexImitateBenchmarkFromFile
    {
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
