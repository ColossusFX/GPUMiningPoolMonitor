using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GPUPoolMonitor.Models;

namespace GPUPoolMonitor
{
    public class YiimpPool
    {
        private readonly HttpClient HttpClient = new HttpClient();

        public async Task<Wallet> YiimpBalance<Wallet>(string pool, string wallet)
        {
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await HttpClient.GetStringAsync($"{pool}/api/wallet?address={wallet}").ConfigureAwait(false);

            return JsonConvert.DeserializeObject<Wallet>(response, Converter.Settings);
        }

        internal static class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                NullValueHandling = NullValueHandling.Ignore,
                Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
            };
        }
    }
}
