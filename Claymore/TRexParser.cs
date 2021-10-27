using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Claymore
{
    class TRexParser : ICloneable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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
    }
}
