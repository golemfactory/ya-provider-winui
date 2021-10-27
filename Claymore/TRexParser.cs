using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Claymore
{
    public class TRexParser : ICloneable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly object __lockObj = new object();
        ClaymoreLiveStatus _liveStatus;

        public bool BenchmarkFinished { get; set; }

        public TRexParser()
        {
            _liveStatus = new ClaymoreLiveStatus(true, 5);
}
        public object Clone()
        {
            throw new NotImplementedException();
        }

        public void BeforeParsing(bool enableRecording)
        {
        }

        public void SetFinished()
        {

        }

        public void ParseLine(string lineText)
        {

        }

        public ClaymoreLiveStatus GetLiveStatusCopy()
        {
            lock (__lockObj)
            {
                return (ClaymoreLiveStatus)_liveStatus.Clone();
                // Your code...
            }
        }
    }
}
