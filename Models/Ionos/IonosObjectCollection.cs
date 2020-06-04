using Newtonsoft.Json;

namespace HPI.BBB.Autoscaler.Models.Ionos
{
    public class IonosObjectCollection
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public IonosObjectCollection[] Items { get; set; }
    }
}