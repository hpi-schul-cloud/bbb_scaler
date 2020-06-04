using Newtonsoft.Json;

namespace HPI.BBB.Autoscaler.Models.Ionos
{
    class MachineProperties
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cores")]
        public long Cores { get; set; }

        [JsonProperty("ram")]
        public long Ram { get; set; }

        [JsonProperty("availabilityZone")]
        public string AvailabilityZone { get; set; }

        [JsonProperty("vmState")]
        public string VmState { get; set; }

        [JsonProperty("bootCdrom")]
        public object BootCdrom { get; set; }

        [JsonProperty("bootVolume")]
        public IonosObjectCollection BootVolume { get; set; }

        [JsonProperty("cpuFamily")]
        public string CpuFamily { get; set; }
    }
}
