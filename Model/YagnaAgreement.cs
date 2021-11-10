using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Model
{
    public class YagnaAgreement
    {
        public class YagnaAgreementDemand
        {
            public Dictionary<string, object>? Properties { get; set; }
            public string? Constraints { get; set; }
            public string? DemandID { get; set; }
            public DateTime? Timestamp { get; set; }
        }
        public class YagnaAgreementOffer
        {
            public Dictionary<string, object>? Properties { get; set; }
            public string? Constraints { get; set; }
            public string? OfferID { get; set; }
            public string? ProviderID { get; set; }
            public DateTime? Timestamp { get; set; }
        }
        public string? AgreementID { get; set; }

        public YagnaAgreementOffer? Offer { get; set; }
        public YagnaAgreementDemand? Demand { get; set; }

        public DateTime? ValidTo { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? State { get; set; }
        public DateTime? Timestamp { get; set; }
        public string? AppSessionID { get; set; }

    }

    /*
     * Sample output
    {
       "agreementId":"cd6482cd3a7acfbbb2131f1da576ac537882321c7e1e0ae62d1a320559ee559a",
       "demand":{
          "properties":{
             "golem.com.payment.debit-notes.accept-timeout?":15,
             "golem.com.payment.platform.zksync-rinkeby-tglm.address":"0x03713ff71def97bce801ee8916adbd6fbd277e8b",
             "golem.node.debug.subnet":"LazySubnet",
             "golem.node.id.name":"miner+",
             "golem.srv.comp.expiration":1628177686037,
             "golem.srv.comp.task_package":"hash:sha3:9a3b5d67b0b27746283cb5f287c13eab1beaa12d92a9f536b747c7ae:http://yacn2.dev.golem.network:8000/local-image-c76719083b.gvmi"
          },
          "constraints":"(&(golem.runtime.name=gminer)\n\t(golem.com.payment.platform.zksync-rinkeby-tglm.address=*))",
          "demandId":"462d1c20a9ae4873bad6d3c7af4e05b9-c4b34a39a28969eac1a6f497e2644751f00f7231eff5c9b10d516af48f358cc4",
          "requestorId":"0x03713ff71def97bce801ee8916adbd6fbd277e8b",
          "timestamp":"2021-08-05T15:06:39.228769507Z"
       },
       "offer":{
          "properties":{
             "golem.activity.mining.algo":"ethash",
             "golem.activity.mining.app":"phoenix",
             "golem.com.payment.debit-notes.accept-timeout?":15,
             "golem.com.payment.platform.erc20-rinkeby-tglm.address":"0xb7c49ec8001678692b266dcde6041b75d3a2364d",
             "golem.com.payment.platform.zksync-rinkeby-tglm.address":"0xb7c49ec8001678692b266dcde6041b75d3a2364d",
             "golem.com.pricing.model":"linear",
             "golem.com.pricing.model.linear.coeffs":[
                0.0,
                0.0,
                0.1,
                0.0,
                0.0
             ],
             "golem.com.scheme":"payu",
             "golem.com.scheme.payu.interval_sec":120.0,
             "golem.com.usage.vector":[
                "golem.usage.mining.hash-rate",
                "golem.usage.duration_sec",
                "golem.usage.mining.hash",
                "golem.usage.mining.share"
             ],
             "golem.inf.cpu.architecture":"x86_64",
             "golem.inf.cpu.cores":8,
             "golem.inf.cpu.threads":3,
             "golem.inf.mem.gib":21.468276530504227,
             "golem.inf.storage.gib":264.98778076171874,
             "golem.node.debug.subnet":"LazySubnet",
             "golem.node.id.name":"fsdf sdff",
             "golem.runtime.name":"gminer",
             "golem.runtime.version":"0.1.0",
             "golem.srv.caps.multi-activity":true
          },
          "constraints":"(&\n  (golem.srv.comp.expiration>1628175993646)\n  (golem.node.debug.subnet=LazySubnet)\n)",
          "offerId":"6691f9cc404c480a83b9250b97ea1f2b-e41fa69db78058eb650e2a26394786204e7891876415dde4984d12c20b0944ff",
          "providerId":"0xf871b5073bdea0a83c6b216d93d047b50256a93b",
          "timestamp":"2021-08-05T15:06:39.228769507Z"
       },
       "validTo":"2021-08-05T16:06:39.226189Z",
       "approvedDate":"2021-08-05T15:06:39.426294500Z",
       "state":"Approved",
       "timestamp":"2021-08-05T15:06:39.228769507Z",
       "appSessionId":"ya-provider-10392",
       "proposedSignature":"NoSignature",
       "approvedSignature":"NoSignature",
       "committedSignature":"NoSignature"
    }

        *
        */


}
