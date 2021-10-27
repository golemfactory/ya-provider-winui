using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Claymore;
using GolemUI.Interfaces;
using Microsoft.Extensions.Logging;

namespace GolemUI
{
    namespace GolemUI.Command
    {

        public enum ProblemWithExeFile
        {
            Timeout,
            Antivirus,
            FileMissing,
            None
        };

        public delegate void OnProblemsWithExeFileEventHander(ProblemWithExeFile problem);



        public class TRexBenchmark
        {
            private string _trex_working_dir = @"plugins\t-rex";
            private string _trex_exe_path = @"plugins\t-rex\T-Rex.exe";

            private TRexParser _trexParserBenchmark;
            private TRexParser _trexParserPreBenchmark;
            private readonly ILogger _logger;

            public string BenchmarkError { get; set; }

            public LogLineHandler? LineHandler { get; set; }


            public event OnProblemsWithExeFileEventHander? ProblemWithExe;

            private Process _trexProcess;

            public TRexBenchmark(int totalClaymoreReportsNeeded, ILogger logger)
            {
                _logger = logger;
                _trexParserBenchmark = new TRexParser();
                _trexParserPreBenchmark = new TRexParser();
            }

            public bool RunBenchmark(string cards, string niceness, string pool, string ethereumAddress,
                string nodeName)
            {
                if (!System.IO.File.Exists(_trex_exe_path))
                {
                    ProblemWithExe?.Invoke(ProblemWithExeFile.FileMissing);
                    return false;
                }

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



                arguments.AddRange(
                    $"-wd 0 -r -1 -epool {pool} -ewal {ethereumAddress} -eworker \"benchmark:0x0/{nodeName}:{ethereumAddress}/0\" -clnew 1 -clKernel 0"
                        .Split(' '));


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
                    while (_claymoreProcess.WaitForExit(300))
                    {
                        //do stuff waiting for claymore process
                    }
                    this.BenchmarkFinished = true;
                });*/
                //this.BenchmarkProgress = 0.1f;
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
    }
}
