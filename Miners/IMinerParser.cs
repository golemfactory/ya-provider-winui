using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Miners
{
    public interface IMinerParser
    {
        public BenchmarkLiveStatus GetLiveStatusCopy();

        public bool AreAllCardInfosParsed();

        public void BeforeParsing(bool enableRecording);

        public void SetFinished();

        public void ParseLine(string line);

        public Task<bool> TimerBasedUpdateTick();
    }
}
