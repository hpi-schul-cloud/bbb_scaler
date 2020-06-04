using Newtonsoft.Json;

namespace HPI.BBB.Autoscaler.Models.Ionos
{
    class IonosNIC
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("metadata")]
        public NICMetadata Metadata { get; set; }

        [JsonProperty("properties")]
        public NICProperties Properties { get; set; }

        [JsonProperty("entities")]
        public NICEntities Entities { get; set; }
    }
}