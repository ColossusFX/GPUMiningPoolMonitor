using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crypto.Pools.Generic;
using Newtonsoft.Json;
using System.Timers;
using Crypto.Pools.Generic.Models;
using GPUPoolMonitor.Models;

namespace GPUPoolMonitor
{
    class Program
    {
        // https://ethermine.org/miners/D49270155D6678eA9bDd2E89bebe7e9A6045704c
        // https://ethermine.org/miners/4020017ffeC1EeC49c590d99485AC71C372Dbf5D
        // XVG
        // D9187LrhBuk45VSvUfGNdrdgbfkBb3omyQ

        public static Timer Timer { get; set; }
        public static Ethermine Ethermine { get; private set; }
        public static Antminepool Antminepool { get; private set; }
        public static EtherscanRequest Etherscan { get; private set; }
        public static Miner Miner { get; private set; }

        public static bool alert;
        public static bool alert2;

        private static void Main(string[] args)
        {
            alert = false;
            alert2 = false;

            Timer = new Timer();       // Doesn't require any args
            //Timer.Interval = 10;
            Timer.Elapsed += Timer_Elapsed;    // Uses an event instead of a delegate   
            Timer.Start();                   // Start the timer 
            Timer.Interval = 60000 * 15;

            Ethermine = new Ethermine();
            Antminepool = new Antminepool();
            Etherscan = new EtherscanRequest();

            Console.WriteLine("************************************************************************");
            WorkerCheck("D49270155D6678eA9bDd2E89bebe7e9A6045704c", 4, 700);
            Console.WriteLine("************************************************************************ \n");

            Console.WriteLine("************************************************************************");
            WorkerCheck("4020017ffeC1EeC49c590d99485AC71C372Dbf5D", 2, 300);
            Console.WriteLine("************************************ \n");

            Console.WriteLine("************************************************************************");
            DualMinerStats("D9187LrhBuk45VSvUfGNdrdgbfkBb3omyQ");
            Console.WriteLine("************************************************************************ \n");

            Console.ReadLine();
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("************************************************************************");
            WorkerCheck("D49270155D6678eA9bDd2E89bebe7e9A6045704c", 4, 700);
            Console.WriteLine("************************************************************************ \n");

            Console.WriteLine("************************************************************************");
            WorkerCheck("4020017ffeC1EeC49c590d99485AC71C372Dbf5D", 2, 300);
            Console.WriteLine("************************************************************************ \n");

            Console.WriteLine("************************************************************************");
            DualMinerStats("D9187LrhBuk45VSvUfGNdrdgbfkBb3omyQ");
            Console.WriteLine("************************************************************************ \n");
        }

        private static void DualMinerStats(string dualmineraddress)
        {
            var dualMinerStats = Antminepool.AntminePoolClient<Wallet>(dualmineraddress);

            Console.Write("XVG Total {0} {1}", dualMinerStats.Unpaid, " | ");
            Console.Write("XVG Balance {0} {1}", dualMinerStats.Balance, " | ");
            Console.Write("XVG Total {0} {1}", dualMinerStats.Total + dualMinerStats.Unpaid, " | ");
            Console.Write("XVG Total Unsold {0} {1}", dualMinerStats.Unsold, " | ");
            Console.Write("\n");
        }

        private static void WorkerCheck(string address, int workercount, decimal hashratealert)
        {
            //Console.WriteLine("Alert = {0}", alert);

            var workerDict = new Dictionary<string, decimal>();

            Miner = Ethermine.GetMiner(address);
            var EthBalance = Etherscan.EthBalance<Etherscan>("0x" + address);

            // Check stats here
            var miningStatistics = Miner.GetStatisticsAsync().Result;

            foreach (var worker in Miner.GetWorkersAsync().Result)
            {
                Console.Write(worker.Name + ": " + (worker.ReportedHashrate / 1000000) + " | ");
            }

            Console.Write("\n");

            var avgHashrate = (miningStatistics.ReportedHashrate / 1000000);

            if (avgHashrate > hashratealert)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Average Hashrate {0}", avgHashrate);
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Average Hashrate {0}", avgHashrate);
                Console.ResetColor();
            }

            Console.WriteLine("Active Workers {0}", miningStatistics.ActiveWorkers);

            Console.WriteLine("Last Seen {0}", miningStatistics.LastSeen);

            // Earnings
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("\t ETH/Day {0}", Math.Round(miningStatistics.CoinPerMin * 60 * 24, 7) + " | ");
            Console.Write("\t ETH/Week {0}", Math.Round(miningStatistics.CoinPerMin * 60 * 24 * 7, 7) + "\n");

            Console.Write("\t $/Day {0}", Math.Round(miningStatistics.USDPerMin * 60 * 24, 2) + " | ");
            Console.Write("\t\t $/Week {0}", Math.Round(miningStatistics.USDPerMin * 60 * 24 * 7, 2) + "\n");

            Console.Write("\t BTC/Day {0}", Math.Round(miningStatistics.BTCPerMin * 60 * 24, 7) + " | ");
            Console.Write("\t BTC/Week {0}", Math.Round(miningStatistics.BTCPerMin * 60 * 24 * 7, 7) + "\n");

            Console.ForegroundColor = ConsoleColor.Yellow;

           Console.WriteLine("Unpaid {0}", decimal.Parse(miningStatistics.Unpaid.ToString()) / 1000000000000000000m);

            Console.ForegroundColor = ConsoleColor.Green;

            try
            {
                Console.WriteLine("Wallet Balance {0}", Math.Round(decimal.Parse(EthBalance.Result) / 1000000000000000000m, 5));
            }
            catch (FormatException)
            {
                throw;
            }

            Console.ResetColor();

            // Worker Count
            ActiveWorkers(workercount, miningStatistics);

            // Avg Hashrate check
            HashrateCheck(hashratealert, workerDict, avgHashrate);

            // Payout
            PayOuts();
        }

        private static void PayOuts()
        {
            foreach (var payment in Miner.GetPayoutsAsync().Result.Take(1))
            {
                Console.WriteLine("Last Paid {0}", payment.PaidOn.ToShortDateString());

                if (payment.PaidOn.ToShortDateString() == DateTime.Now.ToShortDateString())
                {
                    // Send email
                    Console.WriteLine("Last Paid {0}", payment.PaidOn.Date);
                }
            }
        }

        private static void HashrateCheck(decimal hashratealert, Dictionary<string, decimal> workerDict, decimal avgHashrate)
        {
            //Console.WriteLine("Alert = {0}", alert);

            if (!alert && avgHashrate < hashratealert)
            {
                foreach (var worker in Miner.GetWorkersAsync().Result)
                {
                    workerDict[worker.Name] = Math.Round(worker.ReportedHashrate / 1000000, 1);
                }

                WorkerSpeedCheckNotification(workerDict);

                alert = true;
            }
            else
            {
                alert = false;
            }
        }

        private static void ActiveWorkers(int workercount, MinerStatistics miningStatistics)
        {
            //Console.WriteLine("Alert 2 = {0}", alert2);

            if (!alert2 && miningStatistics.ActiveWorkers < workercount)
            {
                foreach (var worker in Miner.GetWorkersAsync().Result)
                {
                    WorkerOfflineNotification(worker.Name, miningStatistics.LastSeen);
                }

                alert2 = true;
            }
            else
            {
                alert2 = false;
            }
        }

        private static void WorkerOfflineNotification(string workerName, DateTime date)
        {
            // Email Client
            var emailClient = new EmailSender("Paul", "contact@iamaflip.co.uk", workerName);

            // Check for worker speeds...
            emailClient.SendEmailWorkerMinerCount(workerName, date);
        }

        private static void WorkerSpeedCheckNotification(Dictionary<string, decimal> dict)
        {
            // Email Client
            var emailClient = new EmailSender("Paul", "contact@iamaflip.co.uk");

            // Check for worker speeds...
            emailClient.SendEmailWorkerSpeedUpdate(dict);
        }

    }
}
