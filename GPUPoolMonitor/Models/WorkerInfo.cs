using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPUPoolMonitor.Models
{
    public class WorkerInfo
    {
        public List<WorkerDetail> Workers { get; set; }

        public string EtherscanAPIKey { get; set; }

        public TimeSpan TimeSpan { get; set; }

        public string MailRecipientName { get; set; }

        public string MailRecipientAddress { get; set; }

        public string MailServer { get; set; }

        public string MailUserName { get; set; }

        public string MailPassword { get; set; }

        public string YiimpPoolURL { get; set; }
    }

    public class WorkerDetail
    {
        public string WorkerAddress { get; set; }

        public string WorkerAddressDual { get; set; }

        public int AlertHashRate { get; set; }

        public bool AlertActive { get; set; }

        public string EtherscanAPIKey { get; set; }
    }
}
