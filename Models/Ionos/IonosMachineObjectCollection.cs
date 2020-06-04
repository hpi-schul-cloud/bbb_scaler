using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace HPI.BBB.Autoscaler.Models.Ionos
{
    public class IonosMachineObjectCollection
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("href")]
        public Uri Href { get; set; }

        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public List<IonosMachineItem> Items { get; set; }
    }
}
