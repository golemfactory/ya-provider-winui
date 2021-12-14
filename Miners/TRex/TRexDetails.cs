using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Miners.TRex
{
    public class TRexDetails
    {
        public class TRexJsonDetailsGpu
        {
            public class TRexJsonDetailsGpuShares
            {
                public int? accepted_count { get; set; }
                public int? invalid_count { get; set; }
                public double? last_share_diff { get; set; }
                public int? last_share_submit_ts { get; set; }
                public double? max_share_diff { get; set; }
                public int? max_share_submit_ts { get; set; }
                public int? rejected_count { get; set; }
                public int? solved_count { get; set; }
            }

            public TRexJsonDetailsGpuShares? shares { get; set; }
            public int? cclock { get; set; }
            public int? dag_build_mode { get; set; }
            public int? device_id { get; set; }
            public string? efficiency { get; set; }
            public int? gpu_id { get; set; }
            public int? gpu_user_id { get; set; }
            public int? hashrate { get; set; }
            public int? hashrate_day { get; set; }
            public int? hashrate_hour { get; set; }
            public int? hashrate_instant { get; set; }
            public int? hashrate_minute { get; set; }
            public double? intensity { get; set; }
            public double? lhr_tune { get; set; }
            public bool? low_load { get; set; }
            public int? mclock { get; set; }
            public int? mtweak { get; set; }
            public string? name { get; set; }
            public bool? paused { get; set; }
            public int? pci_bus { get; set; }
            public int? pci_domain { get; set; }
            public int? pci_id { get; set; }
            public bool? potentially_unstable { get; set; }
            public int? power { get; set; }
            public int? power_avr { get; set; }
            public int? temperature { get; set; }
            public string? uuid { get; set; }
            public string? vendor { get; set; }
        }

        public List<TRexJsonDetailsGpu>? gpus { get; set; }
        public int? accepted_count { get; set; }

        public string? algorithm { get; set; }
        public string? api { get; set; }
        public string? build_date { get; set; }
        public string? coin { get; set; }
        public string? description { get; set; }
        public string? driver { get; set; }
        public int? gpu_total { get; set; }
        public int? hashrate { get; set; }
        public int? hashrate_day { get; set; }
        public int? hashrate_hour { get; set; }
        public int? hashrate_minute { get; set; }
        public int? invalid_count { get; set; }
        public string? name { get; set; }
        public string? os { get; set; }
        public bool? paused { get; set; }
        public int? rejected_count { get; set; }
        public string? revision { get; set; }
        public double? sharerate { get; set; }
        public double? sharerate_average { get; set; }
        public int? solved_count { get; set; }
        public int? success { get; set; }
        public int? time { get; set; }
        public int? uptime { get; set; }
        public bool? validate_shares { get; set; }
        public string? version { get; set; }
    }

    /*
     Sample query for reference
     {
"accepted_count":0,
"active_pool":{
  "difficulty":"4.00 G",
  "last_submit_ts":0,
  "ping":0,
  "proxy":"",
  "retries":0,
  "url":"n1.mining-proxy.imapp.pl:8069",
  "user":"0xd593411f3e6e79995e787b5f81d10e12fa6ecf04",
  "worker":"benchmark:0x0/:0xd593411f3e6e79995e787b5f81d10e12fa6ecf04/0"
},
"algorithm":"ethash",
"api":"4.1",
"build_date":"Nov 10 2021 18:13:23",
"coin":"",
"description":"T-Rex NVIDIA GPU miner",
"driver":"471.11",
"gpu_total":1,
"gpus":[
  {
     "cclock":1765,
     "dag_build_mode":0,
     "device_id":0,
     "efficiency":"0.00H/W",
     "gpu_id":0,
     "gpu_user_id":0,
     "hashrate":0,
     "hashrate_day":0,
     "hashrate_hour":0,
     "hashrate_instant":0,
     "hashrate_minute":0,
     "intensity":18.0,
     "lhr_tune":0.0,
     "low_load":false,
     "mclock":6994,
     "mtweak":0,
     "name":"RTX 3060 Laptop GPU",
     "paused":false,
     "pci_bus":1,
     "pci_domain":0,
     "pci_id":0,
     "potentially_unstable":false,
     "power":89,
     "power_avr":58,
     "shares":{
        "accepted_count":0,
        "invalid_count":0,
        "last_share_diff":0,
        "last_share_submit_ts":0,
        "max_share_diff":0,
        "max_share_submit_ts":0,
        "rejected_count":0,
        "solved_count":0
     },
     "temperature":63,
     "uuid":"3f3d099864eb765503fad52aae16543e",
     "vendor":"ASUS"
  }
],
"hashrate":0,
"hashrate_day":0,
"hashrate_hour":0,
"hashrate_minute":0,
"invalid_count":0,
"name":"t-rex",
"os":"win",
"paused":false,
"rejected_count":0,
"revision":"75a91be00af3",
"sharerate":0.0,
"sharerate_average":0.0,
"solved_count":0,
"success":1,
"time":1636723680,
"uptime":9,
"validate_shares":false,
"version":"0.24.6"
}
    */
}
