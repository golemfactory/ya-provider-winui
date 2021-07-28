using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.DesignViewModel
{
    public class MockupUsageDescriptionViewModel
    {

        public string Description => "Cpu threads";
        public int Current => 3;
        public int Total => 8;

        public MockupUsageDescriptionViewModel()
        {

        }
    }
}
