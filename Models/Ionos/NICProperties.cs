using Newtonsoft.Json;

namespace HPI.BBB.Autoscaler.Models.Ionos
{
    class NICProperties
    {
        [JsonProperty("name")]
        public object Name { get; set; }

        [JsonProperty("mac")]
        public string Mac { get; set; }

        [JsonProperty("ips")]
        public string[] Ips { get; set; }

        [JsonProperty("dhcp")]
        public bool Dhcp { get; set; }

        [JsonProperty("lan")]
        public long Lan { get; set; }

        [JsonProperty("firewallActive")]
        public bool FirewallActive { get; set; }
    }
}