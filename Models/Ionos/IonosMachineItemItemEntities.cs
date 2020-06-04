using Newtonsoft.Json;

namespace HPI.BBB.Autoscaler.Models.Ionos
{
    public class IonosMachineItemItemEntities
    {
        [JsonProperty("firewallrules")]
        public IonosObjectCollection Firewallrules { get; set; }
    }
}