using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPUPoolMonitor.Models
{
    public class DualWorkers
    {
        public string Address { get; set; }

        public int AlertRate { get; set; }

        public bool AlertActive { get; set; }
    }
}
