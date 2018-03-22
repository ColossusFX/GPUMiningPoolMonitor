using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GPUPoolMonitor
{
    public class EtherscanRequest
    {
        private readonly HttpClient HttpClient = new HttpClient();

        public async Task<Etherscan> EtherBalance<Etherscan>(string wallet, string apikey)
        {
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await HttpClient.GetStringAsync($"https://api.etherscan.io/api?module=account&action=balance&address={wallet}&tag=latest&apikey={apikey}").ConfigureAwait(false);

            return JsonConvert.DeserializeObject<Etherscan>(response);
        }
    }
}
