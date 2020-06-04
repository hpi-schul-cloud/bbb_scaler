using System.Collections.Generic;

namespace HPI.BBB.Autoscaler.Models
{
    public class Workload
    {
        public float CPUUtilization { get; set; }
        public long AvailableMemory { get; set; }
        public long TotalMemory { get; set; }
        public long UsedMemory { get; set; }
        public float MemoryUtilization { get; internal set; }
    }
}