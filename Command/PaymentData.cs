using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GolemUI.Command
{
    public class Network
    {
        public string Id { get;  }
        private Network(string _id)
        {
            Id = _id;
        }

        public static readonly Network Mainnet = new Network("mainnet");
        public static readonly Network Rinkeby = new Network("rinkeby");
    }

    public class PaymentDriver
    {
        public string Id { get; }

        private PaymentDriver(string id)
        {
            Id = id;
        }

        public static readonly PaymentDriver ERC20 = new PaymentDriver("erc20");
        public static readonly PaymentDriver ZkSync = new PaymentDriver("zksync");
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class PaymentStatus
    {
        public decimal Amount { get; set; }
        public decimal Reserved { get; set; }

        public StatusNotes? Outgoing { get; set; }
        public StatusNotes? Incoming { get; set; }
        public string? Driver { get; set; }

        public string? Network { get; set; }

        public string? Token { get; set; }

    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class StatusNotes
    {
        public StatValue? Requested { get; set; }

        public StatValue? Accepted { get; set; }

        public StatValue? Confirmed { get; set; }

    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class StatValue
    {
        public decimal TotalAmount { get; set; }

        public uint AgreementsCount { get; set; }

    }


}