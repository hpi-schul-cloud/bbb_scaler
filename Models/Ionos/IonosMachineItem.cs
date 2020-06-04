using Newtonsoft.Json;
using System;

namespace HPI.BBB.Autoscaler.Models.Ionos
{
    public class IonosMachineItem
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("href")]
        public Uri Href { get; set; }

        [JsonProperty("metadata")]
        public IonosMachineItemMetadata Metadata { get; set; }

        [JsonProperty("properties")]
        public IonosMachineItemItemProperties Properties { get; set; }

        [JsonProperty("entities", NullValueHandling = NullValueHandling.Ignore)]
        public IonosMachineItemItemEntities Entities { get; set; }
    }
}