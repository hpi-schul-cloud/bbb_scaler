using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace HPI.BBB.Autoscaler.Models.Ionos
{
    class IonosMachine
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("metadata")]
        public MachineMetadata Metadata { get; set; }

        [JsonProperty("properties")]
        public MachineProperties Properties { get; set; }

        [JsonProperty("entities")]
        public MachineEntities Entities { get; set; }

        [JsonIgnore]
        public string PrimaryIP { get; set; }
        public string SecondaryIP { get; internal set; }
    }
}
