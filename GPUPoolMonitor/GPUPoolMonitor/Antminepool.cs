using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GPUPoolMonitor
{
    public class Antminepool
    {
        private string response;

        public Wallet AntminePoolClient<Wallet>(string wallet)
        {
            var client = (HttpWebRequest)WebRequest.Create(new Uri("http://antminepool.com/api/wallet?address=" + wallet));
            client.AutomaticDecompression = DecompressionMethods.GZip;
            client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36 Edge/15.15063";
            client.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
            client.Host = "yiimp.ccminer.org";
            client.KeepAlive = true;

            try
            {
                using (var s = client.GetResponse().GetResponseStream())
                using (var sw = new StreamReader(s))
                {

                    response = sw.ReadToEnd();
                    //Console.WriteLine(response);
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex);
            }

            catch (ArgumentNullException ex)
            {
                Console.WriteLine(ex);
            }

            return JsonConvert.DeserializeObject<Wallet>(response);
        }
    }
}
