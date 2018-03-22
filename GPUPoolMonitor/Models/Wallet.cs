using Newtonsoft.Json;

namespace GPUPoolMonitor.Models
{
    public class Wallet
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("unsold")]
        public float Unsold { get; set; }

        [JsonProperty("balance")]
        public float Balance { get; set; }

        [JsonProperty("unpaid")]
        public float Unpaid { get; set; }

        [JsonProperty("paid24h")]
        public float Paid24H { get; set; }

        [JsonProperty("total")]
        public float Total { get; set; }
    }
}
