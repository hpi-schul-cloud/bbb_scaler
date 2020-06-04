using Newtonsoft.Json;

namespace HPI.BBB.Autoscaler.Models.Ionos
{
    class Firewallrules
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }
    }
}