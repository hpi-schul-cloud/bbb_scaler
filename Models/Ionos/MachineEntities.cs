using Newtonsoft.Json;

namespace HPI.BBB.Autoscaler.Models.Ionos
{
    class MachineEntities
    {
        [JsonProperty("cdroms")]
        public IonosMachineObjectCollection Cdroms { get; set; }

        [JsonProperty("volumes")]
        public IonosMachineObjectCollection Volumes { get; set; }

        [JsonProperty("nics")]
        public IonosMachineObjectCollection Nics { get; set; }
    }
}
