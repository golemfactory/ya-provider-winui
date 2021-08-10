using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.UI.AnimatedIcons
{
    public class EllipseData
    {
        public double EllipseWidth { get; set; }
        public double EllipseHeight { get; set; }
        public double EllipsePosX { get; set; }
        public double EllipsePosY { get; set; }

    }

    public class DesignWorkingAnimIconModel /*: INotifyPropertyChanged*/
    {
        List<EllipseData> _circles = new List<EllipseData>();

        public List<EllipseData> Circles
        {
            get
            {
                return _circles;
            }
        }
        //public event PropertyChangedEventHandler PropertyChanged;

        public double EllipseWidth { get; }
        public double EllipseHeight { get; }

        public DesignWorkingAnimIconModel()
        {
            _circles.Add(new EllipseData() { EllipseHeight = 20, EllipseWidth = 20, EllipsePosX = 0, EllipsePosY = 30 });
            _circles.Add(new EllipseData() { EllipseHeight = 10, EllipseWidth = 10, EllipsePosX = 40, EllipsePosY = 40 });



            EllipseWidth = 20.0;
            EllipseHeight = 20.0;
        }
    }
}
