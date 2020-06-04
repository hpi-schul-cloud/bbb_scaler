using Newtonsoft.Json;

namespace HPI.BBB.Autoscaler.Models.Ionos
{
    class NICEntities
    {
        [JsonProperty("firewallrules")]
        public Firewallrules Firewallrules { get; set; }
    }
}