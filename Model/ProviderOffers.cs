using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GolemUI.Model
{
    public class ProviderOffer
    {
        public JObject? properties { get; set; }
        public string? constraints { get; set; }
        public string? offerId { get; set; }
        public string? providerId { get; set; }
        public DateTime? timestamp { get; set; }

        public string DisplayName
        {
            get
            {
                JToken? exeunitName = properties?["golem.runtime.name"];

                if (exeunitName != null && timestamp != null)
                {
                    return "Offer " + exeunitName.ToString() + " - " + timestamp?.ToLocalTime();
                }
                return "Unknown offer";
            }
        }

        public TimeSpan? TimeAgo
        {
            get
            {
                if (timestamp == null)
                {
                    return null;
                }
                return (DateTime.Now - timestamp?.ToLocalTime());
            }
        }
    }

}
