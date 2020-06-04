using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace HPI.BBB.Autoscaler.Models.Ionos
{
    class IonosMachineUpdate
    {
        public string Name { get; set; }
        public long? Cores { get; set; }
        public long? Ram { get; set; }
        public string AvailabilityZone { get; set; }
        public string CpuFamily { get; set; }
        public string BootVolume { get; set; }
        public string BootCdrom { get; set; }
    }
}
