using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.DesignViewModel
{
    public class MockupDashboardStatusViewModel
    {
        public DashboardStatusEnum Status => DashboardStatusEnum.Ready;
        public string StatusAdditionalInfo => "4 GB Mode";

        public MockupDashboardStatusViewModel()
        {

        }
    }
}
