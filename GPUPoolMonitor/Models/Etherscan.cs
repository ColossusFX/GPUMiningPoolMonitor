using Newtonsoft.Json;

namespace GPUPoolMonitor.Models
{
    public class Etherscan
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("result")]
        public string Result { get; set; }
    }
}