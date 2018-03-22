using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPUPoolMonitor
{
        public class Wallet
        {
            [JsonProperty("unsold")]
            public double Unsold { get; set; }

            [JsonProperty("balance")]
            public long Balance { get; set; }

            [JsonProperty("unpaid")]
            public double Unpaid { get; set; }

            [JsonProperty("paid24h")]
            public long Paid24H { get; set; }

            [JsonProperty("total")]
            public double Total { get; set; }
        }
    }
