using System;
using System.Collections.Generic;
using System.Linq;
using Crypto.Pools.Generic;
using System.Timers;
using Crypto.Pools.Generic.Models;
using GPUPoolMonitor.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace GPUPoolMonitor
{
    static class Program
    {
        private static Timer Timer;
        public static Ethermine Ethermine { get; private set; }
        public static EtherscanRequest Etherscan { get; private set; }
        public static YiimpPool NLPool { get; private set; }
        public static Miner Miner { get; private set; }
        public static WorkerInfo UserData { get; set; }

        private static async Task Main(string[] args)
        {
            // Load user data from config file
            UserData = GetUserData<WorkerInfo>();

            Timer = new Timer();
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();
            var interval = UserData.TimeSpan;
            Timer.Interval = interval.TotalMilliseconds;

            Ethermine = new Ethermine();
            Etherscan = new EtherscanRequest();
            NLPool = new YiimpPool();

            // Eth / ZCash Mining Pool Stats
            await EthMinerStatsAsync().ConfigureAwait(false);

            // Dual mining stats - Yiimp Pool etc
            await DualMinerStatsAsync().ConfigureAwait(false);

            Console.ReadLine();
        }

        // Add config fle to collection
        public static WorkerInfo GetUserData<WorkerInfo>()
        {
            return JsonConvert.DeserializeObject<WorkerInfo>(File.ReadAllText("default.conf"));
        }

        private static async Task DualMinerStatsAsync()
        {
            foreach (var dual in UserData.Workers)
            {
                if (dual.WorkerAddressDual.Length > 0)
                {
                    LineBreak("*", false, Console.WindowWidth);

                    await DualMinerStatsAsync(UserData.YiimpPoolURL, dual.WorkerAddressDual).ConfigureAwait(false);
                }
            }
            LineBreak("*", true, Console.WindowWidth);
        }

        private static async Task EthMinerStatsAsync()
        {
            var workerDict = new Dictionary<string, decimal>();

            foreach (var worker in UserData.Workers)
            {
                if (worker.WorkerAddress.Length > 0)
                {
                    LineBreak("*", false, Console.WindowWidth);
                    await WorkerCheckAsync(worker.WorkerAddress, worker.AlertHashRate).ConfigureAwait(false);
                    LineBreak("*", true, Console.WindowWidth);

                    // Check stats here
                    var miningStatistics = await Miner.GetStatisticsAsync().ConfigureAwait(false);

                    // Woker Average Hashrate
                    var avgHashrate = (miningStatistics.ReportedHashrate / 1000000);

                    if (!worker.AlertActive)
                    {
                        if (avgHashrate < worker.AlertHashRate)
                        {
                            foreach (var miner in await Miner.GetWorkersAsync().ConfigureAwait(false))
                            {
                                workerDict[miner.Name] = Math.Round(miner.ReportedHashrate / 1000000, 1);
                            }

                            await WorkerSpeedCheckNotificationAsync(workerDict).ConfigureAwait(false);
                            worker.AlertActive = true;
                        }
                        else
                        {
                            worker.AlertActive = false;
                        }
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Please enter worker address settings in default.conf", "Warning", System.Windows.Forms.MessageBoxButtons.OK);
                }
            }
        }

        private static async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Eth / ZCash Mining Pool Stats
            await EthMinerStatsAsync().ConfigureAwait(false);

            // Dual mining stats - Yiimp Pool etc
            await DualMinerStatsAsync().ConfigureAwait(false);
        }

        private static void LineBreak(string symbol, bool newline, int length)
        {
            for (int i = 0; i < length; i++)
            {
                Console.Write(symbol);
            }
            if (newline)
                Console.WriteLine("\n");
        }

        private static async Task DualMinerStatsAsync(string poolurl, string dualmineraddress)
        {
            var dualMinerStats = await NLPool.YiimpBalance<Models.Wallet>(poolurl, dualmineraddress).ConfigureAwait(false);

            Console.Write("{0} Total {1} {2}", dualMinerStats.Currency, dualMinerStats.Unpaid, " | ");
            Console.Write("{0}  Balance {1} {2}", dualMinerStats.Currency, dualMinerStats.Balance, " | ");
            Console.Write("{0}  Total {1} {2}", dualMinerStats.Currency, dualMinerStats.Total + dualMinerStats.Unpaid, " | ");
            Console.Write("{0}  Total Unsold {1} {2}", dualMinerStats.Currency, dualMinerStats.Unsold, " | ");
            Console.Write("\n");
        }

        private static async Task WorkerCheckAsync(string address, decimal hashratealert)
        {
            Miner = Ethermine.GetMiner(address);

            // EtherScan ETH Balance
            var EthBalance = await Etherscan.EtherBalance<Etherscan>("0x" + address, UserData.EtherscanAPIKey).ConfigureAwait(false);

            // Check stats here
            var miningStatistics = await Miner.GetStatisticsAsync().ConfigureAwait(false);

            // Worker Hashrate
            foreach (var worker in await Miner.GetWorkersAsync().ConfigureAwait(false))
            {
                Console.Write(worker.Name + ": " + (worker.ReportedHashrate / 1000000) + " | ");
            }

            Console.Write("\n");

            // Woker Average Hashrate
            var avgHashrate = (miningStatistics.ReportedHashrate / 1000000);

            // If hashrate is over alert level - make text green
            if (avgHashrate > hashratealert)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Average Hashrate {0}", avgHashrate);
                Console.ResetColor();
            }
            // If hashrate is under alert level - make text red
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Average Hashrate {0}", avgHashrate);
                Console.ResetColor();
            }

            // Number of active workers
            Console.WriteLine("Active Workers {0}", miningStatistics.ActiveWorkers);

            // Worker last seen
            Console.WriteLine("Last Seen {0}", miningStatistics.LastSeen);

            // Earnings ETH
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("\t ETH/Day {0}", Math.Round(miningStatistics.CoinPerMin * 60 * 24, 7) + " | ");
            Console.Write("\t ETH/Week {0}", Math.Round(miningStatistics.CoinPerMin * 60 * 24 * 7, 7) + "\n");

            // Earnings Dollar
            Console.Write("\t $/Day {0:C}", Math.Round(miningStatistics.USDPerMin * 60 * 24, 2) + " | ");
            Console.Write("\t\t $/Week {0:C}", Math.Round(miningStatistics.USDPerMin * 60 * 24 * 7, 2) + "\n");

            // Earnings BTC
            Console.Write("\t BTC/Day {0}", Math.Round(miningStatistics.BTCPerMin * 60 * 24, 7) + " | ");
            Console.Write("\t BTC/Week {0}", Math.Round(miningStatistics.BTCPerMin * 60 * 24 * 7, 7) + "\n");

            Console.ForegroundColor = ConsoleColor.Yellow;

            // Balance from Main pool unpaid
            Console.WriteLine("Unpaid {0}", Math.Round(decimal.Parse(miningStatistics.Unpaid.ToString()) / 1000000000000000000m, 5));

            Console.ForegroundColor = ConsoleColor.Green;

            try
            {
                Console.WriteLine("Wallet Balance {0}", Math.Round(decimal.Parse(EthBalance.Result) / 1000000000000000000m, 5));
            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex);
            }

            Console.ResetColor();

            // Payout
            PayOuts();
        }

        private static void PayOuts()
        {
            foreach (var payment in Miner.GetPayoutsAsync().Result.Take(1))
            {
                if (payment.PaidOn.ToShortDateString() == DateTime.Now.ToShortDateString())
                {
                    // Send email
                    Console.WriteLine("Paid Today @ {0}", payment.PaidOn.Date.ToShortDateString());
                }
                else
                {
                    Console.WriteLine("Last Paid {0}", payment.PaidOn.ToShortDateString());
                }
            }
        }

        private static async Task WorkerSpeedCheckNotificationAsync(Dictionary<string, decimal> dict)
        {
            // Email Client
            var emailClient = new EmailSender(UserData.MailRecipientName, UserData.MailRecipientAddress);

            // Check for worker speeds...
            await emailClient.SendEmailWorkerSpeedUpdate(dict).ConfigureAwait(false);
        }

    }
}
