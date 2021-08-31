using GolemUI.Claymore;
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
        public volatile bool PreBenchmarkFinished;

        public volatile bool RunningPreRecorded;
        public volatile float BenchmarkProgress;
        public volatile float BenchmarkSpeed;

        private object _sync = new object();
        private readonly ILogger _logger;

        public string? GPUVendor { get; set; }

        private string? _unsafeGpuDetails;

        //private ClaymoreLiveStatus _liveStatus = new ClaymoreLiveStatus();
        private ClaymoreParser _claymoreParserBenchmark;
        private ClaymoreParser _claymoreParserPreBenchmark;

        ClaymoreImitateBenchmarkFromFile? _imitate;

        public ClaymoreParser ClaymoreParserBenchmark { get { return _claymoreParserBenchmark; } }
        public ClaymoreParser ClaymoreParserPreBenchmark { get { return _claymoreParserPreBenchmark; } }

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



        Process? _claymoreProcess;

        public ClaymoreBenchmark(int totalClaymoreReportsNeeded, ILogger logger)
        {
            _logger = logger;
            _claymoreParserBenchmark = new ClaymoreParser(isBenchmark: true, isPreBenchmark: false, totalClaymoreReportsNeeded, logger);
            _claymoreParserPreBenchmark = new ClaymoreParser(isBenchmark: true, isPreBenchmark: true, totalClaymoreReportsNeeded, logger);

        }



        public void Stop()
        {

            if (_claymoreProcess != null)
            {
                if (!_claymoreProcess.HasExited)
                {
                    _claymoreProcess.Kill(true);
                }
                _claymoreProcess = null;
            }
            _claymoreParserPreBenchmark.SetFinished();
            _claymoreParserBenchmark.SetFinished();
            if (_imitate != null)
            {
                _imitate.Stop();
            }
        }


        public bool RunBenchmarkRecording(string brFile, bool isPreBenchmark)
        {
            _imitate = new ClaymoreImitateBenchmarkFromFile();

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
                _claymoreParserPreBenchmark.BeforeParsing(enableRecording: false);
                _imitate.OutputDataReceived += OnOutputDataPreRecv;
                _imitate.OutputErrorReceived += OnErrorDataPreRecv;
                _imitate.OnExited += OnPreBenchmarkExit;
            }
            else
            {
                _claymoreParserBenchmark.BeforeParsing(enableRecording: false);
                _imitate.OutputDataReceived += OnOutputDataRecv;
                _imitate.OutputErrorReceived += OnErrorDataRecv;
                _imitate.OnExited += OnBenchmarkExit;
            }


            _imitate.Run();
            return true;
        }


        public bool RunPreBenchmark()
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

            arguments.AddRange("-epool test -li 200".Split(' '));

            foreach (var arg in arguments)
            {
                if (arg == null)
                {
                    throw new ArgumentNullException();
                }
                startInfo.Arguments += arg + " ";
                //startInfo.ArgumentList.Add(arg);
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
                BenchmarkError = $"Process: {this._claymore_exe_path} cannot be run, check antivirus settings";
                _claymoreProcess = null;
                return false;
            }
            _claymoreParserPreBenchmark.BeforeParsing(enableRecording: true);
            _claymoreProcess.OutputDataReceived += OnOutputDataPreRecv;
            _claymoreProcess.ErrorDataReceived += OnErrorDataPreRecv;
            _claymoreProcess.Exited += OnPreBenchmarkExit;
            _claymoreProcess.EnableRaisingEvents = true;
            _claymoreProcess.BeginErrorReadLine();
            _claymoreProcess.BeginOutputReadLine();

            return true;
        }


        public bool RunBenchmark(string cards, string niceness, string pool, string ethereumAddress, string nodeName)
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



            arguments.AddRange($"-wd 0 -epool {pool} -ewal {ethereumAddress} -eworker \"benchmark:0x0/{nodeName}:{ethereumAddress}/0\" -clnew 1 -clKernel 0".Split(' '));


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
                BenchmarkError = $"Process: {this._claymore_exe_path} cannot be run, check antivirus settings"; ;
                _claymoreProcess = null;
                return false;
            }
            _claymoreParserBenchmark.BeforeParsing(enableRecording: true);
            _claymoreProcess.OutputDataReceived += OnOutputDataRecv;
            _claymoreProcess.ErrorDataReceived += OnErrorDataRecv;
            _claymoreProcess.Exited += OnBenchmarkExit;
            _claymoreProcess.EnableRaisingEvents = true;
            _claymoreProcess.BeginErrorReadLine();
            _claymoreProcess.BeginOutputReadLine();

            /*
            var t = new Thread(() =>
            {
                while (_claymoreProcess.WaitForExit(300))
                {
                    //do stuff waiting for claymore process
                }
                this.BenchmarkFinished = true;
            });*/
            this.BenchmarkProgress = 0.1f;
            //t.Start();
            return true;
        }

        void OnPreBenchmarkExit(object? sender, EventArgs e)
        {
            _claymoreParserPreBenchmark.SetFinished();
        }

        void OnBenchmarkExit(object? sender, EventArgs e)
        {
            _claymoreParserBenchmark.SetFinished();
        }

        void OnOutputDataRecv(object sender, DataReceivedEventArgs e)
        {

            string? lineText = e.Data;
            //output contains spelling error avaiable instead of available, checking for boths:
            if (lineText == null)
                return;
            _logger?.LogInformation("OUTPUT: {0}", lineText);
            _claymoreParserBenchmark.ParseLine(lineText);
        }

        void OnOutputDataPreRecv(object sender, DataReceivedEventArgs e)
        {
            string? lineText = e.Data;
            //output contains spelling error avaiable instead of available, checking for boths:
            if (lineText == null)
                return;
            _logger?.LogInformation("PREOUT: {0}", lineText);
            _claymoreParserPreBenchmark.ParseLine(lineText);
        }

        void OnErrorDataPreRecv(object sender, DataReceivedEventArgs e)
        {
            if (LineHandler != null && e.Data != null)
            {
                LineHandler("claymore", e.Data);
            }
        }

        void OnErrorDataRecv(object sender, DataReceivedEventArgs e)
        {
            if (LineHandler != null && e.Data != null)
            {
                LineHandler("claymore", e.Data);
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
    public class ClaymoreImitateBenchmarkFromFile
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