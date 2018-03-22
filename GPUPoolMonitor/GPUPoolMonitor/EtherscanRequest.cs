using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GPUPoolMonitor
{
    public class EtherscanRequest
    {
        private static WebClient client = new WebClient();

        public Etherscan EthBalance<Etherscan>(string wallet)
        {
            var response = client.DownloadString("https://api.etherscan.io/api?module=account&action=balance&address=" + wallet + "&tag=latest&apikey=JVY9619VQDFNNQV6S6HUVS54J6WQNV92SF");

            return JsonConvert.DeserializeObject<Etherscan>(response);
        }
    }
}
