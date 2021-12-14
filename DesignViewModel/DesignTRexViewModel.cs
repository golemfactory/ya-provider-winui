using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolemUI.Miners.TRex;
using Newtonsoft.Json;

namespace GolemUI.DesignViewModel
{
    class DesignTRexViewModel
    {
        public TRexDetails TRexDetails
        {
            get
            {
                var result = "";
                TRexDetails details = JsonConvert.DeserializeObject<TRexDetails>(result);
                return details;
            }
        }
    }
}
